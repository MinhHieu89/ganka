using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPermissionRepository"/>.
/// Read-only queries for permission data.
/// </summary>
public sealed class PermissionRepository : IPermissionRepository
{
    private readonly AuthDbContext _dbContext;

    public PermissionRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _dbContext.Permissions
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }
}
