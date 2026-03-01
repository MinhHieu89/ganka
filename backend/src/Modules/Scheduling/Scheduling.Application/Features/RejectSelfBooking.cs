using FluentValidation;
using Scheduling.Application.Interfaces;
using Scheduling.Contracts.Dtos;
using Shared.Domain;

namespace Scheduling.Application.Features;

/// <summary>
/// Validator for <see cref="RejectSelfBookingCommand"/>.
/// </summary>
public class RejectSelfBookingCommandValidator : AbstractValidator<RejectSelfBookingCommand>
{
    public RejectSelfBookingCommandValidator()
    {
        RuleFor(x => x.SelfBookingRequestId).NotEmpty().WithMessage("Self-booking request ID is required.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Rejection reason is required.");
    }
}

/// <summary>
/// Wolverine handler for rejecting a self-booking request.
/// </summary>
public static class RejectSelfBookingHandler
{
    public static async Task<Result> Handle(
        RejectSelfBookingCommand command,
        ISelfBookingRepository selfBookingRepository,
        IUnitOfWork unitOfWork,
        IValidator<RejectSelfBookingCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(Error.Validation(errors));
        }

        var request = await selfBookingRepository.GetByIdAsync(command.SelfBookingRequestId, ct);
        if (request is null)
            return Result.Failure(Error.NotFound("SelfBookingRequest", command.SelfBookingRequestId));

        try
        {
            request.Reject(command.Reason);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
