using System;
using System.Threading.Tasks;

namespace ERPPlatform.Security;

public class PasswordResetTokenInfo
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public string? IpAddress { get; set; }
}

public interface IPasswordResetTokenService
{
    /// <summary>
    /// Checks if a request is rate-limited (by IP or email).
    /// Returns true if rate-limited (too many requests), false if allowed.
    /// </summary>
    bool IsRateLimited(string email, string? ipAddress = null);

    /// <summary>
    /// Generates a cryptographically random, single-use token for password reset.
    /// </summary>
    Task<string> GenerateResetTokenAsync(string email, Guid? tenantId = null, string? ipAddress = null, int expiryMinutes = 30);

    /// <summary>
    /// Validates if a reset token is valid, active, and not expired or used.
    /// </summary>
    Task<PasswordResetTokenInfo?> ValidateResetTokenAsync(string token);

    /// <summary>
    /// Consumes a reset token atomically so it cannot be reused, and invalidates any previous tokens for this user.
    /// </summary>
    Task<PasswordResetTokenInfo?> ConsumeResetTokenAsync(string token);
}
