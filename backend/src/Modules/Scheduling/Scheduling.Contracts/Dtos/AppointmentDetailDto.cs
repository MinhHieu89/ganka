namespace Scheduling.Contracts.Dtos;

public sealed record AppointmentDetailDto(
    Guid Id,
    Guid? PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    DateTime StartTime,
    DateTime EndTime,
    Guid AppointmentTypeId,
    int Status,
    string? Notes,
    string? GuestName,
    string? GuestPhone,
    string? GuestReason,
    int Source,
    DateTime? CheckedInAt);
