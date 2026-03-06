namespace Billing.Domain.Enums;

/// <summary>
/// Status of a payment transaction against an invoice.
/// Pending = awaiting confirmation, Confirmed = payment received, Refunded = payment reversed.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Confirmed = 1,
    Refunded = 2
}

/// <summary>
/// Approval status for discount requests.
/// Discounts require manager approval before being applied to an invoice.
/// </summary>
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>
/// Status of a refund request through the multi-step approval workflow.
/// Requested -> Approved -> Processed (or Rejected at any point).
/// </summary>
public enum RefundStatus
{
    Requested = 0,
    Approved = 1,
    Processed = 2,
    Rejected = 3
}
