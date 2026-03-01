namespace Scheduling.Contracts.Dtos;

/// <summary>
/// Integration event published when an appointment is booked.
/// Used for cross-module communication (e.g., notifications).
/// </summary>
public record AppointmentBookedIntegrationEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime StartTime,
    DateTime EndTime);
