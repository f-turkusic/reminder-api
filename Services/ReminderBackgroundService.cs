using Microsoft.EntityFrameworkCore;
using ReminderApi.Models;

namespace ReminderApi.Services;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ReminderBackgroundService> _logger;

    public ReminderBackgroundService(IServiceProvider services, ILogger<ReminderBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bosniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        Console.WriteLine($"[{TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, bosniaTimeZone):yyyy-MM-ddTHH:mm:ss}] Reminder Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, bosniaTimeZone);
                Console.WriteLine($"[{now:yyyy-MM-ddTHH:mm:ss}] Checking for reminders at Bosnia time now.");

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ReminderDbContext>();

                // Get all reminders that are scheduled and ready to send
                var remindersToSend = await db.Reminders
                    .Where(r => r.Status == ReminderStatus.Scheduled && r.SendAt <= now)
                    .ToListAsync(stoppingToken);

                foreach (var reminder in remindersToSend)
                {
                    // Log reminder to console
                    Console.WriteLine($"[{now:yyyy-MM-ddTHH:mm:ss}] Reminder sent: {reminder.Message}");

                    // optional send email
                    if (!string.IsNullOrEmpty(reminder.Email))
                    {
                        // await EmailService.SendEmail(reminder.Email, "Reminder", reminder.Message);
                    }

                    // Mark as sent
                    reminder.Status = ReminderStatus.Sent;
                }

                if (remindersToSend.Any())
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                var errorTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, bosniaTimeZone);
                Console.WriteLine($"[{errorTime:yyyy-MM-ddTHH:mm:ss}] Error processing reminders: {ex.Message}");
            }

            // Check every 30 seconds
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
