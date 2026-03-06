using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Query to retrieve all invoices for a specific visit.
/// Returns lightweight summaries without full child collections.
/// </summary>
public sealed record GetInvoicesByVisitQuery(Guid VisitId);

/// <summary>
/// Wolverine static handler for retrieving invoice summaries by visit ID.
/// Returns lightweight InvoiceSummaryDto list without full child entities.
/// </summary>
public static class GetInvoicesByVisitHandler
{
    public static async Task<Result<List<InvoiceSummaryDto>>> Handle(
        GetInvoicesByVisitQuery query,
        IInvoiceRepository invoiceRepository,
        CancellationToken ct)
    {
        var invoices = await invoiceRepository.GetAllByVisitIdAsync(query.VisitId, ct);

        var summaries = invoices.Select(invoice => new InvoiceSummaryDto(
            Id: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            PatientName: invoice.PatientName,
            Status: (int)invoice.Status,
            TotalAmount: invoice.TotalAmount,
            PaidAmount: invoice.PaidAmount,
            BalanceDue: invoice.BalanceDue,
            CreatedAt: invoice.CreatedAt)).ToList();

        return summaries;
    }
}
