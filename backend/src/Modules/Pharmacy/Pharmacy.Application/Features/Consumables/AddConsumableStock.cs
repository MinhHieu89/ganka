using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Application.Features.Consumables;

/// <summary>
/// Command to add stock to a consumable item.
/// Behavior differs by TrackingMode:
/// - SimpleStock: increments ConsumableItem.CurrentStock directly.
/// - ExpiryTracked: creates a new ConsumableBatch with the supplied batch details.
///
/// For ExpiryTracked items, BatchNumber and ExpiryDate are required.
/// </summary>
/// <param name="ConsumableItemId">The consumable item to add stock to.</param>
/// <param name="Quantity">Quantity to add (must be positive).</param>
/// <param name="BatchNumber">Batch number (required for ExpiryTracked items).</param>
/// <param name="ExpiryDate">Expiry date (required for ExpiryTracked items).</param>
/// <param name="Notes">Optional notes about the stock addition.</param>
public sealed record AddConsumableStockCommand(
    Guid ConsumableItemId,
    int Quantity,
    string? BatchNumber,
    DateOnly? ExpiryDate,
    string? Notes);

/// <summary>
/// Validates the AddConsumableStockCommand at API boundary.
/// Item-level tracking mode checks (BatchNumber/ExpiryDate required for ExpiryTracked)
/// are enforced by the handler after loading the item.
/// </summary>
public class AddConsumableStockCommandValidator : AbstractValidator<AddConsumableStockCommand>
{
    public AddConsumableStockCommandValidator()
    {
        RuleFor(x => x.ConsumableItemId)
            .NotEmpty().WithMessage("Consumable item ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}

/// <summary>
/// Wolverine static handler for adding stock to a consumable item.
///
/// Workflow:
/// 1. Validate command
/// 2. Load item by ID
/// 3. Branch by TrackingMode:
///    - SimpleStock: call item.AddStock(qty)
///    - ExpiryTracked: create ConsumableBatch, add via repository
/// 4. Save via unit of work
/// </summary>
public static class AddConsumableStockHandler
{
    public static async Task<Result<Guid>> Handle(
        AddConsumableStockCommand command,
        IValidator<AddConsumableStockCommand> validator,
        IConsumableRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        // Step 1: Validate
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<Guid>(Error.ValidationWithDetails(errors));
        }

        // Step 2: Load item
        var item = await repository.GetByIdAsync(command.ConsumableItemId, ct);
        if (item is null)
            return Result.Failure<Guid>(Error.NotFound("ConsumableItem", command.ConsumableItemId));

        // Step 3: Branch by tracking mode
        if (item.TrackingMode == ConsumableTrackingMode.SimpleStock)
        {
            // SimpleStock: increment CurrentStock directly
            item.AddStock(command.Quantity);
            repository.Update(item);
            await unitOfWork.SaveChangesAsync(ct);
            return item.Id;
        }
        else
        {
            // ExpiryTracked: require BatchNumber and ExpiryDate
            if (string.IsNullOrWhiteSpace(command.BatchNumber))
                return Result.Failure<Guid>(Error.Validation(
                    "Batch number is required for expiry-tracked consumables."));

            if (command.ExpiryDate is null)
                return Result.Failure<Guid>(Error.Validation(
                    "Expiry date is required for expiry-tracked consumables."));

            var batch = ConsumableBatch.Create(
                consumableItemId: item.Id,
                batchNumber: command.BatchNumber,
                expiryDate: command.ExpiryDate.Value,
                quantity: command.Quantity);

            repository.AddBatch(batch);
            await unitOfWork.SaveChangesAsync(ct);
            return batch.Id;
        }
    }
}
