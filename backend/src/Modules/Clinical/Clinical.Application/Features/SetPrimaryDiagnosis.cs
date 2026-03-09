using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for setting a diagnosis as primary on a visit.
/// Swaps the primary role: target becomes Primary, previous Primary becomes Secondary.
/// Requires the visit to be editable (not signed).
/// </summary>
public static class SetPrimaryDiagnosisHandler
{
    public static async Task<Result> Handle(
        SetPrimaryDiagnosisCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdWithDetailsAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        try
        {
            visit.SetPrimaryDiagnosis(command.DiagnosisId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
