using Shared.Domain;

namespace Clinical.Domain.Events;

/// <summary>
/// Domain event raised when a clinical visit is cancelled.
/// Used internally within the Clinical module. The Clinical.Application layer
/// converts this to an integration event for cross-module communication (billing invoice voiding).
/// </summary>
public sealed record VisitCancelledEvent(
    Guid VisitId,
    Guid BranchId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
