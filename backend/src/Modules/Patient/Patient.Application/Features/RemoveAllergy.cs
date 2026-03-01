using Patient.Application.Interfaces;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record RemoveAllergyCommand(Guid PatientId, Guid AllergyId);

/// <summary>
/// Wolverine handler for removing an allergy from a patient.
/// </summary>
public static class RemoveAllergyHandler
{
    public static async Task<Result> Handle(
        RemoveAllergyCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdWithTrackingAsync(command.PatientId, cancellationToken);
        if (patient is null)
            return Result.Failure(Error.NotFound("Patient", command.PatientId));

        patient.RemoveAllergy(command.AllergyId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
