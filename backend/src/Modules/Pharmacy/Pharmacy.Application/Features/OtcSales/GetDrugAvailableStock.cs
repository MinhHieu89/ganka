using Pharmacy.Application.Interfaces;

namespace Pharmacy.Application.Features.OtcSales;

/// <summary>
/// Query to retrieve the total available stock for a specific drug.
/// Used by OTC sales UI to show inline stock availability warnings.
/// </summary>
public sealed record GetDrugAvailableStockQuery(Guid DrugCatalogItemId);

/// <summary>
/// Wolverine static handler for retrieving available stock for a drug.
/// Returns the sum of CurrentQuantity across all non-expired batches (FEFO query).
/// </summary>
public static class GetDrugAvailableStockHandler
{
    public static async Task<int> Handle(
        GetDrugAvailableStockQuery query,
        IDrugBatchRepository batchRepository,
        CancellationToken ct)
    {
        var batches = await batchRepository.GetAvailableBatchesFEFOAsync(
            query.DrugCatalogItemId, ct);

        return batches.Sum(b => b.CurrentQuantity);
    }
}
