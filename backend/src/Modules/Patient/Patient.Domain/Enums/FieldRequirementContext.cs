namespace Patient.Domain.Enums;

/// <summary>
/// Defines contexts where patient field requirements differ.
/// Registration allows optional Address/CCCD, while downstream workflows require them.
/// </summary>
public enum FieldRequirementContext
{
    /// <summary>
    /// Patient registration — Address and CCCD are optional.
    /// </summary>
    Registration,

    /// <summary>
    /// Referral letter generation — Address and CCCD are required.
    /// </summary>
    Referral,

    /// <summary>
    /// Legal document export — Address and CCCD are required.
    /// </summary>
    LegalExport,

    /// <summary>
    /// So Y Te (Ministry of Health) reporting — Address and CCCD are required.
    /// </summary>
    SoYTeReporting
}
