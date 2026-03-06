using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;

namespace Pharmacy.Application.Features.StockImport;

/// <summary>
/// Query to retrieve a paginated list of stock imports.
/// </summary>
public sealed record GetStockImportsQuery(
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// Paginated result for stock imports.
/// </summary>
public sealed record PagedStockImportsResult(
    List<StockImportDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Wolverine static handler for retrieving paginated stock imports.
/// Returns import history for audit and review purposes.
/// </summary>
public static class GetStockImportsHandler
{
    public static async Task<Result<PagedStockImportsResult>> Handle(
        GetStockImportsQuery query,
        IStockImportRepository repository,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, totalCount) = await repository.GetAllAsync(page, pageSize, ct);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Result.Success(new PagedStockImportsResult(
            Items: items,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages));
    }
}
