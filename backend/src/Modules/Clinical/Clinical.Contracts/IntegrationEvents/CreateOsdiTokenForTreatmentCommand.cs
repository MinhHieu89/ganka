namespace Clinical.Contracts.IntegrationEvents;

/// <summary>
/// Cross-module command sent from Treatment module to Clinical module
/// to create a DB-backed OSDI token for the treatment session self-fill flow.
/// </summary>
public sealed record CreateOsdiTokenForTreatmentCommand(string Token);

/// <summary>
/// Response containing the created token details.
/// </summary>
public sealed record CreateOsdiTokenForTreatmentResponse(
    string Token,
    string Url,
    DateTime ExpiresAt);
