using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;
using Shared.Domain;

namespace Pharmacy.Application.Features.Inventory;

/// <summary>
/// Query to retrieve all batches for a specific drug catalog item.
/// Returns batches ordered by expiry date (FEFO order) for inventory inspection.
/// PHR-01: Drug batch detail view with full batch history including empty/expired batches.
/// </summary>
/// <param name="DrugCatalogItemId">The drug catalog item to retrieve batches for.</param>
public sealed record GetDrugBatchesQuery(Guid DrugCatalogItemId);

/// <summary>
/// Wolverine static handler for retrieving drug batches for a specific drug.
///
/// Returns all batches (active, expired, and empty) so the pharmacist can see full history.
/// Batches are ordered by ExpiryDate ascending (FEFO order) for easy stock management.
///
/// This handler bridges the GET /inventory/{drugId}/batches endpoint (Plan 17) to
/// IDrugBatchRepository.GetBatchesForDrugAsync (Plan 07).
/// </summary>
public static class GetDrugBatchesHandler
{
    public static async Task<Result<List<DrugBatchDto>>> Handle(
        GetDrugBatchesQuery query,
        IDrugBatchRepository drugBatchRepository,
        ISupplierRepository supplierRepository,
        CancellationToken ct)
    {
        var batches = await drugBatchRepository.GetBatchesForDrugAsync(query.DrugCatalogItemId, ct);

        // Batch-load supplier names for all unique supplier IDs
        var supplierIds = batches.Select(b => b.SupplierId).Distinct().ToList();
        var suppliers = await supplierRepository.GetAllActiveAsync(ct);
        var supplierNames = (suppliers ?? []).ToDictionary(s => s.Id, s => s.Name);

        // Map domain entities to DTOs, ordered by expiry date (FEFO order for UI display)
        var dtos = batches
            .OrderBy(b => b.ExpiryDate)
            .Select(b => MapToDto(b, supplierNames))
            .ToList();

        return Result<List<DrugBatchDto>>.Success(dtos);
    }

    private static DrugBatchDto MapToDto(DrugBatch batch, Dictionary<Guid, string> supplierNames) =>
        new(
            Id: batch.Id,
            DrugCatalogItemId: batch.DrugCatalogItemId,
            SupplierId: batch.SupplierId,
            SupplierName: supplierNames.GetValueOrDefault(batch.SupplierId, ""),
            BatchNumber: batch.BatchNumber,
            ExpiryDate: batch.ExpiryDate,
            InitialQuantity: batch.InitialQuantity,
            CurrentQuantity: batch.CurrentQuantity,
            PurchasePrice: batch.PurchasePrice,
            IsExpired: batch.IsExpired,
            IsNearExpiry: batch.IsNearExpiry(30));
}
