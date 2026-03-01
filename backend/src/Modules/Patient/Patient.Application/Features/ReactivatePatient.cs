using Patient.Application.Interfaces;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record ReactivatePatientCommand(Guid PatientId);

/// <summary>
/// Wolverine handler for reactivating a previously deactivated patient.
/// </summary>
public static class ReactivatePatientHandler
{
    public static async Task<Result> Handle(
        ReactivatePatientCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdWithTrackingAsync(command.PatientId, cancellationToken);
        if (patient is null)
            return Result.Failure(Error.NotFound("Patient", command.PatientId));

        patient.Reactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
