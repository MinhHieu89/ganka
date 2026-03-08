using Shared.Domain;
using Treatment.Domain.Enums;

namespace Treatment.Domain.Events;

/// <summary>
/// Domain event raised when a treatment session is completed.
/// Used for cross-module communication to trigger consumable deduction
/// in the inventory module based on items used during the session.
/// </summary>
public sealed record TreatmentSessionCompletedEvent(
    Guid PackageId,
    Guid SessionId,
    Guid PatientId,
    TreatmentType TreatmentType,
    List<TreatmentSessionCompletedEvent.ConsumableUsage> Consumables) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Represents a consumable item used during the treatment session.
    /// </summary>
    /// <param name="ConsumableItemId">The inventory item identifier.</param>
    /// <param name="Quantity">The quantity consumed during the session.</param>
    public sealed record ConsumableUsage(Guid ConsumableItemId, int Quantity);
}
