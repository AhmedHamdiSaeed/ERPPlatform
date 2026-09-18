using System;
using System.Threading.Tasks;
using ERPPlatform.Email.Otp;
using Shouldly;
using Xunit;

namespace ERPPlatform.Application.Tests.Security;

public class OtpServiceTests
{
    private readonly OtpService _otpService = new();

    [Fact]
    public async Task GenerateOtpAsync_ReturnsSixDigitCode()
    {
        var email = "user@test.com";
        var otp = await _otpService.GenerateOtpAsync(email);

        otp.ShouldNotBeNullOrWhiteSpace();
        otp.Length.ShouldBe(6);
        int.TryParse(otp, out _).ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateOtpAsync_Succeeds_ForCorrectCode()
    {
        var email = "validate@test.com";
        var otp = await _otpService.GenerateOtpAsync(email);

        var isValid = await _otpService.ValidateOtpAsync(email, otp);
        isValid.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateOtpAsync_Fails_ForWrongCode()
    {
        var email = "wrong@test.com";
        await _otpService.GenerateOtpAsync(email);

        var isValid = await _otpService.ValidateOtpAsync(email, "000000");
        isValid.ShouldBeFalse();
    }

    [Fact]
    public async Task ConsumeOtpAsync_ConsumesCodeAtomically_SecondAttemptFails()
    {
        var email = "consume@test.com";
        var otp = await _otpService.GenerateOtpAsync(email);

        // First consume succeeds
        var firstTry = await _otpService.ConsumeOtpAsync(email, otp);
        firstTry.ShouldBeTrue();

        // Second consume fails because it's single-use
        var secondTry = await _otpService.ConsumeOtpAsync(email, otp);
        secondTry.ShouldBeFalse();
    }

    [Fact]
    public async Task OtpService_IsolateTenants_WithSameEmail()
    {
        var email = "same@tenant.com";
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        var otpTenant1 = await _otpService.GenerateOtpAsync(email, tenant1);
        var otpTenant2 = await _otpService.GenerateOtpAsync(email, tenant2);

        // Tenant 1 code should not validate for Tenant 2
        var validOnTenant2 = await _otpService.ValidateOtpAsync(email, otpTenant1, tenant2);
        validOnTenant2.ShouldBeFalse();

        // Tenant 1 code should validate for Tenant 1
        var validOnTenant1 = await _otpService.ValidateOtpAsync(email, otpTenant1, tenant1);
        validOnTenant1.ShouldBeTrue();
    }
}
