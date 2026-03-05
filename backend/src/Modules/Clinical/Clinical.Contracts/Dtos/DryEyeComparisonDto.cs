namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for comparing dry eye assessment data across two visits.
/// Contains full assessment data with visit metadata for each side.
/// </summary>
public sealed record DryEyeComparisonDto(
    DryEyeComparisonVisitData Visit1,
    DryEyeComparisonVisitData Visit2);

/// <summary>
/// Dry eye assessment data with visit metadata for comparison.
/// </summary>
public sealed record DryEyeComparisonVisitData(
    Guid VisitId,
    DateTime VisitDate,
    DryEyeAssessmentDto? Assessment);

/// <summary>
/// Query to compare dry eye assessment data across two visits.
/// </summary>
public sealed record GetDryEyeComparisonQuery(
    Guid PatientId,
    Guid VisitId1,
    Guid VisitId2);
