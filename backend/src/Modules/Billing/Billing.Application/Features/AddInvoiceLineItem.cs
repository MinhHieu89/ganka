using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Enums;
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
/// Loads invoice, calls AddLineItem domain method, saves, returns updated DTO.
/// </summary>
public static class AddInvoiceLineItemHandler
{
    public static async Task<Result<InvoiceDto>> Handle(
        AddInvoiceLineItemCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure<InvoiceDto>(
                Error.NotFound("Invoice", command.InvoiceId));

        try
        {
            invoice.AddLineItem(
                command.Description,
                command.DescriptionVi,
                command.UnitPrice,
                command.Quantity,
                (Department)command.Department,
                command.SourceId,
                command.SourceType);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<InvoiceDto>(
                Error.Custom("Error.InvalidOperation", ex.Message));
        }

        await unitOfWork.SaveChangesAsync(ct);

        return CreateInvoiceHandler.MapToDto(invoice);
    }
}
