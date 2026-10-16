
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/participant")]
[Authorize(Roles = "Participant")]
public class ParticipantController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ParticipantController(RaceDayDbContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);

    /// <summary>Returns the logged-in Participant's enrolments.</summary>
    [HttpGet("enrollments")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var entries = await _db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserID == userId)
            .OrderByDescending(e => e.EnrollmentDate)
            .Select(e => new
            {
                e.EnrollmentID,
                e.EventID,
                e.Event.EventName,
                e.Event.EventDate,
                e.CategoryID,
                e.Category.CategoryName,
                e.EnrollmentDate,
                e.Status
            })
            .ToListAsync();

        return Ok(entries);
    }

    /// <summary>Enrols the logged-in Participant in an event category.</summary>
    [HttpPost("enrollments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll(EnrollRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var category = await _db.Categories
            .Include(c => c.Event)
            .SingleOrDefaultAsync(c =>
                c.CategoryID == request.CategoryID);

        if (category == null)
            return NotFound(new
            {
                message = "Category not found."
            });

        if (category.EventID != request.EventID)
            return BadRequest(new
            {
                message = "The category does not belong to this event."
            });

        if (category.Event.EventDate.Date < DateTime.Today)
            return BadRequest(new
            {
                message = "You cannot enrol in an event that has passed."
            });

        bool alreadyEnrolled = await _db.Enrollments.AnyAsync(e =>
            e.UserID == userId &&
            e.EventID == request.EventID &&
            e.CategoryID == request.CategoryID);

        if (alreadyEnrolled)
            return Conflict(new
            {
                message = "You are already enrolled in this category."
            });

        int currentCount = await _db.Enrollments.CountAsync(e =>
            e.CategoryID == request.CategoryID &&
            e.Status != "Cancelled");

        if (currentCount >= category.MaximumParticipants)
            return Conflict(new
            {
                message = "This category is full."
            });

        int? maxId = await _db.Enrollments
            .MaxAsync(e => (int?)e.EnrollmentID);

        var enrollment = new Enrollment
        {
            EnrollmentID = maxId.HasValue ? maxId.Value + 1 : 1,
            UserID = userId,
            EventID = request.EventID,
            CategoryID = request.CategoryID,
            EnrollmentDate = DateTime.Now,
            Status = "Confirmed"
        };

        _db.Enrollments.Add(enrollment);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "The enrolment could not be saved. Check the event, category and duplicate-entry constraints."
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            enrollment.EnrollmentID,
            enrollment.UserID,
            enrollment.EventID,
            enrollment.CategoryID,
            enrollment.EnrollmentDate,
            enrollment.Status
        });
    }

    /// <summary>Returns results linked to the logged-in Participant's enrolments only.</summary>
    [HttpGet("results")]
    public async Task<IActionResult> GetMyResults()
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var results = await _db.Results
            .AsNoTracking()
            .Where(r => r.Enrollment.UserID == userId)
            .Select(r => new
            {
                r.ResultID,
                r.EnrollmentID,
                EventName = r.Enrollment.Event.EventName,
                CategoryName = r.Enrollment.Category.CategoryName,
                r.FinishTime,
                r.Position,
                r.Pace,
                r.ResultDate
            })
            .ToListAsync();

        return Ok(results);
    }
}

