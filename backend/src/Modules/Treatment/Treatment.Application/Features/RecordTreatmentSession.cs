using FluentValidation;
using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Enums;

namespace Treatment.Application.Features;

/// <summary>
/// Command to record a completed treatment session within a package.
/// Captures device parameters, OSDI score (TRT-03), clinical notes,
/// consumables (TRT-11), and optional interval override reason (TRT-05).
/// </summary>
public sealed record RecordTreatmentSessionCommand(
    Guid PackageId,
    string ParametersJson,
    decimal? OsdiScore,
    string? OsdiSeverity,
    string? ClinicalNotes,
    Guid PerformedById,
    Guid? VisitId,
    DateTime? ScheduledAt,
    string? IntervalOverrideReason,
    List<RecordTreatmentSessionCommand.ConsumableInput> Consumables)
{
    /// <summary>
    /// Input DTO for a consumable item used during the session.
    /// </summary>
    public sealed record ConsumableInput(Guid ConsumableItemId, string ConsumableName, int Quantity);
}

/// <summary>
/// Response from recording a treatment session.
/// Contains the created session DTO and an optional interval warning.
/// </summary>
public sealed record RecordSessionResponse(
    TreatmentSessionDto Session,
    IntervalWarning? Warning);

/// <summary>
/// Warning returned when a session is recorded before the minimum interval has passed.
/// Soft enforcement per TRT-05: session is still recorded but caller is informed.
/// </summary>
public sealed record IntervalWarning(int DaysSinceLast, int MinIntervalDays);

/// <summary>
/// Validator for <see cref="RecordTreatmentSessionCommand"/>.
/// Ensures PackageId and ParametersJson are provided.
/// </summary>
public class RecordTreatmentSessionCommandValidator : AbstractValidator<RecordTreatmentSessionCommand>
{
    public RecordTreatmentSessionCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty().WithMessage("Package ID is required.");
        RuleFor(x => x.ParametersJson).NotEmpty().WithMessage("Parameters JSON is required.");
    }
}

/// <summary>
/// Wolverine static handler for recording a treatment session.
/// Loads the package, checks minimum interval (soft warning), delegates to domain RecordSession,
/// persists via UnitOfWork, and returns the session DTO with optional interval warning.
/// Auto-completion (TRT-04) and domain events (TRT-11) are handled by the domain method.
/// </summary>
public static class RecordTreatmentSessionHandler
{
    public static async Task<Result<RecordSessionResponse>> Handle(
        RecordTreatmentSessionCommand command,
        ITreatmentPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IValidator<RecordTreatmentSessionCommand> validator,
        CancellationToken ct)
    {
        // Validate command
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<RecordSessionResponse>.Failure(Error.ValidationWithDetails(errors));
        }

        // Load package with sessions
        var package = await packageRepository.GetByIdAsync(command.PackageId, ct);
        if (package is null)
            return Result<RecordSessionResponse>.Failure(
                Error.NotFound("TreatmentPackage", command.PackageId));

        // Check if package is active (domain will throw, but we return a clean Result)
        if (package.Status != PackageStatus.Active)
            return Result<RecordSessionResponse>.Failure(
                Error.Validation(
                    $"Cannot record session on a package in '{package.Status}' status. Package must be Active."));

        // Check interval warning (TRT-05: soft enforcement -- warn but don't block)
        IntervalWarning? warning = null;
        var completedSessions = package.Sessions
            .Where(s => s.Status == SessionStatus.Completed && s.CompletedAt.HasValue)
            .ToList();

        if (completedSessions.Count > 0)
        {
            var lastCompleted = completedSessions
                .OrderByDescending(s => s.CompletedAt)
                .First();

            var daysSinceLast = (int)(DateTime.UtcNow - lastCompleted.CompletedAt!.Value).TotalDays;

            if (daysSinceLast < package.MinIntervalDays)
            {
                warning = new IntervalWarning(daysSinceLast, package.MinIntervalDays);
            }
        }

        // Convert consumables to domain format
        var consumables = command.Consumables
            .Select(c => (c.ConsumableItemId, c.ConsumableName, c.Quantity))
            .ToList();

        // Record session via domain method (handles status, events, auto-completion)
        var session = package.RecordSession(
            parametersJson: command.ParametersJson,
            osdiScore: command.OsdiScore,
            osdiSeverity: command.OsdiSeverity,
            clinicalNotes: command.ClinicalNotes,
            performedById: command.PerformedById,
            visitId: command.VisitId,
            scheduledAt: command.ScheduledAt,
            intervalOverrideReason: command.IntervalOverrideReason,
            consumables: consumables);

        // Persist (package is tracked via GetByIdAsync)
        await unitOfWork.SaveChangesAsync(ct);

        // Map to DTO
        var sessionDto = MapSessionToDto(session);

        return new RecordSessionResponse(sessionDto, warning);
    }

    /// <summary>
    /// Maps a TreatmentSession domain entity to a TreatmentSessionDto.
    /// Reusable across handlers that need to return session data.
    /// </summary>
    internal static TreatmentSessionDto MapSessionToDto(
        Treatment.Domain.Entities.TreatmentSession session,
        string? performedByName = null)
    {
        return new TreatmentSessionDto(
            Id: session.Id,
            SessionNumber: session.SessionNumber,
            Status: session.Status.ToString(),
            ParametersJson: session.ParametersJson,
            OsdiScore: session.OsdiScore,
            OsdiSeverity: session.OsdiSeverity,
            ClinicalNotes: session.ClinicalNotes,
            PerformedById: session.PerformedById,
            PerformedByName: performedByName,
            VisitId: session.VisitId,
            ScheduledAt: session.ScheduledAt,
            CompletedAt: session.CompletedAt,
            CreatedAt: session.CreatedAt,
            IntervalOverrideReason: session.IntervalOverrideReason,
            Consumables: session.Consumables
                .Select(c => new SessionConsumableDto(
                    c.Id, c.ConsumableItemId, c.ConsumableName, c.Quantity))
                .ToList());
    }
}
