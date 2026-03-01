using Patient.Application.Interfaces;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record RemoveAllergyCommand(Guid PatientId, Guid AllergyId);

/// <summary>
/// Wolverine handler for removing an allergy from a patient.
/// Uses direct repository deletion to avoid concurrency token conflicts
/// when modifying the Patient aggregate root for child entity changes.
/// </summary>
public static class RemoveAllergyHandler
{
    public static async Task<Result> Handle(
        RemoveAllergyCommand command,
        IPatientRepository patientRepository,
        IAllergyRepository allergyRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var patientExists = await patientRepository.ExistsAsync(command.PatientId, cancellationToken);
        if (!patientExists)
            return Result.Failure(Error.NotFound("Patient", command.PatientId));

        var allergy = await allergyRepository.GetByIdAsync(command.AllergyId, cancellationToken);
        if (allergy is null || allergy.PatientId != command.PatientId)
            return Result.Failure(Error.NotFound("Allergy", command.AllergyId));

        allergyRepository.Remove(allergy);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
