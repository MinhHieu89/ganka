using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Shared.Application;
using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Command to start a new barcode-based stocktaking session.
/// Fails if another session is already in progress.
/// </summary>
public sealed record StartStocktakingSessionCommand(string Name, string? Notes);

/// <summary>
/// Wolverine static handler for starting a new stocktaking session.
/// Only one InProgress session is allowed at a time per branch.
/// </summary>
public static class StartStocktakingSessionHandler
{
    public static async Task<Result<Guid>> Handle(
        StartStocktakingSessionCommand command,
        IStocktakingRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var existing = await repository.GetCurrentSessionAsync(ct);
        if (existing is not null)
            return Result.Failure<Guid>(Error.Validation(
                "A stocktaking session is already in progress. Complete or cancel it before starting a new one."));

        var session = StocktakingSession.Create(
            name: command.Name,
            startedById: currentUser.UserId,
            branchId: new BranchId(currentUser.BranchId));

        repository.Add(session);
        await unitOfWork.SaveChangesAsync(ct);

        return session.Id;
    }
}
