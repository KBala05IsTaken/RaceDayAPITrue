using System.ComponentModel.DataAnnotations;
namespace RaceDayAPI.DTOs;

public class RegisterRequest
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(12), StringLength(128)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Organiser|Participant)$")]
    public string Role { get; set; } = "Participant";
}