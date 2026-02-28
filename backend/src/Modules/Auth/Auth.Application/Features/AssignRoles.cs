using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Shared.Domain;

namespace Auth.Application.Features;

/// <summary>
/// Wolverine handler for <see cref="AssignRolesCommand"/>.
/// Replaces the admin AssignRoles endpoint logic from UserService.
/// Clears existing roles and assigns the new set.
/// </summary>
public static class AssignRolesHandler
{
    public static async Task<Result> Handle(
        AssignRolesCommand command,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdWithRolesAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("User", command.UserId));

        var roles = new List<Role>();
        foreach (var roleId in command.RoleIds)
        {
            var role = await roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role is not null)
                roles.Add(role);
        }

        // Clear existing roles
        var existingRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        foreach (var roleId in existingRoleIds)
            user.RemoveRole(roleId);

        // Assign new roles
        foreach (var role in roles)
            user.AssignRole(role);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
