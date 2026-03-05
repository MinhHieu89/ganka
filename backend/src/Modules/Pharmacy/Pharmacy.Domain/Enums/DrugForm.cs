namespace Pharmacy.Domain.Enums;

/// <summary>
/// Pharmaceutical form/dosage form of a drug catalog item.
/// Values are relevant to ophthalmology clinic use cases.
/// </summary>
public enum DrugForm
{
    /// <summary>Eye drops (Thuốc nhỏ mắt)</summary>
    EyeDrops = 0,

    /// <summary>Tablet (Viên nén)</summary>
    Tablet = 1,

    /// <summary>Capsule (Viên nang)</summary>
    Capsule = 2,

    /// <summary>Ointment (Thuốc mỡ)</summary>
    Ointment = 3,

    /// <summary>Injection (Thuốc tiêm)</summary>
    Injection = 4,

    /// <summary>Gel</summary>
    Gel = 5,

    /// <summary>Solution (Dung dịch)</summary>
    Solution = 6,

    /// <summary>Suspension (Hỗn dịch)</summary>
    Suspension = 7,

    /// <summary>Cream (Kem)</summary>
    Cream = 8,

    /// <summary>Spray (Xịt)</summary>
    Spray = 9
}
