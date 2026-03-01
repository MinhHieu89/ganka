namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for public booking status check endpoint.
/// Patients use their reference number to check booking status.
/// </summary>
public record BookingStatusDto(
    string ReferenceNumber,
    int Status,
    string? RejectionReason,
    DateTime? AppointmentDate);
