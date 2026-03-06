using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;

namespace Pharmacy.Application.Features.Consumables;

/// <summary>
/// Query to retrieve consumable items that are below their minimum stock level.
/// CON-02: Low-stock alerts for consumables warehouse management.
/// Only items where MinStockLevel &gt; 0 are included (zero means no threshold configured).
/// Both SimpleStock and ExpiryTracked modes are covered — the repository handles stock computation.
/// </summary>
public sealed record GetConsumableAlertsQuery();

/// <summary>
/// Wolverine static handler for retrieving consumable low-stock alerts.
/// Delegates to IConsumableRepository.GetAlertsAsync which applies:
/// - Two-step aggregation for ExpiryTracked items (batch sum avoids N+1)
/// - Filters: IsActive = true, MinStockLevel &gt; 0, computed stock &lt; MinStockLevel
/// Follows GetLowStockAlertsHandler pattern.
/// </summary>
public static class GetConsumableAlertsHandler
{
    public static async Task<List<ConsumableItemDto>> Handle(
        GetConsumableAlertsQuery query,
        IConsumableRepository repository,
        CancellationToken ct)
    {
        return await repository.GetAlertsAsync(ct);
    }
}
