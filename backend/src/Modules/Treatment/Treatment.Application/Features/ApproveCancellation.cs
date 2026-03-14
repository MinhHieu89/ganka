using Auth.Contracts.Queries;
using FluentValidation;
using Shared.Domain;
using Treatment.Application.Interfaces;
using Wolverine;

namespace Treatment.Application.Features;

/// <summary>
/// Command to approve a pending cancellation request with manager PIN verification.
/// Manager provides their ID, PIN, and the deduction percentage to apply.
/// </summary>
public sealed record ApproveCancellationCommand(
    Guid PackageId,
    Guid ManagerId,
    string ManagerPin,
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
        RuleFor(x => x.ManagerPin).NotEmpty().WithMessage("Manager PIN is required.");
        RuleFor(x => x.DeductionPercent)
            .InclusiveBetween(10m, 20m)
            .WithMessage("Deduction percent must be between 10 and 20.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="ApproveCancellationCommand"/>.
/// Verifies manager PIN via cross-module query to Auth module,
/// recalculates refund amount with the manager's chosen deduction percentage,
/// then approves the cancellation request.
/// </summary>
public static class ApproveCancellationHandler
{
    public static async Task<Result> Handle(
        ApproveCancellationCommand command,
        ITreatmentPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus,
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

        // Verify manager PIN via cross-module query to Auth module
        var pinResponse = await messageBus.InvokeAsync<VerifyManagerPinResponse>(
            new VerifyManagerPinQuery(command.ManagerId, command.ManagerPin), cancellationToken);

        if (!pinResponse.IsValid)
            return Result.Failure(Error.Validation("Invalid manager PIN."));

        // Approve the cancellation (domain method validates status and transitions)
        // The manager may adjust the deduction percentage at approval time;
        // the domain will recalculate the refund amount if the override differs
        package.ApproveCancellation(command.ManagerId, null, command.DeductionPercent);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
