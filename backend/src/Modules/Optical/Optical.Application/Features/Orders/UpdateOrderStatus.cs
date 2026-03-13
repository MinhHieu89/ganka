using Billing.Contracts.Dtos;
using Billing.Contracts.Queries;
using Optical.Application.Interfaces;
using Optical.Domain.Enums;
using Shared.Domain;
using Wolverine;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Command to transition a glasses order to a new status.
/// Enforces OPT-04: blocks Ordered->Processing without confirmed full payment from Billing module.
/// </summary>
public sealed record UpdateOrderStatusCommand(Guid OrderId, int NewStatus, string? Notes);

/// <summary>
/// Wolverine static handler for <see cref="UpdateOrderStatusCommand"/>.
/// Implements the critical OPT-04 payment gate: when transitioning to Processing, queries
/// the Billing module via IMessageBus to verify the visit invoice has no outstanding balance.
/// Other status transitions proceed without payment check.
/// </summary>
public static class UpdateOrderStatusHandler
{
    public static async Task<Result> Handle(
        UpdateOrderStatusCommand command,
        IGlassesOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, ct);
        if (order is null)
            return Result.Failure(Error.NotFound("GlassesOrder", command.OrderId));

        var newStatus = (GlassesOrderStatus)command.NewStatus;

        // OPT-04 payment gate: block Ordered -> Processing unless fully paid
        if (order.Status == GlassesOrderStatus.Ordered && newStatus == GlassesOrderStatus.Processing)
        {
            // Cross-module server-side check via Billing -- this is NOT a stale frontend check
            var invoice = await messageBus.InvokeAsync<InvoiceDto?>(
                new GetVisitInvoiceQuery(order.VisitId), ct);

            if (invoice is null || invoice.BalanceDue > 0)
            {
                return Result.Failure(
                    Error.Validation("Payment must be completed before processing the order."));
            }

            // Payment confirmed -- mark on the entity and transition
            order.ConfirmPayment();
        }

        try
        {
            order.TransitionTo(newStatus);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
