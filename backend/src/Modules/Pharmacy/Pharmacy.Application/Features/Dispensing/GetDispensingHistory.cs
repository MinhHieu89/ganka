using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;

namespace Pharmacy.Application.Features.Dispensing;

/// <summary>
/// Query for paginated dispensing history with optional patient filter.
/// </summary>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Number of records per page.</param>
/// <param name="PatientId">Optional patient filter for per-patient dispensing history.</param>
public sealed record GetDispensingHistoryQuery(int Page, int PageSize, Guid? PatientId = null);

/// <summary>
/// DTO for a paginated list of dispensing records.
/// </summary>
public sealed record DispensingHistoryDto(List<DispensingRecordDto> Items, int TotalCount);

/// <summary>
/// Wolverine static handler for retrieving paginated dispensing history.
/// Supports optional patient filter for viewing dispensing records per patient.
/// </summary>
public static class GetDispensingHistoryHandler
{
    public static async Task<Result<DispensingHistoryDto>> Handle(
        GetDispensingHistoryQuery query,
        IDispensingRepository dispensingRepository,
        CancellationToken ct)
    {
        var (items, totalCount) = await dispensingRepository.GetHistoryAsync(
            query.Page,
            query.PageSize,
            query.PatientId,
            ct);

        return new DispensingHistoryDto(items, totalCount);
    }
}
