namespace Auth.Contracts.Queries;

/// <summary>
/// Cross-module query to verify a manager's PIN.
/// Sent from Billing module to Auth module via IMessageBus.
/// </summary>
public sealed record VerifyManagerPinQuery(Guid ManagerId, string Pin);

/// <summary>
/// Response from Auth module for PIN verification.
/// </summary>
public sealed record VerifyManagerPinResponse(bool IsValid);
