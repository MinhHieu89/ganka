using FluentValidation;
using Shared.Application;
using Shared.Domain;
using Treatment.Application.Interfaces;

namespace Treatment.Application.Features;

/// <summary>
/// Command to request cancellation of a treatment package.
/// Creates a CancellationRequest and transitions status to PendingCancellation.
/// Deduction percentage is read from the protocol template's CancellationDeductionPercent.
/// </summary>
public sealed record RequestCancellationCommand(
    Guid PackageId,
    string Reason);

/// <summary>
/// Validator for <see cref="RequestCancellationCommand"/>.
/// </summary>
public class RequestCancellationCommandValidator : AbstractValidator<RequestCancellationCommand>
{
    public RequestCancellationCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Cancellation reason is required.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="RequestCancellationCommand"/>.
/// Loads the package and its protocol template, uses template's CancellationDeductionPercent,
/// calls the domain method to request cancellation.
/// </summary>
public static class RequestCancellationHandler
{
    public static async Task<Result> Handle(
        RequestCancellationCommand command,
        ITreatmentPackageRepository packageRepository,
        ITreatmentProtocolRepository protocolRepository,
        IUnitOfWork unitOfWork,
        IValidator<RequestCancellationCommand> validator,
        ICurrentUser currentUser,
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

        // Check if already pending cancellation (domain will throw but we want a Result error)
        if (package.Status == Treatment.Domain.Enums.PackageStatus.PendingCancellation)
            return Result.Failure(Error.Validation(
                "A cancellation request is already pending for this package."));

        // Check if package is in a modifiable status
        if (package.Status != Treatment.Domain.Enums.PackageStatus.Active &&
            package.Status != Treatment.Domain.Enums.PackageStatus.Paused)
            return Result.Failure(Error.Validation(
                $"Cannot request cancellation for a package in '{package.Status}' status."));

        // Load protocol template to get default deduction percentage
        var protocol = await protocolRepository.GetByIdAsync(package.ProtocolTemplateId, cancellationToken);
        if (protocol is null)
            return Result.Failure(Error.NotFound("TreatmentProtocol", package.ProtocolTemplateId));

        var deductionPercent = protocol.CancellationDeductionPercent;

        // Domain method handles status transition and CancellationRequest creation
        package.RequestCancellation(command.Reason, deductionPercent, currentUser.UserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
