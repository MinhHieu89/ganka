using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Contracts.Queries;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for <see cref="GetPendingInvoicesQuery"/>.
/// Returns draft invoices for the cashier dashboard pending panel.
/// </summary>
public static class GetPendingInvoicesHandler
{
    public static async Task<Result<List<InvoiceDto>>> Handle(
        GetPendingInvoicesQuery query,
        IInvoiceRepository invoiceRepository,
        CancellationToken ct)
    {
        var invoices = await invoiceRepository.GetPendingAsync(query.CashierShiftId, ct);
        var dtos = invoices.Select(CreateInvoiceHandler.MapToDto).ToList();
        return dtos;
    }
}
