using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;

namespace Pharmacy.Application.Features.DrugCatalog;

/// <summary>
/// Wolverine static handler for looking up drug catalog prices by IDs.
/// Invokable from the Billing module via IMessageBus:
///   bus.InvokeAsync&lt;List&lt;DrugCatalogPriceDto&gt;&gt;(new GetDrugCatalogPricesQuery(ids), ct)
/// Returns selling prices and Vietnamese names for billing line item creation.
/// </summary>
public static class GetDrugCatalogPricesHandler
{
    public static async Task<List<DrugCatalogPriceDto>> Handle(
        GetDrugCatalogPricesQuery query,
        IDrugCatalogItemRepository repository,
        CancellationToken ct)
    {
        if (query.CatalogItemIds is null || query.CatalogItemIds.Count == 0)
            return [];

        return await repository.GetPricesByIdsAsync(query.CatalogItemIds, ct);
    }
}
