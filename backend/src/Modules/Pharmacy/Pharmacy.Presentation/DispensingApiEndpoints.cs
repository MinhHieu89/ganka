using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Pharmacy.Application.Features.Dispensing;
using Pharmacy.Application.Features.OtcSales;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Pharmacy.Presentation;

/// <summary>
/// Dispensing and OTC sale API endpoints for the Pharmacy module.
/// Dispensing endpoints handle prescription-based drug dispensing (PHR-05, PHR-07).
/// OTC endpoints handle walk-in sales without prescription (PHR-06).
/// All endpoints require authorization and are grouped under /api/pharmacy.
/// </summary>
public static class DispensingApiEndpoints
{
    public static IEndpointRouteBuilder MapDispensingApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pharmacy").RequireAuthorization();

        MapDispensingEndpoints(group);
        MapOtcSaleEndpoints(group);

        return app;
    }

    private static void MapDispensingEndpoints(RouteGroupBuilder group)
    {
        // GET /api/pharmacy/dispensing/pending?patientId=xxx -- pending prescriptions queue
        group.MapGet("/dispensing/pending", async ([AsParameters] GetPendingParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<Pharmacy.Contracts.Dtos.PendingPrescriptionDto>>>(
                new GetPendingPrescriptionsQuery(p.PatientId), ct);
            return result.ToHttpResult();
        });

        // GET /api/pharmacy/dispensing/pending/count -- count of pending prescriptions (sidebar badge)
        group.MapGet("/dispensing/pending/count", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<Pharmacy.Contracts.Dtos.PendingPrescriptionDto>>>(
                new GetPendingPrescriptionsQuery(null), ct);
            if (!result.IsSuccess)
                return result.ToHttpResult();

            var nonExpired = result.Value?.Count(p => !p.IsExpired) ?? 0;
            return Results.Ok(new { Count = nonExpired });
        });

        // POST /api/pharmacy/dispensing -- dispense drugs against a prescription
        group.MapPost("/dispensing", async (DispenseDrugsCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/pharmacy/dispensing");
        });

        // GET /api/pharmacy/dispensing/history?page=1&pageSize=20&patientId=xxx -- paginated dispensing history
        group.MapGet("/dispensing/history", async ([AsParameters] GetDispensingHistoryParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<DispensingHistoryDto>>(
                new GetDispensingHistoryQuery(p.Page ?? 1, p.PageSize ?? 20, p.PatientId), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapOtcSaleEndpoints(RouteGroupBuilder group)
    {
        // POST /api/pharmacy/otc-sales -- process a walk-in OTC sale
        group.MapPost("/otc-sales", async (CreateOtcSaleCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/pharmacy/otc-sales");
        });

        // GET /api/pharmacy/otc-sales?page=1&pageSize=20 -- paginated OTC sale history
        group.MapGet("/otc-sales", async ([AsParameters] GetOtcSalesParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<OtcSalesPagedResult>>(
                new GetOtcSalesQuery(p.Page ?? 1, p.PageSize ?? 20), ct);
            return result.ToHttpResult();
        });
    }
}

/// <summary>Query string binding for pending prescriptions endpoint.</summary>
public class GetPendingParams
{
    public Guid? PatientId { get; set; }
}

/// <summary>Query string binding for dispensing history endpoint.</summary>
public class GetDispensingHistoryParams
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public Guid? PatientId { get; set; }
}

/// <summary>Query string binding for OTC sales list endpoint.</summary>
public class GetOtcSalesParams
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
