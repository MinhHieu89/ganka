using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Shared.Domain;

namespace Auth.Application.Features;

/// <summary>
/// Query to retrieve all roles with their permissions.
/// </summary>
public sealed record GetRolesQuery;

/// <summary>
/// Wolverine handler for <see cref="GetRolesQuery"/>.
/// Replaces the admin GetRoles endpoint logic from RoleService.
/// </summary>
public static class GetRolesHandler
{
    public static async Task<List<RoleDto>> Handle(
        GetRolesQuery query,
        IRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllWithPermissionsAsync(cancellationToken);

        return roles.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsSystem,
            r.RolePermissions.Select(rp => new PermissionDto(
                rp.Permission.Id,
                rp.Permission.Module.ToString(),
                rp.Permission.Action.ToString(),
                rp.Permission.Description)).ToList())).ToList();
    }
}
