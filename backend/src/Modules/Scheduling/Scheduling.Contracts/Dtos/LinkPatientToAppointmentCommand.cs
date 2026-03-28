namespace Scheduling.Contracts.Dtos;

/// <summary>
/// Command to link a newly registered patient to an existing appointment.
/// Invoked synchronously via IMessageBus from the Patient module.
/// </summary>
public sealed record LinkPatientToAppointmentCommand(
    Guid AppointmentId,
    Guid PatientId,
    string PatientName);
