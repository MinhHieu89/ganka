using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Patient.Application.Features;
using Patient.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Patient.Presentation;

/// <summary>
/// Extension methods for mapping Patient module Minimal API endpoints.
/// All endpoints require authorization.
/// </summary>
public static class PatientApiEndpoints
{
    /// <summary>
    /// Maps all Patient module Minimal API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapPatientApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients").RequireAuthorization();

        MapPatientCrudEndpoints(group);
        MapIntakeEndpoints(group);
        MapAllergyEndpoints(group);
        MapSearchEndpoints(group);
        MapPhotoEndpoints(group);
        MapValidationEndpoints(group);
        MapDashboardEndpoints(app);

        return app;
    }

    private static void MapPatientCrudEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/", async (RegisterPatientCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/patients");
        }).RequirePermissions(Permissions.Patient.Create);

        group.MapGet("/{patientId:guid}", async (Guid patientId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PatientDto>>(new GetPatientByIdQuery(patientId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.View);

        group.MapPut("/{patientId:guid}", async (Guid patientId, UpdatePatientCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PatientId = patientId };
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.Update);

        group.MapPost("/{patientId:guid}/deactivate", async (Guid patientId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new DeactivatePatientCommand(patientId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.Update);

        group.MapPost("/{patientId:guid}/reactivate", async (Guid patientId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new ReactivatePatientCommand(patientId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.Update);

        group.MapGet("/", async (
            [AsParameters] GetPatientListQuery query,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PagedResult<PatientDto>>>(query, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.View);

        group.MapGet("/recent", async (int? count, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<PatientSearchResult>>>(
                new GetRecentPatientsQuery(count ?? 10), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.View);
    }

    private static void MapIntakeEndpoints(RouteGroupBuilder group)
    {
        // Register a new patient from the receptionist intake form
        group.MapPost("/intake", async (RegisterPatientFromIntakeCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/patients");
        }).RequirePermissions(Permissions.Patient.Create);

        // Update an existing patient from the receptionist intake form
        group.MapPut("/{id:guid}/intake", async (Guid id, UpdatePatientFromIntakeCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PatientId = id };
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.Update);
    }

    private static void MapAllergyEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/{patientId:guid}/allergies", async (Guid patientId, AddAllergyCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = command with { PatientId = patientId };
            var result = await bus.InvokeAsync<Result<Guid>>(enriched, ct);
            return result.ToCreatedHttpResult($"/api/patients/{patientId}/allergies");
        }).RequirePermissions(Permissions.Patient.Update);

        group.MapDelete("/{patientId:guid}/allergies/{allergyId:guid}", async (Guid patientId, Guid allergyId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new RemoveAllergyCommand(patientId, allergyId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.Update);
    }

    private static void MapSearchEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/search", async (string? term, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<List<PatientSearchResult>>>(
                new SearchPatientsQuery(term ?? string.Empty), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.View);
    }

    private static void MapPhotoEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/{patientId:guid}/photo", async (Guid patientId, IFormFile file, IMessageBus bus, CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var command = new UploadPatientPhotoCommand(patientId, stream, file.FileName);
            var result = await bus.InvokeAsync<Result<string>>(command, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.Update).DisableAntiforgery();
    }

    private static void MapValidationEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/{patientId:guid}/field-validation", async (Guid patientId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PatientFieldValidationResult>>(
                new ValidatePatientFieldsQuery(patientId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Patient.View);
    }

    private static void MapDashboardEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").RequireAuthorization();

        group.MapGet("/stats", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<DashboardStatsDto>>(new GetDashboardStatsQuery(), ct);
            return result.ToHttpResult();
        });
    }
}
