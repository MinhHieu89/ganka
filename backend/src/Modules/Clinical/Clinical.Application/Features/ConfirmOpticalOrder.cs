using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for confirming an optical order at OpticalCenter.
/// Creates an OpticalOrder record with frame/lens details.
/// Advances the visit to Cashier for combined payment.
/// </summary>
public static class ConfirmOpticalOrderHandler
{
    public static async Task<Result> Handle(
        ConfirmOpticalOrderCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByIdAsync(command.VisitId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("Visit", command.VisitId));

        if (visit.CurrentStage != WorkflowStage.OpticalCenter)
            return Result.Failure(Error.Validation("Optical order can only be confirmed at OpticalCenter stage."));

        try
        {
            var order = OpticalOrder.Create(
                visit.Id,
                command.LensType,
                command.FrameCode,
                command.LensCost,
                command.FrameCost,
                command.TotalPrice,
                currentUser.UserId,
                currentUser.Email);

            visit.AddOpticalOrder(order);
            visit.AdvanceStage(WorkflowStage.Cashier);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
