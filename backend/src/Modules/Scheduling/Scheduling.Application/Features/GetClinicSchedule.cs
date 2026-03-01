using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;

namespace Scheduling.Application.Features;

/// <summary>
/// Query to get the full clinic operating schedule (7 days).
/// </summary>
public record GetClinicScheduleQuery;

/// <summary>
/// Wolverine handler for retrieving the clinic schedule.
/// </summary>
public static class GetClinicScheduleHandler
{
    public static async Task<List<ClinicScheduleDto>> Handle(
        GetClinicScheduleQuery query,
        IClinicScheduleRepository clinicScheduleRepository,
        CancellationToken ct)
    {
        var schedules = await clinicScheduleRepository.GetAllAsync(ct);

        return schedules.Select(s => new ClinicScheduleDto(
            s.DayOfWeek,
            s.IsOpen,
            s.OpenTime,
            s.CloseTime)).ToList();
    }
}
