using FluentValidation;
using Pharmacy.Application.Interfaces;
using Shared.Domain;

namespace Pharmacy.Application.Features.Suppliers;

/// <summary>
/// Command to update an existing supplier.
/// </summary>
public sealed record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? ContactInfo,
    string? Phone,
    string? Email);

/// <summary>
/// Validator for <see cref="UpdateSupplierCommand"/>.
/// </summary>
public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Supplier ID is required.");
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
/// Wolverine static handler for updating an existing supplier.
/// Loads entity by ID, returns NotFound if missing, calls entity Update method.
/// </summary>
public static class UpdateSupplierHandler
{
    public static async Task<Result> Handle(
        UpdateSupplierCommand command,
        ISupplierRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateSupplierCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure(Error.ValidationWithDetails(errors));
        }

        var supplier = await repository.GetByIdAsync(command.Id, ct);
        if (supplier is null)
            return Result.Failure(Error.NotFound("Supplier", command.Id));

        supplier.Update(command.Name, command.ContactInfo, command.Phone, command.Email);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
