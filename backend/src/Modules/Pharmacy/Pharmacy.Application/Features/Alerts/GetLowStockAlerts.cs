using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;

namespace Pharmacy.Application.Features.Alerts;

/// <summary>
/// Query to retrieve drugs whose total available stock is at or below MinStockLevel.
/// Per PHR-04: alerts for drugs below minimum stock level.
/// Excludes drugs with MinStockLevel = 0 (no threshold set, filtered in repository).
/// </summary>
public sealed record GetLowStockAlertsQuery;

/// <summary>
/// Wolverine static handler for retrieving low stock alerts.
/// Delegates to IDrugBatchRepository.GetLowStockAlertsAsync.
/// </summary>
public static class GetLowStockAlertsHandler
{
    public static async Task<List<LowStockAlertDto>> Handle(
        GetLowStockAlertsQuery query,
        IDrugBatchRepository repository,
        CancellationToken ct)
    {
        return await repository.GetLowStockAlertsAsync(ct);
    }
}
