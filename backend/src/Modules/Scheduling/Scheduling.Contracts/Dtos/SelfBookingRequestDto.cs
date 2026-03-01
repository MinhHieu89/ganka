namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for self-booking request data displayed in the staff management view.
/// </summary>
public record SelfBookingRequestDto(
    Guid Id,
    string PatientName,
    string Phone,
    string? Email,
    DateTime PreferredDate,
    string? PreferredTimeSlot,
    string AppointmentTypeName,
    int Status,
    string ReferenceNumber,
    string? RejectionReason,
    DateTime CreatedAt);
