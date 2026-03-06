using Billing.Domain.Enums;
using Billing.Domain.Events;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Invoice aggregate root. Accumulates charges from all departments during a visit.
/// Implements IAuditable for automatic price change audit logging.
/// </summary>
public class Invoice : AggregateRoot, IAuditable
{
    private readonly List<InvoiceLineItem> _lineItems = [];
    private readonly List<Payment> _payments = [];
    private readonly List<Discount> _discounts = [];
    private readonly List<Refund> _refunds = [];

    public string InvoiceNumber { get; private set; } = default!;
    public Guid? VisitId { get; private set; }
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = default!;
    public InvoiceStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public Guid? CashierShiftId { get; private set; }
    public Guid? FinalizedById { get; private set; }
    public DateTime? FinalizedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public decimal BalanceDue => TotalAmount - PaidAmount;
    public bool IsFullyPaid => BalanceDue <= 0;

    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();
    public IReadOnlyList<Discount> Discounts => _discounts.AsReadOnly();
    public IReadOnlyList<Refund> Refunds => _refunds.AsReadOnly();

    private Invoice() { }

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
            Status = InvoiceStatus.Draft
        };
        invoice.SetBranchId(branchId);
        return invoice;
    }

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
    }

    public void RemoveLineItem(Guid lineItemId)
    {
        EnsureDraft();
        var item = _lineItems.FirstOrDefault(li => li.Id == lineItemId)
            ?? throw new InvalidOperationException($"Line item {lineItemId} not found.");
        _lineItems.Remove(item);
        RecalculateTotals();
    }

    public void RecordPayment(Payment payment)
    {
        EnsureDraft();
        _payments.Add(payment);
        PaidAmount = _payments.Where(p => p.Status == PaymentStatus.Confirmed).Sum(p => p.Amount);
        SetUpdatedAt();
    }

    public void ApplyDiscount(Discount discount)
    {
        EnsureDraft();
        _discounts.Add(discount);
        SetUpdatedAt();
    }

    public void Finalize(Guid cashierShiftId, Guid userId)
    {
        EnsureDraft();
        if (!IsFullyPaid)
            throw new InvalidOperationException("Invoice must be fully paid before finalization.");

        CashierShiftId = cashierShiftId;
        FinalizedById = userId;
        FinalizedAt = DateTime.UtcNow;
        Status = InvoiceStatus.Finalized;
        SetUpdatedAt();

        AddDomainEvent(new InvoiceFinalizedEvent(Id, InvoiceNumber, TotalAmount));
    }

    public void Void()
    {
        Status = InvoiceStatus.Voided;
        SetUpdatedAt();
    }

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Invoice must be in Draft status for this operation.");
    }

    private void RecalculateTotals()
    {
        SubTotal = _lineItems.Sum(li => li.LineTotal);
        DiscountTotal = _discounts
            .Where(d => d.ApprovalStatus == ApprovalStatus.Approved)
            .Sum(d => d.CalculatedAmount);
        TotalAmount = SubTotal - DiscountTotal;
        SetUpdatedAt();
    }
}
