namespace Patient.Contracts.Dtos;

/// <summary>
/// Result of patient field validation by context.
/// Lives in Contracts so Presentation can reference it without depending on Domain.
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
