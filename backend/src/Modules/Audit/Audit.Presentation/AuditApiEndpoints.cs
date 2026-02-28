using Microsoft.AspNetCore.Routing;

namespace Audit.Presentation;

/// <summary>
/// Extension methods for mapping Audit module Minimal API endpoints.
/// </summary>
public static class AuditApiEndpoints
{
    /// <summary>
    /// Maps all Audit module Minimal API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAuditApiEndpoints(this IEndpointRouteBuilder app)
    {
        // TODO: endpoints will be added by Plan 02
        return app;
    }
}
