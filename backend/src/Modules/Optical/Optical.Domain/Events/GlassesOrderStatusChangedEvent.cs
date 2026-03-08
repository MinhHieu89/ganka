using Optical.Domain.Enums;
using Shared.Domain;

namespace Optical.Domain.Events;

/// <summary>
/// Domain event raised when a glasses order transitions to a new lifecycle status.
/// Consumed by handlers for notifications, logging, and downstream integrations.
/// </summary>
/// <param name="OrderId">The ID of the glasses order that changed status.</param>
/// <param name="OldStatus">The previous status before the transition.</param>
/// <param name="NewStatus">The new status after the transition.</param>
public sealed record GlassesOrderStatusChangedEvent(
    Guid OrderId,
    GlassesOrderStatus OldStatus,
    GlassesOrderStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
