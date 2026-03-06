using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;

namespace Pharmacy.Application.Features.OtcSales;

/// <summary>
/// Query to get a paginated list of OTC sales.
/// Supports browsing walk-in sale history.
/// </summary>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Number of items per page.</param>
public sealed record GetOtcSalesQuery(int Page = 1, int PageSize = 20);

/// <summary>
/// Paginated response wrapper for OTC sales.
/// </summary>
public sealed record OtcSalesPagedResult(List<OtcSaleDto> Items, int TotalCount);

/// <summary>
/// Wolverine static handler for retrieving paginated OTC sales.
/// Delegates to IOtcSaleRepository.GetAllAsync.
/// </summary>
public static class GetOtcSalesHandler
{
    public static async Task<Result<OtcSalesPagedResult>> Handle(
        GetOtcSalesQuery query,
        IOtcSaleRepository otcSaleRepository,
        CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, totalCount) = await otcSaleRepository.GetAllAsync(page, pageSize, ct);

        return Result<OtcSalesPagedResult>.Success(new OtcSalesPagedResult(items, totalCount));
    }
}
