namespace Clinical.Domain.Enums;

/// <summary>
/// Type of refraction measurement.
/// Each visit can have one refraction record per type.
/// </summary>
public enum RefractionType
{
    Manifest = 0,
    Autorefraction = 1,
    Cycloplegic = 2
}
