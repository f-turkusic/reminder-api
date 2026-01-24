using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using ReminderApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ReminderDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))); 

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//API versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<ReminderBackgroundService>();
// Register service for sending emails
builder.Services.AddHttpClient<IEmailService, BrevoEmailService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ReminderDbContext>("database");

var jwtKey = "your-secret-key-at-least-32-characters-long-for-dev";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();


var app = builder.Build();  

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHttpsRedirection();
}

// AUTO APPLY MIGRATIONS

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReminderDbContext>();
    db.Database.Migrate();
}

app.MapControllers();

// Simple test endpoint
app.MapGet("/test", () => Results.Ok(new { message = "API is working!" }));

// Token endpoint for local development
if (app.Environment.IsDevelopment())
{
    app.MapPost("/auth/token", () =>
    {
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: "reminder-api-dev",
            audience: "reminder-api",
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, "dev-user") },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );
        
        return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }).WithName("GetToken");
}

app.MapHealthChecks("/health");

// Add a simple error endpoint
app.Map("/error", (HttpContext context) =>
{
    context.Response.StatusCode = 500;
    return Results.Json(new { error = "An unexpected error occurred." });
});

app.Run();
