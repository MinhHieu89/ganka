using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;
using Shared.Application;
using Shared.Domain;
// Auto-advance after sign-off (D-11)

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for signing off a visit, making it immutable.
/// After sign-off, all fields become read-only. Corrections require the amendment workflow.
/// On re-sign after amendment, updates the latest amendment with the actual field-level diff.
/// </summary>
public static class SignOffVisitHandler
{
    public static async Task<Result> Handle(
        SignOffVisitCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdWithDetailsAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        // Capture pre-sign status to detect re-sign after amendment
        var wasAmended = visit.Status == VisitStatus.Amended;

        try
        {
            visit.SignOff(currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        // If re-signing after amendment, update the latest amendment with actual field-level diff
        if (wasAmended && !string.IsNullOrEmpty(command.FieldChangesJson))
        {
            var latestAmendment = visit.Amendments
                .OrderByDescending(a => a.AmendedAt)
                .FirstOrDefault();
            latestAmendment?.UpdateFieldChanges(command.FieldChangesJson);
        }

        // Auto-advance to next workflow stage after sign-off (D-11)
        if (visit.CurrentStage < WorkflowStage.PharmacyOptical)
        {
            var nextStage = (WorkflowStage)((int)visit.CurrentStage + 1);
            visit.AdvanceStage(nextStage);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
