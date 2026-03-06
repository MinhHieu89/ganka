using Billing.Application.Interfaces;
using Shared.Domain;
using Billing.Contracts.Dtos;

namespace Billing.Application.Features;

/// <summary>
/// Query to get a shift report with revenue breakdown by payment method.
/// </summary>
public sealed record GetShiftReportQuery(Guid ShiftId);

/// <summary>
/// Wolverine static handler for generating a shift report.
/// </summary>
public static class GetShiftReportHandler
{
    public static Task<Result<ShiftReportDto>> Handle(
        GetShiftReportQuery query,
        ICashierShiftRepository shiftRepository,
        IPaymentRepository paymentRepository,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
