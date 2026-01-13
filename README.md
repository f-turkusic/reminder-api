# Reminder API

.NET 8 Web API for email reminders.

## Setup
```bash
dotnet restore
dotnet ef database update
dotnet user-secrets set "Brevo:ApiKey" "your-key"
dotnet run
```

API: http://localhost:5125/swagger

## Features
- REST API (POST/GET reminders)
- Background email processing
- Retry logic
- Input validation
- Health checks
- Configurable intervals

## Testing

### API Integration Tests (Newman/Postman)
- **Collection**: `ReminderApi.postman_collection.json` - Ready for CI/CD
- **Environment**: `ReminderApi.postman_environment.json` - Configured for testing
- **CI/CD**: `.github/workflows/api-tests.yml` - Runs API tests on PRs/push

Tests cover: POST/GET endpoints, status codes, response validation, error cases

## Approach & Design Decisions

- **Minimal API**: Used ASP.NET Core Web API with controllers for clean REST endpoints
- **Background Processing**: Hosted service checks reminders every configurable interval (30s prod, 5s dev)
- **Database**: SQLite with EF Core for simplicity and zero-config deployment
- **Email**: Brevo API with 3-attempt retry and exponential backoff for reliability
- **Validation**: DataAnnotations with custom validators and ModelState for input validation
- **Error Handling**: Global exception handling in dev, proper status codes
- **Security**: API keys via user secrets/env vars, never committed
- **Health Checks**: Database connectivity monitoring for production readiness
- **Testing**: Newman/Postman API tests in CI/CD pipeline

## Files
- Controllers/RemindersController.cs
- Services/ReminderBackgroundService.cs
- Services/EmailService.cs
- ReminderApi.postman_collection.json - API tests
- .github/workflows/api-tests.yml - CI/CD pipeline
