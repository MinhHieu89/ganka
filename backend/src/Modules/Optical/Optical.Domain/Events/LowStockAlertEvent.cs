using Shared.Domain;

namespace Optical.Domain.Events;

/// <summary>
/// Domain event raised when a frame or lens stock quantity falls at or below the minimum stock level.
/// Consumed by alert handlers to notify optical staff of low inventory.
/// </summary>
/// <param name="EntityId">Primary key of the frame or lens catalog item that is low on stock.</param>
/// <param name="EntityType">Distinguishes the source entity: "Frame" or "Lens" for alert routing.</param>
/// <param name="Name">Human-readable name of the item (e.g., "Ray-Ban RB3025 (Black, 52-18-140)").</param>
/// <param name="CurrentStock">Current stock quantity at the time the event was raised.</param>
/// <param name="MinStockLevel">The minimum stock threshold that was breached.</param>
public sealed record LowStockAlertEvent(
    Guid EntityId,
    string EntityType,
    string Name,
    int CurrentStock,
    int MinStockLevel) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
