using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for patient visit history query (D-13/D-15).
/// Returns visits ordered by date descending with primary diagnosis text.
/// </summary>
public static class GetPatientVisitHistoryHandler
{
    public static async Task<List<PatientVisitHistoryDto>> Handle(
        GetPatientVisitHistoryQuery query,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        var visits = await visitRepository.GetVisitsByPatientIdAsync(query.PatientId, ct);

        return visits
            .OrderByDescending(v => v.VisitDate)
            .Select(v => new PatientVisitHistoryDto(
                v.Id,
                v.VisitDate,
                v.DoctorName,
                (int)v.Status,
                v.Diagnoses.FirstOrDefault(d => d.Role == DiagnosisRole.Primary)?.DescriptionVi
                    ?? v.Diagnoses.FirstOrDefault()?.DescriptionVi,
                (int)v.CurrentStage))
            .ToList();
    }
}
