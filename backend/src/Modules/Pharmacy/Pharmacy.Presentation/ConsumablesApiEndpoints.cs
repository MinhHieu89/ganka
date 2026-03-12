using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Pharmacy.Application.Features.Consumables;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Pharmacy.Presentation;

/// <summary>
/// Consumables warehouse API endpoints.
/// CON-01: Consumable item CRUD and stock management.
/// CON-02: Low-stock alerts for consumables warehouse.
/// All endpoints require authorization and are grouped under /api/consumables.
/// </summary>
public static class ConsumablesApiEndpoints
{
    public static IEndpointRouteBuilder MapConsumablesApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/consumables").RequireAuthorization();

        // GET /api/consumables -- all active consumables with computed stock levels
        group.MapGet("/", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<ConsumableItemDto>>>(
                new GetConsumableItemsQuery(), ct);
            return result.ToHttpResult();
        });

        // POST /api/consumables -- create a new consumable item
        group.MapPost("/", async (CreateConsumableItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/consumables");
        });

        // PUT /api/consumables/{id} -- update consumable item metadata
        group.MapPut("/{id:guid}", async (Guid id, UpdateConsumableItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateConsumableItemCommand(
                id,
                command.Name,
                command.NameVi,
                command.Unit,
                command.TrackingMode,
                command.MinStockLevel);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        // GET /api/consumables/{id}/batches -- all batches for an ExpiryTracked consumable
        group.MapGet("/{id:guid}/batches", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<ConsumableBatchDto>>>(
                new GetConsumableBatchesQuery(id), ct);
            return result.ToHttpResult();
        });

        // POST /api/consumables/{id}/stock -- add stock to a consumable item
        group.MapPost("/{id:guid}/stock", async (Guid id, AddConsumableStockCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AddConsumableStockCommand(
                id,
                command.Quantity,
                command.BatchNumber,
                command.ExpiryDate,
                command.Notes);
            var result = await bus.InvokeAsync<Result<Guid>>(enriched, ct);
            return result.ToCreatedHttpResult($"/api/consumables/{id}/stock");
        });

        // POST /api/consumables/{id}/adjust -- manually adjust stock quantity
        group.MapPost("/{id:guid}/adjust", async (Guid id, AdjustConsumableStockCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AdjustConsumableStockCommand(
                id,
                command.ConsumableBatchId,
                command.QuantityChange,
                command.Reason,
                command.Notes);
            var result = await bus.InvokeAsync<Result<Guid>>(enriched, ct);
            return result.ToCreatedHttpResult($"/api/consumables/{id}/adjustments");
        });

        // GET /api/consumables/alerts -- consumables below minimum stock level
        group.MapGet("/alerts", async (IMessageBus bus, CancellationToken ct) =>
        {
            var results = await bus.InvokeAsync<List<ConsumableItemDto>>(
                new GetConsumableAlertsQuery(), ct);
            return Results.Ok(results);
        });

        return app;
    }
}
