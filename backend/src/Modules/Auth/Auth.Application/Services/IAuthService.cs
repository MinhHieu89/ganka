using Auth.Contracts.Dtos;
using Shared.Domain;

namespace Auth.Application.Services;

/// <summary>
/// Core authentication service handling login, token refresh, logout, and user profile operations.
/// </summary>
public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress = null);
    Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result> LogoutAsync(Guid userId, string refreshToken);
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId);
    Task<Result> UpdateLanguagePreferenceAsync(Guid userId, string language);
}
