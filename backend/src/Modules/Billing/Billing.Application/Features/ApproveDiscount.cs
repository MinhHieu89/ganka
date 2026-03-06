using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Domain;
using Wolverine;

namespace Billing.Application.Features;

/// <summary>
/// Command to approve a pending discount with manager PIN verification.
/// </summary>
public sealed record ApproveDiscountCommand(
    Guid InvoiceId,
    Guid DiscountId,
    Guid ManagerId,
    string ManagerPin);

/// <summary>
/// Cross-module query to verify a manager's PIN.
/// Sent to Auth module via IMessageBus.
/// </summary>
public sealed record VerifyManagerPinQuery(Guid ManagerId, string Pin);

/// <summary>
/// Response from Auth module for PIN verification.
/// </summary>
public sealed record VerifyManagerPinResponse(bool IsValid);

/// <summary>
/// Wolverine handler for <see cref="ApproveDiscountCommand"/>.
/// Verifies manager PIN via cross-module query, then approves discount and recalculates invoice totals.
/// </summary>
public static class ApproveDiscountHandler
{
    public static async Task<Result> Handle(
        ApproveDiscountCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
