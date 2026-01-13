using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReminderApi.Models;

namespace ReminderApi.Controllers;

[ApiController]
[Route("reminders")]
public class RemindersController : ControllerBase
{
    private readonly ReminderDbContext _db;

    public RemindersController(ReminderDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReminderRequest request)
    {
        if (request.SendAt <= DateTime.UtcNow)
        {
            return BadRequest("SendAt must be in the future.");
        }

        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            Message = request.Message,
            SendAt = request.SendAt,
            Email = request.Email,
            Status = ReminderStatus.Scheduled
        };

        _db.Reminders.Add(reminder);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            reminder.Id,
            Status = reminder.Status.ToString(),
            reminder.SendAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReminders()
    {
        var reminders = await _db.Reminders
            .OrderByDescending(r => r.SendAt)
            .Select(r => new
            {
                id = r.Id.ToString(),
                message = r.Message,
                sendAt = r.SendAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                status = r.Status.ToString()
            })
            .ToListAsync();

        return Ok(reminders);
    }
}
