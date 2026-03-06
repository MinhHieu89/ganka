using Pharmacy.Domain.Entities;

namespace Pharmacy.Domain.Services;

/// <summary>
/// Represents the allocation of a specific quantity from a single batch during FEFO selection.
/// </summary>
/// <param name="BatchId">The ID of the batch being allocated from.</param>
/// <param name="BatchNumber">The batch number for audit and display purposes.</param>
/// <param name="Quantity">The quantity allocated from this batch.</param>
/// <param name="ExpiryDate">The expiry date of this batch.</param>
public sealed record BatchAllocation(Guid BatchId, string BatchNumber, int Quantity, DateOnly ExpiryDate);

/// <summary>
/// Domain service for FEFO (First Expiry, First Out) batch allocation.
/// Selects the earliest-expiring available batches first to minimize stock wastage.
/// Used by both prescription dispensing and OTC sale operations.
/// </summary>
public static class FEFOAllocator
{
    /// <summary>
    /// Allocates the required quantity from the available batches using FEFO ordering.
    /// </summary>
    /// <param name="availableBatches">All batches for the drug. May include expired or zero-quantity batches.</param>
    /// <param name="requiredQuantity">The total quantity needed.</param>
    /// <returns>
    /// A list of batch allocations in FEFO order if sufficient stock is available.
    /// Returns an empty list if total non-expired stock is insufficient (all-or-nothing semantics).
    /// </returns>
    public static List<BatchAllocation> Allocate(
        IReadOnlyList<DrugBatch> availableBatches,
        int requiredQuantity)
    {
        // Filter: exclude expired batches and zero-quantity batches
        // Order: by ExpiryDate ascending (earliest expiry first = FEFO)
        var eligible = availableBatches
            .Where(b => b.CurrentQuantity > 0 && !b.IsExpired)
            .OrderBy(b => b.ExpiryDate)
            .ToList();

        var allocations = new List<BatchAllocation>();
        var remaining = requiredQuantity;

        foreach (var batch in eligible)
        {
            if (remaining <= 0) break;

            var take = Math.Min(remaining, batch.CurrentQuantity);
            allocations.Add(new BatchAllocation(batch.Id, batch.BatchNumber, take, batch.ExpiryDate));
            remaining -= take;
        }

        // All-or-nothing: if we could not satisfy the full quantity, return empty
        if (remaining > 0)
            return [];

        return allocations;
    }
}
