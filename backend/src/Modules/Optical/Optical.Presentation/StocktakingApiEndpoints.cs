using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Optical.Application.Features.Stocktaking;
using Optical.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Optical.Presentation;

/// <summary>
/// Stocktaking session API endpoints for the Optical module.
/// Implements OPT-09: barcode-based stocktaking with discrepancy report.
/// All endpoints require authorization and are grouped under /api/optical/stocktaking.
/// </summary>
public static class StocktakingApiEndpoints
{
    public static IEndpointRouteBuilder MapStocktakingApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/optical").RequireAuthorization();

        MapStocktakingEndpoints(group);

        return app;
    }

    private static void MapStocktakingEndpoints(RouteGroupBuilder group)
    {
        // GET /api/optical/stocktaking?page=1&pageSize=20 -- list stocktaking sessions
        group.MapGet("/stocktaking", async ([AsParameters] GetStocktakingSessionsParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PagedStocktakingSessionsResult>>(
                new GetStocktakingSessionsQuery(p.Page ?? 1, p.PageSize ?? 20), ct);
            return result.ToHttpResult();
        });

        // GET /api/optical/stocktaking/{id} -- get session with all scanned items
        group.MapGet("/stocktaking/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<StocktakingSessionDetailDto>>(
                new GetStocktakingSessionByIdQuery(id), ct);
            return result.ToHttpResult();
        });

        // GET /api/optical/stocktaking/{id}/report -- discrepancy report for completed session
        group.MapGet("/stocktaking/{id:guid}/report", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<DiscrepancyReportDto>>(
                new GetDiscrepancyReportQuery(id), ct);
            return result.ToHttpResult();
        });

        // POST /api/optical/stocktaking -- start a new stocktaking session
        group.MapPost("/stocktaking", async (StartStocktakingSessionCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/optical/stocktaking");
        });

        // POST /api/optical/stocktaking/{id}/scan -- record barcode scan with physical count
        group.MapPost("/stocktaking/{id:guid}/scan", async (Guid id, RecordStocktakingItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new RecordStocktakingItemCommand(id, command.Barcode, command.PhysicalCount);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        // PUT /api/optical/stocktaking/{id}/complete -- complete the stocktaking session
        group.MapPut("/stocktaking/{id:guid}/complete", async (Guid id, CompleteStocktakingCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new CompleteStocktakingCommand(id, command.Notes);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });
    }
}

/// <summary>Query string binding for GET /stocktaking endpoint.</summary>
public class GetStocktakingSessionsParams
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
