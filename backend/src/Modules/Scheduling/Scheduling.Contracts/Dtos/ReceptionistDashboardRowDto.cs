namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for a single row in the receptionist dashboard.
/// Maps the 11-stage clinical workflow to 4 receptionist statuses.
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
