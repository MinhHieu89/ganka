using Microsoft.AspNetCore.Routing;

namespace Auth.Presentation;

/// <summary>
/// Extension methods for mapping Auth module Minimal API endpoints.
/// </summary>
public static class AuthApiEndpoints
{
    /// <summary>
    /// Maps all Auth module Minimal API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthApiEndpoints(this IEndpointRouteBuilder app)
    {
        // TODO: endpoints will be added by Plan 03 and Plan 04
        return app;
    }
}
