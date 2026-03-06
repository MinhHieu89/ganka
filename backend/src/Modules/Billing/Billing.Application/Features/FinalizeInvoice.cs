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
/// Loads invoice, gets userId from ICurrentUser, calls Finalize domain method, saves.
/// </summary>
public static class FinalizeInvoiceHandler
{
    public static async Task<Result> Handle(
        FinalizeInvoiceCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure(
                Error.NotFound("Invoice", command.InvoiceId));

        try
        {
            invoice.Finalize(command.CashierShiftId, currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                Error.Custom("Error.InvalidOperation", ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
