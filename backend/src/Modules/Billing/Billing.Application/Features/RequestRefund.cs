using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using FluentValidation;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Command to request a refund on a finalized invoice.
/// Creates a refund in Requested status pending manager/owner approval.
/// </summary>
public sealed record RequestRefundCommand(
    Guid InvoiceId,
    Guid? InvoiceLineItemId,
    decimal Amount,
    string Reason,
    Guid RequestedById);

/// <summary>
/// Validator for <see cref="RequestRefundCommand"/>.
/// </summary>
public class RequestRefundCommandValidator : AbstractValidator<RequestRefundCommand>
{
    public RequestRefundCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Refund amount must be greater than zero.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Refund reason is required.");
        RuleFor(x => x.RequestedById).NotEmpty();
    }
}

/// <summary>
/// Wolverine handler for <see cref="RequestRefundCommand"/>.
/// Creates a refund request on a finalized invoice.
/// </summary>
public static class RequestRefundHandler
{
    public static async Task<Result<RefundDto>> Handle(
        RequestRefundCommand command,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        IValidator<RequestRefundCommand> validator,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
