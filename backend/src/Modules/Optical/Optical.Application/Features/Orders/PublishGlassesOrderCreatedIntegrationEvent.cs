using Optical.Contracts.IntegrationEvents;
using Optical.Domain.Events;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Wolverine cascading handler that converts the internal domain event
/// (GlassesOrderCreatedEvent) into a cross-module integration event
/// (GlassesOrderCreatedIntegrationEvent) for consumption by other modules.
///
/// This maintains module boundary isolation: the domain event stays internal,
/// and only the contracts integration event crosses module boundaries.
/// </summary>
public static class PublishGlassesOrderCreatedIntegrationEventHandler
{
    public static GlassesOrderCreatedIntegrationEvent Handle(
        GlassesOrderCreatedEvent domainEvent)
    {
        return new GlassesOrderCreatedIntegrationEvent(
            OrderId: domainEvent.OrderId,
            VisitId: domainEvent.VisitId,
            PatientId: domainEvent.PatientId,
            PatientName: domainEvent.PatientName,
            Items: domainEvent.Items
                .Select(i => new GlassesOrderCreatedIntegrationEvent.OrderLineDto(
                    i.Description, i.DescriptionVi, i.UnitPrice, i.Quantity))
                .ToList());
    }
}
