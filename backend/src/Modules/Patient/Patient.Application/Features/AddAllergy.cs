using FluentValidation;
using Patient.Application.Interfaces;
using Patient.Domain.Entities;
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
/// Uses direct repository insertion to avoid concurrency token conflicts
/// when modifying the Patient aggregate root for child entity changes.
/// </summary>
public static class AddAllergyHandler
{
    public static async Task<Result<Guid>> Handle(
        AddAllergyCommand command,
        IPatientRepository patientRepository,
        IAllergyRepository allergyRepository,
        IUnitOfWork unitOfWork,
        IValidator<AddAllergyCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<Guid>.Failure(Error.ValidationWithDetails(errors));
        }

        var patientExists = await patientRepository.ExistsAsync(command.PatientId, cancellationToken);
        if (!patientExists)
            return Result<Guid>.Failure(Error.NotFound("Patient", command.PatientId));

        var allergy = Allergy.Create(command.Name, command.Severity, command.PatientId);
        allergyRepository.Add(allergy);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return allergy.Id;
    }
}
