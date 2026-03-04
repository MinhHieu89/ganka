using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using FluentValidation;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Validator for <see cref="AmendVisitCommand"/>.
/// </summary>
public class AmendVisitCommandValidator : AbstractValidator<AmendVisitCommand>
{
    public AmendVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Amendment reason is required.");
        RuleFor(x => x.FieldChangesJson).NotEmpty().WithMessage("Field changes are required.");
    }
}

/// <summary>
/// Wolverine handler for amending a signed visit.
/// Captures field-level diff, requires reason, transitions visit to Amended status.
/// </summary>
public static class AmendVisitHandler
{
    public static async Task<Result> Handle(
        AmendVisitCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<AmendVisitCommand> validator,
        ICurrentUser currentUser,
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

        var amendment = VisitAmendment.Create(
            visit.Id,
            currentUser.UserId,
            currentUser.Email,
            command.Reason,
            command.FieldChangesJson);

        try
        {
            visit.StartAmendment(amendment);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
