using Microsoft.EntityFrameworkCore;
using ReminderApi.Models;

public class ReminderDbContext : DbContext
{
    public ReminderDbContext(DbContextOptions<ReminderDbContext> options) : base(options) { }
    
    public DbSet<Reminder> Reminders { get; set; }  // This will create a 'Reminders' table in your database.
}