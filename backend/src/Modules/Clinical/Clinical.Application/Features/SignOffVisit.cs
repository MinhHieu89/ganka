using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for signing off a visit, making it immutable.
/// After sign-off, all fields become read-only. Corrections require the amendment workflow.
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
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        try
        {
            visit.SignOff(currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
