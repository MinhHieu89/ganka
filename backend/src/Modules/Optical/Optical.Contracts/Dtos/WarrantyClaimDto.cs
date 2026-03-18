namespace Optical.Contracts.Dtos;

/// <summary>
/// Full warranty claim DTO for API responses.
/// Resolution is int-serialized Optical.Domain.Enums.WarrantyResolution.
/// ApprovalStatus is int-serialized Optical.Domain.Enums.WarrantyApprovalStatus.
/// </summary>
public sealed record WarrantyClaimDto(
    Guid Id,
    Guid GlassesOrderId,
    string? PatientName,
    DateTime ClaimDate,
    int Resolution,
    int ApprovalStatus,
    string AssessmentNotes,
    decimal? DiscountAmount,
    Guid? ApprovedById,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    List<string> DocumentUrls,
    bool RequiresApproval,
    DateTime CreatedAt);

/// <summary>
/// Lightweight warranty claim summary for list views.
/// </summary>
public sealed record WarrantyClaimSummaryDto(
    Guid Id,
    Guid GlassesOrderId,
    string? PatientName,
    DateTime ClaimDate,
    int Resolution,
    int ApprovalStatus,
    bool RequiresApproval,
    string AssessmentNotes,
    DateTime CreatedAt);
