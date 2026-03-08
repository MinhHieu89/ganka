namespace Pharmacy.Domain.Enums;

/// <summary>
/// Flags enum for supplier types. A supplier can provide drugs, optical products, or both.
/// Used to filter suppliers in cross-module queries (e.g., GetOpticalSuppliersQuery).
/// </summary>
[Flags]
public enum SupplierType
{
    /// <summary>No supplier type assigned.</summary>
    None = 0,

    /// <summary>Supplier provides pharmaceutical drugs.</summary>
    Drug = 1,

    /// <summary>Supplier provides optical products (frames, lenses, etc.).</summary>
    Optical = 2
}
