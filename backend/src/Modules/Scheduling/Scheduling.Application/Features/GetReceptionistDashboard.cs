using Scheduling.Contracts.Dtos;

namespace Scheduling.Application.Features;

/// <summary>
/// Query to get the receptionist dashboard (today's patient queue with 4-status mapping).
/// Full handler implementation provided by plan 14-02.
/// </summary>
public record GetReceptionistDashboardQuery(string? StatusFilter, string? Search, int Page, int PageSize);

/// <summary>
/// Result wrapper for the receptionist dashboard query.
/// </summary>
public record ReceptionistDashboardResult(
    List<ReceptionistDashboardRowDto> Items,
    int TotalCount);
