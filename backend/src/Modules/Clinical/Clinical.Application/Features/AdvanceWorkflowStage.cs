using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for advancing a visit through workflow stages.
/// Moves the visit to the specified new stage (must be forward progression).
/// </summary>
public static class AdvanceWorkflowStageHandler
{
    public static async Task<Result> Handle(
        AdvanceWorkflowStageCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        var newStage = (WorkflowStage)command.NewStage;

        try
        {
            visit.AdvanceStage(newStage);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
