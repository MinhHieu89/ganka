using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;

namespace Billing.Application.Features;

/// <summary>
/// Query to retrieve all payments for a specific invoice.
/// </summary>
public sealed record GetPaymentsByInvoiceQuery(Guid InvoiceId);

/// <summary>
/// Wolverine handler for retrieving payments by invoice ID.
/// Stub: returns empty list until implemented.
/// </summary>
public static class GetPaymentsByInvoiceHandler
{
    public static Task<List<PaymentDto>> Handle(
        GetPaymentsByInvoiceQuery query,
        IPaymentRepository paymentRepository,
        CancellationToken ct)
    {
        // Stub: not implemented yet (TDD RED phase)
        return Task.FromResult(new List<PaymentDto>());
    }
}
