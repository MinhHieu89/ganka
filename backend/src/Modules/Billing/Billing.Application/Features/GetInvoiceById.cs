using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Query to retrieve an invoice by its ID with all child collections.
/// </summary>
public sealed record GetInvoiceByIdQuery(Guid InvoiceId);

/// <summary>
/// Wolverine static handler for retrieving a full invoice by ID.
/// Loads invoice with all child entities (LineItems, Payments, Discounts, Refunds)
/// and maps to InvoiceDto with complete details.
/// </summary>
public static class GetInvoiceByIdHandler
{
    public static async Task<Result<InvoiceDto>> Handle(
        GetInvoiceByIdQuery query,
        IInvoiceRepository invoiceRepository,
        CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByIdAsync(query.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure<InvoiceDto>(
                Error.NotFound("Invoice", query.InvoiceId));

        return CreateInvoiceHandler.MapToDto(invoice);
    }
}
