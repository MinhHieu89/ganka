using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Treatment.Application.Features;
using Treatment.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Treatment.Presentation;

/// <summary>
/// Treatment API endpoints covering protocol templates, packages, sessions,
/// modifications, and cancellation workflows.
/// All endpoints require authorization and are grouped under /api/treatments.
/// </summary>
public static class TreatmentApiEndpoints
{
    public static IEndpointRouteBuilder MapTreatmentApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/treatments").RequireAuthorization();

        MapProtocolTemplateEndpoints(group);
        MapPackageEndpoints(group);
        MapSessionEndpoints(group);
        MapModificationEndpoints(group);
        MapCancellationEndpoints(group);

        return app;
    }

    private static void MapProtocolTemplateEndpoints(RouteGroupBuilder group)
    {
        // POST /api/treatments/protocols -- create a new protocol template
        group.MapPost("/protocols",
            async (CreateProtocolTemplateCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<TreatmentProtocolDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/treatments/protocols");
        }).RequirePermissions(Permissions.Treatment.Create);

        // PUT /api/treatments/protocols/{id} -- update an existing protocol template
        group.MapPut("/protocols/{id:guid}",
            async (Guid id, UpdateProtocolTemplateCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { Id = id };
            var result = await bus.InvokeAsync<Result<TreatmentProtocolDto>>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Treatment.Update);

        // GET /api/treatments/protocols -- get all protocol templates (optional filters)
        group.MapGet("/protocols",
            async (int? treatmentType, bool? includeInactive, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<TreatmentProtocolDto>>>(
                new GetProtocolTemplatesQuery(treatmentType, includeInactive ?? false), ct);
            return result.ToHttpResult();
        });

        // GET /api/treatments/protocols/{id} -- get a single protocol template by ID
        group.MapGet("/protocols/{id:guid}",
            async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<TreatmentProtocolDto>>(
                new GetProtocolTemplateByIdQuery(id), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapPackageEndpoints(RouteGroupBuilder group)
    {
        // POST /api/treatments/packages -- create a treatment package from a protocol template
        group.MapPost("/packages",
            async (CreateTreatmentPackageCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<TreatmentPackageDto>>(command, ct);
            return result.ToCreatedHttpResult("/api/treatments/packages");
        }).RequirePermissions(Permissions.Treatment.Create);

        // GET /api/treatments/packages -- get all active treatment packages
        group.MapGet("/packages",
            async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<TreatmentPackageDto>>>(
                new GetActiveTreatmentsQuery(), ct);
            return result.ToHttpResult();
        });

        // GET /api/treatments/packages/{id} -- get a treatment package by ID with full details
        group.MapGet("/packages/{id:guid}",
            async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<TreatmentPackageDto>>(
                new GetTreatmentPackageByIdQuery(id), ct);
            return result.ToHttpResult();
        });

        // GET /api/treatments/packages/due-soon -- get packages due for their next session
        group.MapGet("/packages/due-soon",
            async (IMessageBus bus, CancellationToken ct) =>
        {
            var packages = await bus.InvokeAsync<List<TreatmentPackageDto>>(
                new GetDueSoonSessionsQuery(), ct);
            return Results.Ok(packages);
        });

        // GET /api/treatments/patients/{patientId}/packages -- get all packages for a patient
        group.MapGet("/patients/{patientId:guid}/packages",
            async (Guid patientId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<TreatmentPackageDto>>>(
                new GetPatientTreatmentsQuery(patientId), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapSessionEndpoints(RouteGroupBuilder group)
    {
        // POST /api/treatments/packages/{packageId}/sessions -- record a treatment session
        group.MapPost("/packages/{packageId:guid}/sessions",
            async (Guid packageId, RecordTreatmentSessionCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PackageId = packageId };
            var result = await bus.InvokeAsync<Result<RecordSessionResponse>>(enriched, ct);
            return result.ToCreatedHttpResult("/api/treatments/packages");
        }).RequirePermissions(Permissions.Treatment.Create);

        // GET /api/treatments/packages/{packageId}/sessions -- get all sessions for a package
        group.MapGet("/packages/{packageId:guid}/sessions",
            async (Guid packageId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<TreatmentSessionDto>>>(
                new GetTreatmentSessionsQuery(packageId), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapModificationEndpoints(RouteGroupBuilder group)
    {
        // PUT /api/treatments/packages/{packageId}/modify -- modify a treatment package mid-course
        group.MapPut("/packages/{packageId:guid}/modify",
            async (Guid packageId, ModifyTreatmentPackageCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PackageId = packageId };
            var result = await bus.InvokeAsync<Result<TreatmentPackageDto>>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Treatment.Update);

        // POST /api/treatments/packages/{packageId}/switch -- switch treatment type
        group.MapPost("/packages/{packageId:guid}/switch",
            async (Guid packageId, SwitchTreatmentTypeCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PackageId = packageId };
            var result = await bus.InvokeAsync<Result<TreatmentPackageDto>>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Treatment.Update);

        // POST /api/treatments/packages/{packageId}/pause -- pause or resume a treatment package
        group.MapPost("/packages/{packageId:guid}/pause",
            async (Guid packageId, PauseTreatmentPackageCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PackageId = packageId };
            var result = await bus.InvokeAsync<Result<TreatmentPackageDto>>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Treatment.Update);
    }

    private static void MapCancellationEndpoints(RouteGroupBuilder group)
    {
        // POST /api/treatments/packages/{packageId}/cancel -- request cancellation of a package
        group.MapPost("/packages/{packageId:guid}/cancel",
            async (Guid packageId, RequestCancellationCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PackageId = packageId };
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Treatment.Update);

        // POST /api/treatments/packages/{packageId}/cancel/approve -- approve a pending cancellation
        group.MapPost("/packages/{packageId:guid}/cancel/approve",
            async (Guid packageId, ApproveCancellationCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PackageId = packageId };
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Treatment.Manage);

        // POST /api/treatments/packages/{packageId}/cancel/reject -- reject a pending cancellation
        group.MapPost("/packages/{packageId:guid}/cancel/reject",
            async (Guid packageId, RejectCancellationCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PackageId = packageId };
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Treatment.Manage);

        // GET /api/treatments/cancellations/pending -- get all pending cancellation requests
        group.MapGet("/cancellations/pending",
            async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<TreatmentPackageDto>>>(
                new GetPendingCancellationsQuery(), ct);
            return result.ToHttpResult();
        });
    }
}
