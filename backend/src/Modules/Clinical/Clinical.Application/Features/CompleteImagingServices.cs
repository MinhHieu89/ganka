using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for completing imaging services.
/// Advances the visit from Imaging to DoctorReviewsResults.
/// </summary>
public static class CompleteImagingServicesHandler
{
    public static async Task<Result> Handle(
        CompleteImagingServicesCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        try
        {
            visit.AdvanceStage(WorkflowStage.DoctorReviewsResults);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
