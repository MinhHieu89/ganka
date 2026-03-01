using Shared.Domain;

namespace Scheduling.Domain.Events;

/// <summary>
/// Published when a new appointment is booked.
/// </summary>
public sealed record AppointmentBookedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid AppointmentId { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime StartTime { get; init; }
}
