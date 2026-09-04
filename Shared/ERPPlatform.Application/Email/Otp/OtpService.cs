using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Email.Otp;

public class OtpService : IOtpService, ISingletonDependency
{
    private class OtpEntry
    {
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; }
    }

    private readonly ConcurrentDictionary<string, OtpEntry> _otpStore = new(StringComparer.OrdinalIgnoreCase);

    private static string BuildKey(string email, Guid? tenantId)
    {
        return $"{(tenantId.HasValue ? tenantId.Value.ToString("N") : "host")}:{email.Trim().ToLowerInvariant()}";
    }

    public Task<string> GenerateOtpAsync(string email, Guid? tenantId = null, int expiryMinutes = 10)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        var key = BuildKey(email, tenantId);

        _otpStore[key] = new OtpEntry
        {
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Attempts = 0
        };

        return Task.FromResult(code);
    }

    public Task<bool> ValidateOtpAsync(string email, string otp, Guid? tenantId = null)
    {
        var key = BuildKey(email, tenantId);
        if (!_otpStore.TryGetValue(key, out var entry))
        {
            return Task.FromResult(false);
        }

        if (DateTime.UtcNow > entry.ExpiresAt)
        {
            _otpStore.TryRemove(key, out _);
            return Task.FromResult(false);
        }

        entry.Attempts++;
        if (entry.Attempts > 5)
        {
            _otpStore.TryRemove(key, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(string.Equals(entry.Code, otp?.Trim(), StringComparison.Ordinal));
    }

    public Task<bool> ConsumeOtpAsync(string email, string otp, Guid? tenantId = null)
    {
        var key = BuildKey(email, tenantId);
        if (!_otpStore.TryGetValue(key, out var entry))
        {
            return Task.FromResult(false);
        }

        if (DateTime.UtcNow > entry.ExpiresAt)
        {
            _otpStore.TryRemove(key, out _);
            return Task.FromResult(false);
        }

        if (string.Equals(entry.Code, otp?.Trim(), StringComparison.Ordinal))
        {
            _otpStore.TryRemove(key, out _);
            return Task.FromResult(true);
        }

        entry.Attempts++;
        if (entry.Attempts > 5)
        {
            _otpStore.TryRemove(key, out _);
        }

        return Task.FromResult(false);
    }
}
