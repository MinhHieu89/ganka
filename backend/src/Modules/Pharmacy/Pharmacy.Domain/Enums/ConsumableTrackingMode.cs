namespace Pharmacy.Domain.Enums;

/// <summary>
/// Defines how stock is tracked for a consumable item in the consumables warehouse.
///
/// ExpiryTracked: Items are tracked with individual batches (batch number, expiry date, FEFO).
/// Used for items where regulatory traceability or FEFO dispensing is required
/// (e.g., IPL gel, anesthetic drops, sterile wipes).
///
/// SimpleStock: Items are tracked with a single quantity counter only.
/// Used for non-expiry-critical items where batch traceability is not needed
/// (e.g., eye shields, disposable tips, lid care pads).
/// </summary>
public enum ConsumableTrackingMode
{
    /// <summary>Batch-level tracking with expiry date and FEFO support.</summary>
    ExpiryTracked = 0,

    /// <summary>Simple quantity-only tracking without batch or expiry management.</summary>
    SimpleStock = 1
}
