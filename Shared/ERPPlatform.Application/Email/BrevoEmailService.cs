using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Email;

public class BrevoEmailService : IBrevoEmailService, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrevoEmailService> _logger;
    private static readonly HttpClient _httpClient = new HttpClient();

    public BrevoEmailService(IConfiguration configuration, ILogger<BrevoEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var apiKey = _configuration["Brevo:ApiKey"] ?? string.Empty;
        var senderEmail = _configuration["Brevo:SenderEmail"] ?? "noreply@erpplatform.com";
        var senderName = _configuration["Brevo:SenderName"] ?? "ERP Platform";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning(
                "[BrevoEmailService] Brevo API Key not configured. Simulating email send.\n" +
                "To: {ToEmail} ({ToName})\nSubject: {Subject}\nBody Preview: {BodyPreview}",
                toEmail, toName, subject, htmlBody.Length > 200 ? htmlBody[..200] + "..." : htmlBody);
            return true;
        }

        try
        {
            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail, name = string.IsNullOrWhiteSpace(toName) ? toEmail : toName } },
                subject = subject,
                htmlContent = htmlBody
            };

            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[BrevoEmailService] Email successfully sent to {ToEmail}", toEmail);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("[BrevoEmailService] Failed to send email to {ToEmail}. Status: {StatusCode}, Error: {Error}",
                toEmail, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BrevoEmailService] Exception while sending email to {ToEmail}", toEmail);
            return false;
        }
    }
}
