namespace Patient.Contracts.Enums;

/// <summary>
/// Severity level of an allergy.
/// Mirrors Patient.Domain.Enums.AllergySeverity for Contracts-layer independence.
/// </summary>
public enum AllergySeverity
{
    Mild = 0,
    Moderate = 1,
    Severe = 2
}
