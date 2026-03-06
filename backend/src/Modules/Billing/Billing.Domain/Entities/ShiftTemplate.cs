using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Pre-configured shift template with name and default time range.
/// Examples: "Morning" (Ca sang), "Afternoon" (Ca chieu).
/// </summary>
public class ShiftTemplate : Entity
{
    public string Name { get; private set; } = default!;
    public string? NameVi { get; private set; }
    public TimeOnly DefaultStartTime { get; private set; }
    public TimeOnly DefaultEndTime { get; private set; }
    public bool IsActive { get; private set; }

    private ShiftTemplate() { }

    public static ShiftTemplate Create(
        string name,
        string? nameVi,
        TimeOnly defaultStartTime,
        TimeOnly defaultEndTime)
    {
        return new ShiftTemplate
        {
            Name = name,
            NameVi = nameVi,
            DefaultStartTime = defaultStartTime,
            DefaultEndTime = defaultEndTime,
            IsActive = true
        };
    }
}
