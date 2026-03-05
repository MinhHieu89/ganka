namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for dry eye assessment data with all per-eye measurements and OSDI score.
/// </summary>
public sealed record DryEyeAssessmentDto(
    Guid Id,
    Guid VisitId,
    decimal? OdTbut, decimal? OsTbut,
    decimal? OdSchirmer, decimal? OsSchirmer,
    int? OdMeibomianGrading, int? OsMeibomianGrading,
    decimal? OdTearMeniscus, decimal? OsTearMeniscus,
    int? OdStaining, int? OsStaining,
    decimal? OsdiScore,
    int? OsdiSeverity);

/// <summary>
/// Command to update dry eye assessment per-eye measurement fields.
/// </summary>
public sealed record UpdateDryEyeAssessmentCommand(
    Guid VisitId,
    decimal? OdTbut, decimal? OsTbut,
    decimal? OdSchirmer, decimal? OsSchirmer,
    int? OdMeibomianGrading, int? OsMeibomianGrading,
    decimal? OdTearMeniscus, decimal? OsTearMeniscus,
    int? OdStaining, int? OsStaining);
