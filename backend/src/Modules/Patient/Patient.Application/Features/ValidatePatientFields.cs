using Patient.Application.Interfaces;
using Patient.Contracts.Dtos;
using Patient.Domain.Enums;
using Patient.Domain.Services;
using Shared.Domain;

namespace Patient.Application.Features;

/// <summary>
/// Query to validate patient fields against the strictest common context (Referral).
/// Used by the frontend to show missing field warnings on patient profiles.
/// </summary>
public sealed record ValidatePatientFieldsQuery(Guid PatientId);

/// <summary>
/// Wolverine handler for patient field validation.
/// Loads the patient and validates Address/CCCD against the Referral context
/// (the strictest common downstream context).
/// Maps the Domain tuple result to PatientFieldValidationResult (Contracts.Dtos).
/// </summary>
public static class ValidatePatientFieldsHandler
{
    public static async Task<Result<PatientFieldValidationResult>> Handle(
        ValidatePatientFieldsQuery query,
        IPatientRepository patientRepository,
        CancellationToken cancellationToken)
    {
        var patient = await patientRepository.GetByIdAsync(query.PatientId, cancellationToken);
        if (patient is null)
            return Result<PatientFieldValidationResult>.Failure(Error.NotFound("Patient", query.PatientId));

        var (isValid, missingFields) = PatientFieldValidator.Validate(
            patient.Address,
            patient.Cccd,
            FieldRequirementContext.Referral);

        var result = new PatientFieldValidationResult(
            isValid,
            missingFields.Select(f => new MissingFieldInfo(f.FieldName, f.RequiredForContext)).ToList());

        return result;
    }
}
