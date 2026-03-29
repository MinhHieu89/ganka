using Clinical.Application.Interfaces;
using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Command to red-flag a technician order with a reason.
/// </summary>
public record RedFlagTechnicianOrderCommand(Guid OrderId, string Reason);

/// <summary>
/// Wolverine handler for red-flagging a technician order.
/// Marks red flag with reason and advances visit to DoctorExam stage.
/// </summary>
public static class RedFlagTechnicianOrderHandler
{
    public static async Task<Result> Handle(
        RedFlagTechnicianOrderCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
            return Result.Failure(Error.Validation("Red flag reason is required."));

        var visit = await visitRepository.GetByTechnicianOrderIdAsync(command.OrderId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("TechnicianOrder", command.OrderId));

        var order = visit.TechnicianOrders.FirstOrDefault(o => o.Id == command.OrderId);
        if (order is null)
            return Result.Failure(Error.NotFound("TechnicianOrder", command.OrderId));

        try
        {
            order.MarkRedFlag(command.Reason);
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
