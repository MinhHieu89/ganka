using Shared.Domain;

namespace Auth.Domain.Entities;

/// <summary>
/// Represents a refresh token for JWT token rotation.
/// Uses FamilyId for token family-based theft detection:
/// if a revoked token is reused, the entire family is revoked.
/// </summary>
public class RefreshToken : Entity
{
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public Guid FamilyId { get; private set; }
    public bool RememberMe { get; private set; }

    public User User { get; private set; } = default!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public RefreshToken(string token, Guid userId, DateTime expiresAt, Guid familyId, bool rememberMe = false)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        FamilyId = familyId;
        RememberMe = rememberMe;
    }

    /// <summary>
    /// Revokes this token, optionally recording the replacement token.
    /// </summary>
    public void Revoke(string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
