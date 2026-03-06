using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// CashierShift aggregate root for shift management with cash reconciliation.
/// Tracks opening/closing balances and revenue per payment method.
/// </summary>
public class CashierShift : AggregateRoot, IAuditable
{
    public Guid CashierId { get; private set; }
    public string CashierName { get; private set; } = default!;
    public Guid? ShiftTemplateId { get; private set; }
    public ShiftStatus Status { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public decimal OpeningBalance { get; private set; }
    public decimal ExpectedCashAmount => OpeningBalance + CashReceived - CashRefunds;
    public decimal CashReceived { get; private set; }
    public decimal CashRefunds { get; private set; }
    public decimal? ActualCashCount { get; private set; }
    public decimal? Discrepancy { get; private set; }
    public string? ManagerNote { get; private set; }
    public decimal TotalRevenue { get; private set; }
    public int TransactionCount { get; private set; }

    private CashierShift() { }

    public static CashierShift Create(
        Guid cashierId,
        string cashierName,
        decimal openingBalance,
        Guid? shiftTemplateId,
        BranchId branchId)
    {
        var shift = new CashierShift
        {
            CashierId = cashierId,
            CashierName = cashierName,
            OpeningBalance = openingBalance,
            ShiftTemplateId = shiftTemplateId,
            Status = ShiftStatus.Open,
            OpenedAt = DateTime.UtcNow
        };
        shift.SetBranchId(branchId);
        return shift;
    }

    public void LockForClose()
    {
        EnsureOpen();
        Status = ShiftStatus.Locked;
        SetUpdatedAt();
    }

    public void Close(decimal actualCashCount, string? managerNote)
    {
        EnsureLocked();
        ActualCashCount = actualCashCount;
        Discrepancy = actualCashCount - ExpectedCashAmount;
        ManagerNote = managerNote;
        Status = ShiftStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void AddCashReceived(decimal amount)
    {
        EnsureOpen();
        CashReceived += amount;
        TotalRevenue += amount;
        SetUpdatedAt();
    }

    public void AddNonCashRevenue(decimal amount)
    {
        EnsureOpen();
        TotalRevenue += amount;
        SetUpdatedAt();
    }

    public void AddCashRefund(decimal amount)
    {
        EnsureOpen();
        CashRefunds += amount;
        SetUpdatedAt();
    }

    public void IncrementTransactionCount()
    {
        TransactionCount++;
        SetUpdatedAt();
    }

    private void EnsureOpen()
    {
        if (Status != ShiftStatus.Open)
            throw new InvalidOperationException("Shift must be in Open status for this operation.");
    }

    private void EnsureLocked()
    {
        if (Status != ShiftStatus.Locked)
            throw new InvalidOperationException("Shift must be Locked before closing.");
    }
}
