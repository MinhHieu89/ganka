namespace Patient.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when a patient is registered from intake
/// and needs to be linked back to the originating guest appointment.
/// Consumed by the Scheduling module to update the appointment's PatientId.
/// </summary>
public sealed record PatientLinkedToAppointmentEvent(
    Guid PatientId,
    string PatientName,
    Guid AppointmentId);
