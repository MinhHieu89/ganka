using Clinical.Contracts.IntegrationEvents;
using Clinical.Domain.Events;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine cascading handler that converts the internal domain event
/// (DrugPrescriptionRemovedEvent) into a cross-module integration event
/// (DrugPrescriptionRemovedIntegrationEvent) for consumption by the Billing module.
///
/// This maintains module boundary isolation: the domain event stays internal,
/// and only the contracts integration event crosses module boundaries.
/// </summary>
public static class PublishDrugPrescriptionRemovedIntegrationEventHandler
{
    public static DrugPrescriptionRemovedIntegrationEvent Handle(
        DrugPrescriptionRemovedEvent domainEvent)
    {
        return new DrugPrescriptionRemovedIntegrationEvent(
            VisitId: domainEvent.VisitId,
            BranchId: domainEvent.BranchId,
            DrugNames: domainEvent.DrugNames);
    }
}
