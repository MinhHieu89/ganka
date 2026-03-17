using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Contracts.Queries;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Wolverine handler for <see cref="GetVisitInvoiceQuery"/>.
/// Returns the invoice associated with a specific visit (cross-module query).
/// </summary>
public static class GetVisitInvoiceHandler
{
    public static async Task<Result<InvoiceDto>> Handle(
        GetVisitInvoiceQuery query,
        IInvoiceRepository invoiceRepository,
        CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByVisitIdAsync(query.VisitId, ct);
        if (invoice is null)
            return Result<InvoiceDto>.Failure(Error.NotFound("Invoice", query.VisitId));

        return CreateInvoiceHandler.MapToDto(invoice);
    }
}
