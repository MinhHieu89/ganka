using Shared.Domain;
using Treatment.Domain.Enums;

namespace Treatment.Domain.Entities;

/// <summary>
/// Models the manager-approved cancellation workflow for a TreatmentPackage (TRT-09).
/// One-to-one optional relationship with TreatmentPackage.
/// Captures the requester, reason, configurable deduction percentage, and approval status.
/// </summary>
public class CancellationRequest : Entity
{
    /// <summary>FK to the TreatmentPackage being cancelled.</summary>
    public Guid TreatmentPackageId { get; private set; }

    /// <summary>Staff member who initiated the cancellation request.</summary>
    public Guid RequestedById { get; private set; }

    /// <summary>Timestamp when the cancellation was requested.</summary>
    public DateTime RequestedAt { get; private set; }

    /// <summary>Reason for requesting cancellation.</summary>
    public string Reason { get; private set; } = default!;

    /// <summary>
    /// Deduction percentage applied to calculate the refund amount (typically 10-20%).
    /// Defaults from the protocol template but can be overridden by the manager at approval time.
    /// </summary>
    public decimal DeductionPercent { get; private set; }

    /// <summary>Calculated refund amount after applying the deduction. Set at approval time.</summary>
    public decimal RefundAmount { get; private set; }

    /// <summary>Current status of the cancellation request.</summary>
    public CancellationStatus Status { get; private set; }

    /// <summary>Manager who approved or rejected the request (null while Requested).</summary>
    public Guid? ApprovedById { get; private set; }

    /// <summary>Timestamp of approval or rejection (null while Requested).</summary>
    public DateTime? ApprovedAt { get; private set; }

    /// <summary>Reason for rejection (null unless rejected).</summary>
    public string? RejectionReason { get; private set; }

    private CancellationRequest() { }

    /// <summary>
    /// Creates a new cancellation request in Requested status.
    /// </summary>
    /// <param name="treatmentPackageId">The package to cancel.</param>
    /// <param name="requestedById">Staff member initiating the request.</param>
    /// <param name="reason">Reason for cancellation.</param>
    /// <param name="deductionPercent">Default deduction percentage from protocol template.</param>
    public static CancellationRequest Create(
        Guid treatmentPackageId,
        Guid requestedById,
        string reason,
        decimal deductionPercent)
    {
        return new CancellationRequest
        {
            TreatmentPackageId = treatmentPackageId,
            RequestedById = requestedById,
            RequestedAt = DateTime.UtcNow,
            Reason = reason,
            DeductionPercent = deductionPercent,
            Status = CancellationStatus.Requested
        };
    }

    /// <summary>
    /// Approves the cancellation request. Sets the deduction percentage (may differ from default)
    /// and the calculated refund amount.
    /// </summary>
    /// <param name="managerId">Manager approving the request.</param>
    /// <param name="deductionPercent">Final deduction percentage (manager may override the default).</param>
    /// <param name="refundAmount">Calculated refund amount after deduction.</param>
    public void Approve(Guid managerId, decimal deductionPercent, decimal refundAmount)
    {
        if (Status != CancellationStatus.Requested)
            throw new InvalidOperationException("Only requested cancellations can be approved.");

        Status = CancellationStatus.Approved;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        DeductionPercent = deductionPercent;
        RefundAmount = refundAmount;
        SetUpdatedAt();
    }

    /// <summary>
    /// Rejects the cancellation request. The package remains active.
    /// </summary>
    /// <param name="managerId">Manager rejecting the request.</param>
    /// <param name="reason">Reason for rejection.</param>
    public void Reject(Guid managerId, string reason)
    {
        if (Status != CancellationStatus.Requested)
            throw new InvalidOperationException("Only requested cancellations can be rejected.");

        Status = CancellationStatus.Rejected;
        ApprovedById = managerId;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason;
        SetUpdatedAt();
    }
}
