using Patient.Application.Interfaces;
using Shared.Domain;

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
/// Returns patient count from the Patient module.
/// Appointment, visit, and treatment counts are provided by their respective endpoints.
/// </summary>
public static class GetDashboardStatsHandler
{
    public static async Task<Result<DashboardStatsDto>> Handle(
        GetDashboardStatsQuery query,
        IPatientRepository patientRepository,
        CancellationToken cancellationToken)
    {
        var totalPatients = await patientRepository.GetActiveCountAsync(cancellationToken);

        // TODO: activeVisits and activeTreatments require cross-module queries
        // These are provided as 0 for now and will be wired when cross-module query infrastructure is added
        var stats = new DashboardStatsDto(
            TotalPatients: totalPatients,
            TodayAppointments: 0,  // Will be populated from Scheduling module endpoint
            ActiveVisits: 0,       // Will be populated from Clinical module endpoint
            ActiveTreatments: 0);  // Will be populated from Treatment module endpoint

        return stats;
    }
}
