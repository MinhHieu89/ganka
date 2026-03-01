using Shared.Domain;

namespace Scheduling.Domain.Entities;

/// <summary>
/// Represents a type of appointment with default duration and color coding.
/// Predefined types: NewPatient, FollowUp, Treatment, OrthoK.
/// </summary>
public class AppointmentType : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string NameVi { get; private set; } = string.Empty;
    public int DefaultDurationMinutes { get; private set; }
    public string Color { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private AppointmentType() { }

    public AppointmentType(Guid id, string name, string nameVi, int defaultDurationMinutes, string color)
        : base(id)
    {
        Name = name;
        NameVi = nameVi;
        DefaultDurationMinutes = defaultDurationMinutes;
        Color = color;
        IsActive = true;
    }
}
