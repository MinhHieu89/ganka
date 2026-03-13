using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IInvoiceRepository"/>.
/// Provides CRUD operations with eager loading of child entities.
/// </summary>
public sealed class InvoiceRepository(BillingDbContext context) : IInvoiceRepository
{
    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Discounts)
            .Include(i => i.Refunds)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<Invoice?> GetByVisitIdAsync(Guid visitId, CancellationToken ct)
    {
        return await context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Discounts)
            .Include(i => i.Refunds)
            .FirstOrDefaultAsync(i => i.VisitId == visitId, ct);
    }

    public async Task<List<Invoice>> GetAllByVisitIdAsync(Guid visitId, CancellationToken ct)
    {
        return await context.Invoices
            .Where(i => i.VisitId == visitId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct)
    {
        return await context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Discounts)
            .Include(i => i.Refunds)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, ct);
    }

    public async Task<List<Invoice>> GetByShiftIdAsync(Guid shiftId, CancellationToken ct)
    {
        return await context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.CashierShiftId == shiftId)
            .ToListAsync(ct);
    }

    public async Task<List<Invoice>> GetPendingByPatientIdAsync(Guid patientId, CancellationToken ct)
    {
        return await context.Invoices
            .Where(i => i.PatientId == patientId && i.Status == InvoiceStatus.Draft)
            .ToListAsync(ct);
    }

    public async Task<List<Invoice>> GetPendingAsync(Guid? cashierShiftId, CancellationToken ct)
    {
        var query = context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Discounts)
            .Include(i => i.Refunds)
            .Where(i => i.Status == InvoiceStatus.Draft);

        if (cashierShiftId.HasValue)
            query = query.Where(i => i.CashierShiftId == cashierShiftId.Value);

        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync(ct);
    }

    /// <summary>
    /// Generates the next invoice number using a SQL SEQUENCE for atomic, race-condition-free numbering.
    /// The sequence is global (not year-scoped); the year prefix handles display only.
    /// </summary>
    public async Task<string> GetNextInvoiceNumberAsync(int year, CancellationToken ct)
    {
        var nextVal = await context.Database
            .SqlQueryRaw<long>("SELECT NEXT VALUE FOR billing.InvoiceNumberSeq AS Value")
            .FirstAsync(ct);

        return $"HD-{year}-{nextVal:D5}";
    }

    public void Add(Invoice invoice)
    {
        context.Invoices.Add(invoice);
    }

    public void Update(Invoice invoice)
    {
        context.Invoices.Update(invoice);
    }
}
