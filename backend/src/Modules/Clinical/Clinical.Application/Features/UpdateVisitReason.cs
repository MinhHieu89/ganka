using Clinical.Application.Interfaces;
using Shared.Domain;

namespace Clinical.Application.Features;

public sealed record UpdateVisitReasonCommand(Guid VisitId, string? Reason);

public static class UpdateVisitReasonHandler
{
    public static async Task<Result> Handle(
        UpdateVisitReasonCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        try
        {
            visit.UpdateReason(command.Reason);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
