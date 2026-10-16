
using System.ComponentModel.DataAnnotations;
namespace RaceDayAPI.DTOs;

public class ResultRequest
{
    // Example JSON value: "00:28:35"
    [Required]
    public TimeSpan FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int Position { get; set; }

    [Range(typeof(decimal), "0.01", "999.99")]
    public decimal? Pace { get; set; }
}

