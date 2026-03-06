using Billing.Domain.Enums;
using Billing.Domain.Events;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Invoice aggregate root. Central billing entity that accumulates charges from all departments
/// during a visit. Supports progressive line item building, payment tracking, and discount management.
/// Implements IAuditable for automatic price change audit logging (FIN-09).
/// </summary>
public class Invoice : AggregateRoot, IAuditable
{
    private readonly List<InvoiceLineItem> _lineItems = [];
    private readonly List<Payment> _payments = [];
    private readonly List<Discount> _discounts = [];
    private readonly List<Refund> _refunds = [];

    /// <summary>Unique invoice number for display and reference.</summary>
    public string InvoiceNumber { get; private set; } = string.Empty;

    /// <summary>Visit this invoice is linked to (nullable for OTC sales).</summary>
    public Guid? VisitId { get; private set; }

    /// <summary>Patient this invoice belongs to.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>Denormalized patient name to avoid cross-module joins.</summary>
    public string PatientName { get; private set; } = string.Empty;

    /// <summary>Current invoice lifecycle status.</summary>
    public InvoiceStatus Status { get; private set; }

    /// <summary>Sum of all line item totals before discounts.</summary>
    public decimal SubTotal { get; private set; }

    /// <summary>Sum of all applied discount amounts.</summary>
    public decimal DiscountTotal { get; private set; }

    /// <summary>Final amount after discounts: SubTotal - DiscountTotal.</summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>Sum of all confirmed payment amounts.</summary>
    public decimal PaidAmount { get; private set; }

    /// <summary>Cashier shift this invoice was finalized in (nullable until finalized).</summary>
    public Guid? CashierShiftId { get; private set; }

    /// <summary>User who finalized this invoice (nullable until finalized).</summary>
    public Guid? FinalizedById { get; private set; }

    /// <summary>UTC timestamp when the invoice was finalized (nullable until finalized).</summary>
    public DateTime? FinalizedAt { get; private set; }

    /// <summary>Concurrency token for optimistic concurrency control.</summary>
    public byte[] RowVersion { get; private set; } = [];

    /// <summary>Remaining balance: TotalAmount - PaidAmount.</summary>
    public decimal BalanceDue => TotalAmount - PaidAmount;

    /// <summary>Whether the invoice has been fully paid.</summary>
    public bool IsFullyPaid => BalanceDue <= 0;

    /// <summary>Line items on this invoice.</summary>
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    /// <summary>Payments recorded against this invoice.</summary>
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();

    /// <summary>Discounts applied to this invoice.</summary>
    public IReadOnlyList<Discount> Discounts => _discounts.AsReadOnly();

    /// <summary>Refunds processed for this invoice.</summary>
    public IReadOnlyList<Refund> Refunds => _refunds.AsReadOnly();

    /// <summary>Private constructor for EF Core materialization.</summary>
    private Invoice() { }

    /// <summary>
    /// Factory method for creating a new draft invoice.
    /// </summary>
    public static Invoice Create(
        string invoiceNumber,
        Guid patientId,
        string patientName,
        Guid? visitId,
        BranchId branchId)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            PatientId = patientId,
            PatientName = patientName,
            VisitId = visitId,
            Status = InvoiceStatus.Draft,
            SubTotal = 0,
            DiscountTotal = 0,
            TotalAmount = 0,
            PaidAmount = 0
        };

        invoice.SetBranchId(branchId);
        return invoice;
    }

    /// <summary>
    /// Adds a line item to the invoice and recalculates totals.
    /// Only allowed when invoice is in Draft status.
    /// </summary>
    public void AddLineItem(
        string description,
        string? descriptionVi,
        decimal unitPrice,
        int quantity,
        Department department,
        Guid? sourceId = null,
        string? sourceType = null)
    {
        EnsureDraft();

        var lineItem = InvoiceLineItem.Create(
            Id, description, descriptionVi, unitPrice, quantity, department, sourceId, sourceType);

        _lineItems.Add(lineItem);
        RecalculateTotals();
        SetUpdatedAt();
    }

    /// <summary>
    /// Removes a line item from the invoice by ID and recalculates totals.
    /// Only allowed when invoice is in Draft status.
    /// </summary>
    public void RemoveLineItem(Guid lineItemId)
    {
        EnsureDraft();

        var lineItem = _lineItems.FirstOrDefault(li => li.Id == lineItemId)
            ?? throw new InvalidOperationException($"Line item with ID {lineItemId} not found.");

        _lineItems.Remove(lineItem);
        RecalculateTotals();
        SetUpdatedAt();
    }

    /// <summary>
    /// Records a payment against this invoice.
    /// Recalculates PaidAmount from all confirmed payments.
    /// </summary>
    public void RecordPayment(Payment payment)
    {
        _payments.Add(payment);
        PaidAmount = _payments
            .Where(p => p.Status == PaymentStatus.Confirmed)
            .Sum(p => p.Amount);
        SetUpdatedAt();
    }

    /// <summary>
    /// Applies a discount to this invoice.
    /// Only allowed when invoice is in Draft status.
    /// </summary>
    public void ApplyDiscount(Discount discount)
    {
        EnsureDraft();

        _discounts.Add(discount);
        RecalculateTotals();
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds a refund request to this invoice.
    /// Only allowed when invoice is in Finalized status.
    /// </summary>
    public void AddRefund(Refund refund)
    {
        if (Status != InvoiceStatus.Finalized)
            throw new InvalidOperationException(
                "Refunds can only be requested on finalized invoices.");

        _refunds.Add(refund);
        SetUpdatedAt();
    }

    /// <summary>
    /// Recalculates totals after a discount status change (e.g., approval).
    /// Call this after approving/rejecting a discount to update DiscountTotal and TotalAmount.
    /// </summary>
    public void RecalculateAfterDiscountApproval()
    {
        RecalculateTotals();
        SetUpdatedAt();
    }

    /// <summary>
    /// Finalizes the invoice after full payment.
    /// Sets status to Finalized, records cashier shift and finalizing user,
    /// and raises InvoiceFinalizedEvent.
    /// </summary>
    public void Finalize(Guid cashierShiftId, Guid userId)
    {
        EnsureDraft();

        if (_lineItems.Count == 0)
            throw new InvalidOperationException(
                "Cannot finalize an invoice with no line items.");

        if (TotalAmount <= 0)
            throw new InvalidOperationException(
                "Cannot finalize an invoice with zero or negative total amount.");

        if (!IsFullyPaid)
            throw new InvalidOperationException(
                "Cannot finalize invoice with outstanding balance.");

        Status = InvoiceStatus.Finalized;
        CashierShiftId = cashierShiftId;
        FinalizedById = userId;
        FinalizedAt = DateTime.UtcNow;
        SetUpdatedAt();

        AddDomainEvent(new InvoiceFinalizedEvent(Id, InvoiceNumber, TotalAmount));
    }

    /// <summary>
    /// Voids the invoice, marking it as cancelled.
    /// </summary>
    public void Void()
    {
        if (Status == InvoiceStatus.Voided)
            throw new InvalidOperationException("Invoice is already voided.");

        Status = InvoiceStatus.Voided;
        SetUpdatedAt();
    }

    /// <summary>
    /// Guard method: throws if the invoice is not in Draft status.
    /// </summary>
    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException(
                $"Cannot modify invoice in '{Status}' status. Only Draft invoices can be modified.");
    }

    /// <summary>
    /// Recalculates SubTotal, DiscountTotal, and TotalAmount from current line items and approved discounts.
    /// </summary>
    private void RecalculateTotals()
    {
        SubTotal = _lineItems.Sum(li => li.LineTotal);
        DiscountTotal = _discounts
            .Where(d => d.ApprovalStatus == ApprovalStatus.Approved)
            .Sum(d => d.CalculatedAmount);
        TotalAmount = SubTotal - DiscountTotal;
    }
}
