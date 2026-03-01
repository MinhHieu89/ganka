namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for appointment data used in calendar display and lists.
/// </summary>
public record AppointmentDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    DateTime StartTime,
    DateTime EndTime,
    Guid AppointmentTypeId,
    string AppointmentTypeName,
    int Status,
    string Color,
    string? Notes);
