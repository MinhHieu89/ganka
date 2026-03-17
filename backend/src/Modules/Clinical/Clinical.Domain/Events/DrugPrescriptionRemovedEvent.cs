using Shared.Domain;

namespace Clinical.Domain.Events;

/// <summary>
/// Domain event raised when a drug prescription is removed from a visit.
/// Used internally within the Clinical module. The Clinical.Application layer
/// converts this to an integration event for cross-module communication (billing).
/// DrugNames included for logging/notification purposes.
/// </summary>
public sealed record DrugPrescriptionRemovedEvent(
    Guid VisitId,
    Guid BranchId,
    List<string> DrugNames) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
