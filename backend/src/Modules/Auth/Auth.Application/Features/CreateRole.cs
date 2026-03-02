using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using FluentValidation;
using Shared.Domain;

namespace Auth.Application.Features;

/// <summary>
/// Validator for <see cref="CreateRoleCommand"/>.
/// Migrated from Validators/CreateRoleCommandValidator.cs.
/// </summary>
public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Role name must not exceed 50 characters.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="CreateRoleCommand"/>.
/// Replaces the admin CreateRole endpoint logic from RoleService.
/// </summary>
public static class CreateRoleHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateRoleCommand command,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateRoleCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<Guid>.Failure(Error.ValidationWithDetails(errors));
        }

        var nameExists = await roleRepository.NameExistsAsync(command.Name, cancellationToken: cancellationToken);
        if (nameExists)
            return Result<Guid>.Failure(Error.Conflict("A role with this name already exists."));

        // Use default branch
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var role = new Role(command.Name, command.Description, isSystem: false, branchId);

        if (command.PermissionIds.Count > 0)
        {
            var permissions = await permissionRepository.GetByIdsAsync(command.PermissionIds, cancellationToken);
            role.UpdatePermissions(permissions);
        }

        roleRepository.Add(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}
