using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Lists all events in date order.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _db.Events
            .AsNoTracking()
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.EventDate,
                e.Location,
                e.Description,
                e.EventType,
                e.OrganiserID
            })
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>
    /// Returns one event, its categories and its route.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvent(int id)
    {
        var item = await _db.Events
            .AsNoTracking()
            .Where(e => e.EventID == id)
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.EventDate,
                e.Location,
                e.Description,
                e.EventType,
                e.OrganiserID,

                Categories = e.Categories.Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName,
                    c.DistanceKM,
                    c.MaximumParticipants,
                    c.EntryFee
                }),

                Route = e.Route == null ? null : new
                {
                    e.Route.RouteID,
                    e.Route.RouteName,
                    e.Route.DistanceKM,
                    e.Route.StartPoint,
                    e.Route.FinishPoint,
                    e.Route.RouteDescription
                }
            })
            .SingleOrDefaultAsync();

        if (item == null)
            return NotFound(new { message = "Event not found." });

        return Ok(item);
    }
}
