using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for skipping the RefractionVA stage.
/// Creates a StageSkip audit record, sets RefractionSkipped flag, and advances to DoctorExam.
/// </summary>
public static class SkipRefractionHandler
{
    public static async Task<Result> Handle(
        SkipRefractionCommand command,
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
            var reason = (SkipReason)command.Reason;
            visit.SkipRefraction(reason, command.FreeTextNote, currentUser.UserId, currentUser.Email);
            visit.AdvanceStage(WorkflowStage.DoctorExam);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
