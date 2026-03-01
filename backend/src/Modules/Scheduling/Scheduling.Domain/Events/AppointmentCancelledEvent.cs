using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Domain.Events;

/// <summary>
/// Published when an appointment is cancelled.
/// </summary>
public sealed record AppointmentCancelledEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public Guid AppointmentId { get; init; }
    public CancellationReason Reason { get; init; }
}
