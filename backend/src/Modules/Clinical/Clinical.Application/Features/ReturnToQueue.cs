using Clinical.Application.Interfaces;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Command to return a technician order back to the queue.
/// </summary>
public record ReturnToQueueCommand(Guid OrderId);

/// <summary>
/// Wolverine handler for returning a technician order to the queue.
/// Clears technician assignment. Does NOT change visit stage (stays at PreExam).
/// </summary>
public static class ReturnToQueueHandler
{
    public static async Task<Result> Handle(
        ReturnToQueueCommand command,
        IVisitRepository visitRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var visit = await visitRepository.GetByTechnicianOrderIdAsync(command.OrderId, ct);
        if (visit is null)
            return Result.Failure(Error.NotFound("TechnicianOrder", command.OrderId));

        var order = visit.TechnicianOrders.FirstOrDefault(o => o.Id == command.OrderId);
        if (order is null)
            return Result.Failure(Error.NotFound("TechnicianOrder", command.OrderId));

        order.ReturnToQueue();

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
