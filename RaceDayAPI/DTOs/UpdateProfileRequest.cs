
using System.ComponentModel.DataAnnotations;
namespace RaceDayAPI.DTOs;

public class UpdateProfileRequest
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    // Leave blank to keep the existing password.
    [MinLength(12), StringLength(128)]
    public string? NewPassword { get; set; }
}

