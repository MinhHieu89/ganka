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
        throw new NotImplementedException();
    }
}
