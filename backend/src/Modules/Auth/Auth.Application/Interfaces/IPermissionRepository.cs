using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

/// <summary>
/// Read-only repository interface for Permission entities.
/// </summary>
public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
