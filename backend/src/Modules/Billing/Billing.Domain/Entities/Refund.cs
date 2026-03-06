using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Represents a refund request for an invoice or specific line item.
/// Follows a multi-step approval workflow: Requested -> Approved -> Processed (or Rejected).
/// Requires manager/owner approval before processing. Full audit trail maintained.
/// Implements IAuditable for field-level change tracking via AuditInterceptor.
/// </summary>
public class Refund : Entity, IAuditable
{
    /// <summary>Foreign key to the invoice being refunded.</summary>
    public Guid InvoiceId { get; private set; }

    /// <summary>Foreign key to a specific line item for partial refunds (nullable).</summary>
    public Guid? InvoiceLineItemId { get; private set; }

    /// <summary>Refund amount in VND.</summary>
    public decimal Amount { get; private set; }

    /// <summary>Reason for the refund request (required for audit trail).</summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>Current status of this refund through the approval workflow.</summary>
    public RefundStatus Status { get; private set; }

    /// <summary>Foreign key to the user who requested the refund.</summary>
    public Guid RequestedById { get; private set; }

    /// <summary>UTC timestamp when the refund was requested.</summary>
    public DateTime RequestedAt { get; private set; }

    /// <summary>Foreign key to the manager/owner who approved (nullable until acted upon).</summary>
    public Guid? ApprovedById { get; private set; }

    /// <summary>UTC timestamp when the refund was approved/rejected (nullable).</summary>
    public DateTime? ApprovedAt { get; private set; }

    /// <summary>Foreign key to the user who processed the refund (nullable until processed).</summary>
    public Guid? ProcessedById { get; private set; }

    /// <summary>UTC timestamp when the refund was processed (nullable).</summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>Reason for rejection (nullable — set only when rejected).</summary>
    public string? RejectionReason { get; private set; }

    /// <summary>Optional notes about the refund (e.g. processing details).</summary>
    public string? Notes { get; private set; }

    /// <summary>Private constructor for EF Core materialization.</summary>
    private Refund() { }

    /// <summary>
    /// Factory method for creating a new refund request.
    /// Status defaults to Requested — requires manager Approve() before Process().
    /// </summary>
    public static Refund Create(
        Guid invoiceId,
        decimal amount,
        string reason,
        Guid requestedById,
        Guid? invoiceLineItemId = null,
        string? notes = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Refund amount must be positive.", nameof(amount));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Refund reason is required.", nameof(reason));

        return new Refund
        {
            InvoiceId = invoiceId,
            InvoiceLineItemId = invoiceLineItemId,
            Amount = amount,
            Reason = reason,
            Status = RefundStatus.Requested,
            RequestedById = requestedById,
            RequestedAt = DateTime.UtcNow,
            Notes = notes
        };
    }

    /// <summary>
    /// Approves the refund request. Only callable when status is Requested.
    /// </summary>
    /// <param name="managerId">The manager/owner who approved this refund.</param>
    public void Approve(Guid managerId)
    {
        if (Status != RefundStatus.Requested)
            throw new InvalidOperationException(
                $"Cannot approve refund in '{Status}' status. Only Requested refunds can be approved.");

        Status = RefundStatus.Approved;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Rejects the refund request with a mandatory reason. Only callable when status is Requested or Approved.
    /// </summary>
    /// <param name="managerId">The manager/owner who rejected this refund.</param>
    /// <param name="reason">Reason for rejecting the refund.</param>
    public void Reject(Guid managerId, string reason)
    {
        if (Status is not (RefundStatus.Requested or RefundStatus.Approved))
            throw new InvalidOperationException(
                $"Cannot reject refund in '{Status}' status. Only Requested or Approved refunds can be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required.", nameof(reason));

        Status = RefundStatus.Rejected;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason;
        SetUpdatedAt();
    }

    /// <summary>
    /// Processes the approved refund. Only callable when status is Approved.
    /// </summary>
    /// <param name="userId">The user who processed the refund (typically cashier).</param>
    public void Process(Guid userId)
    {
        if (Status != RefundStatus.Approved)
            throw new InvalidOperationException(
                $"Cannot process refund in '{Status}' status. Only Approved refunds can be processed.");

        Status = RefundStatus.Processed;
        ProcessedById = userId;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
