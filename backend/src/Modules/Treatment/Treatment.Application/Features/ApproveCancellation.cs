using FluentValidation;
using Shared.Domain;
using Treatment.Application.Interfaces;

namespace Treatment.Application.Features;

/// <summary>
/// Command to approve a pending cancellation request.
/// Manager provides their ID and the deduction percentage to apply.
/// </summary>
public sealed record ApproveCancellationCommand(
    Guid PackageId,
    Guid ManagerId,
    decimal DeductionPercent);

/// <summary>
/// Validator for <see cref="ApproveCancellationCommand"/>.
/// </summary>
public class ApproveCancellationCommandValidator : AbstractValidator<ApproveCancellationCommand>
{
    public ApproveCancellationCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.ManagerId).NotEmpty();
        RuleFor(x => x.DeductionPercent)
            .InclusiveBetween(0m, 100m)
            .WithMessage("Deduction percent must be between 0 and 100.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="ApproveCancellationCommand"/>.
/// Recalculates refund amount with the manager's chosen deduction percentage,
/// then approves the cancellation request.
/// </summary>
public static class ApproveCancellationHandler
{
    public static async Task<Result> Handle(
        ApproveCancellationCommand command,
        ITreatmentPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IValidator<ApproveCancellationCommand> validator,
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
                $"Cannot approve cancellation for a package in '{package.Status}' status."));

        // Approve the cancellation (domain method validates status and transitions)
        package.ApproveCancellation(command.ManagerId, null, command.DeductionPercent);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
