using Auth.Application.Services;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Role management service implementation.
/// </summary>
public sealed class RoleService : IRoleService
{
    private readonly AuthDbContext _dbContext;

    public RoleService(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<RoleDto>>> GetRolesAsync()
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsSystem,
                r.RolePermissions.Select(rp => new PermissionDto(
                    rp.Permission.Id,
                    rp.Permission.Module.ToString(),
                    rp.Permission.Action.ToString(),
                    rp.Permission.Description)).ToList()))
            .ToListAsync();

        return roles;
    }

    public async Task<Result<Guid>> CreateRoleAsync(CreateRoleCommand command)
    {
        var nameExists = await _dbContext.Roles.AnyAsync(r => r.Name == command.Name);
        if (nameExists)
            return Result<Guid>.Failure(Error.Conflict("A role with this name already exists."));

        // Use default branch
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var role = new Role(command.Name, command.Description, isSystem: false, branchId);

        if (command.PermissionIds.Count > 0)
        {
            var permissions = await _dbContext.Permissions
                .Where(p => command.PermissionIds.Contains(p.Id))
                .ToListAsync();

            role.UpdatePermissions(permissions);
        }

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        return role.Id;
    }

    public async Task<Result> UpdateRolePermissionsAsync(UpdateRolePermissionsCommand command)
    {
        var role = await _dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == command.RoleId);

        if (role is null)
            return Result.Failure(Error.NotFound("Role", command.RoleId));

        var permissions = await _dbContext.Permissions
            .Where(p => command.PermissionIds.Contains(p.Id))
            .ToListAsync();

        role.UpdatePermissions(permissions);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }
}
