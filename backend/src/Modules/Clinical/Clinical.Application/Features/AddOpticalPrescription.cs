using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentValidation;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Validator for <see cref="AddOpticalPrescriptionCommand"/>.
/// Only VisitId is strictly required -- all refraction fields are nullable.
/// </summary>
public class AddOpticalPrescriptionCommandValidator : AbstractValidator<AddOpticalPrescriptionCommand>
{
    public AddOpticalPrescriptionCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");
        RuleFor(x => x.LensType).Must(lt => Enum.IsDefined(typeof(LensType), lt))
            .WithMessage("Lens type must be a valid value (0=SingleVision, 1=Bifocal, 2=Progressive, 3=Reading).");
    }
}

/// <summary>
/// Wolverine handler for adding/replacing an optical prescription on a visit.
/// Only one optical Rx per visit -- SetOpticalPrescription clears existing before adding.
/// Returns the ID of the newly created optical prescription.
/// </summary>
public static class AddOpticalPrescriptionHandler
{
    public static async Task<Result<Guid>> Handle(
        AddOpticalPrescriptionCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<AddOpticalPrescriptionCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<Guid>(Error.ValidationWithDetails(errors));
        }

        var visit = await visitRepository.GetByIdWithDetailsAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure<Guid>(Error.NotFound("Visit", command.VisitId));

        var lensType = (LensType)command.LensType;

        var prescription = OpticalPrescription.Create(
            visit.Id, lensType,
            command.OdSph, command.OdCyl, command.OdAxis, command.OdAdd,
            command.OsSph, command.OsCyl, command.OsAxis, command.OsAdd,
            command.FarPd, command.NearPd,
            command.NearOdSph, command.NearOdCyl, command.NearOdAxis,
            command.NearOsSph, command.NearOsCyl, command.NearOsAxis,
            command.Notes);

        try
        {
            // Remove existing optical prescriptions from EF Core change tracker
            var existing = visit.OpticalPrescriptions.ToList();
            if (existing.Count > 0)
                visitRepository.RemoveOpticalPrescriptions(existing);

            // SetOpticalPrescription clears backing field and adds the new one
            visit.SetOpticalPrescription(prescription);
            visitRepository.AddOpticalPrescription(prescription);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Guid>(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(prescription.Id);
    }
}
