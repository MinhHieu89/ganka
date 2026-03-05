using Patient.Application.Interfaces;
using Patient.Contracts.Dtos;

namespace Patient.Application.Features;

/// <summary>
/// Wolverine handler for retrieving a patient's allergies.
/// Cross-module query used by Clinical module for drug-allergy cross-checking.
/// </summary>
public static class GetPatientAllergiesHandler
{
    public static async Task<List<AllergyDto>> Handle(
        GetPatientAllergiesQuery query,
        IPatientRepository patientRepository,
        CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(query.PatientId, cancellationToken);
        if (patient is null)
            return [];

        return patient.Allergies
            .Select(a => new AllergyDto(a.Id, a.Name, a.Severity))
            .ToList();
    }
}
