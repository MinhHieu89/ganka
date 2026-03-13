using Shared.Domain;

namespace Clinical.Domain.Events;

/// <summary>
/// Domain event raised when a new clinical visit is created.
/// Used internally within the Clinical module. The Clinical.Application layer
/// converts this to an integration event for cross-module communication (billing).
/// </summary>
public sealed record VisitCreatedEvent(
    Guid VisitId,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
