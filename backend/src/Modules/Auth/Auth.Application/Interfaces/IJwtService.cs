using Auth.Domain.Entities;
using System.Security.Claims;

namespace Auth.Application.Interfaces;

/// <summary>
/// JWT token generation and validation service.
/// </summary>
public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    int GetRefreshTokenLifetimeDays(bool rememberMe);
}
