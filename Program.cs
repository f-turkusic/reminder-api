using Microsoft.EntityFrameworkCore;
using ReminderApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ReminderDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))); // Use SQLite if you want, change it to UseNpgsql for PostgreSQL or any other DBMS


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<ReminderBackgroundService>();
// Register service for sending emails
builder.Services.AddHttpClient<IEmailService, BrevoEmailService>();

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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add a simple error endpoint
app.Map("/error", (HttpContext context) =>
{
    context.Response.StatusCode = 500;
    return Results.Json(new { error = "An unexpected error occurred." });
});

app.Run();
