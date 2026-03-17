namespace Clinical.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when a drug prescription is added to a visit.
/// Consumed by the Billing module to create pharmacy line items at prescription time
/// (before dispensing), enabling the Vietnamese clinic flow: prescribe -> pay -> dispense.
/// </summary>
public sealed record DrugPrescriptionAddedIntegrationEvent(
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    Guid BranchId,
    List<DrugPrescriptionAddedIntegrationEvent.PrescribedDrugDto> Items)
{
    /// <summary>
    /// Represents a single prescribed drug in the integration event.
    /// DrugCatalogItemId is nullable for off-catalog drugs.
    /// Billing module uses DrugCatalogItemId to look up pricing.
    /// </summary>
    public sealed record PrescribedDrugDto(
        string DrugName,
        Guid? DrugCatalogItemId,
        int Quantity);
}
