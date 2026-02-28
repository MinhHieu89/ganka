using Auth.Application.Services;
using Auth.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Permission query service implementation.
/// Returns all permissions grouped by module for the admin permission matrix UI.
/// </summary>
public sealed class PermissionService : IPermissionService
{
    private readonly AuthDbContext _dbContext;

    public PermissionService(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<PermissionGroupDto>>> GetPermissionsGroupedByModuleAsync()
    {
        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync();

        var grouped = permissions
            .GroupBy(p => p.Module.ToString())
            .Select(g => new PermissionGroupDto(
                g.Key,
                g.Select(p => new PermissionDto(
                    p.Id,
                    p.Module.ToString(),
                    p.Action.ToString(),
                    p.Description)).ToList()))
            .ToList();

        return grouped;
    }
}
