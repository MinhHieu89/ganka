using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;

namespace Pharmacy.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IConsumableRepository"/>.
/// Supports both ExpiryTracked (batch model) and SimpleStock (quantity-only) consumable items.
/// GetAlertsAsync handles both modes:
///   - SimpleStock: CurrentStock &lt; MinStockLevel
///   - ExpiryTracked: Sum of ConsumableBatch.CurrentQuantity &lt; MinStockLevel
/// </summary>
public sealed class ConsumableRepository(PharmacyDbContext context) : IConsumableRepository
{
    public async Task<ConsumableItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.ConsumableItems
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<ConsumableItem>> GetAllActiveAsync(CancellationToken ct)
    {
        return await context.ConsumableItems
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ConsumableBatch>> GetBatchesAsync(Guid consumableItemId, CancellationToken ct)
    {
        return await context.ConsumableBatches
            .AsNoTracking()
            .Where(b => b.ConsumableItemId == consumableItemId)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(ct);
    }

    public async Task<ConsumableBatch?> GetBatchByIdAsync(Guid batchId, CancellationToken ct)
    {
        return await context.ConsumableBatches
            .FirstOrDefaultAsync(b => b.Id == batchId, ct);
    }

    /// <summary>
    /// Returns consumable items that are below their minimum stock level.
    /// For SimpleStock items: compares CurrentStock directly.
    /// For ExpiryTracked items: aggregates batch CurrentQuantity and compares to MinStockLevel.
    /// Only returns items where MinStockLevel > 0 (items without thresholds are never alerted).
    /// </summary>
    public async Task<List<ConsumableItemDto>> GetAlertsAsync(CancellationToken ct)
    {
        // Get all active items with a stock threshold configured
        var items = await context.ConsumableItems
            .AsNoTracking()
            .Where(c => c.IsActive && c.MinStockLevel > 0)
            .ToListAsync(ct);

        // Compute batch stock totals for all ExpiryTracked items in a single query
        var expiryTrackedIds = items
            .Where(c => c.TrackingMode == ConsumableTrackingMode.ExpiryTracked)
            .Select(c => c.Id)
            .ToList();

        Dictionary<Guid, int> batchStockByItem = [];
        if (expiryTrackedIds.Count > 0)
        {
            batchStockByItem = await context.ConsumableBatches
                .AsNoTracking()
                .Where(b => expiryTrackedIds.Contains(b.ConsumableItemId))
                .GroupBy(b => b.ConsumableItemId)
                .Select(g => new { ConsumableItemId = g.Key, TotalStock = g.Sum(b => b.CurrentQuantity) })
                .ToDictionaryAsync(x => x.ConsumableItemId, x => x.TotalStock, ct);
        }

        var alertItems = new List<ConsumableItemDto>();

        foreach (var item in items)
        {
            int effectiveStock;
            if (item.TrackingMode == ConsumableTrackingMode.SimpleStock)
            {
                effectiveStock = item.CurrentStock;
            }
            else
            {
                effectiveStock = batchStockByItem.TryGetValue(item.Id, out var batchStock) ? batchStock : 0;
            }

            if (effectiveStock < item.MinStockLevel)
            {
                alertItems.Add(new ConsumableItemDto(
                    item.Id,
                    item.Name,
                    item.NameVi,
                    item.Unit,
                    (int)item.TrackingMode,
                    effectiveStock,
                    item.MinStockLevel,
                    item.IsActive,
                    IsLowStock: true));
            }
        }

        return alertItems.OrderBy(a => a.Name).ToList();
    }

    public void Add(ConsumableItem item)
    {
        context.ConsumableItems.Add(item);
    }

    public void Update(ConsumableItem item)
    {
        context.ConsumableItems.Update(item);
    }

    public void AddBatch(ConsumableBatch batch)
    {
        context.ConsumableBatches.Add(batch);
    }

    public void AddStockAdjustment(StockAdjustment adjustment)
    {
        context.StockAdjustments.Add(adjustment);
    }
}
