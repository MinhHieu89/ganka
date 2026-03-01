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
/// Public (unauthenticated) booking endpoints for patient self-service.
/// Rate-limited to prevent abuse (5 requests per minute per IP).
/// </summary>
public static class PublicBookingEndpoints
{
    public static IEndpointRouteBuilder MapPublicBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public/booking")
            .RequireRateLimiting("public-booking");

        group.MapPost("/", async (SubmitSelfBookingCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<string>>(command, ct);
            if (result.IsSuccess)
                return Results.Created($"/api/public/booking/status/{result.Value}", new { ReferenceNumber = result.Value });
            return result.ToHttpResult();
        });

        group.MapGet("/status/{referenceNumber}", async (string referenceNumber, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<BookingStatusDto>>(
                new CheckBookingStatusQuery(referenceNumber), ct);
            return result.ToHttpResult();
        });

        group.MapGet("/schedule", async (IMessageBus bus, CancellationToken ct) =>
        {
            var schedule = await bus.InvokeAsync<List<ClinicScheduleDto>>(
                new GetClinicScheduleQuery(), ct);
            return Results.Ok(schedule);
        });

        group.MapGet("/types", async (IMessageBus bus, CancellationToken ct) =>
        {
            var types = await bus.InvokeAsync<List<AppointmentTypeDto>>(
                new GetAppointmentTypesQuery(), ct);
            return Results.Ok(types);
        });

        return app;
    }
}
