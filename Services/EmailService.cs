using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ReminderApi.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string content);
}

public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(
        HttpClient http,
        IConfiguration config,
        ILogger<BrevoEmailService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string content)
    {
        var apiKey = _config["Brevo:ApiKey"];

        var payload = new
        {
            sender = new
            {
                email = _config["Brevo:SenderEmail"],
                name = _config["Brevo:SenderName"]
            },
            to = new[]
            {
                new { email = to }
            },
            subject,
            htmlContent = $"<p>{content}</p>"
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://api.brevo.com/v3/smtp/email");

        request.Headers.Add("api-key", apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Brevo email failed: {Error}", error);
            throw new Exception("Email sending failed");
        }

        _logger.LogInformation("Email sent to {Email}", to);
    }
}
