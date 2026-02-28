using Auth.Application.Services;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Domain;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Core authentication service implementation.
/// Handles login (password verification + JWT generation), token refresh with rotation,
/// logout, and user profile operations.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly AuthDbContext _dbContext;
    private readonly JwtService _jwtService;
    private readonly PasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AuthDbContext dbContext,
        JwtService jwtService,
        PasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress = null)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
        {
            _logger.LogWarning("Login attempt for non-existent email: {Email}", request.Email);
            return Result<LoginResponse>.Failure(Error.Unauthorized());
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for deactivated user: {Email}", request.Email);
            return Result<LoginResponse>.Failure(Error.Unauthorized());
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for user: {Email}", request.Email);
            return Result<LoginResponse>.Failure(Error.Unauthorized());
        }

        // Generate tokens
        var permissions = user.GetEffectivePermissions()
            .Select(p => p.ToString())
            .ToList();

        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user, permissions);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        // Store refresh token with family ID
        var familyId = Guid.NewGuid();
        var refreshTokenLifetimeDays = _jwtService.GetRefreshTokenLifetimeDays(request.RememberMe);
        var refreshToken = new RefreshToken(
            refreshTokenValue,
            user.Id,
            DateTime.UtcNow.AddDays(refreshTokenLifetimeDays),
            familyId);

        _dbContext.RefreshTokens.Add(refreshToken);

        // Record login event on the aggregate
        user.RecordLogin(ipAddress);

        await _dbContext.SaveChangesAsync();

        var userDto = MapToDto(user, permissions);

        return new LoginResponse(accessToken, refreshTokenValue, expiresAt, userDto);
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var existingToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (existingToken is null)
            return Result<LoginResponse>.Failure(Error.Unauthorized());

        // If token is revoked, it's being reused -- revoke entire family (theft detection)
        if (existingToken.IsRevoked)
        {
            _logger.LogWarning(
                "Refresh token reuse detected for family {FamilyId}, user {UserId}. Revoking entire family.",
                existingToken.FamilyId, existingToken.UserId);

            await RevokeTokenFamilyAsync(existingToken.FamilyId);
            return Result<LoginResponse>.Failure(Error.Unauthorized());
        }

        if (existingToken.IsExpired)
            return Result<LoginResponse>.Failure(Error.Unauthorized());

        var user = existingToken.User;
        if (!user.IsActive)
            return Result<LoginResponse>.Failure(Error.Unauthorized());

        // Generate new tokens
        var permissions = user.GetEffectivePermissions()
            .Select(p => p.ToString())
            .ToList();

        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user, permissions);
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

        // Rotate: revoke old token, create new with same family
        existingToken.Revoke(newRefreshTokenValue);

        var newRefreshToken = new RefreshToken(
            newRefreshTokenValue,
            user.Id,
            DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenLifetimeDays(false)),
            existingToken.FamilyId);

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync();

        var userDto = MapToDto(user, permissions);

        return new LoginResponse(accessToken, newRefreshTokenValue, expiresAt, userDto);
    }

    public async Task<Result> LogoutAsync(Guid userId, string refreshToken)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

        if (token is null)
            return Result.Success(); // Idempotent -- already logged out

        token.Revoke();
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Result<UserDto>.Failure(Error.NotFound("User", userId));

        var permissions = user.GetEffectivePermissions()
            .Select(p => p.ToString())
            .ToList();

        return MapToDto(user, permissions);
    }

    public async Task<Result> UpdateLanguagePreferenceAsync(Guid userId, string language)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user is null)
            return Result.Failure(Error.NotFound("User", userId));

        user.SetLanguagePreference(language);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    private async Task RevokeTokenFamilyAsync(Guid familyId)
    {
        var familyTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.FamilyId == familyId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in familyTokens)
        {
            token.Revoke();
        }

        await _dbContext.SaveChangesAsync();
    }

    private static UserDto MapToDto(User user, List<string> permissions)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PreferredLanguage,
            user.IsActive,
            user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            permissions);
    }
}
