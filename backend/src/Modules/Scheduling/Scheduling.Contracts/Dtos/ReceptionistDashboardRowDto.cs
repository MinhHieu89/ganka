namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for a single row in the receptionist dashboard.
/// Maps appointments and visits to the 4-status receptionist model.
/// </summary>
public sealed record ReceptionistDashboardRowDto(
    Guid Id,
    Guid? AppointmentId,
    Guid? VisitId,
    Guid? PatientId,
    string PatientName,
    string? PatientCode,
    int? BirthYear,
    DateTime? AppointmentTime,
    string Source,
    string? Reason,
    string Status,
    DateTime? CheckedInAt,
    bool IsGuestBooking);
