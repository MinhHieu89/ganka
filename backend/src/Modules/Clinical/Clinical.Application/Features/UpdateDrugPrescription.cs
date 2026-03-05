using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using FluentValidation;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Validator for <see cref="UpdateDrugPrescriptionCommand"/>.
/// </summary>
public class UpdateDrugPrescriptionCommandValidator : AbstractValidator<UpdateDrugPrescriptionCommand>
{
    public UpdateDrugPrescriptionCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");
        RuleFor(x => x.PrescriptionId).NotEmpty().WithMessage("Prescription ID is required.");
    }
}

/// <summary>
/// Wolverine handler for updating notes (Loi dan) on an existing drug prescription.
/// </summary>
public static class UpdateDrugPrescriptionHandler
{
    public static async Task<Result> Handle(
        UpdateDrugPrescriptionCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateDrugPrescriptionCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure(Error.ValidationWithDetails(errors));
        }

        var visit = await visitRepository.GetByIdWithDetailsAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        var prescription = visit.DrugPrescriptions.FirstOrDefault(p => p.Id == command.PrescriptionId);
        if (prescription is null)
            return Result.Failure(Error.NotFound("DrugPrescription", command.PrescriptionId));

        prescription.UpdateNotes(command.Notes);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
