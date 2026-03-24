using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Optical.Application.Features.Warranty;
using Shared.Domain;
using Shared.Presentation;
using Wolverine;

namespace Optical.Presentation;

/// <summary>
/// Warranty claim API endpoints for the Optical module.
/// Implements OPT-07: warranty claim management with manager approval and document upload.
/// All endpoints require authorization and are grouped under /api/optical/warranty.
/// </summary>
public static class WarrantyApiEndpoints
{
    public static IEndpointRouteBuilder MapWarrantyApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/optical").RequireAuthorization();

        MapWarrantyEndpoints(group);

        return app;
    }

    private static void MapWarrantyEndpoints(RouteGroupBuilder group)
    {
        // GET /api/optical/warranty?approvalStatusFilter=0&page=1&pageSize=20
        group.MapGet("/warranty", async ([AsParameters] GetWarrantyClaimsParams p, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PagedWarrantyClaimsResult>>(
                new GetWarrantyClaimsQuery(p.ApprovalStatusFilter, p.Page ?? 1, p.PageSize ?? 20), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Optical.View);

        // POST /api/optical/warranty -- file a new warranty claim
        group.MapPost("/warranty", async (CreateWarrantyClaimCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command, ct);
            return result.ToCreatedHttpResult("/api/optical/warranty");
        }).RequirePermissions(Permissions.Optical.Create);

        // PUT /api/optical/warranty/{id}/approve -- manager approves or rejects claim (Replace resolution only)
        group.MapPut("/warranty/{id:guid}/approve", async (Guid id, ApproveWarrantyClaimCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var enriched = new ApproveWarrantyClaimCommand(id, command.IsApproved, command.Notes);
            var result = await bus.InvokeAsync<Result>(enriched, ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Optical.Manage);

        // POST /api/optical/warranty/{id}/documents -- upload supporting document (multipart)
        group.MapPost("/warranty/{id:guid}/documents", async (Guid id, HttpRequest request, IMessageBus bus, CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Request must be multipart/form-data.");

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");
            if (file is null)
                return Results.BadRequest("A file named 'file' is required.");

            const long maxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
            if (file.Length > maxFileSizeBytes)
                return Results.BadRequest("File must not exceed 10 MB.");

            var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
            if (!allowedTypes.Contains(file.ContentType))
                return Results.BadRequest("Only PDF, JPEG, and PNG files are permitted.");

            using var stream = file.OpenReadStream();
            var result = await bus.InvokeAsync<Result<string>>(
                new UploadWarrantyDocumentCommand(id, stream, file.FileName), ct);
            return result.ToHttpResult();
        }).RequirePermissions(Permissions.Optical.Update).DisableAntiforgery();
    }
}

/// <summary>Query string binding for GET /warranty endpoint.</summary>
public class GetWarrantyClaimsParams
{
    public int? ApprovalStatusFilter { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
