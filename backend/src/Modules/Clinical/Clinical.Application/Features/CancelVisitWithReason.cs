using Clinical.Application.Interfaces;
using FluentValidation;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Command to cancel a visit with a reason and user tracking.
/// </summary>
public sealed record CancelVisitWithReasonCommand(
    Guid VisitId,
    string Reason,
    Guid CancelledBy);

/// <summary>
/// Validator for <see cref="CancelVisitWithReasonCommand"/>.
/// </summary>
public class CancelVisitWithReasonCommandValidator : AbstractValidator<CancelVisitWithReasonCommand>
{
    public CancelVisitWithReasonCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty().WithMessage("Visit ID is required.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Cancellation reason is required.");
        RuleFor(x => x.CancelledBy).NotEmpty().WithMessage("User ID is required.");
    }
}

/// <summary>
/// Wolverine handler for cancelling a visit with a reason.
/// Uses CancelWithReason to track reason and who cancelled.
/// </summary>
public static class CancelVisitWithReasonHandler
{
    public static async Task<Result> Handle(
        CancelVisitWithReasonCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        IValidator<CancelVisitWithReasonCommand> validator,
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

        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        try
        {
            visit.CancelWithReason(command.Reason, command.CancelledBy);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
