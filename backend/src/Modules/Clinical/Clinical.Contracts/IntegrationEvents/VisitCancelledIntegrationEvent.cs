namespace Clinical.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when a clinical visit is cancelled.
/// Consumed by Billing module to void the associated invoice.
/// </summary>
public sealed record VisitCancelledIntegrationEvent(
    Guid VisitId,
    Guid BranchId);
