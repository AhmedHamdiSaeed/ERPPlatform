namespace ERPPlatform.Email;

public class BrevoEmailOptions
{
    public const string Position = "Brevo";

    public string ApiKey { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = "noreply@erpplatform.com";
    public string SenderName { get; set; } = "ERP Platform";
    public string SmtpHost { get; set; } = "smtp-relay.brevo.com";
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
}
