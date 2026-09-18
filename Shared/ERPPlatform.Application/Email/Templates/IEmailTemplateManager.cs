using System;
using ERPPlatform.Email.TenantBranding;

namespace ERPPlatform.Email.Templates;

public interface IEmailTemplateManager
{
    string RenderForgotPasswordOtp(TenantBrandingInfo branding, string userName, string otpCode, int expiryMinutes = 10);
    string RenderPasswordResetLink(TenantBrandingInfo branding, string userName, string resetLink, int expiryMinutes = 30);
    string RenderPasswordChangedNotification(TenantBrandingInfo branding, string userName, DateTime changeTime, string? ipAddress = null);
}
