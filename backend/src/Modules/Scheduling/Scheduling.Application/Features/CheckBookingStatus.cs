using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Query to check the status of a self-booking request by reference number.
/// Used by the public status check endpoint.
/// </summary>
public record CheckBookingStatusQuery(string ReferenceNumber);

/// <summary>
/// Wolverine handler for checking booking status.
/// </summary>
public static class CheckBookingStatusHandler
{
    public static async Task<Result<BookingStatusDto>> Handle(
        CheckBookingStatusQuery query,
        ISelfBookingRepository selfBookingRepository,
        IAppointmentRepository appointmentRepository,
        CancellationToken ct)
    {
        var request = await selfBookingRepository.GetByReferenceNumberAsync(query.ReferenceNumber, ct);
        if (request is null)
            return Result<BookingStatusDto>.Failure(Error.NotFound("BookingRequest", query.ReferenceNumber));

        DateTime? appointmentDate = null;
        if (request.CreatedAppointmentId.HasValue)
        {
            var appointment = await appointmentRepository.GetByIdAsync(request.CreatedAppointmentId.Value, ct);
            appointmentDate = appointment?.StartTime;
        }

        return new BookingStatusDto(
            request.ReferenceNumber,
            (int)request.Status,
            request.RejectionReason,
            appointmentDate);
    }
}
