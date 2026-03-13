namespace Clinical.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when a clinical visit is created.
/// Consumed by Billing module to auto-create a Draft invoice and add consultation fee line item.
/// </summary>
public sealed record VisitCreatedIntegrationEvent(
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    Guid BranchId);
