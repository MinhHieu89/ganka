using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Optical.Presentation;

/// <summary>
/// Stocktaking session API endpoints.
/// Implements OPT-09 (barcode-based stocktaking with discrepancy report).
/// All endpoints require authorization and are grouped under /api/optical/stocktaking.
/// Handlers are implemented in plan 08-31.
/// </summary>
public static class StocktakingApiEndpoints
{
    public static IEndpointRouteBuilder MapStocktakingApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Stocktaking endpoints will be populated as Application handlers
        // are implemented in plan 08-31 (stocktaking sessions).
        return app;
    }
}
