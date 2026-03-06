using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IInvoiceRepository"/>.
/// Provides eager loading for Invoice aggregate including LineItems, Payments, Discounts, and Refunds.
/// </summary>
public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly BillingDbContext _context;

    public InvoiceRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Discounts)
            .Include(i => i.Refunds)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<Invoice?> GetByVisitIdAsync(Guid visitId, CancellationToken ct)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Discounts)
            .Include(i => i.Refunds)
            .FirstOrDefaultAsync(i => i.VisitId == visitId, ct);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Discounts)
            .Include(i => i.Refunds)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, ct);
    }

    public async Task<List<Invoice>> GetByShiftIdAsync(Guid shiftId, CancellationToken ct)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.CashierShiftId == shiftId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Invoice>> GetPendingByPatientIdAsync(Guid patientId, CancellationToken ct)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Discounts)
            .Where(i => i.PatientId == patientId && i.Status == InvoiceStatus.Draft)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<string> GetNextInvoiceNumberAsync(int year, CancellationToken ct)
    {
        var prefix = $"HD-{year}-";

        var maxNumber = await _context.Invoices
            .IgnoreQueryFilters()
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .Select(i => (string?)i.InvoiceNumber)
            .MaxAsync(ct);

        if (maxNumber is null)
        {
            return $"HD-{year}-00001";
        }

        // Parse the sequence number from HD-YYYY-NNNNN format
        var sequencePart = maxNumber[prefix.Length..];
        if (int.TryParse(sequencePart, out var currentSequence))
        {
            return $"HD-{year}-{(currentSequence + 1):D5}";
        }

        return $"HD-{year}-00001";
    }

    public void Add(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
    }

    public void Update(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
    }
}
