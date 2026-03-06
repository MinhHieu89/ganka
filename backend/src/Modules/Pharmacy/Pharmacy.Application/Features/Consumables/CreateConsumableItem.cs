using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Application.Features.Consumables;

/// <summary>
/// Command to create a new consumable item in the clinic's consumables warehouse.
/// TrackingMode is int-serialized ConsumableTrackingMode: 0 = ExpiryTracked, 1 = SimpleStock.
/// </summary>
public sealed record CreateConsumableItemCommand(
    string Name,
    string NameVi,
    string Unit,
    int TrackingMode,
    int MinStockLevel);

/// <summary>
/// Validator for <see cref="CreateConsumableItemCommand"/>.
/// </summary>
public class CreateConsumableItemCommandValidator : AbstractValidator<CreateConsumableItemCommand>
{
    public CreateConsumableItemCommandValidator()
    {
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
/// Wolverine static handler for creating a new consumable item.
/// Validates input, creates entity via factory method, persists via repository.
/// Follows CreateSupplierHandler pattern.
/// </summary>
public static class CreateConsumableItemHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateConsumableItemCommand command,
        IConsumableRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateConsumableItemCommand> validator,
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

        var item = ConsumableItem.Create(
            command.Name,
            command.NameVi,
            command.Unit,
            (ConsumableTrackingMode)command.TrackingMode,
            command.MinStockLevel,
            new BranchId(currentUser.BranchId));

        repository.Add(item);
        await unitOfWork.SaveChangesAsync(ct);

        return item.Id;
    }
}
