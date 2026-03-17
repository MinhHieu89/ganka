using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Billing.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ICashierShiftRepository"/>.
/// Provides CRUD operations for CashierShift aggregate.
/// </summary>
public sealed class CashierShiftRepository(BillingDbContext context) : ICashierShiftRepository
{
    public async Task<CashierShift?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.CashierShifts
            .FirstOrDefaultAsync(cs => cs.Id == id, ct);
    }

    public async Task<CashierShift?> GetCurrentOpenAsync(BranchId branchId, CancellationToken ct)
    {
        return await context.CashierShifts
            .FirstOrDefaultAsync(
                cs => cs.BranchId == branchId && cs.Status == ShiftStatus.Open, ct);
    }

    public async Task<List<CashierShift>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        return await context.CashierShifts
            .Where(cs => cs.OpenedAt >= from && cs.OpenedAt <= to)
            .OrderByDescending(cs => cs.OpenedAt)
            .ToListAsync(ct);
    }

    public async Task<CashierShift?> GetLastClosedAsync(BranchId branchId, CancellationToken ct)
    {
        return await context.CashierShifts
            .Where(cs => cs.BranchId == branchId && cs.Status == ShiftStatus.Closed)
            .OrderByDescending(cs => cs.ClosedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ShiftTemplate>> GetActiveShiftTemplatesAsync(BranchId branchId, CancellationToken ct)
    {
        return await context.ShiftTemplates
            .Where(st => st.BranchId == branchId && st.IsActive)
            .OrderBy(st => st.DefaultStartTime)
            .ToListAsync(ct);
    }

    public async Task<(List<CashierShift> Items, int TotalCount)> GetClosedAsync(BranchId branchId, int page, int pageSize, CancellationToken ct)
    {
        var query = context.CashierShifts
            .Where(cs => cs.BranchId == branchId && cs.Status == ShiftStatus.Closed)
            .OrderByDescending(cs => cs.ClosedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public void Add(CashierShift shift)
    {
        context.CashierShifts.Add(shift);
    }

    public void Update(CashierShift shift)
    {
        context.CashierShifts.Update(shift);
    }
}
