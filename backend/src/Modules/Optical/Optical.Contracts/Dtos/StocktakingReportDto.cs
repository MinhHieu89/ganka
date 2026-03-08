namespace Optical.Contracts.Dtos;

/// <summary>
/// Stocktaking session DTO for API responses.
/// Status is int-serialized Optical.Domain.Enums.StocktakingStatus.
/// </summary>
public sealed record StocktakingSessionDto(
    Guid Id,
    string Name,
    int Status,
    Guid StartedById,
    string? StartedByName,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    int TotalItemsScanned,
    int DiscrepancyCount,
    string? Notes);

/// <summary>
/// Individual item scanned during a stocktaking session.
/// Discrepancy = PhysicalCount - SystemCount (positive = over, negative = under).
/// </summary>
public sealed record StocktakingItemDto(
    Guid Id,
    Guid StocktakingSessionId,
    string Barcode,
    Guid? FrameId,
    string? FrameName,
    int PhysicalCount,
    int SystemCount,
    int Discrepancy);

/// <summary>
/// Summary discrepancy report for a completed stocktaking session.
/// OverCount = items with physical count greater than system count.
/// UnderCount = items with physical count less than system count.
/// MissingFromSystem = items scanned but not found in system.
/// </summary>
public sealed record DiscrepancyReportDto(
    Guid SessionId,
    string SessionName,
    DateTime? CompletedAt,
    int TotalScanned,
    int TotalDiscrepancies,
    int OverCount,
    int UnderCount,
    int MissingFromSystem,
    List<StocktakingItemDto> Items);
