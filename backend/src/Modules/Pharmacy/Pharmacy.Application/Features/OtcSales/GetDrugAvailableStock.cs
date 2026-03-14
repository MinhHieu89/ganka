using Pharmacy.Application.Interfaces;

namespace Pharmacy.Application.Features.OtcSales;

/// <summary>
/// Query to retrieve the total available stock for a specific drug.
/// Used by OTC sales UI to show inline stock availability warnings.
/// </summary>
public sealed record GetDrugAvailableStockQuery(Guid DrugCatalogItemId);

/// <summary>
/// Wolverine static handler for retrieving available stock for a drug.
/// Uses GetTotalStockAsync for efficient server-side aggregation instead of
/// loading all batches into memory.
/// </summary>
public static class GetDrugAvailableStockHandler
{
    public static async Task<int> Handle(
        GetDrugAvailableStockQuery query,
        IDrugBatchRepository batchRepository,
        CancellationToken ct)
    {
        return await batchRepository.GetTotalStockAsync(
            query.DrugCatalogItemId, ct);
    }
}
