using System;
using System.Threading.Tasks;

namespace ERPPlatform.Email.Otp;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string email, Guid? tenantId = null, int expiryMinutes = 10);
    Task<bool> ValidateOtpAsync(string email, string otp, Guid? tenantId = null);
    Task<bool> ConsumeOtpAsync(string email, string otp, Guid? tenantId = null);
}
