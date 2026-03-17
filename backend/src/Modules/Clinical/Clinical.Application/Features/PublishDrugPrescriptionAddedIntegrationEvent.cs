using Clinical.Contracts.IntegrationEvents;
using Clinical.Domain.Events;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine cascading handler that converts the internal domain event
/// (DrugPrescriptionAddedEvent) into a cross-module integration event
/// (DrugPrescriptionAddedIntegrationEvent) for consumption by the Billing module.
///
/// This maintains module boundary isolation: the domain event stays internal,
/// and only the contracts integration event crosses module boundaries.
/// </summary>
public static class PublishDrugPrescriptionAddedIntegrationEventHandler
{
    public static DrugPrescriptionAddedIntegrationEvent Handle(
        DrugPrescriptionAddedEvent domainEvent)
    {
        return new DrugPrescriptionAddedIntegrationEvent(
            VisitId: domainEvent.VisitId,
            PatientId: domainEvent.PatientId,
            PatientName: domainEvent.PatientName,
            BranchId: domainEvent.BranchId,
            Items: domainEvent.Items.Select(i =>
                new DrugPrescriptionAddedIntegrationEvent.PrescribedDrugDto(
                    i.DrugName, i.DrugCatalogItemId, i.Quantity)).ToList());
    }
}
