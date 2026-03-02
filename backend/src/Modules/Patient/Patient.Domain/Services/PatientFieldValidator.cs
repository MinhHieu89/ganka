using Patient.Domain.Enums;

namespace Patient.Domain.Services;

/// <summary>
/// Pure domain service that validates patient fields by context.
/// No dependencies — easy to test and reuse across application features.
/// </summary>
public static class PatientFieldValidator
{
    /// <summary>
    /// Validates that required patient fields are present for the given context.
    /// Registration context allows nulls; Referral, LegalExport, and SoYTeReporting require Address and CCCD.
    /// </summary>
    public static PatientFieldValidationResult Validate(
        string? address,
        string? cccd,
        FieldRequirementContext context)
    {
        // Registration context: all fields are optional
        if (context == FieldRequirementContext.Registration)
        {
            return new PatientFieldValidationResult(true, []);
        }

        var missingFields = new List<MissingFieldInfo>();
        var contextName = context.ToString();

        if (string.IsNullOrWhiteSpace(address))
        {
            missingFields.Add(new MissingFieldInfo("Address", contextName));
        }

        if (string.IsNullOrWhiteSpace(cccd))
        {
            missingFields.Add(new MissingFieldInfo("Cccd", contextName));
        }

        return new PatientFieldValidationResult(missingFields.Count == 0, missingFields);
    }
}

/// <summary>
/// Result of patient field validation by context.
/// Defined in Domain layer as the validator's return type.
/// </summary>
public record PatientFieldValidationResult(
    bool IsValid,
    List<MissingFieldInfo> MissingFields);

/// <summary>
/// Information about a single missing field required for a specific context.
/// </summary>
public record MissingFieldInfo(
    string FieldName,
    string RequiredForContext);
