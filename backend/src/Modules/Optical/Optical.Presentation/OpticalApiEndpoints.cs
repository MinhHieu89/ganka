using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Optical.Application.Features.Alerts;
using Optical.Application.Features.Combos;
using Optical.Application.Features.Frames;
using Optical.Application.Features.Lenses;
using Optical.Application.Features.Orders;
using Optical.Application.Features.Prescriptions;
using Optical.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Optical.Presentation;

/// <summary>
/// Optical module API endpoints for frame inventory, lens catalog, glasses orders,
/// combo packages, and prescription history management.
/// Implements OPT-01 (frames), OPT-02 (lenses), OPT-03/OPT-04 (orders),
/// OPT-06 (combos), OPT-08 (prescription history).
/// All endpoints require authorization and are grouped under /api/optical.
/// </summary>
public static class OpticalApiEndpoints
{
    public static IEndpointRouteBuilder MapOpticalApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/optical").RequireAuthorization();

        MapFrameEndpoints(group);
        MapLensEndpoints(group);
        MapOrderEndpoints(group);
        MapComboEndpoints(group);
        MapPrescriptionEndpoints(group);

        return app;
    }

    private static void MapFrameEndpoints(RouteGroupBuilder group)
    {
        // GET /api/optical/frames?includeInactive=false&page=1&pageSize=20
        group.MapGet("/frames", async ([AsParameters] GetFramesParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PagedFramesResult>>(
                new GetFramesQuery(p.IncludeInactive ?? false, p.Page ?? 1, p.PageSize ?? 20), ct);
            return result.ToHttpResult();
        });

        // GET /api/optical/frames/search?searchTerm=xxx&material=0&frameType=0&gender=2&page=1&pageSize=20
        group.MapGet("/frames/search", async ([AsParameters] SearchFramesParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<FrameSearchResult>>(
                new SearchFramesQuery(p.SearchTerm, p.Material, p.FrameType, p.Gender, p.Page ?? 1, p.PageSize ?? 20), ct);
            return result.ToHttpResult();
        });

        // GET /api/optical/frames/{id} -- single frame detail
        group.MapGet("/frames/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<FrameDto>>(new GetFrameByIdQuery(id), ct);
            return result.ToHttpResult();
        });

        // POST /api/optical/frames -- create frame in inventory
        group.MapPost("/frames", async (CreateFrameCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/optical/frames");
        });

        // PUT /api/optical/frames/{id} -- update frame details
        group.MapPut("/frames/{id:guid}", async (Guid id, UpdateFrameCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateFrameCommand(
                id, command.Brand, command.Model, command.Color,
                command.LensWidth, command.BridgeWidth, command.TempleLength,
                command.Material, command.FrameType, command.Gender,
                command.SellingPrice, command.CostPrice, command.Barcode,
                command.StockQuantity, command.IsActive);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        // POST /api/optical/frames/{id}/generate-barcode -- auto-generate EAN-13 barcode
        group.MapPost("/frames/{id:guid}/generate-barcode", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<string>>(new GenerateBarcodeCommand(id), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapLensEndpoints(RouteGroupBuilder group)
    {
        // GET /api/optical/lenses?includeInactive=false -- full lens catalog with stock entries
        group.MapGet("/lenses", async ([AsParameters] GetLensCatalogParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<LensCatalogItemDto>>>(
                new GetLensCatalogQuery(p.IncludeInactive ?? false), ct);
            return result.ToHttpResult();
        });

        // POST /api/optical/lenses -- create lens catalog item
        group.MapPost("/lenses", async (CreateLensCatalogItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/optical/lenses");
        });

        // PUT /api/optical/lenses/{id} -- update lens catalog item
        group.MapPut("/lenses/{id:guid}", async (Guid id, UpdateLensCatalogItemCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateLensCatalogItemCommand(
                id, command.Brand, command.Name, command.LensType,
                command.Material, command.AvailableCoatings,
                command.SellingPrice, command.CostPrice,
                command.PreferredSupplierId, command.IsActive);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        // POST /api/optical/lenses/stock-adjust -- adjust per-power lens stock
        group.MapPost("/lenses/stock-adjust", async (AdjustLensStockCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToHttpResult();
        });

        // GET /api/optical/lenses/alerts -- lens stock entries below minimum
        group.MapGet("/lenses/alerts", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<LowLensStockAlertDto>>>(
                new GetLowLensStockAlertsQuery(), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapOrderEndpoints(RouteGroupBuilder group)
    {
        // GET /api/optical/orders?statusFilter=0&page=1&pageSize=20
        group.MapGet("/orders", async ([AsParameters] GetGlassesOrdersParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PagedGlassesOrdersResult>>(
                new GetGlassesOrdersQuery(p.StatusFilter, p.Page ?? 1, p.PageSize ?? 20), ct);
            return result.ToHttpResult();
        });

        // GET /api/optical/orders/overdue -- orders past estimated delivery date
        // Registered before {id:guid} to ensure literal route takes precedence
        group.MapGet("/orders/overdue", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<GlassesOrderSummaryDto>>>(
                new GetOverdueOrdersQuery(), ct);
            return result.ToHttpResult();
        });

        // GET /api/optical/orders/{id} -- single glasses order with full item details
        group.MapGet("/orders/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<GlassesOrderDto>>(
                new GetGlassesOrderByIdQuery(id), ct);
            return result.ToHttpResult();
        });

        // POST /api/optical/orders -- create glasses order from optical Rx
        group.MapPost("/orders", async (CreateGlassesOrderCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/optical/orders");
        });

        // PUT /api/optical/orders/{id}/status -- update order status (OPT-04 payment gate enforced)
        group.MapPut("/orders/{id:guid}/status", async (Guid id, UpdateOrderStatusCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateOrderStatusCommand(id, command.NewStatus, command.Notes);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });
    }

    private static void MapComboEndpoints(RouteGroupBuilder group)
    {
        // GET /api/optical/combos?includeInactive=false -- preset combo packages
        group.MapGet("/combos", async ([AsParameters] GetComboPackagesParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<ComboPackageDto>>>(
                new GetComboPackagesQuery(p.IncludeInactive ?? false), ct);
            return result.ToHttpResult();
        });

        // POST /api/optical/combos -- create preset combo package (admin)
        group.MapPost("/combos", async (CreateComboPackageCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/optical/combos");
        });

        // PUT /api/optical/combos/{id} -- update combo package
        group.MapPut("/combos/{id:guid}", async (Guid id, UpdateComboPackageCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateComboPackageCommand(
                id, command.Name, command.Description,
                command.FrameId, command.LensCatalogItemId,
                command.ComboPrice, command.OriginalTotalPrice, command.IsActive);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });
    }

    private static void MapPrescriptionEndpoints(RouteGroupBuilder group)
    {
        // GET /api/optical/prescriptions/patient/{patientId} -- prescription history (cross-module via Clinical)
        group.MapGet("/prescriptions/patient/{patientId:guid}", async (Guid patientId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<Optical.Contracts.Queries.OpticalPrescriptionHistoryDto>>>(
                new GetPatientPrescriptionHistoryQuery(patientId), ct);
            return result.ToHttpResult();
        });

        // GET /api/optical/prescriptions/compare?patientId=xxx&id1=xxx&id2=xxx -- year-over-year comparison
        group.MapGet("/prescriptions/compare", async ([AsParameters] GetPrescriptionComparisonParams p, IMessageBus bus, CancellationToken ct) =>
        {
            if (p.PatientId == Guid.Empty || p.Id1 == Guid.Empty || p.Id2 == Guid.Empty)
                return Results.BadRequest("patientId, id1, and id2 are required.");

            var result = await bus.InvokeAsync<Result<PrescriptionComparisonDto>>(
                new GetPrescriptionComparisonQuery(p.PatientId, p.Id1, p.Id2), ct);
            return result.ToHttpResult();
        });
    }
}

/// <summary>Query string binding for GET /frames endpoint.</summary>
public class GetFramesParams
{
    public bool? IncludeInactive { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

/// <summary>Query string binding for GET /frames/search endpoint.</summary>
public class SearchFramesParams
{
    public string? SearchTerm { get; set; }
    public int? Material { get; set; }
    public int? FrameType { get; set; }
    public int? Gender { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

/// <summary>Query string binding for GET /lenses endpoint.</summary>
public class GetLensCatalogParams
{
    public bool? IncludeInactive { get; set; }
}

/// <summary>Query string binding for GET /orders endpoint.</summary>
public class GetGlassesOrdersParams
{
    public int? StatusFilter { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

/// <summary>Query string binding for GET /combos endpoint.</summary>
public class GetComboPackagesParams
{
    public bool? IncludeInactive { get; set; }
}

/// <summary>Query string binding for GET /prescriptions/compare endpoint.</summary>
public class GetPrescriptionComparisonParams
{
    public Guid PatientId { get; set; }
    public Guid Id1 { get; set; }
    public Guid Id2 { get; set; }
}
