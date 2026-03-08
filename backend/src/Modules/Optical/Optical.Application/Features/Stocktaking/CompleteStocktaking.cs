using Optical.Application.Interfaces;
using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Command to mark a stocktaking session as complete.
/// </summary>
public sealed record CompleteStocktakingCommand(Guid SessionId, string? Notes);

/// <summary>
/// Wolverine static handler for completing a stocktaking session.
/// Marks the session as Completed and saves. Returns error if session not found or already completed.
/// </summary>
public static class CompleteStocktakingHandler
{
    public static async Task<Result> Handle(
        CompleteStocktakingCommand command,
        IStocktakingRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var session = await repository.GetByIdAsync(command.SessionId, ct);
        if (session is null)
            return Result.Failure(Error.NotFound("StocktakingSession", command.SessionId));

        try
        {
            session.Complete();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
