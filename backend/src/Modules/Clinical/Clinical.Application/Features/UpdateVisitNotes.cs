using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for updating examination notes on a visit.
/// Notes are free-text and can be updated while the visit is editable.
/// </summary>
public static class UpdateVisitNotesHandler
{
    public static async Task<Result> Handle(
        UpdateVisitNotesCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        try
        {
            visit.UpdateNotes(command.Notes);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
