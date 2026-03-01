using Shared.Domain;

namespace Patient.Domain.Events;

/// <summary>
/// Published when a new patient is registered in the system.
/// </summary>
public sealed record PatientRegisteredEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid PatientId { get; init; }
    public string PatientCode { get; init; } = default!;
    public string FullName { get; init; } = default!;
}
