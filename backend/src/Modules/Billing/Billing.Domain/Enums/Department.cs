namespace Billing.Domain.Enums;

/// <summary>
/// Department categories for revenue allocation.
/// Maps to Vietnamese grouping: Kham benh, Duoc pham, Kinh, Dieu tri.
/// </summary>
public enum Department
{
    Medical = 0,
    Pharmacy = 1,
    Optical = 2,
    Treatment = 3
}
