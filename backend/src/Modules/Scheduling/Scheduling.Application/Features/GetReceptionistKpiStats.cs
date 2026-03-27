using Clinical.Application.Interfaces;
using Clinical.Domain.Enums;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Contracts.Queries;
using Scheduling.Domain.Enums;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Wolverine handler for receptionist KPI stats (today's counts by status).
/// Same data source as dashboard, but just returns aggregated counts.
/// </summary>
public static class GetReceptionistKpiStatsHandler
{
    public static async Task<Result<ReceptionistKpiDto>> Handle(
        GetReceptionistKpiStatsQuery query,
        IAppointmentRepository appointmentRepository,
        IVisitRepository visitRepository,
        CancellationToken ct)
    {
        var appointments = await appointmentRepository.GetTodayAppointmentsAsync(ct);
        var visits = await visitRepository.GetTodayVisitsAsync(ct);

        var visitByAppointment = visits
            .Where(v => v.AppointmentId.HasValue)
            .ToDictionary(v => v.AppointmentId!.Value);

        var todayAppointments = appointments.Count;
        var notArrived = 0;
        var waiting = 0;
        var examining = 0;
        var completed = 0;

        // Count from appointments
        foreach (var apt in appointments)
        {
            if (apt.Status == AppointmentStatus.Cancelled || apt.Status == AppointmentStatus.NoShow)
                continue;

            var hasVisit = visitByAppointment.TryGetValue(apt.Id, out var visit);

            if (!hasVisit)
            {
                notArrived++;
                continue;
            }

            CountVisitStatus(visit!, ref waiting, ref examining, ref completed);
        }

        // Count walk-in visits (no appointment)
        var walkInVisits = visits.Where(v => v.AppointmentId == null || !appointments.Any(a => a.Id == v.AppointmentId));
        foreach (var visit in walkInVisits)
        {
            CountVisitStatus(visit, ref waiting, ref examining, ref completed);
        }

        return Result<ReceptionistKpiDto>.Success(
            new ReceptionistKpiDto(todayAppointments, notArrived, waiting, examining, completed));
    }

    private static void CountVisitStatus(Clinical.Domain.Entities.Visit visit, ref int waiting, ref int examining, ref int completed)
    {
        if (visit.Status == VisitStatus.Cancelled)
            return;

        if (visit.Status == VisitStatus.Signed || visit.CurrentStage == WorkflowStage.Done)
        {
            completed++;
            return;
        }

        if (visit.CurrentStage >= WorkflowStage.RefractionVA)
        {
            examining++;
            return;
        }

        waiting++;
    }
}
