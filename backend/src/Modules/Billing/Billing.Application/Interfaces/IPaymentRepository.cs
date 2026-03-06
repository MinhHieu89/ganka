using Billing.Domain.Entities;

namespace Billing.Application.Interfaces;

/// <summary>
/// Repository interface for Payment entity persistence.
/// Supports querying by invoice, shift, and treatment package for reconciliation.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Gets a payment by ID.
    /// </summary>
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all payments for a specific invoice.
    /// </summary>
    Task<List<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken ct);

    /// <summary>
    /// Gets all payments recorded during a specific cashier shift.
    /// Used for shift reconciliation and reporting.
    /// </summary>
    Task<List<Payment>> GetByShiftIdAsync(Guid shiftId, CancellationToken ct);

    /// <summary>
    /// Gets all payments associated with a treatment package.
    /// Used for tracking 50/50 split payment progress.
    /// </summary>
    Task<List<Payment>> GetByTreatmentPackageIdAsync(Guid treatmentPackageId, CancellationToken ct);

    /// <summary>
    /// Adds a new payment to the change tracker.
    /// </summary>
    void Add(Payment payment);

    /// <summary>
    /// Marks an existing payment as modified in the change tracker.
    /// </summary>
    void Update(Payment payment);
}
