namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for amendment history display.
/// </summary>
public record VisitAmendmentDto(
    Guid Id,
    string AmendedByName,
    string Reason,
    string FieldChangesJson,
    DateTime AmendedAt);
