using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Interfaces;

namespace Shared.Presentation;

/// <summary>
/// Extension methods for mapping Settings Minimal API endpoints.
/// Provides GET/PUT clinic settings endpoints for admin configuration
/// and document header data management.
/// All endpoints require authorization.
/// </summary>
public static class SettingsApiEndpoints
{
    /// <summary>
    /// Maps all Settings API endpoints under /api/settings.
    /// </summary>
    public static IEndpointRouteBuilder MapSettingsApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings")
            .WithTags("Settings")
            .RequireAuthorization();

        // GET /api/settings/clinic
        group.MapGet("/clinic", async (
            IClinicSettingsService service,
            CancellationToken ct) =>
        {
            var settings = await service.GetCurrentAsync(ct);
            return settings is not null ? Results.Ok(settings) : Results.NotFound();
        });

        // PUT /api/settings/clinic
        group.MapPut("/clinic", async (
            UpdateClinicSettingsCommand command,
            IClinicSettingsService service,
            CancellationToken ct) =>
        {
            var result = await service.CreateOrUpdateAsync(command, ct);
            return result.ToHttpResult();
        });

        return app;
    }
}
