using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to apply a discount to an invoice or specific line item.
/// Discount starts as Pending and requires manager approval via ApproveDiscount.
/// </summary>
public sealed record ApplyDiscountCommand(
    Guid InvoiceId,
    Guid? InvoiceLineItemId,
    int DiscountType,
    decimal Value,
    string Reason,
    Guid RequestedById);

/// <summary>
/// Validator for <see cref="ApplyDiscountCommand"/>.
/// </summary>
public class ApplyDiscountCommandValidator : AbstractValidator<ApplyDiscountCommand>
{
    public ApplyDiscountCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Value).GreaterThan(0).WithMessage("Discount value must be greater than zero.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Discount reason is required.");
        RuleFor(x => x.Value).LessThanOrEqualTo(100)
            .When(x => x.DiscountType == (int)Billing.Domain.Enums.DiscountType.Percentage)
            .WithMessage("Percentage discount cannot exceed 100%.");
        RuleFor(x => x.RequestedById).NotEmpty();
    }
}

/// <summary>
/// Wolverine handler for <see cref="ApplyDiscountCommand"/>.
/// Creates a pending discount on a draft invoice.
/// </summary>
public static class ApplyDiscountHandler
{
    public static async Task<Result<DiscountDto>> Handle(
        ApplyDiscountCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IValidator<ApplyDiscountCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<DiscountDto>.Failure(Error.ValidationWithDetails(errors));
        }

        var invoice = await invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<DiscountDto>.Failure(Error.NotFound("Invoice", command.InvoiceId));

        // Only draft invoices can have discounts applied
        if (invoice.Status != InvoiceStatus.Draft)
            return Result<DiscountDto>.Failure(
                Error.Validation("Cannot apply discount to a non-draft invoice. Only Draft invoices can be modified."));

        var discountType = (DiscountType)command.DiscountType;

        // Create the discount entity
        var discount = Discount.Create(
            command.InvoiceId,
            discountType,
            command.Value,
            command.Reason,
            command.RequestedById,
            command.InvoiceLineItemId);

        // Calculate the discount amount based on the base amount
        decimal baseAmount;
        if (command.InvoiceLineItemId.HasValue)
        {
            var lineItem = invoice.LineItems.FirstOrDefault(li => li.Id == command.InvoiceLineItemId.Value);
            if (lineItem is null)
                return Result<DiscountDto>.Failure(
                    Error.NotFound("InvoiceLineItem", command.InvoiceLineItemId.Value));
            baseAmount = lineItem.LineTotal;
        }
        else
        {
            baseAmount = invoice.SubTotal;
        }

        discount.CalculateAmount(baseAmount);

        // Apply discount to invoice (adds to collection and recalculates totals)
        invoice.ApplyDiscount(discount);
        invoiceRepository.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new DiscountDto(
            discount.Id,
            discount.InvoiceLineItemId,
            (int)discount.Type,
            discount.Value,
            discount.CalculatedAmount,
            discount.Reason,
            (int)discount.ApprovalStatus,
            discount.RequestedById,
            discount.RequestedAt,
            discount.ApprovedById,
            discount.ApprovedAt);

        return dto;
    }
}
