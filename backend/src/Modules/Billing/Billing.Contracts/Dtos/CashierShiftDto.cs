namespace Billing.Contracts.Dtos;

/// <summary>
/// Full cashier shift DTO with financial summary.
/// Status is int-serialized Billing.Domain.Enums.ShiftStatus.
/// </summary>
public sealed record CashierShiftDto(
    Guid Id,
    Guid CashierId,
    string CashierName,
    Guid? ShiftTemplateId,
    int Status,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal OpeningBalance,
    decimal ExpectedCashAmount,
    decimal CashReceived,
    decimal CashRefunds,
    decimal? ActualCashCount,
    decimal? Discrepancy,
    string? ManagerNote,
    decimal TotalRevenue,
    int TransactionCount);

/// <summary>
/// Shift report DTO for end-of-shift reconciliation display.
/// RevenueByMethod maps payment method names to their totals.
/// </summary>
public sealed record ShiftReportDto(
    Guid ShiftId,
    string CashierName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    Dictionary<string, decimal> RevenueByMethod,
    int TransactionCount,
    decimal OpeningBalance,
    decimal CashReceived,
    decimal CashRefunds,
    decimal ExpectedCash,
    decimal? ActualCash,
    decimal? Discrepancy,
    string? ManagerNote);

/// <summary>
/// Shift template configuration DTO for scheduling cashier shifts.
/// </summary>
public sealed record ShiftTemplateDto(
    Guid Id,
    string Name,
    string? NameVi,
    string DefaultStartTime,
    string DefaultEndTime,
    bool IsActive);
