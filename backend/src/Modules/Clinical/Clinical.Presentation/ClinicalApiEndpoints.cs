using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Clinical.Application.Features;
using Clinical.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Clinical.Presentation;

/// <summary>
/// Clinical API endpoints for visit lifecycle, refraction, diagnosis, and ICD-10 management.
/// All endpoints require authorization and are grouped under /api/clinical.
/// </summary>
public static class ClinicalApiEndpoints
{
    public static IEndpointRouteBuilder MapClinicalApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clinical").RequireAuthorization();

        MapVisitLifecycleEndpoints(group);
        MapVisitDataEndpoints(group);
        MapIcd10Endpoints(group);

        return app;
    }

    private static void MapVisitLifecycleEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateVisitCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/clinical");
        });

        group.MapGet("/{visitId:guid}", async (Guid visitId, IMessageBus bus, CancellationToken ct) =>
        {
            var dto = await bus.InvokeAsync<VisitDetailDto?>(new GetVisitByIdQuery(visitId), ct);
            return dto is not null ? Results.Ok(dto) : Results.NotFound();
        });

        group.MapGet("/active", async (IMessageBus bus, CancellationToken ct) =>
        {
            var visits = await bus.InvokeAsync<List<ActiveVisitDto>>(new GetActiveVisitsQuery(), ct);
            return Results.Ok(visits);
        });

        group.MapPut("/{visitId:guid}/sign-off", async (Guid visitId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new SignOffVisitCommand(visitId), ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{visitId:guid}/amend", async (Guid visitId, AmendVisitCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AmendVisitCommand(visitId, command.Reason, command.FieldChangesJson);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapPut("/{visitId:guid}/stage", async (Guid visitId, AdvanceWorkflowStageCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AdvanceWorkflowStageCommand(visitId, command.NewStage);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });
    }

    private static void MapVisitDataEndpoints(RouteGroupBuilder group)
    {
        group.MapPut("/{visitId:guid}/notes", async (Guid visitId, UpdateVisitNotesCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateVisitNotesCommand(visitId, command.Notes);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapPut("/{visitId:guid}/refraction", async (Guid visitId, UpdateRefractionCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new UpdateRefractionCommand(
                visitId, command.RefractionType,
                command.OdSph, command.OdCyl, command.OdAxis, command.OdAdd, command.OdPd,
                command.OsSph, command.OsCyl, command.OsAxis, command.OsAdd, command.OsPd,
                command.UcvaOd, command.UcvaOs, command.BcvaOd, command.BcvaOs,
                command.IopOd, command.IopOs, command.IopMethod,
                command.AxialLengthOd, command.AxialLengthOs);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{visitId:guid}/diagnoses", async (Guid visitId, AddVisitDiagnosisCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new AddVisitDiagnosisCommand(
                visitId, command.Icd10Code, command.DescriptionEn, command.DescriptionVi,
                command.Laterality, command.Role, command.SortOrder);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapDelete("/{visitId:guid}/diagnoses/{diagnosisId:guid}", async (Guid visitId, Guid diagnosisId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new RemoveVisitDiagnosisCommand(visitId, diagnosisId), ct);
            return result.ToHttpResult();
        });
    }

    private static void MapIcd10Endpoints(RouteGroupBuilder group)
    {
        group.MapGet("/icd10/search", async (string term, IMessageBus bus, HttpContext httpContext, CancellationToken ct) =>
        {
            var doctorId = httpContext.User.FindFirst("sub")?.Value;
            Guid? parsedDoctorId = Guid.TryParse(doctorId, out var id) ? id : null;
            var results = await bus.InvokeAsync<List<Icd10SearchResultDto>>(
                new SearchIcd10CodesQuery(term, parsedDoctorId), ct);
            return Results.Ok(results);
        });

        group.MapPost("/icd10/favorites/toggle", async (ToggleIcd10FavoriteCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToHttpResult();
        });

        group.MapGet("/icd10/favorites", async (Guid doctorId, IMessageBus bus, CancellationToken ct) =>
        {
            var results = await bus.InvokeAsync<List<Icd10SearchResultDto>>(
                new GetDoctorFavoritesQuery(doctorId), ct);
            return Results.Ok(results);
        });
    }
}
