using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to add a line item to an existing draft invoice.
/// </summary>
public sealed record AddInvoiceLineItemCommand(
    Guid InvoiceId,
    string Description,
    string? DescriptionVi,
    decimal UnitPrice,
    int Quantity,
    int Department,
    Guid? SourceId,
    string? SourceType);

/// <summary>
/// Wolverine static handler for adding a line item to an invoice.
/// </summary>
public static class AddInvoiceLineItemHandler
{
    public static Task<Result<InvoiceDto>> Handle(
        AddInvoiceLineItemCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        throw new NotImplementedException("RED phase stub -- implement in Task 2");
    }
}
