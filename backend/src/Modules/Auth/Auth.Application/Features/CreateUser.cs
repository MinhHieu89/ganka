using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using FluentValidation;
using Shared.Domain;

namespace Auth.Application.Features;

/// <summary>
/// Validator for <see cref="CreateUserCommand"/>.
/// Migrated from Validators/CreateUserCommandValidator.cs.
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.RoleIds)
            .NotEmpty().WithMessage("At least one role must be assigned.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="CreateUserCommand"/>.
/// Replaces the admin CreateUser endpoint logic from UserService.
/// </summary>
public static class CreateUserHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateUserCommand command,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IValidator<CreateUserCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<Guid>.Failure(Error.Validation(errors));
        }

        var emailExists = await userRepository.EmailExistsAsync(command.Email, cancellationToken);
        if (emailExists)
            return Result<Guid>.Failure(Error.Conflict("A user with this email already exists."));

        // Validate role IDs
        var roles = new List<Role>();
        foreach (var roleId in command.RoleIds)
        {
            var role = await roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role is null)
                return Result<Guid>.Failure(Error.Validation("One or more role IDs are invalid."));
            roles.Add(role);
        }

        // Use default branch for now
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var passwordHash = passwordHasher.HashPassword(command.Password);
        var user = User.Create(command.Email, command.FullName, passwordHash, branchId);

        foreach (var role in roles)
            user.AssignRole(role);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
