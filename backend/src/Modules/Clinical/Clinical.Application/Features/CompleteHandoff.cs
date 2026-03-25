using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for completing the glasses handoff to patient.
/// Creates a HandoffChecklist record. Completes the glasses track.
/// If all tracks are complete, advances to Done.
/// </summary>
public static class CompleteHandoffHandler
{
    public static async Task<Result> Handle(
        CompleteHandoffCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        if (visit.CurrentStage != WorkflowStage.ReturnGlasses)
            return Result.Failure(Error.Validation("Handoff can only be completed at ReturnGlasses stage."));

        try
        {
            var checklist = HandoffChecklist.Create(
                visit.Id,
                command.PrescriptionVerified,
                command.FrameCorrect,
                command.PatientConfirmedFit,
                currentUser.UserId,
                currentUser.Email);

            visit.AddHandoffChecklist(checklist);
            visit.CompleteGlassesTrack();

            if (visit.IsComplete)
            {
                visit.AdvanceStage(WorkflowStage.Done);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
