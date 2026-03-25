using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for completing optical lab processing.
/// Advances the visit from OpticalLab to ReturnGlasses stage.
/// </summary>
public static class CompleteOpticalLabHandler
{
    public static async Task<Result> Handle(
        CompleteOpticalLabCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        if (visit.CurrentStage != WorkflowStage.OpticalLab)
            return Result.Failure(Error.Validation("Optical lab can only be completed at OpticalLab stage."));

        try
        {
            visit.AdvanceStage(WorkflowStage.ReturnGlasses);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
