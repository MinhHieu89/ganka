using Patient.Domain.Enums;
using Shared.Domain;

namespace Patient.Domain.Entities;

/// <summary>
/// Allergy entity owned by Patient aggregate.
/// Not an aggregate root -- lifecycle managed through Patient.
/// </summary>
public class Allergy : Entity
{
    public string Name { get; private set; } = string.Empty;
    public AllergySeverity Severity { get; private set; }
    public Guid PatientId { get; private set; }

    private Allergy() { }

    /// <summary>
    /// Factory method for creating a new allergy record.
    /// </summary>
    public static Allergy Create(string name, AllergySeverity severity, Guid patientId)
    {
        return new Allergy
        {
            Name = name,
            Severity = severity,
            PatientId = patientId
        };
    }
}
