using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPaymentRepository"/>.
/// Supports querying payments by invoice, shift, and treatment package.
/// </summary>
public sealed class PaymentRepository : IPaymentRepository
{
    private readonly BillingDbContext _context;

    public PaymentRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Set<Payment>()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken ct)
    {
        return await _context.Set<Payment>()
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.RecordedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Payment>> GetByShiftIdAsync(Guid shiftId, CancellationToken ct)
    {
        return await _context.Set<Payment>()
            .Where(p => p.CashierShiftId == shiftId)
            .OrderByDescending(p => p.RecordedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Payment>> GetByTreatmentPackageIdAsync(Guid treatmentPackageId, CancellationToken ct)
    {
        return await _context.Set<Payment>()
            .Where(p => p.TreatmentPackageId == treatmentPackageId)
            .OrderBy(p => p.SplitSequence)
            .ThenByDescending(p => p.RecordedAt)
            .ToListAsync(ct);
    }

    public void Add(Payment payment)
    {
        _context.Set<Payment>().Add(payment);
    }

    public void Update(Payment payment)
    {
        _context.Set<Payment>().Update(payment);
    }
}
