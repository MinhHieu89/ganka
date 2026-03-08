using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Optical.Presentation;

/// <summary>
/// Warranty claim API endpoints.
/// Implements OPT-07 (warranty tracking with claim workflow).
/// All endpoints require authorization and are grouped under /api/optical/warranty.
/// Handlers are implemented in plan 08-29.
/// </summary>
public static class WarrantyApiEndpoints
{
    public static IEndpointRouteBuilder MapWarrantyApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Warranty endpoints will be populated as Application handlers
        // are implemented in plan 08-29 (warranty claims).
        return app;
    }
}
