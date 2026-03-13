using Optical.Domain.Enums;
using Shared.Domain;

namespace Optical.Domain.Entities;

/// <summary>
/// Entity representing a warranty claim filed against a delivered glasses order.
/// Warranty period is 12 months from the order's delivery date.
/// Resolution workflow: Repair and Discount are handled directly by optical staff;
/// Replace requires manager approval (see RequiresApproval property).
/// Supporting documents (photos, receipts) are stored as Azure Blob URLs.
/// </summary>
public class WarrantyClaim : Entity, IAuditable
{
    private readonly List<string> _documentUrls = [];

    /// <summary>Foreign key to the GlassesOrder being claimed under warranty.</summary>
    public Guid GlassesOrderId { get; private set; }

    /// <summary>Date the warranty claim was filed by the patient or recorded by staff.</summary>
    public DateTime ClaimDate { get; private set; }

    /// <summary>
    /// How the warranty claim will be resolved.
    /// Replace requires manager approval; Repair and Discount can be processed immediately.
    /// </summary>
    public WarrantyResolution Resolution { get; private set; }

    /// <summary>
    /// Approval status for claims requiring manager authorization.
    /// Claims with Resolution != Replace should never transition beyond Pending in normal flow.
    /// </summary>
    public WarrantyApprovalStatus ApprovalStatus { get; private set; }

    /// <summary>
    /// Staff assessment notes describing the defect, patient complaint, and recommended action.
    /// May also contain rejection reasons (appended on Reject call).
    /// </summary>
    public string AssessmentNotes { get; private set; } = string.Empty;

    /// <summary>
    /// Discount amount in VND for Discount resolution type.
    /// Null for Replace and Repair resolutions.
    /// </summary>
    public decimal? DiscountAmount { get; private set; }

    /// <summary>
    /// Foreign key to the manager who approved or rejected the claim.
    /// Null if the claim is still Pending or does not require approval.
    /// </summary>
    public Guid? ApprovedById { get; private set; }

    /// <summary>UTC timestamp when the manager approved or rejected the claim.</summary>
    public DateTime? ApprovedAt { get; private set; }

    /// <summary>
    /// Azure Blob Storage URLs for supporting photos and documents.
    /// Backed by _documentUrls backing field per EF Core encapsulation pattern.
    /// </summary>
    public IReadOnlyList<string> DocumentUrls => _documentUrls.AsReadOnly();

    /// <summary>Private constructor for EF Core materialization.</summary>
    private WarrantyClaim() { }

    /// <summary>
    /// Factory method for creating a new warranty claim.
    /// Sets ApprovalStatus to Pending — caller should check RequiresApproval to determine workflow.
    /// </summary>
    /// <param name="glassesOrderId">The glasses order being claimed against.</param>
    /// <param name="claimDate">Date the claim is being filed.</param>
    /// <param name="resolution">How the claim will be resolved (Replace/Repair/Discount).</param>
    /// <param name="assessmentNotes">Staff assessment of the defect and recommended resolution.</param>
    /// <param name="discountAmount">Amount in VND for Discount resolution (required when resolution is Discount).</param>
    public static WarrantyClaim Create(
        Guid glassesOrderId,
        DateTime claimDate,
        WarrantyResolution resolution,
        string assessmentNotes,
        decimal? discountAmount)
    {
        if (glassesOrderId == Guid.Empty)
            throw new ArgumentException("Glasses order ID is required.", nameof(glassesOrderId));

        if (claimDate > DateTime.UtcNow.AddMinutes(5))
            throw new ArgumentException("Claim date cannot be in the future.", nameof(claimDate));

        if (string.IsNullOrWhiteSpace(assessmentNotes))
            throw new ArgumentException("Assessment notes are required.", nameof(assessmentNotes));

        if (resolution == WarrantyResolution.Discount)
        {
            if (!discountAmount.HasValue || discountAmount.Value <= 0)
                throw new ArgumentException("Discount amount must be greater than zero for Discount resolution.", nameof(discountAmount));
        }

        return new WarrantyClaim
        {
            GlassesOrderId = glassesOrderId,
            ClaimDate = claimDate,
            Resolution = resolution,
            ApprovalStatus = WarrantyApprovalStatus.Pending,
            AssessmentNotes = assessmentNotes,
            DiscountAmount = resolution == WarrantyResolution.Discount ? discountAmount : null
        };
    }

    /// <summary>
    /// Records manager approval for a warranty claim requiring authorization.
    /// Sets ApprovalStatus to Approved and captures the approving manager's identity and timestamp.
    /// </summary>
    /// <param name="approvedById">The manager's user ID authorizing the claim.</param>
    /// <exception cref="InvalidOperationException">Thrown if claim is not in Pending status.</exception>
    public void Approve(Guid approvedById)
    {
        if (ApprovalStatus != WarrantyApprovalStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot approve a warranty claim that is already {ApprovalStatus}.");

        ApprovalStatus = WarrantyApprovalStatus.Approved;
        ApprovedById = approvedById;
        ApprovedAt = DateTime.UtcNow;

        SetUpdatedAt();
    }

    /// <summary>
    /// Records manager rejection of a warranty claim.
    /// Appends the rejection reason to assessment notes for a full audit trail.
    /// </summary>
    /// <param name="rejectedById">The manager's user ID rejecting the claim.</param>
    /// <param name="reason">Reason for rejection, appended to AssessmentNotes.</param>
    /// <exception cref="InvalidOperationException">Thrown if claim is not in Pending status.</exception>
    public void Reject(Guid rejectedById, string reason)
    {
        if (ApprovalStatus != WarrantyApprovalStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot reject a warranty claim that is already {ApprovalStatus}.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required.", nameof(reason));

        ApprovalStatus = WarrantyApprovalStatus.Rejected;
        ApprovedById = rejectedById;
        ApprovedAt = DateTime.UtcNow;
        AssessmentNotes += $"\n[Rejected] {reason}";

        SetUpdatedAt();
    }

    /// <summary>
    /// Adds an Azure Blob Storage URL for a supporting document (photo, receipt, etc.).
    /// </summary>
    /// <param name="url">The fully qualified Azure Blob URL for the uploaded document.</param>
    /// <exception cref="ArgumentException">Thrown if url is null or whitespace.</exception>
    public void AddDocumentUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Document URL cannot be empty.", nameof(url));

        _documentUrls.Add(url);
        SetUpdatedAt();
    }

    /// <summary>
    /// Whether this claim requires manager approval before processing.
    /// Only Replace resolutions require authorization — Repair and Discount are staff-level decisions.
    /// </summary>
    public bool RequiresApproval => Resolution == WarrantyResolution.Replace;
}
