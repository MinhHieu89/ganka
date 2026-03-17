using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Contracts.Queries;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for <see cref="GetAllInvoicesQuery"/>.
/// Returns paginated invoice summaries with optional status filter and search.
/// Used by the Invoice History page to browse all invoices.
/// </summary>
public static class GetAllInvoicesHandler
{
    public static async Task<Result<PaginatedInvoicesResult>> Handle(
        GetAllInvoicesQuery query,
        IInvoiceRepository invoiceRepository,
        CancellationToken ct)
    {
        var (items, totalCount) = await invoiceRepository.GetAllAsync(
            query.Status, query.Search, query.Page, query.PageSize, ct);

        var dtos = items.Select(i => new InvoiceSummaryDto(
            Id: i.Id,
            InvoiceNumber: i.InvoiceNumber,
            PatientName: i.PatientName,
            Status: (int)i.Status,
            TotalAmount: i.TotalAmount,
            PaidAmount: i.PaidAmount,
            BalanceDue: i.BalanceDue,
            CreatedAt: i.CreatedAt)).ToList();

        return new PaginatedInvoicesResult(dtos, totalCount, query.Page, query.PageSize);
    }
}
