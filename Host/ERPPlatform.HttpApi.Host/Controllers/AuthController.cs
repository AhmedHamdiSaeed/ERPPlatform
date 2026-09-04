using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using ERPPlatform.Common;
using ERPPlatform.Email;
using ERPPlatform.Email.Otp;
using ERPPlatform.Email.Templates;
using ERPPlatform.Email.TenantBranding;

namespace ERPPlatform.Controllers;

[ApiController]
[Route("api")]
public class AuthController : AbpControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IdentityUserManager _userManager;
    private readonly IConfiguration _configuration;
    private readonly IBrevoEmailService _brevoEmailService;
    private readonly IEmailTemplateManager _emailTemplateManager;
    private readonly ITenantBrandingProvider _tenantBrandingProvider;
    private readonly IOtpService _otpService;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant,
        IdentityUserManager userManager,
        IConfiguration configuration,
        IBrevoEmailService brevoEmailService,
        IEmailTemplateManager emailTemplateManager,
        ITenantBrandingProvider tenantBrandingProvider,
        IOtpService otpService,
        IPermissionChecker permissionChecker,
        IPermissionGrantRepository permissionGrantRepository,
        ILogger<AuthController> logger)
    {
        _tenantRepository = tenantRepository;
        _currentTenant = currentTenant;
        _userManager = userManager;
        _configuration = configuration;
        _brevoEmailService = brevoEmailService;
        _emailTemplateManager = emailTemplateManager;
        _tenantBrandingProvider = tenantBrandingProvider;
        _otpService = otpService;
        _permissionChecker = permissionChecker;
        _permissionGrantRepository = permissionGrantRepository;
        _logger = logger;
    }

    #region 1. Tenant Check
    /// <summary>
    /// Checks if a tenant exists by name. Returns Result containing tenant details and branding.
    /// </summary>
    [HttpPost("tenant/check")]
    [HttpPost("auth/tenant/check")]
    [HttpPost("mobile/tenant/check")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<TenantCheckResponse>>> CheckTenant([FromBody] TenantCheckRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TenantName))
        {
            return BadRequest(Result<TenantCheckResponse>.Fail("Tenant name is required.", 400));
        }

        var tenant = await _tenantRepository.FindByNameAsync(request.TenantName.Trim(), includeDetails: true);
        if (tenant == null)
        {
            return Ok(Result<TenantCheckResponse>.Fail($"Tenant '{request.TenantName}' does not exist.", 404));
        }

        var branding = await _tenantBrandingProvider.GetBrandingAsync(tenant.Id);

        var data = new TenantCheckResponse
        {
            Exists = true,
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            LogoUrl = branding.LogoUrl,
            SupportEmail = branding.SupportEmail,
            WebsiteUrl = branding.WebsiteUrl
        };

        return Ok(Result<TenantCheckResponse>.Ok(data, "Tenant found successfully."));
    }

    /// <summary>
    /// Gets branding and logo for the current active tenant or specified tenant name.
    /// </summary>
    [HttpGet("tenant/branding")]
    [HttpGet("auth/tenant/branding")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<TenantBrandingInfo>>> GetTenantBranding([FromQuery] string? tenantName)
    {
        var targetName = Request.Headers["X-Tenant-Name"].FirstOrDefault()
                         ?? Request.Headers["__tenant"].FirstOrDefault()
                         ?? tenantName;

        var tid = _currentTenant.Id;
        var branding = await _tenantBrandingProvider.GetBrandingAsync(tid, targetName);
        return Ok(Result<TenantBrandingInfo>.Ok(branding));
    }

    /// <summary>
    /// Allows a tenant admin to update their organization logo in the database.
    /// </summary>
    [HttpPost("tenant/logo")]
    [HttpPost("auth/tenant/logo")]
    public async Task<ActionResult<Result<TenantBrandingInfo>>> SetTenantLogo([FromBody] SetTenantLogoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LogoUrl))
        {
            return BadRequest(Result<TenantBrandingInfo>.Fail("Logo URL or image data is required.", 400));
        }

        var tenantId = _currentTenant.Id;
        var tenantName = Request.Headers["X-Tenant-Name"].FirstOrDefault()
                         ?? Request.Headers["__tenant"].FirstOrDefault()
                         ?? request.TenantName;

        if (!tenantId.HasValue && !string.IsNullOrWhiteSpace(tenantName))
        {
            var tenant = await _tenantRepository.FindByNameAsync(tenantName.Trim());
            if (tenant != null) tenantId = tenant.Id;
        }

        if (!tenantId.HasValue)
        {
            return BadRequest(Result<TenantBrandingInfo>.Fail("Active tenant not found. Please log in to a tenant or specify tenant name.", 400));
        }

        await _tenantBrandingProvider.SetTenantLogoAsync(tenantId.Value, request.LogoUrl.Trim());
        var updated = await _tenantBrandingProvider.GetBrandingAsync(tenantId.Value);

        return Ok(Result<TenantBrandingInfo>.Ok(updated, "Tenant logo updated and saved in database successfully."));
    }
    #endregion

    #region 2. Universal Login & Token Generation
    /// <summary>
    /// Universal Login endpoint for Web, Mobile, and API clients.
    /// Follows Result pattern returning token, roles, permissions, and tenant info.
    /// </summary>
    [HttpPost("auth/login")]
    [HttpPost("mobile/auth/login")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(Result<LoginResponse>.Fail("Login identifier and password are required.", 400));
        }

        // Determine tenant
        var tenantName = Request.Headers["X-Tenant-Name"].FirstOrDefault()
                         ?? Request.Headers["__tenant"].FirstOrDefault()
                         ?? request.TenantName;

        Guid? tenantId = null;
        if (!string.IsNullOrWhiteSpace(tenantName))
        {
            var tenant = await _tenantRepository.FindByNameAsync(tenantName.Trim());
            if (tenant == null)
            {
                return BadRequest(Result<LoginResponse>.Fail($"Tenant '{tenantName}' was not found.", 404));
            }
            tenantId = tenant.Id;
        }

        using (_currentTenant.Change(tenantId))
        {
            var user = await FindUserByIdentifierAsync(request.Login.Trim());
            if (user == null)
            {
                return Unauthorized(Result<LoginResponse>.Fail("Invalid email/phone or password.", 401));
            }

            if (!user.IsActive)
            {
                return Unauthorized(Result<LoginResponse>.Fail("User account is inactive. Please contact support.", 403));
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Unauthorized(Result<LoginResponse>.Fail("User account is temporarily locked out. Please try again later.", 403));
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                await _userManager.AccessFailedAsync(user);
                return Unauthorized(Result<LoginResponse>.Fail("Invalid email/phone or password.", 401));
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            // Load roles & permissions
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissionsAsync(user, roles);

            var branding = await _tenantBrandingProvider.GetBrandingAsync(tenantId, tenantName);

            // Obtain real OpenIddict token so ABP endpoints and application-configuration recognize the session natively
            var tokenResponse = await ObtainOpenIddictTokenAsync(user.UserName ?? user.Email ?? request.Login, request.Password, tenantName);
            if (tokenResponse == null)
            {
                tokenResponse = GenerateJwtToken(user, roles, permissions, tenantId, branding.TenantName);
            }

            var responseData = new LoginResponse
            {
                Token = tokenResponse.Value.Token,
                RefreshToken = tokenResponse.Value.RefreshToken,
                TokenType = "Bearer",
                ExpiresIn = tokenResponse.Value.ExpiresIn,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    Permissions = permissions,
                    TenantId = tenantId,
                    TenantName = branding.TenantName,
                    LogoUrl = branding.LogoUrl
                }
            };

            return Ok(Result<LoginResponse>.Ok(responseData, "Login successful."));
        }
    }
    #endregion

    #region 3. Refresh Token
    /// <summary>
    /// Refreshes an expired or expiring access token following Result pattern.
    /// </summary>
    [HttpPost("auth/refresh")]
    [HttpPost("mobile/auth/refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(Result<RefreshTokenResponse>.Fail("Refresh token is required.", 400));
        }

        var tenantNameHeader = Request.Headers["X-Tenant-Name"].FirstOrDefault()
                               ?? Request.Headers["__tenant"].FirstOrDefault();

        // 1. Try refreshing with OpenIddict server
        var openIddictResult = await ObtainOpenIddictRefreshTokenAsync(request.RefreshToken, tenantNameHeader);
        if (openIddictResult != null)
        {
            var openIddictData = new RefreshTokenResponse
            {
                Token = openIddictResult.Value.Token,
                RefreshToken = openIddictResult.Value.RefreshToken,
                TokenType = "Bearer",
                ExpiresIn = openIddictResult.Value.ExpiresIn
            };
            return Ok(Result<RefreshTokenResponse>.Ok(openIddictData, "Token refreshed successfully."));
        }

        // 2. Fallback to JWT refresh token
        try
        {
            var principal = GetPrincipalFromToken(request.RefreshToken, validateLifetime: false);
            if (principal == null)
            {
                return Unauthorized(Result<RefreshTokenResponse>.Fail("Invalid refresh token.", 401));
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? principal.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(Result<RefreshTokenResponse>.Fail("Invalid token claims.", 401));
            }

            var tenantIdClaim = principal.FindFirst("tenantid")?.Value;
            Guid? tenantId = !string.IsNullOrWhiteSpace(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tid) ? tid : null;
            var tenantName = principal.FindFirst("tenantname")?.Value ?? tenantNameHeader;

            using (_currentTenant.Change(tenantId))
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(Result<RefreshTokenResponse>.Fail("User not found or inactive.", 401));
                }

                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await GetUserPermissionsAsync(user, roles);
                var branding = await _tenantBrandingProvider.GetBrandingAsync(tenantId, tenantName);

                var newTokens = GenerateJwtToken(user, roles, permissions, tenantId, branding.TenantName);

                var data = new RefreshTokenResponse
                {
                    Token = newTokens.Token,
                    RefreshToken = newTokens.RefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = newTokens.ExpiresIn
                };

                return Ok(Result<RefreshTokenResponse>.Ok(data, "Token refreshed successfully."));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token.");
            return Unauthorized(Result<RefreshTokenResponse>.Fail("Failed to refresh token.", 401));
        }
    }
    #endregion

    #region 4. Forgot Password (OTP via Brevo)
    /// <summary>
    /// Sends a 6-digit OTP code to the requested user's email using Brevo email service and tenant-branded template.
    /// </summary>
    [HttpPost("auth/forgot-password")]
    [HttpPost("mobile/auth/forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<Result>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(Result.Fail("Email address is required.", 400));
        }

        var tenantName = Request.Headers["X-Tenant-Name"].FirstOrDefault()
                         ?? Request.Headers["__tenant"].FirstOrDefault()
                         ?? request.TenantName;

        Guid? tenantId = null;
        if (!string.IsNullOrWhiteSpace(tenantName))
        {
            var tenant = await _tenantRepository.FindByNameAsync(tenantName.Trim());
            if (tenant != null)
            {
                tenantId = tenant.Id;
            }
        }

        using (_currentTenant.Change(tenantId))
        {
            var user = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                // Return success to avoid user enumeration
                return Ok(Result.Ok("If the account exists, a verification code has been sent to the email."));
            }

            var otp = await _otpService.GenerateOtpAsync(user.Email, tenantId, expiryMinutes: 10);
            var branding = await _tenantBrandingProvider.GetBrandingAsync(tenantId, tenantName);

            var recipientName = !string.IsNullOrWhiteSpace(user.Name) ? $"{user.Name} {user.Surname}".Trim() : user.UserName;
            var htmlBody = _emailTemplateManager.RenderForgotPasswordOtp(branding, recipientName, otp, expiryMinutes: 10);

            await _brevoEmailService.SendEmailAsync(
                user.Email,
                recipientName,
                $"[{branding.TenantName}] Password Reset OTP Code: {otp}",
                htmlBody
            );

            return Ok(Result.Ok("Verification code has been sent to your email address."));
        }
    }

    /// <summary>
    /// Verifies if the supplied OTP is valid for the email.
    /// </summary>
    [HttpPost("auth/verify-otp")]
    [HttpPost("mobile/auth/verify-otp")]
    [AllowAnonymous]
    public async Task<ActionResult<Result>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
        {
            return BadRequest(Result.Fail("Email and OTP code are required.", 400));
        }

        var tenantName = Request.Headers["X-Tenant-Name"].FirstOrDefault()
                         ?? Request.Headers["__tenant"].FirstOrDefault()
                         ?? request.TenantName;

        Guid? tenantId = null;
        if (!string.IsNullOrWhiteSpace(tenantName))
        {
            var tenant = await _tenantRepository.FindByNameAsync(tenantName.Trim());
            if (tenant != null) tenantId = tenant.Id;
        }

        var isValid = await _otpService.ValidateOtpAsync(request.Email.Trim(), request.Otp.Trim(), tenantId);
        if (!isValid)
        {
            return BadRequest(Result.Fail("Invalid or expired OTP code.", 400));
        }

        return Ok(Result.Ok("OTP verified successfully."));
    }

    /// <summary>
    /// Resets user password after OTP verification.
    /// </summary>
    [HttpPost("auth/reset-password")]
    [HttpPost("mobile/auth/reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<Result>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(Result.Fail("Email, OTP, and new password are required.", 400));
        }

        var tenantName = Request.Headers["X-Tenant-Name"].FirstOrDefault()
                         ?? Request.Headers["__tenant"].FirstOrDefault()
                         ?? request.TenantName;

        Guid? tenantId = null;
        if (!string.IsNullOrWhiteSpace(tenantName))
        {
            var tenant = await _tenantRepository.FindByNameAsync(tenantName.Trim());
            if (tenant != null) tenantId = tenant.Id;
        }

        var consumed = await _otpService.ConsumeOtpAsync(request.Email.Trim(), request.Otp.Trim(), tenantId);
        if (!consumed)
        {
            return BadRequest(Result.Fail("Invalid or expired OTP code.", 400));
        }

        using (_currentTenant.Change(tenantId))
        {
            var user = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                return BadRequest(Result.Fail("User not found.", 404));
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(Result.Fail(string.Join("; ", errors), 400, errors));
            }

            return Ok(Result.Ok("Password has been successfully reset. You can now login."));
        }
    }
    #endregion

    #region Helper Methods
    private async Task<IdentityUser?> FindUserByIdentifierAsync(string login)
    {
        // 1. By Email
        var user = await _userManager.FindByEmailAsync(login);
        if (user != null) return user;

        // 2. By Username
        user = await _userManager.FindByNameAsync(login);
        if (user != null) return user;

        // 3. By Phone Number
        var users = await _userManager.GetUsersForClaimAsync(new Claim(ClaimTypes.MobilePhone, login));
        if (users != null && users.Count > 0) return users.FirstOrDefault();

        // 4. Fallback search by normalized username / phone
        var allUsers = await _userManager.GetUsersInRoleAsync("admin");
        var match = allUsers.FirstOrDefault(u =>
            string.Equals(u.PhoneNumber, login, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(u.UserName, login, StringComparison.OrdinalIgnoreCase));

        return match;
    }

    private async Task<List<string>> GetUserPermissionsAsync(IdentityUser user, IList<string> roles)
    {
        var permissions = new HashSet<string>();

        if (roles.Any(r => string.Equals(r, "admin", StringComparison.OrdinalIgnoreCase)))
        {
            permissions.Add("*");
            return permissions.ToList();
        }

        foreach (var role in roles)
        {
            var roleGrants = await _permissionGrantRepository.GetListAsync("R", role);
            foreach (var grant in roleGrants)
            {
                permissions.Add(grant.Name);
            }
        }

        var userGrants = await _permissionGrantRepository.GetListAsync("U", user.Id.ToString());
        foreach (var grant in userGrants)
        {
            permissions.Add(grant.Name);
        }

        return permissions.ToList();
    }

    private async Task<(string Token, string RefreshToken, int ExpiresIn)?> ObtainOpenIddictTokenAsync(
        string username, string password, string? tenantName)
    {
        try
        {
            var handler = new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            };

            using var client = new System.Net.Http.HttpClient(handler);
            var selfUrl = _configuration["App:SelfUrl"] ?? "https://localhost:44327";
            var tokenEndpoint = $"{selfUrl.TrimEnd('/')}/connect/token";

            var pairs = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", "ERPPlatform_App" },
                { "username", username },
                { "password", password },
                { "scope", "offline_access ERPPlatform" }
            };

            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, tokenEndpoint)
            {
                Content = new System.Net.Http.FormUrlEncodedContent(pairs)
            };

            if (!string.IsNullOrWhiteSpace(tenantName))
            {
                request.Headers.Add("X-Tenant-Name", tenantName.Trim());
                request.Headers.Add("__tenant", tenantName.Trim());
            }

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenIddict token request returned status {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            var accessToken = root.GetProperty("access_token").GetString() ?? string.Empty;
            var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? (rt.GetString() ?? string.Empty) : string.Empty;
            var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 1800;

            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            return (accessToken, refreshToken, expiresIn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain OpenIddict token via /connect/token.");
            return null;
        }
    }

    private async Task<(string Token, string RefreshToken, int ExpiresIn)?> ObtainOpenIddictRefreshTokenAsync(
        string refreshToken, string? tenantName)
    {
        try
        {
            var handler = new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            };

            using var client = new System.Net.Http.HttpClient(handler);
            var selfUrl = _configuration["App:SelfUrl"] ?? "https://localhost:44327";
            var tokenEndpoint = $"{selfUrl.TrimEnd('/')}/connect/token";

            var pairs = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", "ERPPlatform_App" },
                { "refresh_token", refreshToken },
                { "scope", "offline_access ERPPlatform" }
            };

            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, tokenEndpoint)
            {
                Content = new System.Net.Http.FormUrlEncodedContent(pairs)
            };

            if (!string.IsNullOrWhiteSpace(tenantName))
            {
                request.Headers.Add("X-Tenant-Name", tenantName.Trim());
                request.Headers.Add("__tenant", tenantName.Trim());
            }

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenIddict refresh token request returned status {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            var accessToken = root.GetProperty("access_token").GetString() ?? string.Empty;
            var newRefreshToken = root.TryGetProperty("refresh_token", out var rt) ? (rt.GetString() ?? refreshToken) : refreshToken;
            var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 1800;

            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            return (accessToken, newRefreshToken, expiresIn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh OpenIddict token via /connect/token.");
            return null;
        }
    }

    private (string Token, string RefreshToken, int ExpiresIn) GenerateJwtToken(
        IdentityUser user, IList<string> roles, List<string> permissions, Guid? tenantId, string tenantName)
    {
        var jwtSecret = _configuration["Auth:JwtSecret"] ?? "ERPPlatformSecretKeyForJwtSigningMustBe32CharsLong!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = _configuration.GetValue<int?>("Auth:AccessTokenLifetimeMinutes") ?? 180;
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(user.PhoneNumber))
        {
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
        }

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenantid", tenantId.Value.ToString()));
            claims.Add(new Claim("tenantname", tenantName));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = _configuration["AuthServer:Authority"] ?? "https://localhost:44327",
            Audience = "ERPPlatform"
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);

        // Refresh token descriptor (valid for 180 days)
        var refreshDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("token_type", "refresh"),
                new Claim("tenantid", tenantId?.ToString() ?? string.Empty),
                new Claim("tenantname", tenantName)
            }),
            Expires = DateTime.UtcNow.AddDays(_configuration.GetValue<int?>("Auth:RefreshTokenLifetimeDays") ?? 180),
            SigningCredentials = credentials,
            Issuer = _configuration["AuthServer:Authority"] ?? "https://localhost:44327",
            Audience = "ERPPlatform"
        };

        var refreshToken = handler.CreateToken(refreshDescriptor);
        var refreshTokenString = handler.WriteToken(refreshToken);

        return (tokenString, refreshTokenString, expiryMinutes * 60);
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = false)
    {
        var jwtSecret = _configuration["Auth:JwtSecret"] ?? "ERPPlatformSecretKeyForJwtSigningMustBe32CharsLong!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = validateLifetime
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, validationParameters, out _);
    }
    #endregion
}

#region DTO & Response Models
public class TenantCheckRequest
{
    public string TenantName { get; set; } = string.Empty;
}

public class TenantCheckResponse
{
    public bool Exists { get; set; }
    public Guid? TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TenantName { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public UserProfileDto User { get; set; } = new();
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public Guid? TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string? TenantName { get; set; }
}

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string? TenantName { get; set; }
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string? TenantName { get; set; }
}

public class SetTenantLogoRequest
{
    public string LogoUrl { get; set; } = string.Empty;
    public string? TenantName { get; set; }
}
#endregion
