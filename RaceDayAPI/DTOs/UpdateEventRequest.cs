
using System.ComponentModel.DataAnnotations;
namespace RaceDayAPI.DTOs;

public class UpdateEventRequest
{
    [Required, StringLength(100)]
    public string EventName { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required, StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [RegularExpression("^(Running|Walking|Cycling)$")]
    public string EventType { get; set; } = "Running";
}

