using Treatment.Contracts.IntegrationEvents;
using Treatment.Domain.Events;
using Wolverine;

namespace Treatment.Application.Features;

/// <summary>
/// Wolverine cascading handler that converts the internal domain event
/// (TreatmentSessionCompletedEvent) into a cross-module integration event
/// (TreatmentSessionCompletedIntegrationEvent) for consumption by other modules.
///
/// This maintains module boundary isolation: the domain event stays internal,
/// and only the contracts integration event crosses module boundaries.
/// </summary>
public static class PublishSessionCompletedIntegrationEventHandler
{
    public static TreatmentSessionCompletedIntegrationEvent Handle(
        TreatmentSessionCompletedEvent domainEvent)
    {
        return new TreatmentSessionCompletedIntegrationEvent(
            PackageId: domainEvent.PackageId,
            SessionId: domainEvent.SessionId,
            PatientId: domainEvent.PatientId,
            TreatmentType: (int)domainEvent.TreatmentType,
            Consumables: domainEvent.Consumables
                .Select(c => new TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto(
                    c.ConsumableItemId, c.Quantity))
                .ToList(),
            VisitId: domainEvent.VisitId,
            SessionFeeAmount: domainEvent.SessionFeeAmount);
    }
}
