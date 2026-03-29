using Clinical.Application.Interfaces;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Command to accept (claim) a technician order.
/// </summary>
public record AcceptTechnicianOrderCommand(Guid OrderId, Guid TechnicianId, string TechnicianName);

/// <summary>
/// Wolverine handler for accepting a technician order.
/// Assigns the technician. Returns structured error if already accepted (D-15 concurrency).
/// </summary>
public static class AcceptTechnicianOrderHandler
{
    public static async Task<Result> Handle(
        AcceptTechnicianOrderCommand command,
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
            order.Accept(command.TechnicianId, command.TechnicianName);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
