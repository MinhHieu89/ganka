using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Query to retrieve paginated list of glasses orders with optional status filter.
/// </summary>
public sealed record GetGlassesOrdersQuery(int? StatusFilter, int Page = 1, int PageSize = 20);

/// <summary>
/// Paginated result for glasses orders list.
/// </summary>
public sealed record PagedGlassesOrdersResult(List<GlassesOrderSummaryDto> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// Wolverine static handler for <see cref="GetGlassesOrdersQuery"/>.
/// Returns a paginated list of glasses order summaries, optionally filtered by status.
/// TotalCount is fetched independently to support pagination metadata.
/// </summary>
public static class GetGlassesOrdersHandler
{
    public static async Task<Result<PagedGlassesOrdersResult>> Handle(
        GetGlassesOrdersQuery query,
        IGlassesOrderRepository orderRepository,
        CancellationToken ct)
    {
        var orders = await orderRepository.GetAllAsync(query.StatusFilter, query.Page, query.PageSize, ct);
        var totalCount = await orderRepository.GetTotalCountAsync(query.StatusFilter, ct);

        var summaries = orders.Select(o => new GlassesOrderSummaryDto(
            Id: o.Id,
            PatientName: o.PatientName,
            Status: (int)o.Status,
            ProcessingType: (int)o.ProcessingType,
            TotalPrice: o.TotalPrice,
            IsPaymentConfirmed: o.IsPaymentConfirmed,
            EstimatedDeliveryDate: o.EstimatedDeliveryDate,
            IsOverdue: o.IsOverdue,
            CreatedAt: o.CreatedAt)).ToList();

        return Result.Success(new PagedGlassesOrdersResult(
            Items: summaries,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize));
    }
}
