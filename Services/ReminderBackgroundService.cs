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
        _logger.LogInformation("Reminder Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ReminderDbContext>();

                // Get all reminders that are scheduled and ready to send
                var remindersToSend = await db.Reminders
                    .Where(r => r.Status == ReminderStatus.Scheduled && r.SendAt <= DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var reminder in remindersToSend)
                {
                    // Log reminder to console
                    _logger.LogInformation($"Reminder: {reminder.Message} | Email: {reminder.Email ?? "none"} | SendAt: {reminder.SendAt}");

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
                _logger.LogError(ex, "Error processing reminders.");
            }

            // Check every 30 seconds
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
