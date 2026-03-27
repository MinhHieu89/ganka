using FluentValidation;
using Patient.Application.Interfaces;
using Patient.Application.Mappers;
using Patient.Contracts.Dtos;
using Patient.Contracts.Enums;
using Shared.Domain;

namespace Patient.Application.Features;

/// <summary>
/// Command to register a new patient from the receptionist intake form.
/// Includes all intake fields beyond basic registration.
/// </summary>
public sealed record RegisterPatientFromIntakeCommand(
    string FullName,
    string Phone,
    DateTime? DateOfBirth,
    Gender? Gender,
    string? Address,
    string? Cccd,
    string? Email,
    string? Occupation,
    string? OcularHistory,
    string? SystemicHistory,
    string? CurrentMedications,
    decimal? ScreenTimeHours,
    string? WorkEnvironment,
    string? ContactLensUsage,
    string? LifestyleNotes,
    List<AllergyInput>? Allergies);

/// <summary>
/// Validator for <see cref="RegisterPatientFromIntakeCommand"/>.
/// </summary>
public class RegisterPatientFromIntakeCommandValidator : AbstractValidator<RegisterPatientFromIntakeCommand>
{
    public RegisterPatientFromIntakeCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^0\d{9,10}$").WithMessage("Phone must be a valid Vietnamese phone number (e.g., 0901234567).");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth cannot be in the future.");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .When(x => x.Gender.HasValue)
            .WithMessage("Invalid gender value.");

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
/// Wolverine handler for registering a patient from the intake form.
/// Creates patient with all intake fields and generates GK-YYYY-NNNN code.
/// </summary>
public static class RegisterPatientFromIntakeHandler
{
    public static async Task<Result<Guid>> Handle(
        RegisterPatientFromIntakeCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IValidator<RegisterPatientFromIntakeCommand> validator,
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

        // Use default branch
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var patient = Domain.Entities.Patient.Create(
            command.FullName,
            command.Phone,
            Domain.Enums.PatientType.Medical,
            branchId,
            command.DateOfBirth,
            command.Gender?.ToDomainEnum(),
            command.Address,
            command.Cccd);

        // Set intake-specific fields
        patient.UpdateIntake(
            command.FullName,
            command.Phone,
            command.DateOfBirth,
            command.Gender?.ToDomainEnum(),
            command.Address,
            command.Cccd,
            command.Email,
            command.Occupation,
            command.OcularHistory,
            command.SystemicHistory,
            command.CurrentMedications,
            command.ScreenTimeHours,
            Enum.TryParse<Patient.Domain.Enums.WorkEnvironment>(command.WorkEnvironment, true, out var we) ? we : null,
            Enum.TryParse<Patient.Domain.Enums.ContactLensUsage>(command.ContactLensUsage, true, out var cl) ? cl : null,
            command.LifestyleNotes);

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

        // Generate patient code
        var currentYear = DateTime.UtcNow.Year;
        var maxSequence = await patientRepository.GetMaxSequenceNumberForYearAsync(currentYear, cancellationToken);
        var nextSequence = maxSequence + 1;
        patient.SetSequence(currentYear, nextSequence);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.Id;
    }
}
