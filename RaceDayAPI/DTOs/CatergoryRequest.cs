
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs;

public class CategoryRequest
{
    [Required, StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999.99")]
    public decimal DistanceKM { get; set; }

    [Range(1, int.MaxValue)]
    public int MaximumParticipants { get; set; } = 100;

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal EntryFee { get; set; } = 0;
}

