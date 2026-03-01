using Shared.Domain;

namespace Patient.Domain.Events;

/// <summary>
/// Published when patient details are updated.
/// </summary>
public sealed record PatientUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid PatientId { get; init; }
}
