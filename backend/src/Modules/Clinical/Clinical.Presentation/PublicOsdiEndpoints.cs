using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Clinical.Contracts.Dtos;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Clinical.Presentation;

/// <summary>
/// Public (unauthenticated) OSDI questionnaire endpoints for patient self-fill.
/// Rate-limited to prevent abuse. No RequireAuthorization.
/// Follows PublicBookingEndpoints pattern.
/// </summary>
public static class PublicOsdiEndpoints
{
    public static IEndpointRouteBuilder MapPublicOsdiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public/osdi")
            .RequireRateLimiting("public-booking"); // Reuse existing rate limit policy

        group.MapGet("/{token}", async (string token, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<OsdiQuestionnaireDto>>(
                new GetOsdiByTokenQuery(token), ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{token}", async (string token, SubmitOsdiAnswersRequest request, IMessageBus bus, CancellationToken ct) =>
        {
            var command = new SubmitOsdiCommand(token, request.Answers);
            var result = await bus.InvokeAsync<Result<decimal>>(command, ct);
            return result.ToHttpResult();
        });

        return app;
    }
}

/// <summary>
/// Request body for submitting OSDI answers via public endpoint.
/// </summary>
public sealed record SubmitOsdiAnswersRequest(int?[] Answers);
