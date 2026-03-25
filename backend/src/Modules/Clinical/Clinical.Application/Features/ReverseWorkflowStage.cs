using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for reversing a visit to an earlier workflow stage (D-07/D-09).
/// Validates allowed transitions via the domain method and requires a mandatory reason.
/// </summary>
public static class ReverseWorkflowStageHandler
{
    public static async Task<Result> Handle(
        ReverseWorkflowStageCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        var targetStage = (WorkflowStage)command.TargetStage;

        try
        {
            visit.ReverseStage(targetStage, command.Reason);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
