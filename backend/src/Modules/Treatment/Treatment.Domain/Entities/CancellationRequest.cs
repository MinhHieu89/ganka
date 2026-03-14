using Treatment.Domain.Enums;

namespace Treatment.Domain.Entities;

/// <summary>
/// Represents a cancellation request for a treatment package (TRT-09).
/// Follows the Billing.Refund approval pattern: Requested -> Approved/Rejected.
/// Manager PIN verification is handled at the application layer.
/// </summary>
public class CancellationRequest
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>FK to the parent TreatmentPackage.</summary>
    public Guid TreatmentPackageId { get; private set; }

    /// <summary>Current status of the cancellation request.</summary>
    public CancellationRequestStatus Status { get; private set; }

    /// <summary>Reason for the cancellation request.</summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>Deduction percentage applied on cancellation (10-20%).</summary>
    public decimal DeductionPercent { get; private set; }

    /// <summary>Calculated refund amount after deduction.</summary>
    public decimal RefundAmount { get; private set; }

    /// <summary>User who requested the cancellation.</summary>
    public Guid RequestedById { get; private set; }

    /// <summary>When the cancellation was requested.</summary>
    public DateTime RequestedAt { get; private set; }

    /// <summary>Manager who approved/rejected the request.</summary>
    public Guid? ProcessedById { get; private set; }

    /// <summary>When the request was processed.</summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>Manager's note on approval/rejection.</summary>
    public string? ProcessingNote { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private CancellationRequest() { }

    /// <summary>
    /// Factory method to create a new cancellation request.
    /// </summary>
    public static CancellationRequest Create(
        Guid treatmentPackageId,
        string reason,
        decimal deductionPercent,
        decimal refundAmount,
        Guid requestedById)
    {
        return new CancellationRequest
        {
            Id = Guid.NewGuid(),
            TreatmentPackageId = treatmentPackageId,
            Status = CancellationRequestStatus.Requested,
            Reason = reason,
            DeductionPercent = deductionPercent,
            RefundAmount = refundAmount,
            RequestedById = requestedById,
            RequestedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the deduction percentage and recalculates the refund amount.
    /// Used when a manager overrides the deduction at approval time.
    /// </summary>
    public void UpdateDeduction(decimal deductionPercent, decimal refundAmount)
    {
        if (Status != CancellationRequestStatus.Requested)
            throw new InvalidOperationException(
                $"Cannot update deduction for a cancellation request in '{Status}' status.");

        DeductionPercent = deductionPercent;
        RefundAmount = refundAmount;
    }

    /// <summary>
    /// Approves the cancellation request.
    /// </summary>
    public void Approve(Guid processedById, string? note)
    {
        if (Status != CancellationRequestStatus.Requested)
            throw new InvalidOperationException(
                $"Cannot approve a cancellation request in '{Status}' status.");

        Status = CancellationRequestStatus.Approved;
        ProcessedById = processedById;
        ProcessedAt = DateTime.UtcNow;
        ProcessingNote = note;
    }

    /// <summary>
    /// Rejects the cancellation request.
    /// </summary>
    public void Reject(Guid processedById, string? note)
    {
        if (Status != CancellationRequestStatus.Requested)
            throw new InvalidOperationException(
                $"Cannot reject a cancellation request in '{Status}' status.");

        Status = CancellationRequestStatus.Rejected;
        ProcessedById = processedById;
        ProcessedAt = DateTime.UtcNow;
        ProcessingNote = note;
    }
}
