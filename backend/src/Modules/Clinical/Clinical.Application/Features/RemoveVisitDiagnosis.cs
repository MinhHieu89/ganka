using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for removing a diagnosis from a visit.
/// Requires the visit to be editable (not signed).
/// </summary>
public static class RemoveVisitDiagnosisHandler
{
    public static async Task<Result> Handle(
        RemoveVisitDiagnosisCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdWithDetailsAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        try
        {
            visit.RemoveDiagnosis(command.DiagnosisId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
