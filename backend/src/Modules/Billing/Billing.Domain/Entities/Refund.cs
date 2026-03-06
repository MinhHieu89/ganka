using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Refund entity with multi-step approval workflow.
/// Requires manager/owner approval before processing.
/// </summary>
public class Refund : Entity, IAuditable
{
    public Guid InvoiceId { get; private set; }
    public Guid? InvoiceLineItemId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } = default!;
    public RefundStatus Status { get; private set; }
    public Guid RequestedById { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ProcessedById { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? Notes { get; private set; }

    private Refund() { }

    public static Refund Create(
        Guid invoiceId,
        decimal amount,
        string reason,
        Guid requestedById,
        Guid? invoiceLineItemId = null,
        string? notes = null)
    {
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

    public void Approve(Guid managerId)
    {
        if (Status != RefundStatus.Requested)
            throw new InvalidOperationException("Only requested refunds can be approved.");

        Status = RefundStatus.Approved;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Reject(Guid managerId, string reason)
    {
        if (Status != RefundStatus.Requested)
            throw new InvalidOperationException("Only requested refunds can be rejected.");

        Status = RefundStatus.Rejected;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason;
        SetUpdatedAt();
    }

    public void Process(Guid userId)
    {
        if (Status != RefundStatus.Approved)
            throw new InvalidOperationException("Only approved refunds can be processed.");

        Status = RefundStatus.Processed;
        ProcessedById = userId;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
