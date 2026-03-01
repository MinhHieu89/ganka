using Patient.Application.Interfaces;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record DeactivatePatientCommand(Guid PatientId);

/// <summary>
/// Wolverine handler for soft-deleting (deactivating) a patient.
/// </summary>
public static class DeactivatePatientHandler
{
    public static async Task<Result> Handle(
        DeactivatePatientCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdWithTrackingAsync(command.PatientId, cancellationToken);
        if (patient is null)
            return Result.Failure(Error.NotFound("Patient", command.PatientId));

        patient.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
