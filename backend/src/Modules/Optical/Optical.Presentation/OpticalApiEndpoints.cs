using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Optical.Presentation;

/// <summary>
/// Optical module API endpoints for frame catalog and lens catalog management.
/// Implements OPT-01 (frame inventory) and OPT-02 (lens catalog).
/// All endpoints require authorization and are grouped under /api/optical.
/// Handlers are implemented in subsequent plans (08-25 through 08-27).
/// </summary>
public static class OpticalApiEndpoints
{
    public static IEndpointRouteBuilder MapOpticalApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoint groups registered here will be populated as Application handlers
        // are implemented in plans 08-25 (frames), 08-26 (lenses), 08-27 (orders).
        // This stub satisfies the Bootstrapper wiring requirement (OPT-01 through OPT-04).
        return app;
    }
}
