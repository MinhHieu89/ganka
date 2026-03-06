using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using FluentValidation;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to remove a line item from a draft invoice.
/// </summary>
public sealed record RemoveInvoiceLineItemCommand(Guid InvoiceId, Guid LineItemId);

/// <summary>
/// Validator for <see cref="RemoveInvoiceLineItemCommand"/>.
/// </summary>
public class RemoveInvoiceLineItemValidator : AbstractValidator<RemoveInvoiceLineItemCommand>
{
    public RemoveInvoiceLineItemValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty().WithMessage("Invoice ID is required.");
        RuleFor(x => x.LineItemId).NotEmpty().WithMessage("Line item ID is required.");
    }
}

/// <summary>
/// Wolverine static handler for removing a line item from a draft invoice.
/// Loads invoice, calls RemoveLineItem domain method, saves, returns updated DTO.
/// </summary>
public static class RemoveInvoiceLineItemHandler
{
    public static async Task<Result<InvoiceDto>> Handle(
        RemoveInvoiceLineItemCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IValidator<RemoveInvoiceLineItemCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<InvoiceDto>(Error.ValidationWithDetails(errors));
        }

        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure<InvoiceDto>(
                Error.NotFound("Invoice", command.InvoiceId));

        try
        {
            invoice.RemoveLineItem(command.LineItemId);
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
