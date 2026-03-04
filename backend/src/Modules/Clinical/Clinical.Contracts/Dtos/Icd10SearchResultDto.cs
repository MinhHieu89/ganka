namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for ICD-10 search results with bilingual descriptions and favorite status.
/// </summary>
public record Icd10SearchResultDto(
    string Code,
    string DescriptionEn,
    string DescriptionVi,
    string Category,
    bool RequiresLaterality,
    bool IsFavorite);
