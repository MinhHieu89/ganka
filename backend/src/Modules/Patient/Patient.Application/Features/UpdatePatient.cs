using FluentValidation;
using Patient.Application.Interfaces;
using Patient.Contracts.Dtos;
using Shared.Domain;

namespace Patient.Application.Features;

/// <summary>
/// Validator for <see cref="UpdatePatientCommand"/>.
/// </summary>
public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("Patient ID is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^0\d{9,10}$").WithMessage("Phone must be a valid Vietnamese phone number.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="UpdatePatientCommand"/>.
/// </summary>
public static class UpdatePatientHandler
{
    public static async Task<Result> Handle(
        UpdatePatientCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdatePatientCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(Error.Validation(errors));
        }

        var patient = await patientRepository.GetByIdWithTrackingAsync(command.PatientId, cancellationToken);
        if (patient is null)
            return Result.Failure(Error.NotFound("Patient", command.PatientId));

        patient.Update(
            command.FullName,
            command.Phone,
            command.DateOfBirth,
            command.Gender,
            command.Address,
            command.Cccd);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
