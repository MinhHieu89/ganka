using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Scheduling.Application.Features;
using Scheduling.Contracts.Dtos;
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

        return app;
    }

    private static void MapAppointmentEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/", async (BookAppointmentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/appointments");
        });

        group.MapPut("/{appointmentId:guid}/cancel", async (Guid appointmentId, CancelAppointmentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new CancelAppointmentCommand(appointmentId, command.CancellationReason, command.CancellationNote);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapPut("/{appointmentId:guid}/reschedule", async (Guid appointmentId, RescheduleAppointmentCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new RescheduleAppointmentCommand(appointmentId, command.NewStartTime);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });

        group.MapGet("/by-doctor/{doctorId:guid}", async (Guid doctorId, DateTime dateFrom, DateTime dateTo, IMessageBus bus, CancellationToken ct) =>
        {
            var appointments = await bus.InvokeAsync<List<AppointmentDto>>(
                new GetAppointmentsByDoctorQuery(doctorId, dateFrom, dateTo), ct);
            return Results.Ok(appointments);
        });

        group.MapGet("/by-patient/{patientId:guid}", async (Guid patientId, IMessageBus bus, CancellationToken ct) =>
        {
            var appointments = await bus.InvokeAsync<List<AppointmentDto>>(
                new GetAppointmentsByPatientQuery(patientId), ct);
            return Results.Ok(appointments);
        });
    }

    private static void MapSelfBookingManagementEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/self-bookings/pending", async (IMessageBus bus, CancellationToken ct) =>
        {
            var requests = await bus.InvokeAsync<List<SelfBookingRequestDto>>(
                new GetPendingSelfBookingsQuery(), ct);
            return Results.Ok(requests);
        });

        group.MapPost("/self-bookings/{id:guid}/approve", async (Guid id, ApproveSelfBookingCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new ApproveSelfBookingCommand(id, command.DoctorId, command.DoctorName, command.PatientName, command.StartTime);
            var result = await bus.InvokeAsync<Result<Guid>>(enriched, ct);
            return result.ToCreatedHttpResult("/api/appointments");
        });

        group.MapPost("/self-bookings/{id:guid}/reject", async (Guid id, RejectSelfBookingCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new RejectSelfBookingCommand(id, command.Reason);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        });
    }

    private static void MapReferenceDataEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/types", async (IMessageBus bus, CancellationToken ct) =>
        {
            var types = await bus.InvokeAsync<List<AppointmentTypeDto>>(
                new GetAppointmentTypesQuery(), ct);
            return Results.Ok(types);
        });

        group.MapGet("/schedule", async (IMessageBus bus, CancellationToken ct) =>
        {
            var schedule = await bus.InvokeAsync<List<ClinicScheduleDto>>(
                new GetClinicScheduleQuery(), ct);
            return Results.Ok(schedule);
        });
    }
}
