using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for querying OSDI score history for trend chart display.
/// Returns chronological OSDI data points for a patient across visits.
/// Filters to only assessments with non-null OSDI scores.
/// </summary>
public static class GetOsdiHistoryHandler
{
    public static async Task<OsdiHistoryResponse> Handle(
        GetOsdiHistoryQuery query,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        // Get all dry eye assessments for the patient (already ordered by visit date)
        var assessments = await visitRepository.GetDryEyeAssessmentsByPatientAsync(query.PatientId, ct);

        // Filter to only those with OSDI scores
        var withScore = assessments.Where(a => a.OsdiScore.HasValue).ToList();

        if (withScore.Count == 0)
            return new OsdiHistoryResponse([]);

        // For each assessment, get the visit date
        var items = new List<OsdiHistoryDto>(withScore.Count);
        foreach (var assessment in withScore)
        {
            var visit = await visitRepository.GetByIdAsync(assessment.VisitId, ct);
            if (visit is null) continue;

            items.Add(new OsdiHistoryDto(
                assessment.VisitId,
                visit.VisitDate,
                assessment.OsdiScore!.Value,
                (int)assessment.OsdiSeverity!.Value));
        }

        return new OsdiHistoryResponse(items);
    }
}
