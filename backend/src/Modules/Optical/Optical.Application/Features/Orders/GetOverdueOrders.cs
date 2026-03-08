using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Query to retrieve glasses orders that are past their estimated delivery date and not yet delivered.
/// </summary>
public sealed record GetOverdueOrdersQuery();

/// <summary>
/// Wolverine static handler for <see cref="GetOverdueOrdersQuery"/>.
/// Delegates to GlassesOrderRepository.GetOverdueOrdersAsync which queries the database
/// for orders where EstimatedDeliveryDate &lt; UtcNow and Status != Delivered.
/// Returns a list of GlassesOrderSummaryDto ordered by estimated delivery date ascending
/// (oldest overdue first) for alert dashboard display.
/// </summary>
public static class GetOverdueOrdersHandler
{
    public static async Task<Result<List<GlassesOrderSummaryDto>>> Handle(
        GetOverdueOrdersQuery query,
        IGlassesOrderRepository orderRepository,
        CancellationToken ct)
    {
        var overdueOrders = await orderRepository.GetOverdueOrdersAsync(ct);

        var summaries = overdueOrders.Select(o => new GlassesOrderSummaryDto(
            Id: o.Id,
            PatientName: o.PatientName,
            Status: (int)o.Status,
            ProcessingType: (int)o.ProcessingType,
            TotalPrice: o.TotalPrice,
            IsPaymentConfirmed: o.IsPaymentConfirmed,
            EstimatedDeliveryDate: o.EstimatedDeliveryDate,
            IsOverdue: o.IsOverdue,
            CreatedAt: o.CreatedAt)).ToList();

        return Result.Success(summaries);
    }
}
