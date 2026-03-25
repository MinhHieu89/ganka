using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for getting all active visits for the workflow dashboard.
/// Maps visits to ActiveVisitDto with HasAllergies flag and WaitMinutes calculation.
/// </summary>
public static class GetActiveVisitsHandler
{
    public static async Task<List<ActiveVisitDto>> Handle(
        GetActiveVisitsQuery query,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        var visits = await visitRepository.GetActiveVisitsAsync(ct);

        return visits.Select(v => new ActiveVisitDto(
            v.Id,
            v.PatientId,
            v.PatientName,
            v.DoctorName,
            (int)v.CurrentStage,
            v.VisitDate,
            v.HasAllergies,
            (int)(DateTime.UtcNow - v.VisitDate).TotalMinutes,
            v.CurrentStage == Clinical.Domain.Enums.WorkflowStage.PharmacyOptical
        )).ToList();
    }
}
