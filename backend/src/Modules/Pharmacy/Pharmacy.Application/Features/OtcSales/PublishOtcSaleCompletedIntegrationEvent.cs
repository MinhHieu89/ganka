using Pharmacy.Contracts.IntegrationEvents;
using Pharmacy.Domain.Events;

namespace Pharmacy.Application.Features.OtcSales;

/// <summary>
/// Wolverine cascading handler that converts the internal domain event
/// (OtcSaleCompletedEvent) into a cross-module integration event
/// (OtcSaleCompletedIntegrationEvent) for consumption by other modules (e.g., Billing).
///
/// This maintains module boundary isolation: the domain event stays internal,
/// and only the contracts integration event crosses module boundaries.
/// </summary>
public static class PublishOtcSaleCompletedIntegrationEventHandler
{
    public static OtcSaleCompletedIntegrationEvent Handle(
        OtcSaleCompletedEvent domainEvent)
    {
        return new OtcSaleCompletedIntegrationEvent(
            OtcSaleId: domainEvent.OtcSaleId,
            PatientId: domainEvent.PatientId,
            CustomerName: domainEvent.CustomerName,
            Items: domainEvent.Items
                .Select(i => new OtcSaleCompletedIntegrationEvent.DrugLineDto(
                    i.DrugName, i.DrugNameVi, i.Quantity, i.UnitPrice))
                .ToList(),
            BranchId: domainEvent.BranchId);
    }
}
