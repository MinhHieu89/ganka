namespace Audit.Contracts.Dtos;

/// <summary>
/// DTO representing an access log entry for API responses.
/// </summary>
public sealed record AccessLogDto(
    Guid Id,
    DateTime Timestamp,
    string? UserEmail,
    string Action,
    string Resource,
    string IpAddress,
    int StatusCode);
