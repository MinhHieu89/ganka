using Billing.Domain.Entities;
using Shared.Domain;

namespace Billing.Application.Interfaces;

/// <summary>
/// Repository interface for CashierShift aggregate persistence.
/// Supports active shift lookup and date range queries for reporting.
/// </summary>
public interface ICashierShiftRepository
{
    /// <summary>
    /// Gets a cashier shift by ID.
    /// </summary>
    Task<CashierShift?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets the currently open shift for a specific branch.
    /// Returns null if no shift is currently open.
    /// </summary>
    Task<CashierShift?> GetCurrentOpenAsync(BranchId branchId, CancellationToken ct);

    /// <summary>
    /// Gets all shifts within a date range for reporting.
    /// </summary>
    Task<List<CashierShift>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct);

    /// <summary>
    /// Gets the last closed shift for a branch.
    /// Used to determine the default opening balance for a new shift.
    /// </summary>
    Task<CashierShift?> GetLastClosedAsync(BranchId branchId, CancellationToken ct);

    /// <summary>
    /// Adds a new cashier shift to the change tracker.
    /// </summary>
    void Add(CashierShift shift);

    /// <summary>
    /// Marks an existing cashier shift as modified in the change tracker.
    /// </summary>
    void Update(CashierShift shift);
}
