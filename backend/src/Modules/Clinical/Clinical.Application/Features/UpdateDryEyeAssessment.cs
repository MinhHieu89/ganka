using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using FluentValidation;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Validator for <see cref="UpdateDryEyeAssessmentCommand"/>.
/// Validates ranges: MeibomianGrading 0-3, Staining 0-5, TBUT >= 0, Schirmer >= 0, TearMeniscus >= 0.
/// All nullable -- only validated if provided.
/// </summary>
public class UpdateDryEyeAssessmentCommandValidator : AbstractValidator<UpdateDryEyeAssessmentCommand>
{
    public UpdateDryEyeAssessmentCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");

        // TBUT >= 0
        When(x => x.OdTbut.HasValue, () => RuleFor(x => x.OdTbut!.Value).GreaterThanOrEqualTo(0).WithMessage("TBUT must be >= 0."));
        When(x => x.OsTbut.HasValue, () => RuleFor(x => x.OsTbut!.Value).GreaterThanOrEqualTo(0).WithMessage("TBUT must be >= 0."));

        // Schirmer >= 0
        When(x => x.OdSchirmer.HasValue, () => RuleFor(x => x.OdSchirmer!.Value).GreaterThanOrEqualTo(0).WithMessage("Schirmer must be >= 0."));
        When(x => x.OsSchirmer.HasValue, () => RuleFor(x => x.OsSchirmer!.Value).GreaterThanOrEqualTo(0).WithMessage("Schirmer must be >= 0."));

        // Meibomian grading 0-3
        When(x => x.OdMeibomianGrading.HasValue, () => RuleFor(x => x.OdMeibomianGrading!.Value).InclusiveBetween(0, 3).WithMessage("Meibomian grading must be between 0 and 3."));
        When(x => x.OsMeibomianGrading.HasValue, () => RuleFor(x => x.OsMeibomianGrading!.Value).InclusiveBetween(0, 3).WithMessage("Meibomian grading must be between 0 and 3."));

        // Tear meniscus >= 0
        When(x => x.OdTearMeniscus.HasValue, () => RuleFor(x => x.OdTearMeniscus!.Value).GreaterThanOrEqualTo(0).WithMessage("Tear meniscus must be >= 0."));
        When(x => x.OsTearMeniscus.HasValue, () => RuleFor(x => x.OsTearMeniscus!.Value).GreaterThanOrEqualTo(0).WithMessage("Tear meniscus must be >= 0."));

        // Staining 0-5
        When(x => x.OdStaining.HasValue, () => RuleFor(x => x.OdStaining!.Value).InclusiveBetween(0, 5).WithMessage("Staining must be between 0 and 5."));
        When(x => x.OsStaining.HasValue, () => RuleFor(x => x.OsStaining!.Value).InclusiveBetween(0, 5).WithMessage("Staining must be between 0 and 5."));
    }
}

/// <summary>
/// Wolverine handler for creating/updating dry eye assessment on a visit.
/// Finds or creates a DryEyeAssessment and updates all per-eye fields.
/// Follows the same find-or-create pattern as UpdateVisitRefraction.
/// </summary>
public static class UpdateDryEyeAssessmentHandler
{
    public static async Task<Result> Handle(
        UpdateDryEyeAssessmentCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateDryEyeAssessmentCommand> validator,
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

        // Find existing dry eye assessment or create a new one
        var existing = visit.DryEyeAssessments.FirstOrDefault();

        if (existing is not null)
        {
            // Update existing -- but check editability first
            try
            {
                existing.Update(
                    command.OdTbut, command.OsTbut,
                    command.OdSchirmer, command.OsSchirmer,
                    command.OdMeibomianGrading, command.OsMeibomianGrading,
                    command.OdTearMeniscus, command.OsTearMeniscus,
                    command.OdStaining, command.OsStaining);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(Error.Validation(ex.Message));
            }
        }
        else
        {
            // Create new dry eye assessment
            var assessment = DryEyeAssessment.Create(visit.Id);
            assessment.Update(
                command.OdTbut, command.OsTbut,
                command.OdSchirmer, command.OsSchirmer,
                command.OdMeibomianGrading, command.OsMeibomianGrading,
                command.OdTearMeniscus, command.OsTearMeniscus,
                command.OdStaining, command.OsStaining);

            try
            {
                visit.AddDryEyeAssessment(assessment);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(Error.Validation(ex.Message));
            }

            visitRepository.AddDryEyeAssessment(assessment);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
