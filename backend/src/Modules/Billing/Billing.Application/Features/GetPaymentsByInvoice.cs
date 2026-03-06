using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;

namespace Billing.Application.Features;

/// <summary>
/// Query to retrieve all payments for a specific invoice.
/// </summary>
public sealed record GetPaymentsByInvoiceQuery(Guid InvoiceId);

/// <summary>
/// Wolverine handler for retrieving payments by invoice ID.
/// Maps Payment domain entities to PaymentDto records.
/// </summary>
public static class GetPaymentsByInvoiceHandler
{
    public static async Task<List<PaymentDto>> Handle(
        GetPaymentsByInvoiceQuery query,
        IPaymentRepository paymentRepository,
        CancellationToken ct)
    {
        var payments = await paymentRepository.GetByInvoiceIdAsync(query.InvoiceId, ct);

        return payments.Select(p => new PaymentDto(
            p.Id,
            p.InvoiceId,
            (int)p.Method,
            p.Amount,
            (int)p.Status,
            p.ReferenceNumber,
            p.CardLast4,
            p.CardType,
            p.Notes,
            p.RecordedById,
            p.RecordedAt,
            p.CashierShiftId,
            p.TreatmentPackageId,
            p.IsSplitPayment,
            p.SplitSequence)).ToList();
    }
}
