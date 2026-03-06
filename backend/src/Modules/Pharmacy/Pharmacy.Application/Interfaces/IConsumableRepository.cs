using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

/// <summary>
/// Repository interface for ConsumableItem and ConsumableBatch persistence operations.
/// Supports both ExpiryTracked (batch model) and SimpleStock (quantity-only) consumables.
/// </summary>
public interface IConsumableRepository
{
    /// <summary>
    /// Gets a consumable item by ID (returns domain entity for mutation).
    /// </summary>
    Task<ConsumableItem?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all active consumable items.
    /// </summary>
    Task<List<ConsumableItem>> GetAllActiveAsync(CancellationToken ct);

    /// <summary>
    /// Gets all batches for a specific consumable item (ExpiryTracked mode only).
    /// </summary>
    Task<List<ConsumableBatch>> GetBatchesAsync(Guid consumableItemId, CancellationToken ct);

    /// <summary>
    /// Gets a consumable batch by ID (returns domain entity for mutation).
    /// </summary>
    Task<ConsumableBatch?> GetBatchByIdAsync(Guid batchId, CancellationToken ct);

    /// <summary>
    /// Gets consumable items where IsLowStock is true.
    /// Used for low stock alert notifications.
    /// </summary>
    Task<List<ConsumableItemDto>> GetAlertsAsync(CancellationToken ct);

    /// <summary>
    /// Adds a new consumable item to the change tracker.
    /// </summary>
    void Add(ConsumableItem item);

    /// <summary>
    /// Marks a consumable item as modified in the change tracker.
    /// </summary>
    void Update(ConsumableItem item);

    /// <summary>
    /// Adds a new consumable batch to the change tracker.
    /// </summary>
    void AddBatch(ConsumableBatch batch);

    /// <summary>
    /// Adds a new StockAdjustment to the change tracker.
    /// </summary>
    void AddStockAdjustment(StockAdjustment adjustment);
}
