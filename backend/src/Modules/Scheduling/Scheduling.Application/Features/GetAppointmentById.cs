using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Contracts.Queries;
using Shared.Domain;

namespace Scheduling.Application.Features;

public static class GetAppointmentByIdHandler
{
    public static async Task<Result<AppointmentDetailDto>> Handle(
        GetAppointmentByIdQuery query,
        IAppointmentRepository appointmentRepository,
        CancellationToken ct)
    {
        var appointment = await appointmentRepository.GetByIdAsync(query.AppointmentId, ct);
        if (appointment == null)
            return Result<AppointmentDetailDto>.Failure(Error.NotFound("Appointment", query.AppointmentId));

        return Result<AppointmentDetailDto>.Success(new AppointmentDetailDto(
            Id: appointment.Id,
            PatientId: appointment.PatientId,
            PatientName: appointment.PatientName,
            DoctorId: appointment.DoctorId,
            DoctorName: appointment.DoctorName,
            StartTime: appointment.StartTime,
            EndTime: appointment.EndTime,
            AppointmentTypeId: appointment.AppointmentTypeId,
            Status: (int)appointment.Status,
            Notes: appointment.Notes,
            GuestName: appointment.GuestName,
            GuestPhone: appointment.GuestPhone,
            GuestReason: appointment.GuestReason,
            Source: (int)appointment.Source,
            CheckedInAt: appointment.CheckedInAt));
    }
}
