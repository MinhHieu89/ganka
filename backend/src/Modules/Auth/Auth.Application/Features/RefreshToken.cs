using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Domain;

namespace Auth.Application.Features;

// --- Command record ---
public sealed record RefreshTokenCommand(string RefreshToken);

// --- Response record ---
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User,
    bool RememberMe);

// --- Handler ---
public sealed class RefreshTokenHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand command)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken);

        if (existingToken is null)
            return Result<RefreshTokenResponse>.Failure(Error.Unauthorized());

        // If token is revoked, it's being reused -- revoke entire family (theft detection)
        if (existingToken.IsRevoked)
        {
            _logger.LogWarning(
                "Refresh token reuse detected for family {FamilyId}, user {UserId}. Revoking entire family.",
                existingToken.FamilyId, existingToken.UserId);

            await _refreshTokenRepository.RevokeAllByFamilyIdAsync(
                existingToken.FamilyId, "Token reuse detected");
            await _unitOfWork.SaveChangesAsync();

            return Result<RefreshTokenResponse>.Failure(Error.Unauthorized());
        }

        if (existingToken.IsExpired)
            return Result<RefreshTokenResponse>.Failure(Error.Unauthorized());

        var user = existingToken.User;
        if (!user.IsActive)
            return Result<RefreshTokenResponse>.Failure(Error.Unauthorized());

        // Generate new tokens
        var permissions = user.GetEffectivePermissions()
            .Select(p => p.ToString())
            .ToList();

        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user, permissions);
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

        // Rotate: revoke old token, create new with same family
        existingToken.Revoke(newRefreshTokenValue);

        var newRefreshToken = new Domain.Entities.RefreshToken(
            newRefreshTokenValue,
            user.Id,
            DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenLifetimeDays(existingToken.RememberMe)),
            existingToken.FamilyId,
            existingToken.RememberMe);

        _refreshTokenRepository.Add(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PreferredLanguage,
            user.IsActive,
            user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            permissions);

        return new RefreshTokenResponse(accessToken, newRefreshTokenValue, expiresAt, userDto, existingToken.RememberMe);
    }
}
