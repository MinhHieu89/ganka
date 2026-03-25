using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for requesting imaging during DoctorExam.
/// Creates an ImagingRequest with services and advances to Imaging stage.
/// </summary>
public static class RequestImagingHandler
{
    public static async Task<Result> Handle(
        RequestImagingCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        if (command.Services is null || command.Services.Count == 0)
            return Result.Failure(Error.Validation("At least one imaging service must be specified."));

        try
        {
            visit.RequestImaging(currentUser.UserId, command.Note, command.Services);
            visit.AdvanceStage(WorkflowStage.Imaging);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
