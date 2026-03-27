using Shared.Domain;

namespace Scheduling.Domain.Events;

/// <summary>
/// Published when a patient checks in for their appointment.
/// </summary>
public sealed record AppointmentCheckedInEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid AppointmentId { get; init; }
    public Guid? PatientId { get; init; }
    public DateTime CheckedInAt { get; init; }
}
