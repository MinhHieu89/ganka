namespace Clinical.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when a drug prescription is removed from a visit.
/// Consumed by the Billing module to remove prescription-linked line items from the invoice.
/// </summary>
public sealed record DrugPrescriptionRemovedIntegrationEvent(
    Guid VisitId,
    Guid BranchId,
    List<string> DrugNames);
