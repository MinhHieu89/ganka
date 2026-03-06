using Billing.Domain.Entities;

namespace Billing.Application.Interfaces;

/// <summary>
/// Repository interface for Invoice aggregate persistence.
/// Supports eager loading of line items, payments, discounts, and refunds.
/// </summary>
public interface IInvoiceRepository
{
    /// <summary>
    /// Gets an invoice by ID with all child entities (line items, payments, discounts, refunds) loaded.
    /// </summary>
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets the invoice for a specific visit (progressive invoice lookup).
    /// Returns null if no invoice exists for the visit yet.
    /// </summary>
    Task<Invoice?> GetByVisitIdAsync(Guid visitId, CancellationToken ct);

    /// <summary>
    /// Gets all invoices for a specific visit (summary list).
    /// Returns empty list if no invoices exist for the visit.
    /// </summary>
    Task<List<Invoice>> GetAllByVisitIdAsync(Guid visitId, CancellationToken ct);

    /// <summary>
    /// Gets an invoice by its formatted invoice number (e.g., HD-2026-00001).
    /// </summary>
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct);

    /// <summary>
    /// Gets all invoices associated with a cashier shift.
    /// Used for shift reports and reconciliation.
    /// </summary>
    Task<List<Invoice>> GetByShiftIdAsync(Guid shiftId, CancellationToken ct);

    /// <summary>
    /// Gets draft (unpaid/pending) invoices for a specific patient.
    /// </summary>
    Task<List<Invoice>> GetPendingByPatientIdAsync(Guid patientId, CancellationToken ct);

    /// <summary>
    /// Generates the next sequential invoice number for the given year.
    /// Format: HD-YYYY-NNNNN (e.g., HD-2026-00001).
    /// </summary>
    Task<string> GetNextInvoiceNumberAsync(int year, CancellationToken ct);

    /// <summary>
    /// Gets all draft invoices, optionally filtered by cashier shift.
    /// Used for the cashier dashboard pending invoices panel.
    /// </summary>
    Task<List<Invoice>> GetPendingAsync(Guid? cashierShiftId, CancellationToken ct);

    /// <summary>
    /// Adds a new invoice to the change tracker.
    /// </summary>
    void Add(Invoice invoice);

    /// <summary>
    /// Marks an existing invoice as modified in the change tracker.
    /// </summary>
    void Update(Invoice invoice);
}
