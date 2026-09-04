using ERPPlatform.Email.TenantBranding;

namespace ERPPlatform.Email.Templates;

public interface IEmailTemplateManager
{
    string RenderForgotPasswordOtp(TenantBrandingInfo branding, string userName, string otpCode, int expiryMinutes = 10);
}
