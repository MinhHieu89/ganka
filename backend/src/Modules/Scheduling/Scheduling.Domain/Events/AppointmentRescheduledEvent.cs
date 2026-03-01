using Shared.Domain;

namespace Scheduling.Domain.Events;

/// <summary>
/// Published when an appointment is rescheduled to a new time.
/// </summary>
public sealed record AppointmentRescheduledEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid AppointmentId { get; init; }
    public DateTime OldStart { get; init; }
    public DateTime NewStart { get; init; }
}
