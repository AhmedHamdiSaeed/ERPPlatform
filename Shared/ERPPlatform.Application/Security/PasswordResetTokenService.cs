using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ERPPlatform.Security;

public class PasswordResetTokenService : IPasswordResetTokenService, ISingletonDependency
{
    private class RateLimitTracker
    {
        public ConcurrentQueue<DateTime> Timestamps { get; } = new();
    }

    private readonly ConcurrentDictionary<string, PasswordResetTokenInfo> _tokenStore = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, RateLimitTracker> _rateLimitStore = new(StringComparer.OrdinalIgnoreCase);

    private const int MaxRequestsPerWindow = 5;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(15);

    public bool IsRateLimited(string email, string? ipAddress = null)
    {
        var now = DateTime.UtcNow;
        var keys = new[]
        {
            $"email:{email.Trim().ToLowerInvariant()}",
            !string.IsNullOrWhiteSpace(ipAddress) ? $"ip:{ipAddress.Trim()}" : null
        }.Where(k => k != null).Cast<string>();

        foreach (var key in keys)
        {
            var tracker = _rateLimitStore.GetOrAdd(key, _ => new RateLimitTracker());
            lock (tracker)
            {
                // Purge old timestamps
                while (tracker.Timestamps.TryPeek(out var oldest) && (now - oldest) > RateLimitWindow)
                {
                    tracker.Timestamps.TryDequeue(out _);
                }

                if (tracker.Timestamps.Count >= MaxRequestsPerWindow)
                {
                    return true; // Rate limit exceeded
                }

                tracker.Timestamps.Enqueue(now);
            }
        }

        return false;
    }

    public Task<string> GenerateResetTokenAsync(string email, Guid? tenantId = null, string? ipAddress = null, int expiryMinutes = 30)
    {
        // 1. Generate 32 bytes (256 bits) of cryptographically secure random bytes
        var randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
        var token = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // 2. Invalidate any existing unused tokens for this user/tenant
        foreach (var kvp in _tokenStore)
        {
            if (string.Equals(kvp.Value.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
                kvp.Value.TenantId == tenantId &&
                !kvp.Value.IsUsed)
            {
                kvp.Value.IsUsed = true;
            }
        }

        // 3. Store the new token
        var now = DateTime.UtcNow;
        var info = new PasswordResetTokenInfo
        {
            Token = token,
            Email = normalizedEmail,
            TenantId = tenantId,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(expiryMinutes),
            IsUsed = false,
            IpAddress = ipAddress
        };

        _tokenStore[token] = info;

        // Cleanup expired tokens older than 2 hours periodically
        CleanupExpiredTokens();

        return Task.FromResult(token);
    }

    public Task<PasswordResetTokenInfo?> ValidateResetTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<PasswordResetTokenInfo?>(null);
        }

        if (!_tokenStore.TryGetValue(token.Trim(), out var info))
        {
            return Task.FromResult<PasswordResetTokenInfo?>(null);
        }

        if (info.IsUsed || DateTime.UtcNow > info.ExpiresAt)
        {
            return Task.FromResult<PasswordResetTokenInfo?>(null);
        }

        return Task.FromResult<PasswordResetTokenInfo?>(info);
    }

    public Task<PasswordResetTokenInfo?> ConsumeResetTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<PasswordResetTokenInfo?>(null);
        }

        if (!_tokenStore.TryGetValue(token.Trim(), out var info))
        {
            return Task.FromResult<PasswordResetTokenInfo?>(null);
        }

        if (info.IsUsed || DateTime.UtcNow > info.ExpiresAt)
        {
            return Task.FromResult<PasswordResetTokenInfo?>(null);
        }

        // Mark as used atomically
        lock (info)
        {
            if (info.IsUsed || DateTime.UtcNow > info.ExpiresAt)
            {
                return Task.FromResult<PasswordResetTokenInfo?>(null);
            }

            info.IsUsed = true;
        }

        // Invalidate all tokens for this email
        foreach (var kvp in _tokenStore)
        {
            if (string.Equals(kvp.Value.Email, info.Email, StringComparison.OrdinalIgnoreCase) &&
                kvp.Value.TenantId == info.TenantId)
            {
                kvp.Value.IsUsed = true;
            }
        }

        return Task.FromResult<PasswordResetTokenInfo?>(info);
    }

    private void CleanupExpiredTokens()
    {
        var cutoff = DateTime.UtcNow.AddHours(-2);
        foreach (var kvp in _tokenStore)
        {
            if (kvp.Value.ExpiresAt < cutoff || (kvp.Value.IsUsed && kvp.Value.CreatedAt < cutoff))
            {
                _tokenStore.TryRemove(kvp.Key, out _);
            }
        }
    }
}
