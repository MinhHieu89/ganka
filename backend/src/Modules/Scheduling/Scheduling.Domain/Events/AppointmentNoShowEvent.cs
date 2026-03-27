using Shared.Domain;

namespace Scheduling.Domain.Events;

/// <summary>
/// Published when an appointment is marked as no-show.
/// </summary>
public sealed record AppointmentNoShowEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid AppointmentId { get; init; }
    public Guid NoShowBy { get; init; }
}
