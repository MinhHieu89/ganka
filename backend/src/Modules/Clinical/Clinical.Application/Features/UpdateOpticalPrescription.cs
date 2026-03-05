using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;
using FluentValidation;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Validator for <see cref="UpdateOpticalPrescriptionCommand"/>.
/// VisitId and PrescriptionId are required. Refraction fields are nullable.
/// </summary>
public class UpdateOpticalPrescriptionCommandValidator : AbstractValidator<UpdateOpticalPrescriptionCommand>
{
    public UpdateOpticalPrescriptionCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");
        RuleFor(x => x.PrescriptionId).NotEmpty().WithMessage("Prescription ID is required.");
        RuleFor(x => x.LensType).Must(lt => Enum.IsDefined(typeof(LensType), lt))
            .WithMessage("Lens type must be a valid value (0=SingleVision, 1=Bifocal, 2=Progressive, 3=Reading).");
    }
}

/// <summary>
/// Wolverine handler for updating an existing optical prescription on a visit.
/// Finds the prescription by ID within the visit's collection and calls Update().
/// </summary>
public static class UpdateOpticalPrescriptionHandler
{
    public static async Task<Result> Handle(
        UpdateOpticalPrescriptionCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateOpticalPrescriptionCommand> validator,
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

        var prescription = visit.OpticalPrescriptions.FirstOrDefault(p => p.Id == command.PrescriptionId);
        if (prescription is null)
            return Result.Failure(Error.NotFound("OpticalPrescription", command.PrescriptionId));

        var lensType = (LensType)command.LensType;

        try
        {
            prescription.Update(
                lensType,
                command.OdSph, command.OdCyl, command.OdAxis, command.OdAdd,
                command.OsSph, command.OsCyl, command.OsAxis, command.OsAdd,
                command.FarPd, command.NearPd,
                command.NearOdSph, command.NearOdCyl, command.NearOdAxis,
                command.NearOsSph, command.NearOsCyl, command.NearOsAxis,
                command.Notes);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
