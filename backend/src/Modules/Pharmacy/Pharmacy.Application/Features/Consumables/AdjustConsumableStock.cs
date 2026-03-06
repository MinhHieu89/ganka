using FluentValidation;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Application.Features.Consumables;

/// <summary>
/// Command to manually adjust the stock quantity of a consumable item.
/// CON-02: Supports manual corrections, write-offs, damage recording, and inventory count corrections.
///
/// Behavior differs by TrackingMode:
/// - SimpleStock: ConsumableBatchId must be null; adjusts ConsumableItem.CurrentStock directly.
///   (No StockAdjustment record — the domain constraint requires a non-null batch FK;
///    the IAuditable audit interceptor on ConsumableItem records the change timestamp.)
/// - ExpiryTracked: ConsumableBatchId required; adjusts the specific ConsumableBatch.
///   Creates a StockAdjustment audit record with ConsumableBatchId set.
/// </summary>
/// <param name="ConsumableItemId">The consumable item to adjust.</param>
/// <param name="ConsumableBatchId">Batch to adjust (required for ExpiryTracked; null for SimpleStock).</param>
/// <param name="QuantityChange">Signed quantity change: positive adds, negative removes. Must not be zero.</param>
/// <param name="Reason">The reason category for this adjustment (Correction, WriteOff, Damage, Expired, Other).</param>
/// <param name="Notes">Optional detail notes about the adjustment.</param>
public sealed record AdjustConsumableStockCommand(
    Guid ConsumableItemId,
    Guid? ConsumableBatchId,
    int QuantityChange,
    StockAdjustmentReason Reason,
    string? Notes);

/// <summary>
/// Validates the AdjustConsumableStockCommand at API boundary.
/// </summary>
public class AdjustConsumableStockCommandValidator : AbstractValidator<AdjustConsumableStockCommand>
{
    public AdjustConsumableStockCommandValidator()
    {
        RuleFor(x => x.ConsumableItemId)
            .NotEmpty().WithMessage("Consumable item ID is required.");

        RuleFor(x => x.QuantityChange)
            .NotEqual(0).WithMessage("Quantity change must not be zero. Use a non-zero signed value.");
    }
}

/// <summary>
/// Wolverine static handler for manually adjusting consumable stock.
/// Mirrors AdjustStockHandler pattern for drug batch adjustments.
///
/// Workflow:
/// 1. Validate command
/// 2. Load item by ID
/// 3. Branch by TrackingMode:
///    - SimpleStock: apply QuantityChange to item.CurrentStock (AddStock or RemoveStock)
///    - ExpiryTracked: load batch, apply QuantityChange; create StockAdjustment audit record
/// 4. Save via unit of work
/// </summary>
public static class AdjustConsumableStockHandler
{
    public static async Task<Result<Guid>> Handle(
        AdjustConsumableStockCommand command,
        IValidator<AdjustConsumableStockCommand> validator,
        IConsumableRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
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
            return await AdjustSimpleStock(command, item, repository, unitOfWork, ct);
        }
        else
        {
            return await AdjustExpiryTrackedStock(command, repository, unitOfWork, currentUser.UserId, ct);
        }
    }

    private static async Task<Result<Guid>> AdjustSimpleStock(
        AdjustConsumableStockCommand command,
        ConsumableItem item,
        IConsumableRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        // Apply QuantityChange to ConsumableItem.CurrentStock
        try
        {
            if (command.QuantityChange > 0)
                item.AddStock(command.QuantityChange);
            else
                item.RemoveStock(Math.Abs(command.QuantityChange));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Guid>(Error.Custom(
                "ConsumableStockAdjustment.InsufficientStock",
                ex.Message));
        }

        repository.Update(item);

        await unitOfWork.SaveChangesAsync(ct);
        return item.Id;
    }

    private static async Task<Result<Guid>> AdjustExpiryTrackedStock(
        AdjustConsumableStockCommand command,
        IConsumableRepository repository,
        IUnitOfWork unitOfWork,
        Guid adjustedById,
        CancellationToken ct)
    {
        // ExpiryTracked: must have a batch ID
        if (command.ConsumableBatchId is null)
            return Result.Failure<Guid>(Error.Validation(
                "Batch ID is required for expiry-tracked consumable adjustments."));

        var batch = await repository.GetBatchByIdAsync(command.ConsumableBatchId.Value, ct);
        if (batch is null)
            return Result.Failure<Guid>(Error.NotFound("ConsumableBatch", command.ConsumableBatchId.Value));

        // Apply QuantityChange to batch via domain methods
        try
        {
            if (command.QuantityChange > 0)
                batch.AddStock(command.QuantityChange);
            else
                batch.Deduct(Math.Abs(command.QuantityChange));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Guid>(Error.Custom(
                "ConsumableStockAdjustment.InsufficientStock",
                ex.Message));
        }

        // Create StockAdjustment audit record for ExpiryTracked batch adjustments
        var adjustment = StockAdjustment.Create(
            drugBatchId: null,
            consumableBatchId: command.ConsumableBatchId,
            quantityChange: command.QuantityChange,
            reason: command.Reason,
            notes: command.Notes,
            adjustedById: adjustedById);

        repository.AddStockAdjustment(adjustment);

        await unitOfWork.SaveChangesAsync(ct);
        return adjustment.Id;
    }
}
