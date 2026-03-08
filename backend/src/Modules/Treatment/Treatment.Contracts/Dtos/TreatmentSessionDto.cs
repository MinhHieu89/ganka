namespace Treatment.Contracts.Dtos;

/// <summary>
/// DTO representing a single treatment session within a package.
/// Status is string representation of session status enum.
/// Includes OSDI assessment data and consumables used.
/// </summary>
public sealed record TreatmentSessionDto(
    Guid Id,
    int SessionNumber,
    string Status,
    string ParametersJson,
    decimal? OsdiScore,
    string? OsdiSeverity,
    string? ClinicalNotes,
    Guid PerformedById,
    string? PerformedByName,
    Guid? VisitId,
    DateTime? ScheduledAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    string? IntervalOverrideReason,
    List<SessionConsumableDto> Consumables);

/// <summary>
/// DTO representing a consumable item used during a treatment session.
/// </summary>
public sealed record SessionConsumableDto(
    Guid Id,
    Guid ConsumableItemId,
    string ConsumableName,
    int Quantity);
