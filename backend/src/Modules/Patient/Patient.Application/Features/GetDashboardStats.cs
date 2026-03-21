using Patient.Application.Interfaces;
using Shared.Domain;
using Wolverine;
using Scheduling.Contracts.Queries;
using Clinical.Contracts.Dtos;
using Treatment.Contracts.Queries;

namespace Patient.Application.Features;

/// <summary>
/// Query to retrieve dashboard statistics.
/// </summary>
public sealed record GetDashboardStatsQuery;

/// <summary>
/// Response containing dashboard statistics.
/// </summary>
public sealed record DashboardStatsDto(
    int TotalPatients,
    int TodayAppointments,
    int ActiveVisits,
    int ActiveTreatments);

/// <summary>
/// Wolverine handler for dashboard statistics.
/// Aggregates counts from Patient, Scheduling, Clinical, and Treatment modules
/// via IMessageBus cross-module queries.
/// </summary>
public static class GetDashboardStatsHandler
{
    public static async Task<Result<DashboardStatsDto>> Handle(
        GetDashboardStatsQuery query,
        IPatientRepository patientRepository,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var totalPatients = await patientRepository.GetActiveCountAsync(cancellationToken);

        // Cross-module queries via Wolverine message bus with graceful degradation
        int todayAppointments = 0;
        int activeVisits = 0;
        int activeTreatments = 0;

        try { todayAppointments = await bus.InvokeAsync<int>(new GetTodayAppointmentCountQuery(), cancellationToken); } catch { /* graceful degradation */ }
        try { activeVisits = await bus.InvokeAsync<int>(new GetActiveVisitCountQuery(), cancellationToken); } catch { /* graceful degradation */ }
        try { activeTreatments = await bus.InvokeAsync<int>(new GetActiveTreatmentCountQuery(), cancellationToken); } catch { /* graceful degradation */ }

        var stats = new DashboardStatsDto(
            TotalPatients: totalPatients,
            TodayAppointments: todayAppointments,
            ActiveVisits: activeVisits,
            ActiveTreatments: activeTreatments);

        return stats;
    }
}
