using System;
using System.ComponentModel.DataAnnotations;

namespace ReminderApi.Models;

public class CreateReminderRequest
{
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
    public string Message { get; set; } = null!;

    [Required(ErrorMessage = "SendAt is required.")]
    [CustomValidation(typeof(CreateReminderRequest), nameof(ValidateSendAt))]
    public DateTime SendAt { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; }

    public static ValidationResult? ValidateSendAt(DateTime sendAt, ValidationContext context)
    {
        if (sendAt <= DateTime.UtcNow)
        {
            return new ValidationResult("SendAt must be in the future.");
        }
        return ValidationResult.Success;
    }
}
