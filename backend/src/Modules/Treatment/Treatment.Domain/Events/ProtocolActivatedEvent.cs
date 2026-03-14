using Shared.Domain;

namespace Treatment.Domain.Events;

/// <summary>
/// Domain event raised when a treatment protocol template is re-activated.
/// </summary>
public sealed record ProtocolActivatedEvent(
    Guid ProtocolId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
