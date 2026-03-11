using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Pharmacy.Application.Features;
using Pharmacy.Application.Features.Alerts;
using Pharmacy.Application.Features.Inventory;
using Pharmacy.Application.Features.Suppliers;
using Pharmacy.Application.Features.StockImport;
using Pharmacy.Application.Features.DrugCatalog;
using Pharmacy.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Pharmacy.Presentation;

/// <summary>
/// Pharmacy API endpoints covering suppliers, inventory, stock import, alerts, and drug pricing.
/// All endpoints require authorization and are grouped under /api/pharmacy.
/// </summary>
public static class PharmacyApiEndpoints
{
    public static IEndpointRouteBuilder MapPharmacyApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pharmacy").RequireAuthorization();

        MapDrugCatalogEndpoints(group);
        MapSupplierEndpoints(group);
        MapInventoryEndpoints(group);
        MapStockImportEndpoints(group);
        MapAlertEndpoints(group);

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

    private static void MapSupplierEndpoints(RouteGroupBuilder group)
    {
        // GET /api/pharmacy/suppliers -- list all active suppliers
        group.MapGet("/suppliers", async (IMessageBus bus, CancellationToken ct) =>
        {
            var results = await bus.InvokeAsync<List<SupplierDto>>(new GetSuppliersQuery(), ct);
            return Results.Ok(results);
        });

        // POST /api/pharmacy/suppliers -- create supplier
        group.MapPost("/suppliers", async (CreateSupplierCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/pharmacy/suppliers");
        });

        // PUT /api/pharmacy/suppliers/{id} -- update supplier
        group.MapPut("/suppliers/{id:guid}", async (Guid id, UpdateSupplierCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateSupplierCommand(id, command.Name, command.ContactInfo, command.Phone, command.Email);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        // PATCH /api/pharmacy/suppliers/{id}/toggle-active -- toggle supplier active/inactive status
        group.MapPatch("/suppliers/{id:guid}/toggle-active", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new ToggleSupplierActiveCommand(id), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapInventoryEndpoints(RouteGroupBuilder group)
    {
        // GET /api/pharmacy/inventory?expiryAlertDays=30 -- drug inventory with computed stock levels
        group.MapGet("/inventory", async ([AsParameters] GetDrugInventoryParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<DrugInventoryDto>>>(
                new GetDrugInventoryQuery(p.ExpiryAlertDays ?? 30), ct);
            return result.ToHttpResult();
        });

        // GET /api/pharmacy/inventory/{drugId}/batches -- batches for a specific drug
        group.MapGet("/inventory/{drugId:guid}/batches", async (Guid drugId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<DrugBatchDto>>>(
                new GetDrugBatchesQuery(drugId), ct);
            return result.ToHttpResult();
        });

        // PUT /api/pharmacy/inventory/{drugId}/pricing -- update drug pricing and min stock level
        group.MapPut("/inventory/{drugId:guid}/pricing", async (Guid drugId, UpdateDrugCatalogPricingCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateDrugCatalogPricingCommand(drugId, command.SellingPrice, command.MinStockLevel);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        // POST /api/pharmacy/inventory/adjust -- manual stock adjustment
        group.MapPost("/inventory/adjust", async (AdjustStockCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/pharmacy/inventory/adjustments");
        });
    }

    private static void MapStockImportEndpoints(RouteGroupBuilder group)
    {
        // GET /api/pharmacy/stock-imports?page=1&pageSize=20 -- paginated stock import history
        group.MapGet("/stock-imports", async ([AsParameters] GetStockImportsParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PagedStockImportsResult>>(
                new GetStockImportsQuery(p.Page ?? 1, p.PageSize ?? 20), ct);
            return result.ToHttpResult();
        });

        // POST /api/pharmacy/stock-imports -- create stock import from supplier invoice
        group.MapPost("/stock-imports", async (CreateStockImportCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/pharmacy/stock-imports");
        });

        // POST /api/pharmacy/stock-imports/excel -- parse Excel file and return preview (no save)
        group.MapPost("/stock-imports/excel", async (HttpRequest request, IMessageBus bus, CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Request must be multipart/form-data.");

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");
            if (file is null)
                return Results.BadRequest("A file named 'file' is required.");

            if (!Guid.TryParse(form["supplierId"], out var supplierId))
                return Results.BadRequest("A valid 'supplierId' is required.");

            using var stream = file.OpenReadStream();
            var result = await bus.InvokeAsync<Result<ExcelImportPreview>>(
                new ImportStockFromExcelCommand(stream, supplierId, file.FileName), ct);
            return result.ToHttpResult();
        }).DisableAntiforgery();
    }

    private static void MapAlertEndpoints(RouteGroupBuilder group)
    {
        // GET /api/pharmacy/alerts/expiry?days=90 -- expiry alerts within threshold
        group.MapGet("/alerts/expiry", async ([AsParameters] GetExpiryAlertsParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var results = await bus.InvokeAsync<List<ExpiryAlertDto>>(
                new GetExpiryAlertsQuery(p.Days ?? 90), ct);
            return Results.Ok(results);
        });

        // GET /api/pharmacy/alerts/low-stock -- drugs below minimum stock level
        group.MapGet("/alerts/low-stock", async (IMessageBus bus, CancellationToken ct) =>
        {
            var results = await bus.InvokeAsync<List<LowStockAlertDto>>(
                new GetLowStockAlertsQuery(), ct);
            return Results.Ok(results);
        });
    }
}

/// <summary>Query string binding for drug catalog search endpoint.</summary>
public class SearchDrugCatalogParams
{
    public string? Term { get; set; }
}

/// <summary>Query string binding for drug inventory endpoint.</summary>
public class GetDrugInventoryParams
{
    public int? ExpiryAlertDays { get; set; }
}

/// <summary>Query string binding for stock imports list endpoint.</summary>
public class GetStockImportsParams
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

/// <summary>Query string binding for expiry alerts endpoint.</summary>
public class GetExpiryAlertsParams
{
    public int? Days { get; set; }
}
