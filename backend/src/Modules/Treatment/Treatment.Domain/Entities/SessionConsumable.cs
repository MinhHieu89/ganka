namespace Treatment.Domain.Entities;

/// <summary>
/// Represents a consumable item used during a treatment session (TRT-11).
/// Links to Pharmacy.ConsumableItem via ConsumableItemId for inventory deduction.
/// Child entity of TreatmentSession.
/// </summary>
public class SessionConsumable
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>FK to the parent TreatmentSession.</summary>
    public Guid TreatmentSessionId { get; private set; }

    /// <summary>FK to Pharmacy.ConsumableItem for inventory tracking.</summary>
    public Guid ConsumableItemId { get; private set; }

    /// <summary>Denormalized consumable name for display without cross-module query.</summary>
    public string ConsumableName { get; private set; } = string.Empty;

    /// <summary>Quantity of this consumable used during the session.</summary>
    public int Quantity { get; private set; }

    /// <summary>Private parameterless constructor required by EF Core for materialisation.</summary>
    private SessionConsumable() { }

    /// <summary>
    /// Factory method to create a new session consumable record.
    /// </summary>
    public static SessionConsumable Create(
        Guid treatmentSessionId,
        Guid consumableItemId,
        string consumableName,
        int quantity)
    {
        return new SessionConsumable
        {
            Id = Guid.NewGuid(),
            TreatmentSessionId = treatmentSessionId,
            ConsumableItemId = consumableItemId,
            ConsumableName = consumableName,
            Quantity = quantity
        };
    }
}
