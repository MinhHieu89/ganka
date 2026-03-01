# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-28)

**Core value:** Doctors can manage chronic eye disease patients (Dry Eye, Myopia Control) with structured data tracking, image comparison across visits, and treatment progress reporting
**Current focus:** Phase 01.2: Refactor frontend to shadcn/ui with TanStack Start file-based routing

## Current Position

Phase: 01.2 of 9 (Refactor frontend to shadcn/ui with TanStack Start file-based routing)
Plan: 4 of 6 in current phase
Status: Executing Phase 01.2 -- Plan 04 complete
Last activity: 2026-03-01 -- Completed 01.2-04 DataTable and Field+Controller migration

Progress: [######....] 50%

## Performance Metrics

**Velocity:**
- Total plans completed: 17
- Average duration: 8min
- Total execution time: 2.12 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 1 | 6 | 73min | 12min |

**Recent Trend:**
- Last 5 plans: 6min, 4min, 3min, 3min, 3min
- Trend: stable

*Updated after each plan completion*
| Phase 01 P06 | 12min | 2 tasks | 15 files |
| Phase 01.1 P01 | 10min | 2 tasks | 30 files |
| Phase 01.1 P02 | 3min | 2 tasks | 10 files |
| Phase 01.1 P04 | 9min | 2 tasks | 34 files |
| Phase 01.1 P03 | 11min | 2 tasks | 13 files |
| Phase 01.1 P05 | 8min | 2 tasks | 21 files |
| Phase 01.1 P06 | 6min | 2 tasks | 7 files |
| Phase 01.1 P08 | 4min | 2 tasks | 9 files |
| Phase 01.1 P07 | 3min | 2 tasks | 2 files |
| Phase 01.1 P09 | 3min | 1 tasks | 27 files |
| Phase 01.2 P01a | 3min | 1 tasks | 24 files |
| Phase 01.2 P01b | 3min | 1 tasks | 39 files |
| Phase 01.2 P02 | 4min | 2 tasks | 7 files |
| Phase 01.2 P04 | 5min | 2 tasks | 7 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [Roadmap]: 9 phases derived from 96 v1 requirements across 18 categories
- [Roadmap]: Build order follows domain dependency chain -- HIS upstream, downstream modules consume events
- [Roadmap]: Three critical pitfalls (generic data model, immutable records, ICD-10 laterality) addressed in Phase 1
- [01-01]: Used .slnx format (modern .NET 10 default) instead of legacy .sln
- [01-01]: Custom Result<T> with implicit T conversion, no Ardalis.Result dependency
- [01-01]: Assembly Marker pattern for Wolverine handler discovery across modules
- [01-01]: FrameworkReference Microsoft.AspNetCore.App on Shared.Infrastructure for claim reading
- [01-02]: TanStack Start v1.163 uses Vite plugin (no vinxi) -- getRouter() export, vite.config.ts
- [01-02]: Stone base + Green accent theme, zero border radius per user decision
- [01-02]: Vietnamese as default language, @tabler/icons-react for all icons
- [01-04]: AuditInterceptor + AccessLoggingMiddleware placed in Audit.Infrastructure (not Shared.Infrastructure) to avoid circular project references
- [01-04]: IAuditReadContext interface for Application-layer DB access without circular dependency
- [01-04]: StorageBlobInfo renamed to avoid Azure.Storage.Blobs.Models.BlobInfo collision
- [01-04]: ReferenceDbContext with "reference" schema for cross-module ICD-10 reference data
- [01-04]: ICD-10 seed data as EmbeddedResource in Audit.Infrastructure assembly
- [Phase 01]: [01-03]: Service interfaces in Application, implementations in Infrastructure to avoid circular dependency
- [Phase 01]: [01-03]: Argon2id via Konscious.Security.Cryptography with 64MB memory, 4 parallelism, 3 iterations
- [Phase 01]: [01-03]: 8 system roles with preset permission templates (Admin, Doctor, Technician, Nurse, Cashier, OpticalStaff, Manager, Accountant)
- [Phase 01]: [01-05]: Dialog component extended with hideCloseButton prop for non-dismissible session warning modal
- [Phase 01]: [01-05]: Session timeout: 30 min inactivity, 2-min warning, activity throttled to 30s intervals
- [Phase 01]: [01-05]: Admin sidebar conditionally rendered based on Auth.Manage or Auth.View permission
- [Phase 01]: [01-05]: Separate React Hook Form instances for create vs edit mode to avoid union type issues
- [Phase 01]: [01-06]: NetArchTest.eNhancedEdition uses ResideInNamespaceContaining (not ResideInNamespaceStartingWith)
- [Phase 01]: [01-06]: Architecture tests gracefully skip scaffold-only modules via Assembly.Load try-catch
- [Phase 01]: [01-06]: IAuditable heuristic uses relaxed threshold for early phases
- [Phase 01.1]: [01.1-01]: Repository-per-aggregate pattern with IUnitOfWork for explicit persistence
- [Phase 01.1]: [01.1-01]: Presentation layer with Minimal API extension methods (MapXxxApiEndpoints pattern)
- [Phase 01.1]: [01.1-01]: Backward-compatible interface aliases during incremental migration (IAuditReadContext -> IAuditReadRepository, Services.IJwtService -> Interfaces.IJwtService)
- [Phase 01.1]: [01.1-01]: Kept WolverineFx.Http in Application csproj until endpoints migrated to Presentation (Plans 03-04)
- [Phase 01.1]: [01.1-02]: Removed IAuditReadContext backward-compat alias after old endpoints deleted
- [Phase 01.1]: [01.1-02]: Removed WolverineFx.Http and FrameworkReference from Audit.Application.csproj -- Application layer now HTTP-free
- [Phase 01.1]: [01.1-02]: [AsParameters] attribute for query string binding on Minimal API GET endpoints
- [Phase 01.1]: Allowed Shared.Domain in Presentation layer for Result<T>/Error HTTP response mapping (arch test updated)
- [Phase 01.1]: Combined Plan 03+04 deletions in single Task 2 commit due to concurrent execution
- [Phase 01.1]: Added Microsoft.Extensions.Logging.Abstractions to Auth.Application for handler ILogger<> after FrameworkReference removal
- [Phase 01.1]: Used Contracts LoginResponse for handler return types (canonical cross-module DTO location)
- [Phase 01.1]: Logout handler revokes ALL user tokens (RevokeAllByUserIdAsync) -- simpler than single-token revocation
- [Phase 01.1]: Handler-invoked FluentValidation via IValidator<T> injection and manual ValidateAsync in Handle method
- [Phase 01.1]: Removed WolverineFx.Http and FrameworkReference from Auth.Application -- no HTTP concerns in Application layer
- [Phase 01.1]: [01.1-05]: Reflection-based navigation property wiring in TestHelpers for domain entities with private setters
- [Phase 01.1]: [01.1-05]: Custom TestAsyncQueryable for EF Core IQueryable ToListAsync in unit tests (no MockQueryable NuGet)
- [Phase 01.1]: [01.1-05]: Two handler test patterns -- CreateSut() for instance handlers, direct Handle() for Wolverine static handlers
- [Phase 01.1]: [01.1-06]: Shared.Presentation as centralized HTTP response mapping layer (ResultExtensions + HttpContextExtensions)
- [Phase 01.1]: [01.1-06]: Private MapError() helper to DRY error-to-IResult mapping across ToHttpResult/ToCreatedHttpResult
- [Phase 01.1]: [01.1-08]: IoC.cs pattern -- one static class per layer, one extension method returning IServiceCollection
- [Phase 01.1]: [01.1-08]: Module DbContexts with AuditInterceptor stay in Program.cs ConfigureDbContext helper (cross-module dependency)
- [Phase 01.1]: [01.1-08]: Registration order: Shared first, Audit second (AuditInterceptor), Auth third
- [Phase 01.1]: [01.1-08]: FluentValidation.DependencyInjectionExtensions moved from Bootstrapper to Auth.Application.csproj
- [Phase 01.1]: [01.1-07]: Route groups with MapGroup for prefix consolidation, group-level RequireAuthorization on admin routes
- [Phase 01.1]: [01.1-07]: Kept Shared.Domain using in Auth.Presentation for Result/Result<T> types in bus.InvokeAsync calls
- [Phase 01.1]: [01.1-09]: NuGet CPM with CentralPackageFloatingVersionsEnabled for wildcard version patterns (5.*, 12.*, 10.0.*-*)
- [Phase 01.1]: [01.1-09]: 25 unique packages centralized in Directory.Packages.props, all .csproj use Include-only PackageReference
- [Phase 01.2]: [01.2-01a]: Allowed sidebar overwrite -- latest shadcn sidebar includes all expected exports
- [Phase 01.2]: [01.2-01a]: Restored hideCloseButton custom prop on DialogContent after shadcn overwrite
- [Phase 01.2]: [01.2-01a]: lucide-react accepted as shadcn internal icon dependency alongside @tabler/icons-react
- [Phase 01.2]: [01.2-01b]: Pure re-export wrappers for all 20 shadcn primitives -- enables future customization in ONE place
- [Phase 01.2]: [01.2-01b]: Layout components (TopBar, AppShell, AppSidebar, LanguageToggle) also use wrapper imports, not direct ui/
- [Phase 01.2]: [01.2-01b]: Wrapper import pattern enforced: @/shared/components/Button, never @/shared/components/ui/button
- [Phase 01.2]: [01.2-02]: SiteHeader replaces TopBar with route-based breadcrumbs, language toggle, and user dropdown
- [Phase 01.2]: [01.2-02]: NavUser in SidebarFooter following dashboard-01 convention
- [Phase 01.2]: [01.2-02]: Disabled sidebar placeholder items with Tooltip "Coming soon" for Phases 2-9
- [Phase 01.2]: [01.2-02]: IconSelector from @tabler/icons-react used instead of lucide ChevronsUpDown
- [Phase 01.2]: DataTable receives pre-configured TanStack Table instance (not raw data) -- consumers own sorting/pagination/expansion state
- [Phase 01.2]: Field+Controller for form fields, Field+FieldLabel (without Controller) for non-form filter inputs -- consistent styling, appropriate complexity

### Roadmap Evolution

- Phase 01.1 inserted after Phase 1: Change the current code structure of the backend (URGENT)
- Phase 01.2 inserted after Phase 1: Refactor frontend to shadcn/ui with TanStack Start file-based routing (URGENT)

### Pending Todos

None yet.

### Blockers/Concerns

- So Y Te API specification not publicly documented -- must obtain before Phase 7-8 for regulatory compliance (deadline 31/12/2026)
- Zalo OA account not yet created -- should start creation process during Phase 3-4 development (v2 dependency but long lead time)
- TanStack Start is RC stage -- pin versions carefully, plan upgrade task when v1.0 stable ships
- Barcode scanner hardware decision needed before Phase 8 planning

## Session Continuity

Last session: 2026-03-01
Stopped at: Completed 01.2-04-PLAN.md
Resume file: .planning/phases/01.2-refactor-frontend-to-shadcn-ui-with-tanstack-start-file-based-routing/01.2-04-SUMMARY.md
