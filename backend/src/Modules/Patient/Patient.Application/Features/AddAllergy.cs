using FluentValidation;
using Patient.Application.Interfaces;
using Patient.Domain.Enums;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record AddAllergyCommand(Guid PatientId, string Name, AllergySeverity Severity);

/// <summary>
/// Validator for <see cref="AddAllergyCommand"/>.
/// </summary>
public class AddAllergyCommandValidator : AbstractValidator<AddAllergyCommand>
{
    public AddAllergyCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Allergy name is required.")
            .MaximumLength(200).WithMessage("Allergy name must not exceed 200 characters.");

        RuleFor(x => x.Severity)
            .IsInEnum().WithMessage("Invalid allergy severity.");
    }
}

/// <summary>
/// Wolverine handler for adding an allergy to a patient.
/// </summary>
public static class AddAllergyHandler
{
    public static async Task<Result<Guid>> Handle(
        AddAllergyCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IValidator<AddAllergyCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<Guid>.Failure(Error.Validation(errors));
        }

        var patient = await patientRepository.GetByIdWithTrackingAsync(command.PatientId, cancellationToken);
        if (patient is null)
            return Result<Guid>.Failure(Error.NotFound("Patient", command.PatientId));

        var allergy = patient.AddAllergy(command.Name, command.Severity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return allergy.Id;
    }
}
