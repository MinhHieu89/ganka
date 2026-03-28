using Patient.Application.Interfaces;
using Patient.Application.Mappers;
using Patient.Contracts.Dtos;
using Shared.Domain;

namespace Patient.Application.Features;

public sealed record GetPatientByIdQuery(Guid PatientId);

/// <summary>
/// Wolverine handler for retrieving a patient by ID with allergies.
/// </summary>
public static class GetPatientByIdHandler
{
    public static async Task<Result<PatientDto>> Handle(
        GetPatientByIdQuery query,
        IPatientRepository patientRepository,
        CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(query.PatientId, cancellationToken);
        if (patient is null)
            return Result<PatientDto>.Failure(Error.NotFound("Patient", query.PatientId));

        var dto = MapToDto(patient);
        return dto;
    }

    internal static PatientDto MapToDto(Domain.Entities.Patient patient)
    {
        return new PatientDto(
            patient.Id,
            patient.FullName,
            patient.Phone,
            patient.PatientCode,
            patient.PatientType.ToContractEnum(),
            patient.DateOfBirth,
            patient.Gender?.ToContractEnum(),
            patient.Address,
            patient.Cccd,
            patient.Email,
            patient.Occupation,
            patient.PhotoUrl,
            patient.IsActive,
            patient.CreatedAt,
            patient.Allergies.Select(a => new AllergyDto(a.Id, a.Name, a.Severity.ToContractEnum())).ToList(),
            patient.OcularHistory,
            patient.SystemicHistory,
            patient.CurrentMedications,
            patient.ScreenTimeHours,
            patient.WorkEnvironment?.ToString(),
            patient.ContactLensUsage?.ToString(),
            patient.LifestyleNotes);
    }
}
