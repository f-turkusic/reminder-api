using System;
using System.ComponentModel.DataAnnotations;

namespace ReminderApi.Models;

public class CreateReminderRequest
{
    [Required]
    public string Message { get; set; } = null!;

    [Required]
    public DateTime SendAt { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
