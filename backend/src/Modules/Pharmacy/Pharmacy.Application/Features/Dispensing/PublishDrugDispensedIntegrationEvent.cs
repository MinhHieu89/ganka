using Pharmacy.Contracts.IntegrationEvents;
using Pharmacy.Domain.Events;

namespace Pharmacy.Application.Features.Dispensing;

/// <summary>
/// Wolverine cascading handler that converts the internal domain event
/// (DrugDispensedEvent) into a cross-module integration event
/// (DrugDispensedIntegrationEvent) for consumption by other modules (e.g., Billing).
///
/// This maintains module boundary isolation: the domain event stays internal,
/// and only the contracts integration event crosses module boundaries.
/// </summary>
public static class PublishDrugDispensedIntegrationEventHandler
{
    public static DrugDispensedIntegrationEvent Handle(
        DrugDispensedEvent domainEvent)
    {
        return new DrugDispensedIntegrationEvent(
            VisitId: domainEvent.VisitId,
            PatientId: domainEvent.PatientId,
            PatientName: domainEvent.PatientName,
            Items: domainEvent.Items
                .Select(i => new DrugDispensedIntegrationEvent.DrugLineDto(
                    i.DrugName, i.DrugNameVi, i.Quantity, i.UnitPrice))
                .ToList());
    }
}
