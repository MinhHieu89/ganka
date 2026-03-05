using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Pharmacy.Application.Features;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Pharmacy.Presentation;

/// <summary>
/// Pharmacy API endpoints for drug catalog search and CRUD management.
/// All endpoints require authorization and are grouped under /api/pharmacy.
/// </summary>
public static class PharmacyApiEndpoints
{
    public static IEndpointRouteBuilder MapPharmacyApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pharmacy").RequireAuthorization();

        MapDrugCatalogEndpoints(group);

        return app;
    }

    private static void MapDrugCatalogEndpoints(RouteGroupBuilder group)
    {
        // GET /api/pharmacy/drugs/search?term=xxx -- search drug catalog
        group.MapGet("/drugs/search", async ([AsParameters] SearchDrugCatalogParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var results = await bus.InvokeAsync<List<DrugCatalogItemDto>>(
                new SearchDrugCatalogQuery(p.Term ?? ""), ct);
            return Results.Ok(results);
        });

        // GET /api/pharmacy/drugs -- list all active drugs (admin)
        group.MapGet("/drugs", async (IMessageBus bus, CancellationToken ct) =>
        {
            var results = await bus.InvokeAsync<List<DrugCatalogItemDto>>(
                new GetAllActiveDrugsQuery(), ct);
            return Results.Ok(results);
        });

        // POST /api/pharmacy/drugs -- create drug catalog item (admin)
        group.MapPost("/drugs", async (CreateDrugCatalogItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/pharmacy/drugs");
        });

        // PUT /api/pharmacy/drugs/{id} -- update drug catalog item (admin)
        group.MapPut("/drugs/{id:guid}", async (Guid id, UpdateDrugCatalogItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateDrugCatalogItemCommand(
                id, command.Name, command.NameVi, command.GenericName,
                command.Form, command.Strength, command.Route,
                command.Unit, command.DefaultDosageTemplate);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });
    }
}

/// <summary>
/// Query string binding for drug catalog search endpoint.
/// </summary>
public class SearchDrugCatalogParams
{
    public string? Term { get; set; }
}
