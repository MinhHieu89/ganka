using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Discount entity with manager approval workflow.
/// Supports both percentage and fixed amount discounts.
/// </summary>
public class Discount : Entity, IAuditable
{
    public Guid InvoiceId { get; private set; }
    public Guid? InvoiceLineItemId { get; private set; }
    public DiscountType Type { get; private set; }
    public decimal Value { get; private set; }
    public decimal CalculatedAmount { get; private set; }
    public string Reason { get; private set; } = default!;
    public ApprovalStatus ApprovalStatus { get; private set; }
    public Guid RequestedById { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private Discount() { }

    public static Discount Create(
        Guid invoiceId,
        DiscountType type,
        decimal value,
        string reason,
        Guid requestedById,
        Guid? invoiceLineItemId = null)
    {
        return new Discount
        {
            InvoiceId = invoiceId,
            InvoiceLineItemId = invoiceLineItemId,
            Type = type,
            Value = value,
            Reason = reason,
            ApprovalStatus = ApprovalStatus.Pending,
            RequestedById = requestedById,
            RequestedAt = DateTime.UtcNow
        };
    }

    public void Approve(Guid managerId)
    {
        ApprovalStatus = ApprovalStatus.Approved;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Reject(Guid managerId, string reason)
    {
        ApprovalStatus = ApprovalStatus.Rejected;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason;
        SetUpdatedAt();
    }

    public void CalculateAmount(decimal baseAmount)
    {
        CalculatedAmount = Type == DiscountType.Percentage
            ? Math.Round(baseAmount * Value / 100m, 0)
            : Value;
        SetUpdatedAt();
    }
}
