using Microsoft.AspNetCore.Builder;
using Shared.Domain;

namespace Shared.Presentation;

/// <summary>
/// Extension methods for applying permission-based authorization to minimal API endpoints.
/// Uses the <see cref="Permissions.ClaimType"/> claim from JWT tokens.
/// Pass constants from <see cref="Permissions"/> (e.g. <c>Permissions.Optical.Manage</c>).
/// </summary>
public static class EndpointAuthorizationExtensions
{
    /// <summary>
    /// Requires the caller to have at least one of the specified permissions.
    /// Permissions are checked against the "permissions" claim in the JWT token.
    /// </summary>
    public static TBuilder RequirePermissions<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireAssertion(context =>
                permissions.Any(p => context.User.HasClaim(Permissions.ClaimType, p)));
        });
    }
}
