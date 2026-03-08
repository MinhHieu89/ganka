namespace Treatment.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when a treatment session is completed.
/// Consumed by other modules (e.g., Pharmacy) for cross-module reactions
/// such as consumable stock deduction (TRT-11).
/// </summary>
public sealed record TreatmentSessionCompletedIntegrationEvent(
    Guid PackageId,
    Guid SessionId,
    Guid PatientId,
    int TreatmentType,
    List<TreatmentSessionCompletedIntegrationEvent.ConsumableUsageDto> Consumables)
{
    /// <summary>
    /// Represents a consumable item used during the treatment session.
    /// </summary>
    /// <param name="ConsumableItemId">The inventory item identifier.</param>
    /// <param name="Quantity">The quantity consumed during the session.</param>
    public sealed record ConsumableUsageDto(Guid ConsumableItemId, int Quantity);
}
