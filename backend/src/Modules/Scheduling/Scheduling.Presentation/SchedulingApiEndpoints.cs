using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Scheduling.Application.Features;
using Scheduling.Contracts.Dtos;
using Scheduling.Contracts.Queries;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Scheduling.Presentation;

/// <summary>
/// Authenticated Scheduling API endpoints for staff use.
/// Includes appointment CRUD, calendar queries, and self-booking management.
/// </summary>
public static class SchedulingApiEndpoints
{
    public static IEndpointRouteBuilder MapSchedulingApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").RequireAuthorization();

        MapAppointmentEndpoints(group);
        MapSelfBookingManagementEndpoints(group);
        MapReferenceDataEndpoints(group);

        var schedulingGroup = app.MapGroup("/api/scheduling").RequireAuthorization();
        MapReceptionistEndpoints(schedulingGroup);

        return app;
    }

    private static void MapAppointmentEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/", async (BookAppointmentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/appointments");
        }).RequirePermissions(Permissions.Scheduling.Create);

        group.MapPut("/{appointmentId:guid}/cancel", async (Guid appointmentId, CancelAppointmentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new CancelAppointmentCommand(appointmentId, command.CancellationReason, command.CancellationNote);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Scheduling.Update);

        group.MapPut("/{appointmentId:guid}/reschedule", async (Guid appointmentId, RescheduleAppointmentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new RescheduleAppointmentCommand(appointmentId, command.NewStartTime);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Scheduling.Update);

        group.MapGet("/by-doctor/{doctorId:guid}", async (Guid doctorId, DateTime dateFrom, DateTime dateTo, IMessageBus bus, CancellationToken ct) =>
        {
            var appointments = await bus.InvokeAsync<List<AppointmentDto>>(
                new GetAppointmentsByDoctorQuery(doctorId, dateFrom, dateTo), ct);
            return Results.Ok(appointments);
        }).RequirePermissions(Permissions.Scheduling.View);

        group.MapGet("/by-patient/{patientId:guid}", async (Guid patientId, IMessageBus bus, CancellationToken ct) =>
        {
            var appointments = await bus.InvokeAsync<List<AppointmentDto>>(
                new GetAppointmentsByPatientQuery(patientId), ct);
            return Results.Ok(appointments);
        }).RequirePermissions(Permissions.Scheduling.View);

        group.MapGet("/{appointmentId:guid}", async (Guid appointmentId, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<AppointmentDetailDto>>(
                new GetAppointmentByIdQuery(appointmentId), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Scheduling.View);
    }

    private static void MapSelfBookingManagementEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/self-bookings/pending", async (IMessageBus bus, CancellationToken ct) =>
        {
            var requests = await bus.InvokeAsync<List<SelfBookingRequestDto>>(
                new GetPendingSelfBookingsQuery(), ct);
            return Results.Ok(requests);
        }).RequirePermissions(Permissions.Scheduling.View);

        group.MapPost("/self-bookings/{id:guid}/approve", async (Guid id, ApproveSelfBookingCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new ApproveSelfBookingCommand(id, command.DoctorId, command.DoctorName, command.PatientName, command.StartTime);
            var result = await bus.InvokeAsync<Result<Guid>>(enriched, ct);
            return result.ToCreatedHttpResult("/api/appointments");
        }).RequirePermissions(Permissions.Scheduling.Update);

        group.MapPost("/self-bookings/{id:guid}/reject", async (Guid id, RejectSelfBookingCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new RejectSelfBookingCommand(id, command.Reason);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Scheduling.Update);
    }

    private static void MapReceptionistEndpoints(RouteGroupBuilder group)
    {
        // Check-in an appointment: marks checked-in and creates a Visit
        group.MapPost("/appointments/check-in", async (CheckInAppointmentRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct) =>
        {
            if (!httpContext.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var command = new CheckInAppointmentCommand(request.AppointmentId, userId);
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Scheduling.Update);

        // Book a guest appointment (no patient record yet, per D-11)
        group.MapPost("/appointments/guest", async (BookGuestAppointmentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/scheduling/appointments");
        }).RequirePermissions(Permissions.Scheduling.Create);

        // Mark appointment as no-show
        group.MapPost("/appointments/{id:guid}/no-show", async (Guid id, MarkNoShowRequest? request, HttpContext httpContext, IMessageBus bus, CancellationToken ct) =>
        {
            if (!httpContext.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var command = new MarkAppointmentNoShowCommand(id, userId, request?.Notes);
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Scheduling.Update);

        // Get available time slots for a given date and optional doctor
        group.MapGet("/slots", async (DateTime date, Guid? doctorId, IMessageBus bus, CancellationToken ct) =>
        {
            var query = new GetAvailableSlotsQuery(date, doctorId);
            var result = await bus.InvokeAsync<Result<List<AvailableSlotDto>>>(query, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Scheduling.View);

        // Get receptionist dashboard (today's patient queue with 4-status mapping)
        group.MapGet("/receptionist/dashboard", async ([AsParameters] ReceptionistDashboardParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var query = new GetReceptionistDashboardQuery(p.Status, p.Search, p.Page ?? 1, p.PageSize ?? 20);
            var result = await bus.InvokeAsync<Result<ReceptionistDashboardDto>>(query, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Scheduling.View);

        // Get receptionist KPI stats (counts per status for today)
        group.MapGet("/receptionist/kpi", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<ReceptionistKpiDto>>(new GetReceptionistKpiStatsQuery(), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Scheduling.View);
    }

    private static void MapReferenceDataEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/types", async (IMessageBus bus, CancellationToken ct) =>
        {
            var types = await bus.InvokeAsync<List<AppointmentTypeDto>>(
                new GetAppointmentTypesQuery(), ct);
            return Results.Ok(types);
        }).RequirePermissions(Permissions.Scheduling.View);

        group.MapGet("/schedule", async (IMessageBus bus, CancellationToken ct) =>
        {
            var schedule = await bus.InvokeAsync<List<ClinicScheduleDto>>(
                new GetClinicScheduleQuery(), ct);
            return Results.Ok(schedule);
        }).RequirePermissions(Permissions.Scheduling.View);

    }
}

/// <summary>
/// Request body for check-in endpoint. UserId is extracted from HttpContext.
/// </summary>
public record CheckInAppointmentRequest(Guid AppointmentId);

/// <summary>
/// Request body for mark-no-show endpoint. Notes are optional.
/// </summary>
public record MarkNoShowRequest(string? Notes);

/// <summary>
/// Query string binding for receptionist dashboard endpoint.
/// </summary>
public class ReceptionistDashboardParams
{
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
