using Patient.Application.Interfaces;
using Patient.Contracts.Dtos;
using Patient.Domain.Enums;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record GetPatientListQuery(
    int Page = 1,
    int PageSize = 20,
    Gender? Gender = null,
    bool? HasAllergies = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? Search = null);

/// <summary>
/// Wolverine handler for paginated patient listing with optional filters.
/// </summary>
public static class GetPatientListHandler
{
    public static async Task<Result<PagedResult<PatientDto>>> Handle(
        GetPatientListQuery query,
        IPatientRepository patientRepository,
        CancellationToken cancellationToken)
    {
        var (patients, totalCount) = await patientRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Gender,
            query.HasAllergies,
            query.DateFrom,
            query.DateTo,
            query.Search,
            cancellationToken);

        var items = patients.Select(GetPatientByIdHandler.MapToDto).ToList();

        return new PagedResult<PatientDto>(items, totalCount, query.Page, query.PageSize);
    }
}
