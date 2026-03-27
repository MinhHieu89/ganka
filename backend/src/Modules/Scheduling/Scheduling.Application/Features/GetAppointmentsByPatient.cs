using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;

namespace Scheduling.Application.Features;

/// <summary>
/// Query to get all appointments for a patient.
/// </summary>
public record GetAppointmentsByPatientQuery(Guid PatientId);

/// <summary>
/// Wolverine handler for retrieving appointments by patient.
/// </summary>
public static class GetAppointmentsByPatientHandler
{
    public static async Task<List<AppointmentDto>> Handle(
        GetAppointmentsByPatientQuery query,
        IAppointmentRepository appointmentRepository,
        CancellationToken ct)
    {
        var appointments = await appointmentRepository.GetByPatientAsync(query.PatientId, ct);

        var types = await appointmentRepository.GetAllAppointmentTypesAsync(ct);
        var typeMap = types.ToDictionary(t => t.Id);

        return appointments.Select(a =>
        {
            typeMap.TryGetValue(a.AppointmentTypeId, out var appointmentType);
            return new AppointmentDto(
                a.Id,
                a.PatientId ?? Guid.Empty,
                a.PatientName,
                a.DoctorId,
                a.DoctorName,
                a.StartTime,
                a.EndTime,
                a.AppointmentTypeId,
                appointmentType?.Name ?? "Unknown",
                appointmentType?.NameVi ?? "Unknown",
                (int)a.Status,
                appointmentType?.Color ?? "#6b7280",
                a.Notes);
        }).ToList();
    }
}
