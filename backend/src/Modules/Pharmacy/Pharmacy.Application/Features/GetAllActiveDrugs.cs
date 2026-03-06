using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;

namespace Pharmacy.Application.Features;

/// <summary>
/// Query to retrieve all active drugs in the catalog.
/// Used by admin endpoints for drug catalog management.
/// </summary>
public sealed record GetAllActiveDrugsQuery;

/// <summary>
/// Wolverine static handler for retrieving all active drug catalog items.
/// Returns full list of active drugs as DTOs for admin management.
/// </summary>
public static class GetAllActiveDrugsHandler
{
    public static async Task<List<DrugCatalogItemDto>> Handle(
        GetAllActiveDrugsQuery query,
        IDrugCatalogItemRepository repository,
        CancellationToken ct)
    {
        var items = await repository.GetAllActiveAsync(ct);
        return items.Select(d => new DrugCatalogItemDto(
            d.Id,
            d.Name,
            d.NameVi,
            d.GenericName,
            (int)d.Form,
            d.Strength,
            (int)d.Route,
            d.Unit,
            d.DefaultDosageTemplate,
            d.IsActive,
            d.SellingPrice,
            d.MinStockLevel)).ToList();
    }
}
