using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Infrastructure.Services;

/// <summary>
/// JWT token generation service.
/// Reads signing key from IConfiguration, token lifetimes from SystemSettings table (with config fallbacks).
/// Access tokens contain: sub (userId), email, full_name, branch_id, permissions, roles claims.
/// </summary>
public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly AuthDbContext _dbContext;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtService(IConfiguration configuration, AuthDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;

        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key not configured.");
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, IEnumerable<string> permissions)
    {
        var lifetimeMinutes = GetSettingValue("AccessTokenLifetimeMinutes", 15);
        var expiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("full_name", user.FullName),
            new("branch_id", user.BranchId.Value.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims
        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        // Add permission claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // We want to validate expired tokens
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the refresh token lifetime in days based on whether "remember me" is checked.
    /// </summary>
    public int GetRefreshTokenLifetimeDays(bool rememberMe)
    {
        return rememberMe
            ? GetSettingValue("RememberMeRefreshTokenLifetimeDays", 30)
            : GetSettingValue("RefreshTokenLifetimeDays", 7);
    }

    private int GetSettingValue(string key, int defaultValue)
    {
        try
        {
            var setting = _dbContext.SystemSettings
                .AsNoTracking()
                .FirstOrDefault(s => s.Key == key);

            if (setting is not null && int.TryParse(setting.Value, out var value))
                return value;
        }
        catch
        {
            // If DB is not ready yet (startup), use defaults
        }

        return defaultValue;
    }
}
