using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

    public async Task<(List<Invoice> Items, int TotalCount)> GetAllAsync(
        int? status, string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Discounts)
            .Include(i => i.Refunds)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(i => (int)i.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(i =>
                i.PatientName.ToLower().Contains(searchLower) ||
                i.InvoiceNumber.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
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
    /// Uses raw ADO.NET to avoid EF Core wrapping NEXT VALUE FOR in a subquery.
    /// </summary>
    public async Task<string> GetNextInvoiceNumberAsync(int year, CancellationToken ct)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR billing.InvoiceNumberSeq";
        command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();

        var result = await command.ExecuteScalarAsync(ct);
        var nextVal = Convert.ToInt64(result);

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
