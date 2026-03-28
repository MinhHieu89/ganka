using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Scheduling.Contracts.Dtos;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler that creates a Visit when a patient checks in for their appointment.
/// Handles the cross-module integration event from the Scheduling module.
/// </summary>
public static class CreateVisitOnCheckInHandler
{
    public static async Task Handle(
        AppointmentCheckedInIntegrationEvent integrationEvent,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var visit = Visit.Create(
            integrationEvent.PatientId,
            integrationEvent.PatientName,
            integrationEvent.DoctorId,
            integrationEvent.DoctorName,
            branchId,
            integrationEvent.HasAllergies,
            integrationEvent.AppointmentId,
            source: VisitSource.Appointment);

        await visitRepository.AddAsync(visit, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
