# Reminder API

.NET 8 Web API for reminders with background processing.

## How to run

1. Install .NET 8 SDK
2. `dotnet restore`
3. `dotnet ef database update` (creates reminders.db)
4. `dotnet run`
5. API runs on https://localhost:5001/swagger

## API

### POST /reminders
Create reminder:
```json
{
  "message": "Don't forget meeting",
  "sendAt": "2026-01-13T15:00:00Z",
  "email": "user@example.com"
}
```

### GET /reminders
Get all reminders:
```json
[
  { "id": "1", "message": "Check logs", "sendAt": "2025-10-10T14:30:00Z", "status": "Scheduled" }
]
```

## Design

- .NET 8 + ASP.NET Core
- SQLite database (no setup needed)
- BackgroundService checks every 30s for due reminders
- Status: Scheduled → Sent
- Email field optional

## Files

- Controllers/RemindersController.cs - API endpoints
- Models/ - Reminder, Status enum
- Services/ReminderBackgroundService.cs - background processing
- Data/ReminderDbContext.cs - EF Core setup
