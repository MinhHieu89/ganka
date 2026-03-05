using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;

namespace Pharmacy.Application.Features;

/// <summary>
/// Wolverine static handler for searching the drug catalog.
/// Invokable from the Clinical module via IMessageBus:
///   bus.InvokeAsync&lt;List&lt;DrugCatalogItemDto&gt;&gt;(new SearchDrugCatalogQuery(term), ct)
/// Returns matching active drugs from the pharmacy catalog.
/// </summary>
public static class SearchDrugCatalogHandler
{
    public static async Task<List<DrugCatalogItemDto>> Handle(
        SearchDrugCatalogQuery query,
        IDrugCatalogItemRepository repository,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.SearchTerm))
            return [];

        return await repository.SearchAsync(query.SearchTerm.Trim(), ct);
    }
}
