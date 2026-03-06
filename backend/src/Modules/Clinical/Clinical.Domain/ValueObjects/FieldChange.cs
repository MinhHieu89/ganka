namespace Clinical.Domain.ValueObjects;

/// <summary>
/// Represents a single field-level change within an amendment.
/// Serialized to JSON in VisitAmendment.FieldChangesJson.
/// </summary>
public record FieldChange(string FieldName, string? OldValue, string? NewValue);
