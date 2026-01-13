using Microsoft.EntityFrameworkCore;
using ReminderApi.Models;

namespace ReminderApi.Services;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    private readonly IEmailService? _emailService;

    public ReminderBackgroundService(IServiceProvider services, ILogger<ReminderBackgroundService> logger, IConfiguration configuration)
    {
        _services = services;
        _logger = logger;
        _configuration = configuration;
    }

    public ReminderBackgroundService(
    IServiceProvider services,
    ILogger<ReminderBackgroundService> logger,
    IConfiguration configuration,
    IEmailService emailService)
    {
        _services = services;
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = _configuration.GetValue<int>("ReminderCheckIntervalSeconds", 30); // Default to 30 if not set
        var bosniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        Console.WriteLine($"[{TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, bosniaTimeZone):yyyy-MM-ddTHH:mm:ss}] Reminder Background Service started with check interval: {intervalSeconds} seconds.");

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

                    bool emailSent = false;
                    // send email
                    if (!string.IsNullOrEmpty(reminder.Email) && _emailService != null)
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(reminder.Email, "Reminder", reminder.Message);
                            emailSent = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send email for reminder {Id}", reminder.Id);
                        }
                    }

                    // Mark as sent only if email was sent successfully
                    if (emailSent)
                    {
                        reminder.Status = ReminderStatus.Sent;
                    }
                }

                if (remindersToSend.Any())
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                var errorTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, bosniaTimeZone);
                Console.WriteLine($"[{errorTime:yyyy-MM-ddTHH:mm:ss}] Error processing reminders: {ex.Message}");
            }

            // Check every configured interval
            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }
}
