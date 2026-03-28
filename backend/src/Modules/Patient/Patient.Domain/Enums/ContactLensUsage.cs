namespace Patient.Domain.Enums;

/// <summary>
/// Patient's contact lens type, relevant for dry eye assessment.
/// </summary>
public enum ContactLensUsage
{
    None = 0,
    Soft = 1,
    Rgp = 2,
    // ReSharper disable once InconsistentNaming
    Ortho_K = 3,
    Other = 4
}
