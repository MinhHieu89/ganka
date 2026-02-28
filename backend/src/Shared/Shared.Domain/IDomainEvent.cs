namespace Shared.Domain;

/// <summary>
/// Marker interface for domain events published by aggregate roots.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
