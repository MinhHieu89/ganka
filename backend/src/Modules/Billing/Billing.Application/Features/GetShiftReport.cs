using Billing.Application.Interfaces;
using Billing.Contracts.Dtos;
using Shared.Domain;

namespace Billing.Application.Features;

/// <summary>
/// Query to get a shift report with revenue breakdown by payment method.
/// </summary>
public sealed record GetShiftReportQuery(Guid ShiftId);

/// <summary>
/// Wolverine static handler for generating a shift report.
/// Groups payments by method and computes revenue breakdown.
/// </summary>
public static class GetShiftReportHandler
{
    public static async Task<Result<ShiftReportDto>> Handle(
        GetShiftReportQuery query,
        ICashierShiftRepository shiftRepository,
        IPaymentRepository paymentRepository,
        CancellationToken ct)
    {
        var shift = await shiftRepository.GetByIdAsync(query.ShiftId, ct);
        if (shift is null)
        {
            return Result.Failure<ShiftReportDto>(
                Error.NotFound("CashierShift", query.ShiftId));
        }

        // Load all payments for the shift and group by payment method
        var payments = await paymentRepository.GetByShiftIdAsync(query.ShiftId, ct);
        var revenueByMethod = payments
            .GroupBy(p => p.Method.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        var report = new ShiftReportDto(
            ShiftId: shift.Id,
            CashierName: shift.CashierName,
            OpenedAt: shift.OpenedAt,
            ClosedAt: shift.ClosedAt,
            RevenueByMethod: revenueByMethod,
            TransactionCount: shift.TransactionCount,
            OpeningBalance: shift.OpeningBalance,
            CashReceived: shift.CashReceived,
            CashRefunds: shift.CashRefunds,
            ExpectedCash: shift.ExpectedCashAmount,
            ActualCash: shift.ActualCashCount,
            Discrepancy: shift.Discrepancy,
            ManagerNote: shift.ManagerNote);

        return report;
    }
}
