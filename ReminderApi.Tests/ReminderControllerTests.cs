using Microsoft.EntityFrameworkCore;
using ReminderApi.Controllers;
using ReminderApi.Models;
using Xunit;
using FluentAssertions;

namespace ReminderApi.Tests;

public class ReminderControllerTests
{
    private ReminderDbContext GetDb()
    {
        var options = new DbContextOptionsBuilder<ReminderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ReminderDbContext(options);
    }

    [Fact]
    public async Task CreateReminder_ShouldAddReminder()
    {
        var db = GetDb();
        var controller = new RemindersController(db);

        var request = new CreateReminderRequest
        {
            Message = "Test Reminder",
            SendAt = DateTime.UtcNow.AddMinutes(5),
            Email = "test@example.com"
        };

        var result = await controller.Create(request);

        db.Reminders.Count().Should().Be(1);
        db.Reminders.First().Message.Should().Be("Test Reminder");
    }
}
