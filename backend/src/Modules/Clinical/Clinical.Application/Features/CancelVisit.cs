using Clinical.Application.Interfaces;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Command to cancel a clinical visit. Only Draft visits can be cancelled.
/// </summary>
public sealed record CancelVisitCommand(Guid VisitId);

/// <summary>
/// Wolverine handler for cancelling a clinical visit.
/// Loads the visit, calls Cancel() (which guards Draft-only),
/// and saves changes. The Visit.Cancel() method raises VisitCancelledEvent
/// which cascades to VisitCancelledIntegrationEvent for billing invoice voiding.
/// </summary>
public static class CancelVisitHandler
{
    public static async Task<Result> Handle(
        CancelVisitCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        visit.Cancel();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
