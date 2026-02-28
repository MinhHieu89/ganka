using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Shared.Domain;

namespace Auth.Application.Features;

/// <summary>
/// Wolverine handler for <see cref="UpdateRolePermissionsCommand"/>.
/// Replaces the admin UpdateRolePermissions endpoint logic from RoleService.
/// Replaces all permissions on a role with the provided set.
/// </summary>
public static class UpdateRolePermissionsHandler
{
    public static async Task<Result> Handle(
        UpdateRolePermissionsCommand command,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdWithPermissionsAsync(command.RoleId, cancellationToken);
        if (role is null)
            return Result.Failure(Error.NotFound("Role", command.RoleId));

        var permissions = await permissionRepository.GetByIdsAsync(command.PermissionIds, cancellationToken);
        role.UpdatePermissions(permissions);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
