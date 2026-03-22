using FluentValidation;
using Patient.Application.Interfaces;
using Patient.Application.Mappers;
using Patient.Contracts.Dtos;
using Patient.Contracts.Enums;
using Shared.Domain;

namespace Patient.Application.Features;

/// <summary>
/// Validator for <see cref="RegisterPatientCommand"/>.
/// Medical type requires DOB and Gender; WalkIn requires only name and phone.
/// </summary>
public class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^0\d{9,10}$").WithMessage("Phone must be a valid Vietnamese phone number (e.g., 0901234567).");

        RuleFor(x => x.DateOfBirth)
            .NotNull().When(x => x.PatientType == PatientType.Medical)
            .WithMessage("Date of birth is required for Medical patients.");

        RuleFor(x => x.Gender)
            .NotNull().When(x => x.PatientType == PatientType.Medical)
            .WithMessage("Gender is required for Medical patients.");

        RuleForEach(x => x.Allergies).ChildRules(allergy =>
        {
            allergy.RuleFor(a => a.Name)
                .NotEmpty().WithMessage("Allergy name is required.")
                .MaximumLength(200).WithMessage("Allergy name must not exceed 200 characters.");

            allergy.RuleFor(a => a.Severity)
                .IsInEnum().WithMessage("Invalid allergy severity.");
        });
    }
}

/// <summary>
/// Wolverine handler for <see cref="RegisterPatientCommand"/>.
/// Registers a new patient with auto-generated GK-YYYY-NNNN code.
/// </summary>
public static class RegisterPatientHandler
{
    public static async Task<Result<Guid>> Handle(
        RegisterPatientCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IValidator<RegisterPatientCommand> validator,
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

        var phoneExists = await patientRepository.PhoneExistsAsync(command.Phone, cancellationToken);
        if (phoneExists)
            return Result<Guid>.Failure(Error.ValidationWithDetails(
                new Dictionary<string, string[]>
                {
                    ["Phone"] = ["A patient with this phone number already exists."]
                }));

        // Use default branch for now
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var patient = Domain.Entities.Patient.Create(
            command.FullName,
            command.Phone,
            command.PatientType.ToDomainEnum(),
            branchId,
            command.DateOfBirth,
            command.Gender?.ToDomainEnum(),
            command.Address,
            command.Cccd);

        // Add allergies if provided
        if (command.Allergies is { Count: > 0 })
        {
            foreach (var allergy in command.Allergies)
            {
                patient.AddAllergy(allergy.Name, allergy.Severity.ToDomainEnum());
            }
        }

        patientRepository.Add(patient);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate patient code using year-scoped sequence
        var currentYear = DateTime.UtcNow.Year;
        var maxSequence = await patientRepository.GetMaxSequenceNumberForYearAsync(currentYear, cancellationToken);
        var nextSequence = maxSequence + 1;

        patient.SetSequence(currentYear, nextSequence);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.Id;
    }
}
