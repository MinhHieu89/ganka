using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Handles LinkPatientToAppointmentCommand.
/// Links a newly registered patient to their guest appointment.
/// Invoked synchronously from the Patient module via IMessageBus.
/// </summary>
public static class LinkPatientToAppointmentHandler
{
    public static async Task Handle(
        LinkPatientToAppointmentCommand command,
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var appointment = await appointmentRepository.GetByIdAsync(command.AppointmentId, ct);
        if (appointment is null) return;

        appointment.LinkPatient(command.PatientId, command.PatientName);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
