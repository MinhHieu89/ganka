using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Optical.Domain.Entities;
using Shared.Domain;

namespace Optical.Application.Features.Lenses;

/// <summary>
/// Query to retrieve the full lens catalog with stock entries.
/// </summary>
public sealed record GetLensCatalogQuery(bool IncludeInactive = false);

/// <summary>
/// Wolverine static handler for fetching all lens catalog items.
/// Maps each LensCatalogItem and its LensStockEntry children to DTOs.
/// Supports filtering inactive items via <see cref="GetLensCatalogQuery.IncludeInactive"/>.
/// </summary>
public static class GetLensCatalogHandler
{
    public static async Task<List<LensCatalogItemDto>> Handle(
        GetLensCatalogQuery query,
        ILensCatalogRepository repository,
        CancellationToken ct)
    {
        var items = await repository.GetAllAsync(query.IncludeInactive, ct);

        return items.Select(MapToDto).ToList();
    }

    private static LensCatalogItemDto MapToDto(LensCatalogItem item)
    {
        var stockEntries = item.StockEntries
            .Select(e => new LensStockEntryDto(
                Id: e.Id,
                LensCatalogItemId: e.LensCatalogItemId,
                Sph: e.Sph,
                Cyl: e.Cyl,
                Add: e.Add,
                Quantity: e.Quantity,
                MinStockLevel: e.MinStockLevel))
            .ToList();

        return new LensCatalogItemDto(
            Id: item.Id,
            Brand: item.Brand,
            Name: item.Name,
            LensType: item.LensType,
            Material: (int)item.Material,
            AvailableCoatings: (int)item.AvailableCoatings,
            SellingPrice: item.SellingPrice,
            CostPrice: item.CostPrice,
            IsActive: item.IsActive,
            PreferredSupplierId: item.PreferredSupplierId,
            SupplierName: null,   // populated by infrastructure when joining supplier data
            StockEntries: stockEntries,
            CreatedAt: item.CreatedAt);
    }
}
