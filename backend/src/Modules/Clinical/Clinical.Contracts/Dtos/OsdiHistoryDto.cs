namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for a single OSDI history entry, used in the OSDI trend chart.
/// </summary>
public sealed record OsdiHistoryDto(
    Guid VisitId,
    DateTime VisitDate,
    decimal OsdiScore,
    int Severity);

/// <summary>
/// Response containing a list of OSDI history entries for trend analysis.
/// </summary>
public sealed record OsdiHistoryResponse(
    List<OsdiHistoryDto> Items);
