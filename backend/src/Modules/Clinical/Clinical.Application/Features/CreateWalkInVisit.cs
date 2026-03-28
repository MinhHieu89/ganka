using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Command to create a walk-in visit for an existing patient.
/// </summary>
public sealed record CreateWalkInVisitCommand(
    Guid PatientId,
    Guid? DoctorId,
    string? DoctorName,
    string? Reason);

/// <summary>
/// Validator for <see cref="CreateWalkInVisitCommand"/>.
/// </summary>
public class CreateWalkInVisitCommandValidator : AbstractValidator<CreateWalkInVisitCommand>
{
    public CreateWalkInVisitCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient is required.");
    }
}

/// <summary>
/// Wolverine handler for creating a walk-in visit.
/// Creates a visit with VisitSource.WalkIn and no AppointmentId.
/// </summary>
public static class CreateWalkInVisitHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateWalkInVisitCommand command,
        IVisitRepository visitRepository,
        Patient.Application.Interfaces.IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateWalkInVisitCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<Guid>.Failure(Error.ValidationWithDetails(errors));
        }

        // Load patient to get name and allergies
        var patient = await patientRepository.GetByIdAsync(command.PatientId, ct);
        if (patient is null)
            return Result<Guid>.Failure(Error.NotFound("Patient", command.PatientId));

        var branchId = new BranchId(currentUser.BranchId);
        var hasAllergies = patient.Allergies.Count > 0;

        var visit = Visit.Create(
            command.PatientId,
            patient.FullName,
            command.DoctorId ?? Guid.Empty,
            command.DoctorName ?? string.Empty,
            branchId,
            hasAllergies,
            appointmentId: null,
            source: VisitSource.WalkIn,
            reason: command.Reason);

        await visitRepository.AddAsync(visit, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return visit.Id;
    }
}
