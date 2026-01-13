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
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}] Reminder Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}] Checking for reminders at UTC now.");

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ReminderDbContext>();

                // Get all reminders that are scheduled and ready to send
                var remindersToSend = await db.Reminders
                    .Where(r => r.Status == ReminderStatus.Scheduled && r.SendAt <= DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var reminder in remindersToSend)
                {
                    // Log reminder to console
                    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}] Reminder sent: {reminder.Message}");

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
                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}] Error processing reminders: {ex.Message}");
            }

            // Check every 30 seconds
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
