using Shared.Domain;

namespace Treatment.Domain.Events;

/// <summary>
/// Domain event raised when a treatment protocol template is deactivated.
/// </summary>
public sealed record ProtocolDeactivatedEvent(
    Guid ProtocolId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
