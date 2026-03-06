using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Application.Features.Consumables;

/// <summary>
/// Command to update an existing consumable item's metadata.
/// TrackingMode is int-serialized ConsumableTrackingMode: 0 = ExpiryTracked, 1 = SimpleStock.
/// </summary>
public sealed record UpdateConsumableItemCommand(
    Guid Id,
    string Name,
    string NameVi,
    string Unit,
    int TrackingMode,
    int MinStockLevel);

/// <summary>
/// Validator for <see cref="UpdateConsumableItemCommand"/>.
/// </summary>
public class UpdateConsumableItemCommandValidator : AbstractValidator<UpdateConsumableItemCommand>
{
    public UpdateConsumableItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Consumable item ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Consumable item name is required.")
            .MaximumLength(200).WithMessage("Consumable item name must not exceed 200 characters.");

        RuleFor(x => x.NameVi)
            .NotEmpty().WithMessage("Vietnamese name is required.")
            .MaximumLength(200).WithMessage("Vietnamese name must not exceed 200 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit of measure is required.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

        RuleFor(x => x.TrackingMode)
            .Must(v => Enum.IsDefined(typeof(ConsumableTrackingMode), v))
            .WithMessage("Invalid tracking mode.");

        RuleFor(x => x.MinStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level cannot be negative.");
    }
}

/// <summary>
/// Wolverine static handler for updating an existing consumable item.
/// Loads entity by ID, returns NotFound if missing, calls entity Update method.
/// Follows UpdateSupplierHandler pattern.
/// </summary>
public static class UpdateConsumableItemHandler
{
    public static async Task<Result> Handle(
        UpdateConsumableItemCommand command,
        IConsumableRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateConsumableItemCommand> validator,
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

        var item = await repository.GetByIdAsync(command.Id, ct);
        if (item is null)
            return Result.Failure(Error.NotFound("ConsumableItem", command.Id));

        item.Update(
            command.Name,
            command.NameVi,
            command.Unit,
            (ConsumableTrackingMode)command.TrackingMode,
            command.MinStockLevel);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
