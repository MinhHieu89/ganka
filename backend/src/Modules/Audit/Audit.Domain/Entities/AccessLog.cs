using Audit.Domain.Enums;

namespace Audit.Domain.Entities;

/// <summary>
/// Standalone access log entry tracking HTTP requests, login attempts, and record views.
/// Immutable -- no UPDATE or DELETE operations allowed.
/// </summary>
public sealed class AccessLog
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public AccessAction Action { get; private set; }

    /// <summary>
    /// The resource path accessed (e.g., "/api/patients/123").
    /// </summary>
    public string Resource { get; private set; } = string.Empty;

    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public int StatusCode { get; private set; }
    public Guid? BranchId { get; private set; }

    private AccessLog() { }

    public static AccessLog Create(
        Guid? userId,
        string? userEmail,
        AccessAction action,
        string resource,
        string ipAddress,
        string userAgent,
        int statusCode,
        Guid? branchId)
    {
        return new AccessLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            Resource = resource,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            StatusCode = statusCode,
            BranchId = branchId
        };
    }
}
