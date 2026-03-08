using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Query to retrieve a single glasses order by ID with full item details.
/// </summary>
public sealed record GetGlassesOrderByIdQuery(Guid Id);

/// <summary>
/// Wolverine static handler for <see cref="GetGlassesOrderByIdQuery"/>.
/// Returns a full GlassesOrderDto including all line items and computed properties.
/// Returns NotFound if the order does not exist.
/// </summary>
public static class GetGlassesOrderByIdHandler
{
    public static async Task<Result<GlassesOrderDto>> Handle(
        GetGlassesOrderByIdQuery query,
        IGlassesOrderRepository orderRepository,
        CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(query.Id, ct);
        if (order is null)
            return Result.Failure<GlassesOrderDto>(Error.NotFound("GlassesOrder", query.Id));

        var items = order.Items.Select(i => new GlassesOrderItemDto(
            Id: i.Id,
            FrameId: i.FrameId,
            FrameName: null,    // Denormalized name not stored; use Description for display
            LensCatalogItemId: i.LensCatalogItemId,
            LensName: null,     // Denormalized name not stored; use Description for display
            Description: i.ItemDescription,
            UnitPrice: i.UnitPrice,
            Quantity: i.Quantity,
            LineTotal: i.LineTotal)).ToList();

        var dto = new GlassesOrderDto(
            Id: order.Id,
            PatientId: order.PatientId,
            PatientName: order.PatientName,
            VisitId: order.VisitId,
            OpticalPrescriptionId: order.OpticalPrescriptionId,
            Status: (int)order.Status,
            ProcessingType: (int)order.ProcessingType,
            IsPaymentConfirmed: order.IsPaymentConfirmed,
            EstimatedDeliveryDate: order.EstimatedDeliveryDate,
            DeliveredAt: order.DeliveredAt,
            TotalPrice: order.TotalPrice,
            ComboPackageId: order.ComboPackageId,
            ComboPackageName: null, // Cross-module lookup deferred; FK stored as ComboPackageId
            Notes: order.Notes,
            IsOverdue: order.IsOverdue,
            IsUnderWarranty: order.IsUnderWarranty,
            Items: items,
            CreatedAt: order.CreatedAt);

        return Result.Success(dto);
    }
}
