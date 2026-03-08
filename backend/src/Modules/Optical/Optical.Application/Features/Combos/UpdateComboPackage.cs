using FluentValidation;
using Optical.Application.Interfaces;
using Shared.Domain;

namespace Optical.Application.Features.Combos;

/// <summary>
/// Command to update an existing combo package.
/// </summary>
public sealed record UpdateComboPackageCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? FrameId,
    Guid? LensCatalogItemId,
    decimal ComboPrice,
    decimal? OriginalTotalPrice,
    bool IsActive);

/// <summary>
/// Validator for <see cref="UpdateComboPackageCommand"/>.
/// </summary>
public class UpdateComboPackageCommandValidator : AbstractValidator<UpdateComboPackageCommand>
{
    public UpdateComboPackageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Combo package ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Combo package name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ComboPrice)
            .GreaterThan(0).WithMessage("Combo price must be greater than zero.");

        RuleFor(x => x.OriginalTotalPrice)
            .GreaterThan(0).WithMessage("Original total price must be greater than zero when provided.")
            .When(x => x.OriginalTotalPrice.HasValue);
    }
}

/// <summary>
/// Wolverine static handler for updating an existing combo package.
/// Loads entity by ID, returns NotFound if missing, updates via entity methods.
/// Also handles IsActive toggling (Activate/Deactivate).
/// </summary>
public static class UpdateComboPackageHandler
{
    public static async Task<Result> Handle(
        UpdateComboPackageCommand command,
        IComboPackageRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateComboPackageCommand> validator,
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

        var combo = await repository.GetByIdAsync(command.Id, ct);
        if (combo is null)
            return Result.Failure(Error.NotFound("ComboPackage", command.Id));

        combo.Update(
            command.Name,
            command.Description,
            command.FrameId,
            command.LensCatalogItemId,
            command.ComboPrice,
            command.OriginalTotalPrice);

        // Handle IsActive toggling
        if (command.IsActive && !combo.IsActive)
            combo.Activate();
        else if (!command.IsActive && combo.IsActive)
            combo.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
