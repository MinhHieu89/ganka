using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Pre-configured shift template with default start/end times.
/// Provides Morning/Afternoon defaults matching clinic operating hours.
/// Cashier selects a template when opening a shift (or creates a custom shift without one).
/// </summary>
public class ShiftTemplate : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string NameVi { get; private set; } = string.Empty;
    public TimeOnly DefaultStartTime { get; private set; }
    public TimeOnly DefaultEndTime { get; private set; }
    public bool IsActive { get; private set; }

    private ShiftTemplate() { }

    /// <summary>
    /// Factory method for creating a new shift template.
    /// </summary>
    public static ShiftTemplate Create(
        string name,
        string nameVi,
        TimeOnly defaultStartTime,
        TimeOnly defaultEndTime)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(nameVi))
            throw new ArgumentException("Vietnamese template name is required.", nameof(nameVi));

        if (defaultEndTime <= defaultStartTime)
            throw new ArgumentException("End time must be after start time.", nameof(defaultEndTime));

        return new ShiftTemplate
        {
            Name = name,
            NameVi = nameVi,
            DefaultStartTime = defaultStartTime,
            DefaultEndTime = defaultEndTime,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the template name and time range.
    /// </summary>
    public void Update(string name, string nameVi, TimeOnly defaultStartTime, TimeOnly defaultEndTime)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(nameVi))
            throw new ArgumentException("Vietnamese template name is required.", nameof(nameVi));

        if (defaultEndTime <= defaultStartTime)
            throw new ArgumentException("End time must be after start time.", nameof(defaultEndTime));

        Name = name;
        NameVi = nameVi;
        DefaultStartTime = defaultStartTime;
        DefaultEndTime = defaultEndTime;
        SetUpdatedAt();
    }

    /// <summary>
    /// Deactivates the template (soft delete).
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Reactivates a previously deactivated template.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
