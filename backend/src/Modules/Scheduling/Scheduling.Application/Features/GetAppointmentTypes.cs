using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;

namespace Scheduling.Application.Features;

/// <summary>
/// Query to get all active appointment types.
/// </summary>
public record GetAppointmentTypesQuery;

/// <summary>
/// Wolverine handler for retrieving appointment types.
/// </summary>
public static class GetAppointmentTypesHandler
{
    public static async Task<List<AppointmentTypeDto>> Handle(
        GetAppointmentTypesQuery query,
        IAppointmentRepository appointmentRepository,
        CancellationToken ct)
    {
        var types = await appointmentRepository.GetAllAppointmentTypesAsync(ct);

        return types.Select(t => new AppointmentTypeDto(
            t.Id,
            t.Name,
            t.NameVi,
            t.DefaultDurationMinutes,
            t.Color)).ToList();
    }
}
