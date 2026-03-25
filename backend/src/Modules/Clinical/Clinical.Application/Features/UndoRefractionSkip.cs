using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for undoing a refraction skip.
/// Clears the RefractionSkipped flag and marks the latest StageSkip as undone.
/// </summary>
public static class UndoRefractionSkipHandler
{
    public static async Task<Result> Handle(
        UndoRefractionSkipCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        try
        {
            visit.UndoRefractionSkip();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
