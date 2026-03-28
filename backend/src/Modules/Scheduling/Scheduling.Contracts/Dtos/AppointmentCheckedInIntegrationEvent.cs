namespace Scheduling.Contracts.Dtos;

/// <summary>
/// Integration event published when a patient checks in for their appointment.
/// Consumed by Clinical module to create a Visit at Reception stage.
/// </summary>
public record AppointmentCheckedInIntegrationEvent(
    Guid AppointmentId,
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    bool HasAllergies,
    DateTime CheckedInAt,
    string? Reason);
