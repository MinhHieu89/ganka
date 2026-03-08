namespace Treatment.Domain.Models;

/// <summary>
/// Represents a consumable item used during a treatment session.
/// Used in domain events to communicate consumable usage for inventory deduction.
/// </summary>
/// <param name="ConsumableItemId">The inventory item identifier.</param>
/// <param name="Quantity">The quantity consumed during the session.</param>
public sealed record ConsumableUsageInfo(Guid ConsumableItemId, int Quantity);
