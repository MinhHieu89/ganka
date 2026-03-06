using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Represents a discount applied to an invoice or a specific line item.
/// Supports both percentage and fixed-amount discounts with manager approval workflow.
/// Implements IAuditable for field-level change tracking via AuditInterceptor.
/// </summary>
public class Discount : Entity, IAuditable
{
    /// <summary>Foreign key to the invoice this discount applies to.</summary>
    public Guid InvoiceId { get; private set; }

    /// <summary>Foreign key to a specific line item (nullable — null means invoice-level discount).</summary>
    public Guid? InvoiceLineItemId { get; private set; }

    /// <summary>Discount type: Percentage (0-100) or FixedAmount (VND).</summary>
    public DiscountType Type { get; private set; }

    /// <summary>Discount value: percentage (0-100) or fixed amount in VND.</summary>
    public decimal Value { get; private set; }

    /// <summary>Calculated VND amount actually deducted from the invoice/line item.</summary>
    public decimal CalculatedAmount { get; private set; }

    /// <summary>Reason for applying the discount (required for audit trail).</summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>Current approval status of this discount request.</summary>
    public ApprovalStatus ApprovalStatus { get; private set; }

    /// <summary>Foreign key to the user who requested the discount (typically cashier).</summary>
    public Guid RequestedById { get; private set; }

    /// <summary>UTC timestamp when the discount was requested.</summary>
    public DateTime RequestedAt { get; private set; }

    /// <summary>Foreign key to the manager who approved/rejected (nullable until acted upon).</summary>
    public Guid? ApprovedById { get; private set; }

    /// <summary>UTC timestamp when the discount was approved/rejected (nullable).</summary>
    public DateTime? ApprovedAt { get; private set; }

    /// <summary>Reason for rejection (nullable — set only when rejected).</summary>
    public string? RejectionReason { get; private set; }

    /// <summary>Private constructor for EF Core materialization.</summary>
    private Discount() { }

    /// <summary>
    /// Factory method for creating a new discount request.
    /// ApprovalStatus defaults to Pending — requires manager Approve() or Reject().
    /// </summary>
    public static Discount Create(
        Guid invoiceId,
        DiscountType type,
        decimal value,
        string reason,
        Guid requestedById,
        Guid? invoiceLineItemId = null)
    {
        if (value <= 0)
            throw new ArgumentException("Discount value must be positive.", nameof(value));

        if (type == DiscountType.Percentage && value > 100)
            throw new ArgumentException(
                "Percentage discount cannot exceed 100.", nameof(value));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Discount reason is required.", nameof(reason));

        return new Discount
        {
            InvoiceId = invoiceId,
            InvoiceLineItemId = invoiceLineItemId,
            Type = type,
            Value = value,
            CalculatedAmount = 0,
            Reason = reason,
            ApprovalStatus = ApprovalStatus.Pending,
            RequestedById = requestedById,
            RequestedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Approves the discount request. Only callable when status is Pending.
    /// </summary>
    /// <param name="managerId">The manager who approved this discount.</param>
    public void Approve(Guid managerId)
    {
        if (ApprovalStatus != ApprovalStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot approve discount in '{ApprovalStatus}' status. Only Pending discounts can be approved.");

        ApprovalStatus = ApprovalStatus.Approved;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Rejects the discount request with a mandatory reason. Only callable when status is Pending.
    /// </summary>
    /// <param name="managerId">The manager who rejected this discount.</param>
    /// <param name="reason">Reason for rejecting the discount.</param>
    public void Reject(Guid managerId, string reason)
    {
        if (ApprovalStatus != ApprovalStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot reject discount in '{ApprovalStatus}' status. Only Pending discounts can be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required.", nameof(reason));

        ApprovalStatus = ApprovalStatus.Rejected;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason;
        SetUpdatedAt();
    }

    /// <summary>
    /// Calculates the actual VND amount to deduct based on the discount type and base amount.
    /// For Percentage: CalculatedAmount = baseAmount * Value / 100.
    /// For FixedAmount: CalculatedAmount = Value (capped at baseAmount).
    /// </summary>
    /// <param name="baseAmount">The base amount (invoice total or line item amount) to calculate against.</param>
    public void CalculateAmount(decimal baseAmount)
    {
        if (baseAmount < 0)
            throw new ArgumentException("Base amount cannot be negative.", nameof(baseAmount));

        CalculatedAmount = Type switch
        {
            DiscountType.Percentage => Math.Round(baseAmount * Value / 100, 0),
            DiscountType.FixedAmount => Math.Min(Value, baseAmount),
            _ => throw new InvalidOperationException($"Unknown discount type: {Type}")
        };

        SetUpdatedAt();
    }
}
