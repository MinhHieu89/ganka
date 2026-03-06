using Billing.Application.Interfaces;
using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPaymentRepository"/>.
/// Provides CRUD operations for Payment entities.
/// </summary>
public sealed class PaymentRepository(BillingDbContext context) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken ct)
    {
        return await context.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .ToListAsync(ct);
    }

    public async Task<List<Payment>> GetByShiftIdAsync(Guid shiftId, CancellationToken ct)
    {
        return await context.Payments
            .Where(p => p.CashierShiftId == shiftId)
            .ToListAsync(ct);
    }

    public async Task<List<Payment>> GetByTreatmentPackageIdAsync(Guid treatmentPackageId, CancellationToken ct)
    {
        return await context.Payments
            .Where(p => p.TreatmentPackageId == treatmentPackageId)
            .ToListAsync(ct);
    }

    public void Add(Payment payment)
    {
        context.Payments.Add(payment);
    }

    public void Update(Payment payment)
    {
        context.Payments.Update(payment);
    }
}
