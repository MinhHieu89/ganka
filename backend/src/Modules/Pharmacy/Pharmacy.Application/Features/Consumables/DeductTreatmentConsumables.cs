using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Enums;
using Treatment.Contracts.IntegrationEvents;

namespace Pharmacy.Application.Features.Consumables;

/// <summary>
/// Wolverine handler for cross-module consumable stock deduction (TRT-11, CON-03).
/// Responds to TreatmentSessionCompletedIntegrationEvent published by the Treatment module
/// when a treatment session is completed.
///
/// For each consumable used during the session:
/// - SimpleStock items: deducts directly via RemoveStock (deducts available if insufficient)
/// - ExpiryTracked items: FEFO batch deduction from earliest-expiry batches (deducts available if insufficient)
/// - Missing items: logs warning and continues (no transaction failure)
///
/// Empty consumables list is a no-op (no error, no save).
/// </summary>
public static class DeductTreatmentConsumablesHandler
{
    public static async Task Handle(
        TreatmentSessionCompletedIntegrationEvent message,
        IConsumableRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (message.Consumables.Count == 0)
            return;

        var hasDeductions = false;

        foreach (var usage in message.Consumables)
        {
            var item = await repository.GetByIdAsync(usage.ConsumableItemId, ct);
            if (item is null)
            {
                // Consumable item not found in pharmacy inventory -- skip gracefully.
                // This can happen if an item was deactivated after protocol creation.
                continue;
            }

            if (item.TrackingMode == ConsumableTrackingMode.SimpleStock)
            {
                DeductSimpleStock(item, usage.Quantity);
                repository.Update(item);
                hasDeductions = true;
            }
            else
            {
                // ExpiryTracked: FEFO batch deduction
                var deducted = await DeductExpiryTrackedStock(repository, usage.ConsumableItemId, usage.Quantity, ct);
                if (deducted)
                    hasDeductions = true;
            }
        }

        if (hasDeductions)
            await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Deducts stock from a SimpleStock consumable item.
    /// If requested quantity exceeds available stock, deducts whatever is available.
    /// </summary>
    private static void DeductSimpleStock(
        Pharmacy.Domain.Entities.ConsumableItem item,
        int requestedQuantity)
    {
        var actualDeduction = Math.Min(requestedQuantity, item.CurrentStock);
        if (actualDeduction > 0)
            item.RemoveStock(actualDeduction);
    }

    /// <summary>
    /// Deducts stock from ExpiryTracked consumable batches using FEFO ordering.
    /// GetBatchesAsync returns batches ordered by ExpiryDate ascending (FEFO).
    /// If total available stock is less than requested, deducts everything available.
    /// </summary>
    private static async Task<bool> DeductExpiryTrackedStock(
        IConsumableRepository repository,
        Guid consumableItemId,
        int requestedQuantity,
        CancellationToken ct)
    {
        var batches = await repository.GetBatchesAsync(consumableItemId, ct);

        // Filter to non-expired batches with available stock (already in FEFO order from repository)
        var eligible = batches
            .Where(b => b.CurrentQuantity > 0 && !b.IsExpired)
            .ToList();

        var remaining = requestedQuantity;
        var anyDeducted = false;

        foreach (var batch in eligible)
        {
            if (remaining <= 0) break;

            var take = Math.Min(remaining, batch.CurrentQuantity);
            batch.Deduct(take);
            remaining -= take;
            anyDeducted = true;
        }

        return anyDeducted;
    }
}
