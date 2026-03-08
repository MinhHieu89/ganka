using FluentValidation;
using Shared.Application;
using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Entities;

namespace Treatment.Application.Features;

/// <summary>
/// Command to modify a treatment package mid-course (TRT-07).
/// Creates a version snapshot before applying changes.
/// </summary>
public sealed record ModifyTreatmentPackageCommand(
    Guid PackageId,
    int? TotalSessions,
    string? ParametersJson,
    int? MinIntervalDays,
    string Reason);

/// <summary>
/// Validator for <see cref="ModifyTreatmentPackageCommand"/>.
/// </summary>
public class ModifyTreatmentPackageCommandValidator : AbstractValidator<ModifyTreatmentPackageCommand>
{
    public ModifyTreatmentPackageCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Modification reason is required.");
        RuleFor(x => x.TotalSessions)
            .InclusiveBetween(1, 6)
            .When(x => x.TotalSessions.HasValue)
            .WithMessage("Total sessions must be between 1 and 6.");
    }
}

/// <summary>
/// Wolverine handler for <see cref="ModifyTreatmentPackageCommand"/>.
/// Modifies an active or paused treatment package, creating a ProtocolVersion snapshot
/// of previous and current state for audit trail (TRT-07).
/// </summary>
public static class ModifyTreatmentPackageHandler
{
    public static async Task<Result<TreatmentPackageDto>> Handle(
        ModifyTreatmentPackageCommand command,
        ITreatmentPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IValidator<ModifyTreatmentPackageCommand> validator,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<TreatmentPackageDto>.Failure(Error.ValidationWithDetails(errors));
        }

        var package = await packageRepository.GetByIdAsync(command.PackageId, cancellationToken);
        if (package is null)
            return Result<TreatmentPackageDto>.Failure(
                Error.NotFound("TreatmentPackage", command.PackageId));

        // Build change description by comparing old vs new values
        var changeDescription = BuildChangeDescription(
            package, command.TotalSessions, command.ParametersJson, command.MinIntervalDays);

        try
        {
            package.Modify(
                totalSessions: command.TotalSessions,
                parametersJson: command.ParametersJson,
                minIntervalDays: command.MinIntervalDays,
                changeDescription: changeDescription,
                changedById: currentUser.UserId,
                reason: command.Reason);
        }
        catch (InvalidOperationException ex)
        {
            return Result<TreatmentPackageDto>.Failure(Error.Validation(ex.Message));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Result<TreatmentPackageDto>.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Load protocol name for the DTO
        return CreateTreatmentPackageHandler.MapToDto(package, "");
    }

    /// <summary>
    /// Builds a human-readable description of changes by comparing old values with new values.
    /// </summary>
    private static string BuildChangeDescription(
        TreatmentPackage package,
        int? newTotalSessions,
        string? newParametersJson,
        int? newMinIntervalDays)
    {
        var changes = new List<string>();

        if (newTotalSessions.HasValue && newTotalSessions.Value != package.TotalSessions)
            changes.Add($"Session count changed from {package.TotalSessions} to {newTotalSessions.Value}");

        if (newParametersJson is not null && newParametersJson != package.ParametersJson)
            changes.Add("Treatment parameters updated");

        if (newMinIntervalDays.HasValue && newMinIntervalDays.Value != package.MinIntervalDays)
            changes.Add($"Minimum interval changed from {package.MinIntervalDays} to {newMinIntervalDays.Value} days");

        return changes.Count > 0 ? string.Join("; ", changes) : "No field changes detected";
    }
}
