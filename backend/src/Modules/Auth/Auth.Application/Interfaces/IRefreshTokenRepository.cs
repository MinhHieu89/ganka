using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

/// <summary>
/// Repository interface for RefreshToken entities.
/// Supports token lookup, family-based revocation, and user-based revocation.
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetActiveByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
    Task RevokeAllByFamilyIdAsync(Guid familyId, string reason, CancellationToken cancellationToken = default);
    Task RevokeAllByUserIdAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
}
