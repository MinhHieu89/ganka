namespace Shared.Domain;

/// <summary>
/// ICD-10 reference code entity. Used across multiple modules for diagnosis coding.
/// Stored in the "reference" schema as cross-module reference data.
/// Seeded on startup from icd10-ophthalmology.json.
/// </summary>
public sealed class Icd10Code
{
    /// <summary>
    /// ICD-10 code (e.g., "H04.121" for Dry Eye, right eye). Primary key.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// English description of the condition.
    /// </summary>
    public string DescriptionEn { get; private set; } = string.Empty;

    /// <summary>
    /// Vietnamese description of the condition.
    /// </summary>
    public string DescriptionVi { get; private set; } = string.Empty;

    /// <summary>
    /// Category grouping (e.g., "Dry Eye", "Myopia", "Glaucoma").
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this code requires laterality specification (.1=right, .2=left, .3=bilateral, .9=unspecified).
    /// </summary>
    public bool RequiresLaterality { get; private set; }

    /// <summary>
    /// Per-user favorites feature (Phase 3). Default false.
    /// </summary>
    public bool IsFavorite { get; private set; }

    private Icd10Code() { }

    public static Icd10Code Create(
        string code,
        string descriptionEn,
        string descriptionVi,
        string category,
        bool requiresLaterality)
    {
        return new Icd10Code
        {
            Code = code,
            DescriptionEn = descriptionEn,
            DescriptionVi = descriptionVi,
            Category = category,
            RequiresLaterality = requiresLaterality,
            IsFavorite = false
        };
    }
}
