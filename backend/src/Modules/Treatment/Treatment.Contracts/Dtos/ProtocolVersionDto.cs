namespace Treatment.Contracts.Dtos;

/// <summary>
/// DTO representing a protocol version snapshot for a treatment package (TRT-07).
/// Each version captures the state change during a mid-course modification.
/// </summary>
public sealed record ProtocolVersionDto(
    int VersionNumber,
    string ChangeDescription,
    string Reason,
    string PreviousJson,
    string CurrentJson,
    Guid ChangedById,
    DateTime ChangedAt);
