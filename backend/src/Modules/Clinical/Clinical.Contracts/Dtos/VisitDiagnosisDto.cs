namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for a visit diagnosis with ICD-10 code and laterality.
/// </summary>
public record VisitDiagnosisDto(
    Guid Id,
    string Icd10Code,
    string DescriptionEn,
    string DescriptionVi,
    int Laterality,
    int Role,
    int SortOrder);
