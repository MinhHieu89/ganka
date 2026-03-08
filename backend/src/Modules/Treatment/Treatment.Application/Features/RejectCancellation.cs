using FluentValidation;
using Shared.Domain;
using Treatment.Application.Interfaces;

namespace Treatment.Application.Features;

/// <summary>
/// Command to reject a pending cancellation request.
/// Transitions the package status back to Active.
/// </summary>
public sealed record RejectCancellationCommand(
    Guid PackageId,
    Guid ManagerId,
    string RejectionReason);

/// <summary>
/// Validator for <see cref="RejectCancellationCommand"/>.
/// </summary>
public class RejectCancellationCommandValidator : AbstractValidator<RejectCancellationCommand>
{
    public RejectCancellationCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.ManagerId).NotEmpty();
        RuleFor(x => x.RejectionReason).NotEmpty().WithMessage("Rejection reason is required.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="RejectCancellationCommand"/>.
/// Validates the package is in PendingCancellation status,
/// then rejects the cancellation and transitions back to Active.
/// </summary>
public static class RejectCancellationHandler
{
    public static async Task<Result> Handle(
        RejectCancellationCommand command,
        ITreatmentPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IValidator<RejectCancellationCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure(Error.ValidationWithDetails(errors));
        }

        var package = await packageRepository.GetByIdAsync(command.PackageId, cancellationToken);
        if (package is null)
            return Result.Failure(Error.NotFound("TreatmentPackage", command.PackageId));

        // Check package is in PendingCancellation status
        if (package.Status != Treatment.Domain.Enums.PackageStatus.PendingCancellation)
            return Result.Failure(Error.Validation(
                $"Cannot reject cancellation for a package in '{package.Status}' status."));

        // Domain method rejects the cancellation and transitions status back to Active
        package.RejectCancellation(command.ManagerId, command.RejectionReason);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
