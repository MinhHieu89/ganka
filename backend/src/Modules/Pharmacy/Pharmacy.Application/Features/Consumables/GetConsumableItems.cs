using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Application.Features.Consumables;

/// <summary>
/// Query to retrieve all active consumable items with current stock information.
/// For SimpleStock items, stock is read directly from ConsumableItem.CurrentStock.
/// For ExpiryTracked items, stock is computed by summing ConsumableBatch.CurrentQuantity across all batches.
/// IsLowStock flag is set when computed stock is below MinStockLevel (and MinStockLevel > 0).
/// </summary>
public sealed record GetConsumableItemsQuery();

/// <summary>
/// Wolverine static handler for retrieving consumable items with stock info.
/// Returns all active items with computed stock levels and low-stock flags.
/// Follows GetDrugInventoryHandler pattern.
/// </summary>
public static class GetConsumableItemsHandler
{
    public static async Task<Result<List<ConsumableItemDto>>> Handle(
        GetConsumableItemsQuery query,
        IConsumableRepository repository,
        CancellationToken ct)
    {
        var items = await repository.GetAllActiveAsync(ct);

        var dtos = new List<ConsumableItemDto>(items.Count);

        foreach (var item in items)
        {
            int currentStock;

            if (item.TrackingMode == ConsumableTrackingMode.SimpleStock)
            {
                currentStock = item.CurrentStock;
            }
            else
            {
                // ExpiryTracked: compute stock from batch sum
                var batches = await repository.GetBatchesAsync(item.Id, ct);
                currentStock = batches.Sum(b => b.CurrentQuantity);
            }

            bool isLowStock = item.MinStockLevel > 0 && currentStock < item.MinStockLevel;

            dtos.Add(new ConsumableItemDto(
                Id: item.Id,
                Name: item.Name,
                NameVi: item.NameVi,
                Unit: item.Unit,
                TrackingMode: (int)item.TrackingMode,
                CurrentStock: currentStock,
                MinStockLevel: item.MinStockLevel,
                IsActive: item.IsActive,
                IsLowStock: isLowStock));
        }

        return dtos;
    }
}
