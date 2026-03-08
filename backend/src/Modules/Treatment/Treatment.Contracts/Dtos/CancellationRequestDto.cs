namespace Treatment.Contracts.Dtos;

/// <summary>
/// DTO representing a cancellation request for a treatment package.
/// Status is string representation of cancellation request status enum.
/// </summary>
public sealed record CancellationRequestDto(
    Guid Id,
    Guid RequestedById,
    string RequestedByName,
    DateTime RequestedAt,
    string Reason,
    decimal DeductionPercent,
    decimal RefundAmount,
    string Status,
    Guid? ApprovedById,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    string? RejectionReason);
