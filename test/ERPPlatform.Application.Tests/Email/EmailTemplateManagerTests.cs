using System;
using ERPPlatform.Email.Templates;
using ERPPlatform.Email.TenantBranding;
using Shouldly;
using Xunit;

namespace ERPPlatform.Application.Tests.Email;

public class EmailTemplateManagerTests
{
    private readonly EmailTemplateManager _manager = new();

    private readonly TenantBrandingInfo _defaultBranding = new()
    {
        TenantName = "Acme Corp",
        LogoUrl = "https://cdn.example.com/logo.png",
        PrimaryColor = "#2563eb",
        SupportEmail = "support@acme.com",
        WebsiteUrl = "https://acme.com"
    };

    [Fact]
    public void RenderPasswordResetLink_IncludesResetLink_AndTenantBranding()
    {
        var html = _manager.RenderPasswordResetLink(
            _defaultBranding,
            "John Doe",
            "https://app.acme.com/reset-password?token=secret123",
            60
        );

        html.ShouldNotBeNullOrWhiteSpace();
        html.ShouldContain("Acme Corp");
        html.ShouldContain("John Doe");
        html.ShouldContain("https://app.acme.com/reset-password?token=secret123");
        html.ShouldContain("60");
        html.ShouldContain("#2563eb");
    }

    [Fact]
    public void RenderPasswordChangedNotification_FormatsTimeWithTimezone_Correctly()
    {
        var utcNow = new DateTime(2026, 9, 18, 20, 30, 0, DateTimeKind.Utc);
        var clientIp = "197.35.12.8";
        var clientTimezone = "Egypt Standard Time"; // Windows ID for GMT+3 (or IANA Africa/Cairo)

        var html = _manager.RenderPasswordChangedNotification(
            _defaultBranding,
            "Sarah Connor",
            utcNow,
            clientIp,
            clientTimezone
        );

        html.ShouldNotBeNullOrWhiteSpace();
        html.ShouldContain("Sarah Connor");
        html.ShouldContain("197.35.12.8");
        html.ShouldContain("Acme Corp");
        // Ensure it contains the formatted time with GMT offset, and does not print raw 'UTC'
        html.ShouldNotContain("20:30:00 UTC");
        html.ShouldContain("GMT+");
    }

    [Fact]
    public void RenderPasswordChangedNotification_HandlesNullTimezone_Gracefully()
    {
        var utcNow = new DateTime(2026, 9, 18, 12, 0, 0, DateTimeKind.Utc);

        var html = _manager.RenderPasswordChangedNotification(
            _defaultBranding,
            "Alex Smith",
            utcNow,
            "127.0.0.1",
            null
        );

        html.ShouldNotBeNullOrWhiteSpace();
        html.ShouldContain("Alex Smith");
        html.ShouldContain("127.0.0.1");
    }

    [Fact]
    public void RenderForgotPasswordOtp_FormatsSecurityWarning_AndOtpCode()
    {
        var html = _manager.RenderForgotPasswordOtp(
            _defaultBranding,
            "Emily Watson",
            "984123",
            10
        );

        html.ShouldNotBeNullOrWhiteSpace();
        html.ShouldContain("984123");
        html.ShouldContain("Emily Watson");
        html.ShouldContain("10");
        html.ShouldContain("Acme Corp");
    }
}
