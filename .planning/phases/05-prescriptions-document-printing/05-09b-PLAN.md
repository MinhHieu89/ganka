---
phase: 05-prescriptions-document-printing
plan: 09b
type: execute
wave: 2
depends_on: ["05-09"]
files_modified:
  - backend/src/Shared/Shared.Presentation/SettingsApiEndpoints.cs
  - backend/src/Shared/Shared.Presentation/Shared.Presentation.csproj
autonomous: true
requirements:
  - PRT-01
  - PRT-02
  - PRT-04
  - PRT-05
must_haves:
  truths:
    - "GET /api/settings/clinic returns ClinicSettingsDto"
    - "PUT /api/settings/clinic updates clinic settings"
    - "Settings endpoints require authorization"
  artifacts:
    - path: "backend/src/Shared/Shared.Presentation/SettingsApiEndpoints.cs"
      provides: "HTTP endpoints for clinic settings CRUD"
      contains: "MapSettingsApiEndpoints"
    - path: "backend/src/Shared/Shared.Presentation/Shared.Presentation.csproj"
      provides: "ProjectReference to Shared.Application for IClinicSettingsService"
      contains: "Shared.Application"
  key_links:
    - from: "SettingsApiEndpoints.cs"
      to: "IClinicSettingsService"
      via: "DI injection in endpoint handler"
      pattern: "IClinicSettingsService"
    - from: "SettingsApiEndpoints.cs"
      to: "Program.cs"
      via: "MapSettingsApiEndpoints extension method (wired in Plan 10)"
      pattern: "MapSettingsApiEndpoints"
---

<objective>
Create backend HTTP endpoints for clinic settings management.

Purpose: Plan 09 creates ClinicSettings entity and IClinicSettingsService, but no HTTP endpoints. The frontend ClinicSettingsPage (Plan 18) needs GET/PUT endpoints to load and save clinic settings. This plan creates those endpoints in Shared.Presentation following the established Minimal API pattern.

Output: SettingsApiEndpoints.cs with GET/PUT clinic settings, updated Shared.Presentation.csproj
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/05-prescriptions-document-printing/05-CONTEXT.md
@.planning/phases/05-prescriptions-document-printing/05-09-SUMMARY.md

@backend/src/Shared/Shared.Presentation/Shared.Presentation.csproj
@backend/src/Modules/Auth/Auth.Presentation/AuthApiEndpoints.cs
@backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs

<interfaces>
From 05-09 (IClinicSettingsService):
```csharp
public interface IClinicSettingsService
{
    Task<ClinicSettingsDto?> GetCurrentAsync(CancellationToken ct);
    Task<Result<Guid>> CreateOrUpdateAsync(UpdateClinicSettingsCommand command, CancellationToken ct);
}

public sealed record ClinicSettingsDto(
    Guid Id, string ClinicName, string? ClinicNameVi, string Address,
    string? Phone, string? Fax, string? LicenseNumber, string? Tagline,
    string? LogoBlobUrl, string? Email, string? Website);

public sealed record UpdateClinicSettingsCommand(
    string ClinicName, string? ClinicNameVi, string Address,
    string? Phone, string? Fax, string? LicenseNumber, string? Tagline,
    string? LogoBlobUrl, string? Email, string? Website);
```

From Shared.Presentation:
```csharp
// ResultExtensions.cs
public static IResult ToHttpResult(this Result result);
public static IResult ToCreatedHttpResult<T>(this Result<T> result, string routeName);
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Create SettingsApiEndpoints with GET/PUT clinic settings</name>
  <files>
    backend/src/Shared/Shared.Presentation/SettingsApiEndpoints.cs,
    backend/src/Shared/Shared.Presentation/Shared.Presentation.csproj
  </files>
  <action>
**Shared.Presentation.csproj** -- add ProjectReference to Shared.Application:
```xml
<ProjectReference Include="..\Shared.Application\Shared.Application.csproj" />
```
This allows SettingsApiEndpoints to reference IClinicSettingsService and DTOs.

**SettingsApiEndpoints.cs** -- Minimal API endpoint group following the established pattern from ClinicalApiEndpoints.cs and AuthApiEndpoints.cs:
```csharp
public static class SettingsApiEndpoints
{
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
```

Follow the exact Minimal API patterns used by other modules (RequireAuthorization, WithTags, result extensions). The `MapSettingsApiEndpoints()` call will be wired in Program.cs by Plan 10 alongside the other endpoint mappings.
  </action>
  <verify>
    <automated>dotnet build backend/src/Shared/Shared.Presentation/Shared.Presentation.csproj</automated>
  </verify>
  <done>GET /api/settings/clinic and PUT /api/settings/clinic endpoints defined. Shared.Presentation references Shared.Application. Builds successfully. Ready to be wired in Program.cs by Plan 10.</done>
</task>

</tasks>

<verification>
- `dotnet build backend/src/Shared/Shared.Presentation/Shared.Presentation.csproj` passes
- SettingsApiEndpoints.cs exports `MapSettingsApiEndpoints` extension method
- GET and PUT endpoints defined under /api/settings/clinic
</verification>

<success_criteria>
Backend HTTP endpoints for clinic settings exist and compile. Frontend ClinicSettingsPage (Plan 18) can call GET/PUT /api/settings/clinic once Plan 10 wires them in Program.cs.
</success_criteria>

<output>
After completion, create `.planning/phases/05-prescriptions-document-printing/05-09b-SUMMARY.md`
</output>
