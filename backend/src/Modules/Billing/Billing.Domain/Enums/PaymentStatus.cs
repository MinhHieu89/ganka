namespace Billing.Domain.Enums;

/// <summary>
/// Status of a payment transaction.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Confirmed = 1,
    Refunded = 2
}

/// <summary>
/// Approval status for discounts requiring manager authorization.
/// </summary>
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>
/// Status of a refund request through the approval workflow.
/// </summary>
public enum RefundStatus
{
    Requested = 0,
    Approved = 1,
    Processed = 2,
    Rejected = 3
}
