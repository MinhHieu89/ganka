using Patient.Application.Interfaces;
using Patient.Application.Mappers;
using Patient.Contracts.Dtos;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record SearchPatientsQuery(string Term);

/// <summary>
/// Wolverine handler for Vietnamese diacritics-insensitive patient search.
/// Searches by name (SQL_Latin1_General_Cp1_CI_AI collation), phone prefix, or exact patient code.
/// </summary>
public static class SearchPatientsHandler
{
    public static async Task<Result<List<PatientSearchResult>>> Handle(
        SearchPatientsQuery query,
        IPatientRepository patientRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Term))
            return new List<PatientSearchResult>();

        var patients = await patientRepository.SearchAsync(query.Term.Trim(), 20, cancellationToken);

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
