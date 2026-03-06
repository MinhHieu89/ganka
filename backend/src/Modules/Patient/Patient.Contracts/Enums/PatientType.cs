namespace Patient.Contracts.Enums;

/// <summary>
/// Type of patient registration.
/// Mirrors Patient.Domain.Enums.PatientType for Contracts-layer independence.
/// </summary>
public enum PatientType
{
    Medical = 0,
    WalkIn = 1
}
