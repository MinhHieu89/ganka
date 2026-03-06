using Patient.Domain.Enums;

namespace Patient.Domain.Services;

/// <summary>
/// Pure domain service that validates patient fields by context.
/// No dependencies — easy to test and reuse across application features.
/// Returns a tuple to avoid Domain depending on Contracts for DTO types.
/// The Application layer maps this tuple to PatientFieldValidationResult (Contracts.Dtos).
/// </summary>
public static class PatientFieldValidator
{
    /// <summary>
    /// Validates that required patient fields are present for the given context.
    /// Registration context allows nulls; Referral, LegalExport, and SoYTeReporting require Address and CCCD.
    /// </summary>
    public static (bool IsValid, List<(string FieldName, string RequiredForContext)> MissingFields) Validate(
        string? address,
        string? cccd,
        FieldRequirementContext context)
    {
        // Registration context: all fields are optional
        if (context == FieldRequirementContext.Registration)
        {
            return (true, []);
        }

        var missingFields = new List<(string FieldName, string RequiredForContext)>();
        var contextName = context.ToString();

        if (string.IsNullOrWhiteSpace(address))
        {
            missingFields.Add(("Address", contextName));
        }

        if (string.IsNullOrWhiteSpace(cccd))
        {
            missingFields.Add(("Cccd", contextName));
        }

        return (missingFields.Count == 0, missingFields);
    }
}
