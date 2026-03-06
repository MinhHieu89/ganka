namespace Pharmacy.Domain.Enums;

/// <summary>
/// Reason for a manual stock adjustment.
/// Used for audit trailing and reporting on stock discrepancies and write-offs.
/// </summary>
public enum StockAdjustmentReason
{
    /// <summary>Stock count correction — physical count differs from system quantity.</summary>
    Correction = 0,

    /// <summary>Write-off — stock is disposed of due to policy or regulatory requirement.</summary>
    WriteOff = 1,

    /// <summary>Damage — stock is physically damaged and unusable.</summary>
    Damage = 2,

    /// <summary>Expired — stock has passed its expiry date and must be removed.</summary>
    Expired = 3,

    /// <summary>Other — any reason not covered by the above categories. Notes field should describe the reason.</summary>
    Other = 4
}
