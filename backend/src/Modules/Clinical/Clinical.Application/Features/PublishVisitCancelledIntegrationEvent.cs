using Clinical.Contracts.IntegrationEvents;
using Clinical.Domain.Events;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine cascading handler that converts the internal domain event
/// (VisitCancelledEvent) into a cross-module integration event
/// (VisitCancelledIntegrationEvent) for consumption by other modules (e.g., Billing).
///
/// This maintains module boundary isolation: the domain event stays internal,
/// and only the contracts integration event crosses module boundaries.
/// </summary>
public static class PublishVisitCancelledIntegrationEventHandler
{
    public static VisitCancelledIntegrationEvent Handle(
        VisitCancelledEvent domainEvent)
    {
        return new VisitCancelledIntegrationEvent(
            VisitId: domainEvent.VisitId);
    }
}
