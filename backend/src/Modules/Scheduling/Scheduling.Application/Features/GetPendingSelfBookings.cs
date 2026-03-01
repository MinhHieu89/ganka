using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;

namespace Scheduling.Application.Features;

/// <summary>
/// Query to get all pending self-booking requests for staff review.
/// </summary>
public record GetPendingSelfBookingsQuery;

/// <summary>
/// Wolverine handler for retrieving pending self-booking requests.
/// </summary>
public static class GetPendingSelfBookingsHandler
{
    public static async Task<List<SelfBookingRequestDto>> Handle(
        GetPendingSelfBookingsQuery query,
        ISelfBookingRepository selfBookingRepository,
        IAppointmentRepository appointmentRepository,
        CancellationToken ct)
    {
        var requests = await selfBookingRepository.GetPendingAsync(ct);

        var types = await appointmentRepository.GetAllAppointmentTypesAsync(ct);
        var typeMap = types.ToDictionary(t => t.Id);

        return requests.Select(r =>
        {
            typeMap.TryGetValue(r.AppointmentTypeId, out var appointmentType);
            return new SelfBookingRequestDto(
                r.Id,
                r.PatientName,
                r.Phone,
                r.Email,
                r.PreferredDate,
                r.PreferredTimeSlot,
                appointmentType?.Name ?? "Unknown",
                (int)r.Status,
                r.ReferenceNumber,
                r.RejectionReason,
                r.CreatedAt);
        }).ToList();
    }
}
