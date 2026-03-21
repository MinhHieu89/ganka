using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Queries;

namespace Scheduling.Application.Features;

/// <summary>
/// Wolverine handler returning today's non-cancelled appointment count.
/// Used by the Patient module dashboard via IMessageBus cross-module query.
/// </summary>
public static class GetTodayAppointmentCountHandler
{
    public static async Task<int> Handle(
        GetTodayAppointmentCountQuery query,
        IAppointmentRepository appointmentRepository,
        CancellationToken ct)
    {
        return await appointmentRepository.GetTodayCountAsync(ct);
    }
}
