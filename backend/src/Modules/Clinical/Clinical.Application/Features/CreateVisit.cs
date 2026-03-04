using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Validator for <see cref="CreateVisitCommand"/>.
/// </summary>
public class CreateVisitCommandValidator : AbstractValidator<CreateVisitCommand>
{
    public CreateVisitCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient is required.");
        RuleFor(x => x.PatientName).NotEmpty().WithMessage("Patient name is required.");
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage("Doctor is required.");
        RuleFor(x => x.DoctorName).NotEmpty().WithMessage("Doctor name is required.");
    }
}

/// <summary>
/// Wolverine handler for creating a new clinical visit.
/// Creates a Visit linked to patient and doctor with optional appointment (check-in vs walk-in).
/// </summary>
public static class CreateVisitHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateVisitCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateVisitCommand> validator,
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

        var branchId = new BranchId(currentUser.BranchId);

        var visit = Visit.Create(
            command.PatientId,
            command.PatientName,
            command.DoctorId,
            command.DoctorName,
            branchId,
            command.HasAllergies,
            command.AppointmentId);

        await visitRepository.AddAsync(visit, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return visit.Id;
    }
}
