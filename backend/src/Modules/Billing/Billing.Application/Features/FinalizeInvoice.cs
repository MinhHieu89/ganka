using Billing.Application.Interfaces;
using Shared.Application;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to finalize a fully-paid draft invoice.
/// </summary>
public sealed record FinalizeInvoiceCommand(
    Guid InvoiceId,
    Guid CashierShiftId);

/// <summary>
/// Wolverine static handler for finalizing an invoice.
/// </summary>
public static class FinalizeInvoiceHandler
{
    public static Task<Result> Handle(
        FinalizeInvoiceCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        throw new NotImplementedException("RED phase stub -- implement in Task 2");
    }
}
