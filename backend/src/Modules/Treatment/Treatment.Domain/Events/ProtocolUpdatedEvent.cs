using Shared.Domain;
using Treatment.Domain.Enums;

namespace Treatment.Domain.Events;

/// <summary>
/// Domain event raised when a treatment protocol template is updated.
/// </summary>
public sealed record ProtocolUpdatedEvent(
    Guid ProtocolId,
    string Name,
    TreatmentType TreatmentType) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
