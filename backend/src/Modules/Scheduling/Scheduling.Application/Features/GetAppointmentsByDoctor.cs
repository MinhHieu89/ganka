using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;

namespace Scheduling.Application.Features;

/// <summary>
/// Query to get appointments for a doctor within a date range (for calendar display).
/// </summary>
public record GetAppointmentsByDoctorQuery(Guid DoctorId, DateTime DateFrom, DateTime DateTo);

/// <summary>
/// Wolverine handler for retrieving appointments by doctor.
/// </summary>
public static class GetAppointmentsByDoctorHandler
{
    public static async Task<List<AppointmentDto>> Handle(
        GetAppointmentsByDoctorQuery query,
        IAppointmentRepository appointmentRepository,
        CancellationToken ct)
    {
        var appointments = await appointmentRepository.GetByDoctorAsync(
            query.DoctorId, query.DateFrom, query.DateTo, ct);

        // Load appointment types for mapping
        var types = await appointmentRepository.GetAllAppointmentTypesAsync(ct);
        var typeMap = types.ToDictionary(t => t.Id);

        return appointments.Select(a =>
        {
            typeMap.TryGetValue(a.AppointmentTypeId, out var appointmentType);
            return new AppointmentDto(
                a.Id,
                a.PatientId,
                a.PatientName,
                a.DoctorId,
                a.DoctorName,
                a.StartTime,
                a.EndTime,
                a.AppointmentTypeId,
                appointmentType?.Name ?? "Unknown",
                (int)a.Status,
                appointmentType?.Color ?? "#6b7280",
                a.Notes);
        }).ToList();
    }
}
