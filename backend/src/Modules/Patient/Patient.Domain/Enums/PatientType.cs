namespace Patient.Domain.Enums;

/// <summary>
/// Type of patient registration.
/// Medical patients require full demographic data; WalkIn patients need only name and phone.
/// </summary>
public enum PatientType
{
    Medical = 0,
    WalkIn = 1
}
