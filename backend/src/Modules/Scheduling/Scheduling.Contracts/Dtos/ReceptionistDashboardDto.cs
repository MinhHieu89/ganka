namespace Scheduling.Contracts.Dtos;

/// <summary>
/// Paginated wrapper for receptionist dashboard results.
/// </summary>
public sealed record ReceptionistDashboardDto(
    List<ReceptionistDashboardRowDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
