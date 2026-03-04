using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentValidation;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Validator for <see cref="UpdateRefractionCommand"/>.
/// Validates ranges: SPH -30..+30, CYL -10..+10, AXIS 1..180, ADD 0.25..4.0, PD 20..80,
/// VA 0.01..2.0, IOP 1..60, AxialLength 15..40. All nullable -- only validated if provided.
/// </summary>
public class UpdateRefractionCommandValidator : AbstractValidator<UpdateRefractionCommand>
{
    public UpdateRefractionCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");

        // SPH range: -30 to +30
        When(x => x.OdSph.HasValue, () => RuleFor(x => x.OdSph!.Value).InclusiveBetween(-30m, 30m).WithMessage("SPH must be between -30 and +30."));
        When(x => x.OsSph.HasValue, () => RuleFor(x => x.OsSph!.Value).InclusiveBetween(-30m, 30m).WithMessage("SPH must be between -30 and +30."));

        // CYL range: -10 to +10
        When(x => x.OdCyl.HasValue, () => RuleFor(x => x.OdCyl!.Value).InclusiveBetween(-10m, 10m).WithMessage("CYL must be between -10 and +10."));
        When(x => x.OsCyl.HasValue, () => RuleFor(x => x.OsCyl!.Value).InclusiveBetween(-10m, 10m).WithMessage("CYL must be between -10 and +10."));

        // AXIS range: 1 to 180
        When(x => x.OdAxis.HasValue, () => RuleFor(x => x.OdAxis!.Value).InclusiveBetween(1m, 180m).WithMessage("AXIS must be between 1 and 180."));
        When(x => x.OsAxis.HasValue, () => RuleFor(x => x.OsAxis!.Value).InclusiveBetween(1m, 180m).WithMessage("AXIS must be between 1 and 180."));

        // ADD range: 0.25 to 4.0
        When(x => x.OdAdd.HasValue, () => RuleFor(x => x.OdAdd!.Value).InclusiveBetween(0.25m, 4.0m).WithMessage("ADD must be between 0.25 and 4.0."));
        When(x => x.OsAdd.HasValue, () => RuleFor(x => x.OsAdd!.Value).InclusiveBetween(0.25m, 4.0m).WithMessage("ADD must be between 0.25 and 4.0."));

        // PD range: 20 to 80
        When(x => x.OdPd.HasValue, () => RuleFor(x => x.OdPd!.Value).InclusiveBetween(20m, 80m).WithMessage("PD must be between 20 and 80."));
        When(x => x.OsPd.HasValue, () => RuleFor(x => x.OsPd!.Value).InclusiveBetween(20m, 80m).WithMessage("PD must be between 20 and 80."));

        // VA range: 0.01 to 2.0
        When(x => x.UcvaOd.HasValue, () => RuleFor(x => x.UcvaOd!.Value).InclusiveBetween(0.01m, 2.0m).WithMessage("VA must be between 0.01 and 2.0."));
        When(x => x.UcvaOs.HasValue, () => RuleFor(x => x.UcvaOs!.Value).InclusiveBetween(0.01m, 2.0m).WithMessage("VA must be between 0.01 and 2.0."));
        When(x => x.BcvaOd.HasValue, () => RuleFor(x => x.BcvaOd!.Value).InclusiveBetween(0.01m, 2.0m).WithMessage("VA must be between 0.01 and 2.0."));
        When(x => x.BcvaOs.HasValue, () => RuleFor(x => x.BcvaOs!.Value).InclusiveBetween(0.01m, 2.0m).WithMessage("VA must be between 0.01 and 2.0."));

        // IOP range: 1 to 60
        When(x => x.IopOd.HasValue, () => RuleFor(x => x.IopOd!.Value).InclusiveBetween(1m, 60m).WithMessage("IOP must be between 1 and 60."));
        When(x => x.IopOs.HasValue, () => RuleFor(x => x.IopOs!.Value).InclusiveBetween(1m, 60m).WithMessage("IOP must be between 1 and 60."));

        // Axial Length range: 15 to 40
        When(x => x.AxialLengthOd.HasValue, () => RuleFor(x => x.AxialLengthOd!.Value).InclusiveBetween(15m, 40m).WithMessage("Axial length must be between 15 and 40."));
        When(x => x.AxialLengthOs.HasValue, () => RuleFor(x => x.AxialLengthOs!.Value).InclusiveBetween(15m, 40m).WithMessage("Axial length must be between 15 and 40."));
    }
}

/// <summary>
/// Wolverine handler for updating refraction data on a visit.
/// Finds or creates a Refraction record of the specified type and updates all fields.
/// </summary>
public static class UpdateRefractionHandler
{
    public static async Task<Result> Handle(
        UpdateRefractionCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateRefractionCommand> validator,
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

        var refractionType = (RefractionType)command.RefractionType;
        var iopMethod = command.IopMethod.HasValue ? (IopMethod?)command.IopMethod.Value : null;

        // Find existing refraction of this type or create a new one
        var existing = visit.Refractions.FirstOrDefault(r => r.Type == refractionType);

        if (existing is not null)
        {
            // Update existing -- but check editability first
            try
            {
                existing.Update(
                    command.OdSph, command.OdCyl, command.OdAxis, command.OdAdd, command.OdPd,
                    command.OsSph, command.OsCyl, command.OsAxis, command.OsAdd, command.OsPd,
                    command.UcvaOd, command.UcvaOs, command.BcvaOd, command.BcvaOs,
                    command.IopOd, command.IopOs, iopMethod,
                    command.AxialLengthOd, command.AxialLengthOs);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(Error.Validation(ex.Message));
            }
        }
        else
        {
            // Create new refraction
            var refraction = Refraction.Create(visit.Id, refractionType);
            refraction.Update(
                command.OdSph, command.OdCyl, command.OdAxis, command.OdAdd, command.OdPd,
                command.OsSph, command.OsCyl, command.OsAxis, command.OsAdd, command.OsPd,
                command.UcvaOd, command.UcvaOs, command.BcvaOd, command.BcvaOs,
                command.IopOd, command.IopOs, iopMethod,
                command.AxialLengthOd, command.AxialLengthOs);

            try
            {
                visit.AddRefraction(refraction);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(Error.Validation(ex.Message));
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
