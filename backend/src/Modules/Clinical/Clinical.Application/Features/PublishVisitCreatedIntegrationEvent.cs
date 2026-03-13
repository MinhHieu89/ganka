using Clinical.Contracts.IntegrationEvents;
using Clinical.Domain.Events;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine cascading handler that converts the internal domain event
/// (VisitCreatedEvent) into a cross-module integration event
/// (VisitCreatedIntegrationEvent) for consumption by other modules (e.g., Billing).
///
/// This maintains module boundary isolation: the domain event stays internal,
/// and only the contracts integration event crosses module boundaries.
/// </summary>
public static class PublishVisitCreatedIntegrationEventHandler
{
    public static VisitCreatedIntegrationEvent Handle(
        VisitCreatedEvent domainEvent)
    {
        return new VisitCreatedIntegrationEvent(
            VisitId: domainEvent.VisitId,
            PatientId: domainEvent.PatientId,
            PatientName: domainEvent.PatientName,
            BranchId: domainEvent.BranchId);
    }
}
