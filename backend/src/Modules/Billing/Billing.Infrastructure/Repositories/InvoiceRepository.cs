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

    public async Task<string> GetNextInvoiceNumberAsync(int year, CancellationToken ct)
    {
        var prefix = $"HD-{year}-";

        var lastInvoice = await context.Invoices
            .IgnoreQueryFilters()
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        var nextSequence = 1;
        if (lastInvoice is not null)
        {
            var sequencePart = lastInvoice[(prefix.Length)..];
            if (int.TryParse(sequencePart, out var currentSequence))
                nextSequence = currentSequence + 1;
        }

        return $"{prefix}{nextSequence:D5}";
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
