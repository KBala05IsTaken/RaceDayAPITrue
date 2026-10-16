using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

public class CreateEventRequest
{
    [Required, StringLength(100)]
    public string EventName { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    [Required, StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [RegularExpression("^(Running|Walking|Cycling)$")]
    public string EventType { get; set; } = "Running";
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Organiser")]
public class OrganiserController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public OrganiserController(RaceDayDbContext context)
    {
        _context = context;
    }

    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent(
        CreateEventRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (request.EventDate.Date < DateTime.Today)
        {
            return BadRequest(new
            {
                message = "The event date cannot be in the past."
            });
        }

        string? userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int organiserId))
            return Unauthorized();

        // Verify the current database role as well.
        var organiser = await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserID == organiserId);

        if (organiser == null || organiser.Role != "Organiser")
            return Forbid();

        int? highestId = await _context.Events
            .MaxAsync(e => (int?)e.EventID);

        int newEventId = highestId.HasValue
            ? checked(highestId.Value + 1)
            : 1;

        var eventItem = new Event
        {
            EventID = newEventId,
            EventName = request.EventName.Trim(),
            EventDate = request.EventDate.Date,
            Location = request.Location.Trim(),
            Description = request.Description?.Trim(),
            EventType = request.EventType,
            OrganiserID = organiserId
        };

        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        return Created(
            $"/api/events/{eventItem.EventID}",
            new
            {
                eventItem.EventID,
                eventItem.EventName,
                eventItem.EventDate,
                eventItem.Location,
                eventItem.EventType,
                eventItem.OrganiserID
            });
    }
}