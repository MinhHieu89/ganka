using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Shared.Domain;

namespace Auth.Application.Features;

/// <summary>
/// Wolverine handler for <see cref="UpdateUserCommand"/>.
/// Replaces the admin UpdateUser endpoint logic from UserService.
/// </summary>
public static class UpdateUserHandler
{
    public static async Task<Result> Handle(
        UpdateUserCommand command,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdWithRolesAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("User", command.UserId));

        if (!command.IsActive && user.IsActive)
            user.Deactivate();

        // Update roles
        var currentRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var newRoles = new List<Role>();
        foreach (var roleId in command.RoleIds)
        {
            var role = await roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role is not null)
                newRoles.Add(role);
        }

        foreach (var roleId in currentRoleIds.Where(r => !command.RoleIds.Contains(r)))
            user.RemoveRole(roleId);

        foreach (var role in newRoles.Where(r => !currentRoleIds.Contains(r.Id)))
            user.AssignRole(role);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
