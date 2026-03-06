using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;

namespace Pharmacy.Application.Features.Inventory;

/// <summary>
/// Query to retrieve all drugs with their computed inventory data.
/// Returns each drug with aggregated batch stock levels and alert flags.
/// PHR-01: Drug inventory list with stock levels and alert flags.
/// PHR-03: Expiry alert flag for batches expiring within the threshold.
/// PHR-04: Low stock flag when total stock falls below MinStockLevel.
/// </summary>
/// <param name="ExpiryAlertDays">
/// Number of days within which expiring batches trigger the HasExpiryAlert flag.
/// Defaults to 30 days if not specified.
/// </param>
public sealed record GetDrugInventoryQuery(int ExpiryAlertDays = 30);

/// <summary>
/// Wolverine static handler for retrieving drug inventory with computed stock levels.
/// Delegates to IDrugCatalogItemRepository.GetAllWithInventoryAsync which performs
/// the aggregation of batch quantities and alert computation.
/// </summary>
public static class GetDrugInventoryHandler
{
    public static async Task<Result<List<DrugInventoryDto>>> Handle(
        GetDrugInventoryQuery query,
        IDrugCatalogItemRepository drugCatalogItemRepository,
        CancellationToken ct)
    {
        var expiryAlertDays = Math.Max(1, query.ExpiryAlertDays);
        var inventory = await drugCatalogItemRepository.GetAllWithInventoryAsync(expiryAlertDays, ct);
        return Result<List<DrugInventoryDto>>.Success(inventory);
    }
}
