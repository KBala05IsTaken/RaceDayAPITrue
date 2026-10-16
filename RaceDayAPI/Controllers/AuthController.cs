
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        RaceDayDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        string email = request.Email.Trim().ToLowerInvariant();

        bool emailExists = await _context.Users.AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        string role = request.Role;

        // Organiser registration requires a server-side invitation
        // code. Keep that code out of source control.
        if (role == "Organiser")
        {
            string? configuredCode =
                _configuration["OrganiserRegistrationCode"];

            string? suppliedCode =
                Request.Headers["X-Organiser-Code"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(configuredCode) ||
                string.IsNullOrWhiteSpace(suppliedCode) ||
                !string.Equals(
                    configuredCode,
                    suppliedCode,
                    StringComparison.Ordinal))
            {
                return Forbid();
            }
        }

        int newId = await GetNextUserId();

        var user = new User
        {
            UserID = newId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = role
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // The unique database constraint remains the final
            // protection against concurrent duplicate registrations.
            return Conflict(new
            {
                message = "The account could not be created. " +
                          "Check whether the email or ID already exists."
            });
        }

        return Created(
            $"/api/auth/account/{user.UserID}",
            new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        string email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });

        var verification = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });

        // If a password hash needs upgrading, save the replacement.
        if (verification ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            await _context.SaveChangesAsync();
        }

        // Remove any previous identity and session state.
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        HttpContext.Session.Clear();

        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserID.ToString()),

            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return Ok(new
        {
            message = "Login successful.",
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new
        {
            message = "You have been logged out."
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        int? userId = HttpContext.Session.GetInt32("UserID");

        if (userId == null)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Unauthorized(new
            {
                message = "Session expired. Please log in again."
            });
        }

        // Read the current database record instead of exposing
        // sensitive information from the session or cookie.
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.UserID == userId.Value)
            .Select(u => new
            {
                u.UserID,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Role
            })
            .SingleOrDefaultAsync();

        if (user == null)
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Unauthorized();
        }

        // Do not accept an old cookie if the database role changed.
        if (User.FindFirstValue(ClaimTypes.Role) != user.Role)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

            return Unauthorized(new
            {
                message = "Your account permissions changed. Log in again."
            });
        }

        return Ok(user);
    }

    private async Task<int> GetNextUserId()
    {
        int? highestId = await _context.Users.MaxAsync(u => (int?)u.UserID);

        return highestId.HasValue
            ? checked(highestId.Value + 1)
            : 1;
    }


}

