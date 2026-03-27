using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Patient.Application.Interfaces;
using Patient.Application.Mappers;
using Patient.Contracts.Dtos;
using Patient.Contracts.Enums;
using Shared.Domain;

namespace Patient.Application.Features;

/// <summary>
/// Command to update an existing patient with intake form fields.
/// Used during receptionist check-in to complete patient profile.
/// </summary>
public sealed record UpdatePatientFromIntakeCommand(
    Guid PatientId,
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
/// Validator for <see cref="UpdatePatientFromIntakeCommand"/>.
/// </summary>
public class UpdatePatientFromIntakeCommandValidator : AbstractValidator<UpdatePatientFromIntakeCommand>
{
    public UpdatePatientFromIntakeCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^0\d{9,10}$").WithMessage("Phone must be a valid Vietnamese phone number.");

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
/// Wolverine handler for updating a patient from the intake form.
/// Updates all demographic + intake fields, syncs allergies.
/// </summary>
public static class UpdatePatientFromIntakeHandler
{
    public static async Task<Result> Handle(
        UpdatePatientFromIntakeCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdatePatientFromIntakeCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure(Error.ValidationWithDetails(errors));
        }

        var patient = await patientRepository.GetByIdWithTrackingAsync(command.PatientId, cancellationToken);
        if (patient is null)
            return Result.Failure(Error.NotFound("Patient", command.PatientId));

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
            command.WorkEnvironment,
            command.ContactLensUsage,
            command.LifestyleNotes);

        // Sync allergies: remove existing, add new
        var existingAllergyIds = patient.Allergies.Select(a => a.Id).ToList();
        foreach (var id in existingAllergyIds)
        {
            patient.RemoveAllergy(id);
        }

        if (command.Allergies is { Count: > 0 })
        {
            foreach (var allergy in command.Allergies)
            {
                patient.AddAllergy(allergy.Name, allergy.Severity.ToDomainEnum());
            }
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(Error.Conflict("Patient record was modified by another user. Please refresh and try again."));
        }

        return Result.Success();
    }
}
