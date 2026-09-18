using System;
using ERPPlatform.Email.TenantBranding;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Email.Templates;

public class EmailTemplateManager : IEmailTemplateManager, ITransientDependency
{
    public string RenderForgotPasswordOtp(TenantBrandingInfo branding, string userName, string otpCode, int expiryMinutes = 10)
    {
        var year = DateTime.UtcNow.Year;
        var logoUrl = !string.IsNullOrWhiteSpace(branding.LogoUrl)
            ? branding.LogoUrl
            : TenantBrandingProvider.DefaultLogoUrl;
        var tenantName = !string.IsNullOrWhiteSpace(branding.TenantName)
            ? branding.TenantName
            : TenantBrandingProvider.DefaultSystemName;

        return $@"
<!DOCTYPE html>
<html lang=""en"" dir=""ltr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Password Reset OTP - {tenantName}</title>
  <style>
    body {{
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
      background-color: #f1f5f9;
      margin: 0;
      padding: 0;
      color: #1e293b;
    }}
    .wrapper {{
      width: 100%;
      background-color: #f1f5f9;
      padding: 40px 16px;
    }}
    .container {{
      max-width: 540px;
      margin: 0 auto;
      background-color: #ffffff;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
      border: 1px solid #e2e8f0;
    }}
    .header {{
      background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
      padding: 32px 24px;
      text-align: center;
      color: #ffffff;
    }}
    .header img {{
      max-height: 56px;
      max-width: 160px;
      margin-bottom: 12px;
      border-radius: 8px;
      background: #ffffff;
      padding: 4px;
    }}
    .header h1 {{
      margin: 0;
      font-size: 20px;
      font-weight: 700;
      letter-spacing: -0.025em;
    }}
    .content {{
      padding: 32px 28px;
    }}
    .greeting {{
      font-size: 16px;
      font-weight: 600;
      margin-bottom: 12px;
      color: #0f172a;
    }}
    .message {{
      font-size: 14px;
      line-height: 1.6;
      color: #475569;
      margin-bottom: 24px;
    }}
    .otp-box {{
      background-color: #f8fafc;
      border: 2px dashed #cbd5e1;
      border-radius: 12px;
      padding: 20px;
      text-align: center;
      margin: 24px 0;
    }}
    .otp-label {{
      font-size: 12px;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: #64748b;
      margin-bottom: 8px;
    }}
    .otp-code {{
      font-family: 'Courier New', Courier, monospace;
      font-size: 32px;
      font-weight: 800;
      letter-spacing: 6px;
      color: #1d4ed8;
      display: inline-block;
    }}
    .expiry {{
      font-size: 12px;
      color: #94a3b8;
      margin-top: 8px;
    }}
    .warning {{
      font-size: 13px;
      line-height: 1.5;
      color: #64748b;
      background-color: #fffbeb;
      border-left: 4px solid #f59e0b;
      padding: 12px 16px;
      border-radius: 6px;
      margin-bottom: 24px;
    }}
    .footer {{
      border-top: 1px solid #f1f5f9;
      padding: 20px 24px;
      text-align: center;
      font-size: 12px;
      color: #94a3b8;
      background-color: #fafafa;
    }}
    .footer a {{
      color: #3b82f6;
      text-decoration: none;
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""container"">
      <div class=""header"">
        <img src=""{logoUrl}"" alt=""{tenantName} Logo"" />
        <h1>{tenantName}</h1>
      </div>
      <div class=""content"">
        <div class=""greeting"">Hello {userName},</div>
        <div class=""message"">
          We received a request to reset your password for your <strong>{tenantName}</strong> account.
          Use the One-Time Password (OTP) below to complete the reset process:
        </div>

        <div class=""otp-box"">
          <div class=""otp-label"">Your Verification Code / رمز التحقق</div>
          <div class=""otp-code"">{otpCode}</div>
          <div class=""expiry"">⏱ Valid for {expiryMinutes} minutes • صالح لمدة {expiryMinutes} دقائق</div>
        </div>

        <div class=""warning"">
          <strong>Security Notice:</strong> If you did not request a password reset, please ignore this email or contact your administrator immediately.
        </div>

        <div class=""message"" style=""margin-bottom: 0; font-size: 13px;"">
          Best regards,<br>
          <strong>{tenantName} Support Team</strong>
        </div>
      </div>
      <div class=""footer"">
        &copy; {year} {tenantName}. All rights reserved.<br>
        <a href=""{branding.WebsiteUrl}"">{branding.WebsiteUrl}</a> • <a href=""mailto:{branding.SupportEmail}"">{branding.SupportEmail}</a>
      </div>
    </div>
  </div>
</body>
</html>";
    }

    public string RenderPasswordResetLink(TenantBrandingInfo branding, string userName, string resetLink, int expiryMinutes = 30)
    {
        var year = DateTime.UtcNow.Year;
        var logoUrl = !string.IsNullOrWhiteSpace(branding.LogoUrl)
            ? branding.LogoUrl
            : TenantBrandingProvider.DefaultLogoUrl;
        var tenantName = !string.IsNullOrWhiteSpace(branding.TenantName)
            ? branding.TenantName
            : TenantBrandingProvider.DefaultSystemName;

        return $@"
<!DOCTYPE html>
<html lang=""en"" dir=""ltr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Reset Your Password - {tenantName}</title>
  <style>
    body {{
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
      background-color: #f1f5f9;
      margin: 0;
      padding: 0;
      color: #1e293b;
    }}
    .wrapper {{
      width: 100%;
      background-color: #f1f5f9;
      padding: 40px 16px;
    }}
    .container {{
      max-width: 540px;
      margin: 0 auto;
      background-color: #ffffff;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
      border: 1px solid #e2e8f0;
    }}
    .header {{
      background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);
      padding: 32px 24px;
      text-align: center;
      color: #ffffff;
    }}
    .header img {{
      max-height: 56px;
      max-width: 160px;
      margin-bottom: 12px;
      border-radius: 8px;
      background: #ffffff;
      padding: 4px;
    }}
    .header h1 {{
      margin: 0;
      font-size: 20px;
      font-weight: 700;
      letter-spacing: -0.025em;
    }}
    .content {{
      padding: 32px 28px;
    }}
    .greeting {{
      font-size: 16px;
      font-weight: 600;
      margin-bottom: 12px;
      color: #0f172a;
    }}
    .message {{
      font-size: 14px;
      line-height: 1.6;
      color: #475569;
      margin-bottom: 24px;
    }}
    .btn-container {{
      text-align: center;
      margin: 32px 0;
    }}
    .btn {{
      display: inline-block;
      background-color: #2563eb;
      color: #ffffff !important;
      font-size: 15px;
      font-weight: 600;
      text-decoration: none;
      padding: 14px 32px;
      border-radius: 10px;
      box-shadow: 0 4px 10px rgba(37, 99, 235, 0.25);
    }}
    .btn:hover {{
      background-color: #1d4ed8;
    }}
    .expiry {{
      font-size: 12px;
      color: #94a3b8;
      text-align: center;
      margin-top: 12px;
    }}
    .url-fallback {{
      background-color: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      padding: 12px 16px;
      font-size: 12px;
      color: #64748b;
      word-break: break-all;
      margin: 20px 0;
    }}
    .warning {{
      font-size: 13px;
      line-height: 1.5;
      color: #64748b;
      background-color: #fffbeb;
      border-left: 4px solid #f59e0b;
      padding: 12px 16px;
      border-radius: 6px;
      margin-bottom: 24px;
    }}
    .footer {{
      border-top: 1px solid #f1f5f9;
      padding: 20px 24px;
      text-align: center;
      font-size: 12px;
      color: #94a3b8;
      background-color: #fafafa;
    }}
    .footer a {{
      color: #3b82f6;
      text-decoration: none;
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""container"">
      <div class=""header"">
        <img src=""{logoUrl}"" alt=""{tenantName} Logo"" />
        <h1>{tenantName}</h1>
      </div>
      <div class=""content"">
        <div class=""greeting"">Hello {userName},</div>
        <div class=""message"">
          We received a request to reset the password for your account on <strong>{tenantName}</strong>.
          Click the button below to choose a new password:
        </div>

        <div class=""btn-container"">
          <a href=""{resetLink}"" class=""btn"" target=""_blank"">Reset Password</a>
          <div class=""expiry"">⏱ This single-use link expires in {expiryMinutes} minutes.</div>
        </div>

        <div class=""url-fallback"">
          If the button above does not work, copy and paste this URL into your browser:<br>
          <a href=""{resetLink}"" style=""color: #2563eb;"">{resetLink}</a>
        </div>

        <div class=""warning"">
          <strong>Security Notice:</strong> If you did not request this password reset, no further action is required. Your account remains secure.
        </div>

        <div class=""message"" style=""margin-bottom: 0; font-size: 13px;"">
          Best regards,<br>
          <strong>{tenantName} Security Team</strong>
        </div>
      </div>
      <div class=""footer"">
        &copy; {year} {tenantName}. All rights reserved.<br>
        <a href=""{branding.WebsiteUrl}"">{branding.WebsiteUrl}</a> • <a href=""mailto:{branding.SupportEmail}"">{branding.SupportEmail}</a>
      </div>
    </div>
  </div>
</body>
</html>";
    }

    public string RenderPasswordChangedNotification(TenantBrandingInfo branding, string userName, DateTime changeTime, string? ipAddress = null)
    {
        var year = DateTime.UtcNow.Year;
        var logoUrl = !string.IsNullOrWhiteSpace(branding.LogoUrl)
            ? branding.LogoUrl
            : TenantBrandingProvider.DefaultLogoUrl;
        var tenantName = !string.IsNullOrWhiteSpace(branding.TenantName)
            ? branding.TenantName
            : TenantBrandingProvider.DefaultSystemName;

        var formattedTime = changeTime.ToString("MMMM dd, yyyy HH:mm 'UTC'");

        return $@"
<!DOCTYPE html>
<html lang=""en"" dir=""ltr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Security Alert: Password Changed - {tenantName}</title>
  <style>
    body {{
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
      background-color: #f1f5f9;
      margin: 0;
      padding: 0;
      color: #1e293b;
    }}
    .wrapper {{
      width: 100%;
      background-color: #f1f5f9;
      padding: 40px 16px;
    }}
    .container {{
      max-width: 540px;
      margin: 0 auto;
      background-color: #ffffff;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
      border: 1px solid #e2e8f0;
    }}
    .header {{
      background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
      padding: 32px 24px;
      text-align: center;
      color: #ffffff;
    }}
    .header img {{
      max-height: 56px;
      max-width: 160px;
      margin-bottom: 12px;
      border-radius: 8px;
      background: #ffffff;
      padding: 4px;
    }}
    .header h1 {{
      margin: 0;
      font-size: 20px;
      font-weight: 700;
      letter-spacing: -0.025em;
    }}
    .content {{
      padding: 32px 28px;
    }}
    .greeting {{
      font-size: 16px;
      font-weight: 600;
      margin-bottom: 12px;
      color: #0f172a;
    }}
    .message {{
      font-size: 14px;
      line-height: 1.6;
      color: #475569;
      margin-bottom: 20px;
    }}
    .info-card {{
      background-color: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      padding: 16px 20px;
      margin-bottom: 24px;
      font-size: 13px;
    }}
    .info-row {{
      display: flex;
      justify-content: space-between;
      padding: 6px 0;
      border-bottom: 1px solid #f1f5f9;
    }}
    .info-row:last-child {{
      border-bottom: none;
    }}
    .info-label {{
      color: #64748b;
      font-weight: 500;
    }}
    .info-value {{
      color: #0f172a;
      font-weight: 600;
    }}
    .warning {{
      font-size: 13px;
      line-height: 1.5;
      color: #991b1b;
      background-color: #fef2f2;
      border-left: 4px solid #ef4444;
      padding: 14px 16px;
      border-radius: 6px;
      margin-bottom: 24px;
    }}
    .footer {{
      border-top: 1px solid #f1f5f9;
      padding: 20px 24px;
      text-align: center;
      font-size: 12px;
      color: #94a3b8;
      background-color: #fafafa;
    }}
    .footer a {{
      color: #3b82f6;
      text-decoration: none;
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""container"">
      <div class=""header"">
        <img src=""{logoUrl}"" alt=""{tenantName} Logo"" />
        <h1>{tenantName}</h1>
      </div>
      <div class=""content"">
        <div class=""greeting"">Hello {userName},</div>
        <div class=""message"">
          The password for your <strong>{tenantName}</strong> account was successfully updated.
        </div>

        <div class=""info-card"">
          <div class=""info-row"">
            <span class=""info-label"">Event:</span>
            <span class=""info-value"">Password Changed</span>
          </div>
          <div class=""info-row"">
            <span class=""info-label"">Timestamp:</span>
            <span class=""info-value"">{formattedTime}</span>
          </div>
          {(string.IsNullOrWhiteSpace(ipAddress) ? "" : $@"
          <div class=""info-row"">
            <span class=""info-label"">IP Address:</span>
            <span class=""info-value"">{ipAddress}</span>
          </div>")}
        </div>

        <div class=""warning"">
          <strong>Didn't make this change?</strong><br>
          If you did not change your password, your account may be compromised. Please contact your system administrator or reach out to support immediately.
        </div>

        <div class=""message"" style=""margin-bottom: 0; font-size: 13px;"">
          Best regards,<br>
          <strong>{tenantName} Security Team</strong>
        </div>
      </div>
      <div class=""footer"">
        &copy; {year} {tenantName}. All rights reserved.<br>
        <a href=""{branding.WebsiteUrl}"">{branding.WebsiteUrl}</a> • <a href=""mailto:{branding.SupportEmail}"">{branding.SupportEmail}</a>
      </div>
    </div>
  </div>
</body>
</html>";
    }
}
