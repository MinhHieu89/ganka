using Clinical.Application.Interfaces;
using Clinical.Domain.Enums;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Command to complete a technician order and advance visit to DoctorExam.
/// </summary>
public record CompleteTechnicianOrderCommand(Guid OrderId);

/// <summary>
/// Wolverine handler for completing a technician order.
/// Marks the order done and advances the visit to DoctorExam stage.
/// </summary>
public static class CompleteTechnicianOrderHandler
{
    public static async Task<Result> Handle(
        CompleteTechnicianOrderCommand command,
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

        try
        {
            order.Complete();
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
