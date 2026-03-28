using Patient.Application.Interfaces;
using Patient.Application.Mappers;
using Patient.Contracts.Dtos;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record GetRecentPatientsQuery(int Count = 10);

/// <summary>
/// Wolverine handler for retrieving recently registered patients.
/// </summary>
public static class GetRecentPatientsHandler
{
    public static async Task<Result<List<PatientSearchResult>>> Handle(
        GetRecentPatientsQuery query,
        IPatientRepository patientRepository,
        CancellationToken cancellationToken)
    {
        var patients = await patientRepository.GetRecentAsync(query.Count, cancellationToken);

        var results = patients.Select(p => new PatientSearchResult(
            p.Id,
            p.FullName,
            p.Phone,
            p.PatientCode,
            p.PatientType.ToContractEnum(),
            p.DateOfBirth?.Year,
            p.Gender?.ToString())).ToList();

        return results;
    }
}
