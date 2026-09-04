using System.Threading.Tasks;

namespace ERPPlatform.Email;

public interface IBrevoEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
}
