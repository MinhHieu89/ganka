using Shared.Domain;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Command to transition a glasses order to a new status.
/// Enforces OPT-04: blocks Ordered->Processing without confirmed payment.
/// Handler implementation provided in plan 08-18.
/// </summary>
public sealed record UpdateOrderStatusCommand(Guid OrderId, int NewStatus, string? Notes);
