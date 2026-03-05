using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for cross-visit dry eye comparison.
/// Returns both visits' dry eye assessment data side-by-side.
/// Includes security check: both visits must belong to the same patient.
/// </summary>
public static class GetDryEyeComparisonHandler
{
    public static async Task<Result<DryEyeComparisonDto>> Handle(
        GetDryEyeComparisonQuery query,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        // Load both visits with details
        var visit1 = await visitRepository.GetByIdWithDetailsAsync(query.VisitId1, ct);
        if (visit1 is null)
            return Result<DryEyeComparisonDto>.Failure(Error.NotFound("Visit", query.VisitId1));

        var visit2 = await visitRepository.GetByIdWithDetailsAsync(query.VisitId2, ct);
        if (visit2 is null)
            return Result<DryEyeComparisonDto>.Failure(Error.NotFound("Visit", query.VisitId2));

        // Security check: both visits must belong to the same patient
        if (visit1.PatientId != query.PatientId || visit2.PatientId != query.PatientId)
            return Result<DryEyeComparisonDto>.Failure(
                Error.Validation("Both visits must belong to the specified patient."));

        // Get assessments (may be null if no dry eye data for that visit)
        var assessment1 = visit1.DryEyeAssessments.FirstOrDefault();
        var assessment2 = visit2.DryEyeAssessments.FirstOrDefault();

        // Map to DTOs
        var dto1 = assessment1 is not null
            ? new DryEyeAssessmentDto(
                assessment1.Id, assessment1.VisitId,
                assessment1.OdTbut, assessment1.OsTbut,
                assessment1.OdSchirmer, assessment1.OsSchirmer,
                assessment1.OdMeibomianGrading, assessment1.OsMeibomianGrading,
                assessment1.OdTearMeniscus, assessment1.OsTearMeniscus,
                assessment1.OdStaining, assessment1.OsStaining,
                assessment1.OsdiScore,
                assessment1.OsdiSeverity.HasValue ? (int)assessment1.OsdiSeverity.Value : null)
            : null;

        var dto2 = assessment2 is not null
            ? new DryEyeAssessmentDto(
                assessment2.Id, assessment2.VisitId,
                assessment2.OdTbut, assessment2.OsTbut,
                assessment2.OdSchirmer, assessment2.OsSchirmer,
                assessment2.OdMeibomianGrading, assessment2.OsMeibomianGrading,
                assessment2.OdTearMeniscus, assessment2.OsTearMeniscus,
                assessment2.OdStaining, assessment2.OsStaining,
                assessment2.OsdiScore,
                assessment2.OsdiSeverity.HasValue ? (int)assessment2.OsdiSeverity.Value : null)
            : null;

        return new DryEyeComparisonDto(
            new DryEyeComparisonVisitData(visit1.Id, visit1.VisitDate, dto1),
            new DryEyeComparisonVisitData(visit2.Id, visit2.VisitDate, dto2));
    }
}
