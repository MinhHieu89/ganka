using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Scheduling.Domain.Events;

namespace Scheduling.Application.Features;

/// <summary>
/// Wolverine cascading handler that converts the internal domain event
/// (AppointmentCheckedInEvent) into a cross-module integration event
/// (AppointmentCheckedInIntegrationEvent) for consumption by Clinical module.
/// Loads appointment data to enrich the event with doctor/patient details.
/// </summary>
public static class PublishAppointmentCheckedInIntegrationEventHandler
{
    public static async Task<AppointmentCheckedInIntegrationEvent?> Handle(
        AppointmentCheckedInEvent domainEvent,
        IAppointmentRepository appointmentRepository,
        Patient.Application.Interfaces.IPatientRepository patientRepository,
        CancellationToken ct)
    {
        var appointment = await appointmentRepository.GetByIdAsync(domainEvent.AppointmentId, ct);
        if (appointment is null) return null;

        var hasAllergies = false;
        var patientName = appointment.PatientName;
        var patientId = appointment.PatientId ?? Guid.Empty;

        if (appointment.PatientId.HasValue)
        {
            var patient = await patientRepository.GetByIdAsync(appointment.PatientId.Value, ct);
            if (patient is not null)
            {
                hasAllergies = patient.Allergies.Count > 0;
                patientName = patient.FullName;
            }
        }

        return new AppointmentCheckedInIntegrationEvent(
            AppointmentId: appointment.Id,
            PatientId: patientId,
            PatientName: patientName,
            DoctorId: appointment.DoctorId,
            DoctorName: appointment.DoctorName,
            HasAllergies: hasAllergies,
            CheckedInAt: domainEvent.CheckedInAt,
            Reason: appointment.GuestReason ?? appointment.Notes);
    }
}
