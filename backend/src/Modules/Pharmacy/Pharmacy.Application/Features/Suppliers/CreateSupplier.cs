using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Application.Features.Suppliers;

/// <summary>
/// Command to create a new supplier.
/// </summary>
public sealed record CreateSupplierCommand(
    string Name,
    string? ContactInfo,
    string? Phone,
    string? Email);

/// <summary>
/// Validator for <see cref="CreateSupplierCommand"/>.
/// </summary>
public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(200).WithMessage("Supplier name must not exceed 200 characters.");
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(x => x.Phone is not null);
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.")
            .When(x => x.Email is not null);
    }
}

/// <summary>
/// Wolverine static handler for creating a new supplier.
/// Validates input, creates entity via factory method, persists via repository.
/// </summary>
public static class CreateSupplierHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateSupplierCommand command,
        ISupplierRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateSupplierCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<Guid>(Error.ValidationWithDetails(errors));
        }

        var supplier = Supplier.Create(
            command.Name,
            command.ContactInfo,
            command.Phone,
            command.Email,
            new BranchId(currentUser.BranchId));

        repository.Add(supplier);
        await unitOfWork.SaveChangesAsync(ct);

        return supplier.Id;
    }
}
