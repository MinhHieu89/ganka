using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Links an ICD-10 code to a visit with laterality and primary/secondary role.
/// Stores bilingual descriptions for display without cross-module joins.
/// </summary>
public class VisitDiagnosis : Entity
{
    public Guid VisitId { get; private set; }
    public string Icd10Code { get; private set; } = string.Empty;
    public string DescriptionEn { get; private set; } = string.Empty;
    public string DescriptionVi { get; private set; } = string.Empty;
    public Laterality Laterality { get; private set; }
    public DiagnosisRole Role { get; private set; }
    public int SortOrder { get; private set; }

    private VisitDiagnosis() { }

    /// <summary>
    /// Factory method for creating a visit diagnosis.
    /// </summary>
    public static VisitDiagnosis Create(
        Guid visitId,
        string icd10Code,
        string descriptionEn,
        string descriptionVi,
        Laterality laterality,
        DiagnosisRole role,
        int sortOrder)
    {
        return new VisitDiagnosis
        {
            VisitId = visitId,
            Icd10Code = icd10Code,
            DescriptionEn = descriptionEn,
            DescriptionVi = descriptionVi,
            Laterality = laterality,
            Role = role,
            SortOrder = sortOrder
        };
    }
}
