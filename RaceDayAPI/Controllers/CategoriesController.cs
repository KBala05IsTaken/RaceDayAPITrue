
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns available categories for an event.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        bool eventExists = await _db.Events
            .AnyAsync(e => e.EventID == eventId);

        if (!eventExists)
            return NotFound(new { message = "Event not found." });

        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.EventID == eventId)
            .OrderBy(c => c.DistanceKM)
            .Select(c => new
            {
                c.CategoryID,
                c.EventID,
                c.CategoryName,
                c.DistanceKM,
                c.MaximumParticipants,
                c.EntryFee
            })
            .ToListAsync();

        return Ok(categories);
    }
}
