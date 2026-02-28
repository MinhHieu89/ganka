using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Shared.Domain;

namespace Auth.Application.Features;

/// <summary>
/// Query to retrieve all permissions grouped by module.
/// </summary>
public sealed record GetPermissionsQuery;

/// <summary>
/// Permissions grouped by module for the admin UI.
/// </summary>
public sealed record PermissionGroupDto(
    string Module,
    List<PermissionDto> Permissions);

/// <summary>
/// Wolverine handler for <see cref="GetPermissionsQuery"/>.
/// Replaces the admin GetPermissions endpoint logic from PermissionService.
/// </summary>
public static class GetPermissionsHandler
{
    public static async Task<List<PermissionGroupDto>> Handle(
        GetPermissionsQuery query,
        IPermissionRepository permissionRepository,
        CancellationToken cancellationToken)
    {
        var permissions = await permissionRepository.GetAllAsync(cancellationToken);

        return permissions
            .GroupBy(p => p.Module.ToString())
            .Select(g => new PermissionGroupDto(
                g.Key,
                g.Select(p => new PermissionDto(
                    p.Id,
                    p.Module.ToString(),
                    p.Action.ToString(),
                    p.Description)).ToList()))
            .ToList();
    }
}
