using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Aggregate root for cashier shift management with cash reconciliation.
/// Tracks opening balance, payments received, refunds, and actual cash count at close.
/// Lifecycle: Open -> Locked -> Closed.
/// </summary>
public class CashierShift : AggregateRoot, IAuditable
{
    /// <summary>User ID of the cashier who opened the shift.</summary>
    public Guid CashierId { get; private set; }

    /// <summary>Denormalized cashier display name to avoid cross-module joins.</summary>
    public string CashierName { get; private set; } = string.Empty;

    /// <summary>Optional shift template used for this shift. Null for custom shifts.</summary>
    public Guid? ShiftTemplateId { get; private set; }

    /// <summary>Current lifecycle status of the shift.</summary>
    public ShiftStatus Status { get; private set; }

    /// <summary>When the shift was opened.</summary>
    public DateTime OpenedAt { get; private set; }

    /// <summary>When the shift was closed. Null while open or locked.</summary>
    public DateTime? ClosedAt { get; private set; }

    /// <summary>Cash in drawer at the start of the shift.</summary>
    public decimal OpeningBalance { get; private set; }

    /// <summary>
    /// Expected cash in drawer: OpeningBalance + CashReceived - CashRefunds.
    /// Computed property, not stored.
    /// </summary>
    public decimal ExpectedCashAmount => OpeningBalance + CashReceived - CashRefunds;

    /// <summary>Sum of confirmed cash payments received during this shift.</summary>
    public decimal CashReceived { get; private set; }

    /// <summary>Sum of cash refunds issued during this shift.</summary>
    public decimal CashRefunds { get; private set; }

    /// <summary>Physical cash count entered by cashier at close. Null until close.</summary>
    public decimal? ActualCashCount { get; private set; }

    /// <summary>
    /// Difference between actual and expected cash: ActualCashCount - ExpectedCashAmount.
    /// Null until close. Positive = overage, negative = shortage.
    /// </summary>
    public decimal? Discrepancy { get; private set; }

    /// <summary>Manager's explanation for any cash discrepancy.</summary>
    public string? ManagerNote { get; private set; }

    /// <summary>Sum of all confirmed payments across all methods during this shift.</summary>
    public decimal TotalRevenue { get; private set; }

    /// <summary>Number of finalized invoices processed during this shift.</summary>
    public int TransactionCount { get; private set; }

    private CashierShift() { }

    /// <summary>
    /// Factory method for opening a new cashier shift.
    /// </summary>
    public static CashierShift Create(
        Guid cashierId,
        string cashierName,
        decimal openingBalance,
        Guid? shiftTemplateId,
        BranchId branchId)
    {
        if (cashierId == Guid.Empty)
            throw new ArgumentException("Cashier ID is required.", nameof(cashierId));

        if (string.IsNullOrWhiteSpace(cashierName))
            throw new ArgumentException("Cashier name is required.", nameof(cashierName));

        if (openingBalance < 0)
            throw new ArgumentException("Opening balance cannot be negative.", nameof(openingBalance));

        var shift = new CashierShift
        {
            CashierId = cashierId,
            CashierName = cashierName,
            ShiftTemplateId = shiftTemplateId,
            Status = ShiftStatus.Open,
            OpenedAt = DateTime.UtcNow,
            OpeningBalance = openingBalance,
            CashReceived = 0,
            CashRefunds = 0,
            TotalRevenue = 0,
            TransactionCount = 0
        };

        shift.SetBranchId(branchId);
        return shift;
    }

    /// <summary>
    /// Locks the shift to prevent new payment assignments.
    /// Cashier prepares for cash count and close.
    /// </summary>
    public void LockForClose()
    {
        EnsureOpen();
        Status = ShiftStatus.Locked;
        SetUpdatedAt();
    }

    /// <summary>
    /// Closes the shift with the actual cash count and optional manager note.
    /// Computes the discrepancy between expected and actual cash.
    /// </summary>
    public void Close(decimal actualCashCount, string? managerNote)
    {
        EnsureLocked();

        if (actualCashCount < 0)
            throw new ArgumentException("Actual cash count cannot be negative.", nameof(actualCashCount));

        ActualCashCount = actualCashCount;
        Discrepancy = actualCashCount - ExpectedCashAmount;
        ManagerNote = managerNote;
        Status = ShiftStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Records a confirmed cash payment received during this shift.
    /// Increments both CashReceived and TotalRevenue.
    /// </summary>
    public void AddCashReceived(decimal amount)
    {
        EnsureOpen();

        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        CashReceived += amount;
        TotalRevenue += amount;
        SetUpdatedAt();
    }

    /// <summary>
    /// Records a confirmed non-cash payment (bank transfer, QR, card).
    /// Increments TotalRevenue only (does not affect cash reconciliation).
    /// </summary>
    public void AddNonCashRevenue(decimal amount)
    {
        EnsureOpen();

        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        TotalRevenue += amount;
        SetUpdatedAt();
    }

    /// <summary>
    /// Records a cash refund issued during this shift.
    /// Increments CashRefunds (reduces ExpectedCashAmount).
    /// </summary>
    public void AddCashRefund(decimal amount)
    {
        EnsureOpen();

        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        CashRefunds += amount;
        SetUpdatedAt();
    }

    /// <summary>
    /// Increments the transaction count when an invoice is finalized.
    /// </summary>
    public void IncrementTransactionCount()
    {
        EnsureOpen();
        TransactionCount++;
        SetUpdatedAt();
    }

    /// <summary>
    /// Guard: validates the shift is in Open status.
    /// Throws if the shift is locked or closed.
    /// </summary>
    private void EnsureOpen()
    {
        if (Status != ShiftStatus.Open)
            throw new InvalidOperationException(
                $"Shift is {Status}. Only open shifts can accept new operations.");
    }

    /// <summary>
    /// Guard: validates the shift is in Locked status for close.
    /// Throws if the shift is not locked.
    /// </summary>
    private void EnsureLocked()
    {
        if (Status != ShiftStatus.Locked)
            throw new InvalidOperationException(
                $"Shift must be locked before closing. Current status: {Status}.");
    }
}
