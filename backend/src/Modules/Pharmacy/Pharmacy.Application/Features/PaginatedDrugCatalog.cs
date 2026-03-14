using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;

namespace Pharmacy.Application.Features;

/// <summary>
/// Query for paginated drug catalog listing with optional search.
/// Used by the admin drug catalog management page for server-side pagination.
/// </summary>
public sealed record PaginatedDrugCatalogQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null);

/// <summary>
/// Paginated result for drug catalog items.
/// </summary>
public sealed record PaginatedDrugCatalogResult(
    List<DrugCatalogItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Wolverine static handler for paginated drug catalog listing.
/// Supports server-side pagination and search filtering by name/nameVi.
/// </summary>
public static class PaginatedDrugCatalogHandler
{
    public static async Task<Result<PaginatedDrugCatalogResult>> Handle(
        PaginatedDrugCatalogQuery query,
        IDrugCatalogItemRepository repository,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var (items, totalCount) = await repository.GetPaginatedAsync(page, pageSize, search, ct);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Result.Success(new PaginatedDrugCatalogResult(
            Items: items,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages));
    }
}
