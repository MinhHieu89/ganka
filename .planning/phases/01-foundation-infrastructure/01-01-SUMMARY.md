---
phase: 01-foundation-infrastructure
plan: 01
subsystem: infra
tags: [dotnet10, modular-monolith, wolverine, ef-core, jwt, ddd, shared-kernel]

# Dependency graph
requires:
  - phase: none
    provides: "First plan - no dependencies"
provides:
  - ".NET 10 solution with 43 projects (Ganka28.slnx)"
  - "Shared.Domain base classes: AggregateRoot (BranchId), Entity, ValueObject, IDomainEvent, IAuditable, BranchId, Result<T>, Error"
  - "Shared.Application interfaces: ICurrentUser, IBranchContext"
  - "Shared.Infrastructure implementations: CurrentUser, BranchContext (JWT claim readers)"
  - "9 module scaffolds with 4-layer pattern (Domain/Contracts/Application/Infrastructure)"
  - "Schema-per-module DbContexts for all 9 modules"
  - "Bootstrapper host with Wolverine, EF Core, JWT Bearer, FluentValidation, Swagger"
  - "Assembly Marker classes for Wolverine handler discovery"
  - "Test project scaffolds: Auth.Integration.Tests (Testcontainers, Bogus), Audit.Unit.Tests (NSubstitute)"
affects: [01-02, 01-03, 01-04, 01-05, 01-06, 01-07]

# Tech tracking
tech-stack:
  added: [".NET 10.0", "WolverineFx 5.x", "WolverineFx.Http 5.x", "WolverineFx.Http.FluentValidation 5.x", "WolverineFx.SqlServer 5.x", "WolverineFx.EntityFrameworkCore 5.x", "Microsoft.EntityFrameworkCore.SqlServer 10.x", "Microsoft.AspNetCore.Authentication.JwtBearer 10.x", "Swashbuckle.AspNetCore 7.x", "FluentValidation 12.x", "Azure.Storage.Blobs 12.x", "Bogus 35.x", "Testcontainers 4.x", "FluentAssertions 8.x", "NSubstitute 5.x"]
  patterns: ["Schema-per-module EF Core DbContext isolation", "4-layer module pattern (Domain/Contracts/Application/Infrastructure)", "Custom Result<T> pattern without Ardalis.Result", "BranchId multi-tenant on AggregateRoot", "Assembly Marker for Wolverine handler discovery", "DDD base classes with private setters"]

key-files:
  created:
    - "backend/Ganka28.slnx"
    - "backend/src/Shared/Shared.Domain/AggregateRoot.cs"
    - "backend/src/Shared/Shared.Domain/Entity.cs"
    - "backend/src/Shared/Shared.Domain/ValueObject.cs"
    - "backend/src/Shared/Shared.Domain/Result.cs"
    - "backend/src/Shared/Shared.Domain/Error.cs"
    - "backend/src/Shared/Shared.Domain/BranchId.cs"
    - "backend/src/Shared/Shared.Domain/IDomainEvent.cs"
    - "backend/src/Shared/Shared.Domain/IAuditable.cs"
    - "backend/src/Shared/Shared.Application/ICurrentUser.cs"
    - "backend/src/Shared/Shared.Application/IBranchContext.cs"
    - "backend/src/Shared/Shared.Infrastructure/CurrentUser.cs"
    - "backend/src/Shared/Shared.Infrastructure/BranchContext.cs"
    - "backend/src/Bootstrapper/Program.cs"
    - "backend/src/Bootstrapper/appsettings.json"
    - "backend/src/Bootstrapper/appsettings.Development.json"
  modified: []

key-decisions:
  - "Used .slnx format (modern .NET 10 default) instead of legacy .sln"
  - "Custom Result<T> with implicit conversion from T -- no Ardalis.Result dependency per user decision"
  - "Added WolverineFx.EntityFrameworkCore package for UseEntityFrameworkCoreTransactions middleware"
  - "FrameworkReference Microsoft.AspNetCore.App on Shared.Infrastructure for IHttpContextAccessor"
  - "Assembly Marker pattern for Wolverine handler discovery across module boundaries"

patterns-established:
  - "Module 4-layer: {Module}.Domain -> Shared.Domain only, {Module}.Contracts -> Shared.Contracts only, {Module}.Application -> Module.Domain + Module.Contracts + Shared.Application, {Module}.Infrastructure -> Module.Application + Module.Domain + Shared.Infrastructure"
  - "DbContext per module with HasDefaultSchema for schema isolation"
  - "BranchId as readonly record struct with implicit Guid conversions"
  - "Result<T> with static factory methods and implicit T conversion"
  - "Error as sealed record with static factory methods (None, NullValue, Validation, NotFound, Unauthorized, Conflict, Custom)"
  - "Bootstrapper registers all module DbContexts via generic ConfigureDbContext<T> helper"

requirements-completed: [ARC-02]

# Metrics
duration: 10min
completed: 2026-02-28
---

# Phase 1 Plan 01: Backend Scaffolding Summary

**.NET 10 modular monolith with 43 projects, shared DDD kernel (AggregateRoot/BranchId/Result<T>), Wolverine HTTP + EF Core + JWT auth bootstrapper**

## Performance

- **Duration:** 10 min
- **Started:** 2026-02-28T13:20:11Z
- **Completed:** 2026-02-28T13:30:31Z
- **Tasks:** 2
- **Files modified:** 90

## Accomplishments
- Created complete .NET 10 solution with 43 projects: 4 shared kernel, 36 module (9 modules x 4 layers), 1 Bootstrapper, 2 test projects
- Implemented shared DDD kernel: AggregateRoot with BranchId multi-tenant support, Entity with soft delete, ValueObject, IDomainEvent, IAuditable, custom Result<T>/Error pattern
- Configured Bootstrapper with Wolverine message bus (SQL Server outbox, EF Core transactions), JWT Bearer auth, FluentValidation, Swagger/OpenAPI
- All 9 module DbContexts scaffolded with schema-per-module isolation (auth, audit, patient, clinical, scheduling, pharmacy, optical, billing, treatment)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create solution structure with all module .csproj files and shared kernel** - `94f406f` (feat)
2. **Task 2: Configure Bootstrapper host with Wolverine, EF Core, JWT, and middleware pipeline** - `39940cf` (feat)

## Files Created/Modified
- `backend/Ganka28.slnx` - Solution file with 43 projects
- `backend/src/Shared/Shared.Domain/*.cs` - DDD base classes (8 files: AggregateRoot, Entity, ValueObject, IDomainEvent, IAuditable, BranchId, Result, Error)
- `backend/src/Shared/Shared.Application/*.cs` - Shared interfaces (ICurrentUser, IBranchContext)
- `backend/src/Shared/Shared.Infrastructure/*.cs` - JWT claim readers (CurrentUser, BranchContext)
- `backend/src/Bootstrapper/Program.cs` - 178-line host wiring with Wolverine, EF Core, JWT, Swagger
- `backend/src/Bootstrapper/appsettings*.json` - Connection string, JWT config, logging
- `backend/src/Modules/*/` - 36 module .csproj files with correct inter-project references
- `backend/src/Modules/*/Infrastructure/*DbContext.cs` - 9 scaffold DbContexts with schema isolation
- `backend/src/Modules/*/Application/Marker.cs` - 9 assembly markers for Wolverine discovery
- `backend/tests/Auth.Integration.Tests/` - xUnit + Testcontainers + Bogus + UserFaker scaffold
- `backend/tests/Audit.Unit.Tests/` - xUnit + NSubstitute + FluentAssertions scaffold
- `backend/.gitignore` - Excludes bin/obj/IDE files

## Decisions Made
- **Used .slnx format:** .NET 10 CLI generates .slnx by default (new XML-based solution format). Works identically with `dotnet sln` commands.
- **Custom Result<T> without Ardalis.Result:** Implemented per user decision. Result has implicit T conversion for ergonomic returns. Error is a sealed record with factory methods.
- **Added WolverineFx.EntityFrameworkCore:** Required for `UseEntityFrameworkCoreTransactions()` -- not in original plan's NuGet list but needed for the functionality specified.
- **FrameworkReference on Shared.Infrastructure:** Added `Microsoft.AspNetCore.App` framework reference so CurrentUser/BranchContext can access IHttpContextAccessor and ClaimsPrincipal without pulling in full ASP.NET NuGet packages.
- **Assembly Marker pattern:** Static Marker classes in each Application project for Wolverine's `Discovery.IncludeAssembly()` -- cleaner than raw assembly scanning.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added WolverineFx.EntityFrameworkCore NuGet package**
- **Found during:** Task 2 (Bootstrapper configuration)
- **Issue:** `UseEntityFrameworkCoreTransactions()` requires `WolverineFx.EntityFrameworkCore` package which was not listed in the plan's NuGet packages
- **Fix:** Added `WolverineFx.EntityFrameworkCore` to Bootstrapper.csproj and corresponding using directive
- **Files modified:** backend/src/Bootstrapper/Bootstrapper.csproj, backend/src/Bootstrapper/Program.cs
- **Verification:** Build succeeds with 0 errors
- **Committed in:** 39940cf (Task 2 commit)

**2. [Rule 3 - Blocking] Added FrameworkReference for ASP.NET Core on Shared.Infrastructure**
- **Found during:** Task 2 (CurrentUser/BranchContext implementation)
- **Issue:** CurrentUser and BranchContext require IHttpContextAccessor and ClaimsPrincipal which are in the ASP.NET Core shared framework
- **Fix:** Added `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to Shared.Infrastructure.csproj
- **Files modified:** backend/src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj
- **Verification:** Build succeeds with 0 errors
- **Committed in:** 39940cf (Task 2 commit)

**3. [Rule 3 - Blocking] Created Assembly Marker classes for Wolverine discovery**
- **Found during:** Task 2 (Wolverine configuration)
- **Issue:** Wolverine's `Discovery.IncludeAssembly()` needs a type from each assembly to reference; empty class libraries have no types after removing Class1.cs
- **Fix:** Created static Marker class in each module's Application project
- **Files modified:** 9 Marker.cs files in module Application projects
- **Verification:** Build succeeds, Wolverine can discover handlers
- **Committed in:** 39940cf (Task 2 commit)

**4. [Rule 2 - Missing Critical] Added .gitignore to exclude build artifacts**
- **Found during:** Task 2 (commit preparation)
- **Issue:** Task 1 commit accidentally included all bin/obj build artifacts since no .gitignore existed
- **Fix:** Created backend/.gitignore, removed tracked bin/obj files from git
- **Files modified:** backend/.gitignore (new), removed ~2000 tracked build artifacts
- **Verification:** git status shows clean after build
- **Committed in:** 39940cf (Task 2 commit)

---

**Total deviations:** 4 auto-fixed (2 blocking, 1 missing critical, 1 blocking)
**Impact on plan:** All auto-fixes necessary for the solution to build and function correctly. No scope creep.

## Issues Encountered
- Shell heredoc variable expansion: Initial batch creation of .csproj files via bash heredoc failed to expand `$MODULE` variable in paths (used `<<CSPROJ` instead of `<<'CSPROJ'`), resulting in literal `$MODULE` in ProjectReference paths. Fixed by rewriting all 18 Application/Infrastructure .csproj files individually.
- NuGet transitive dependency warnings (NU1608): Microsoft.CodeAnalysis.Workspaces.MSBuild version constraint conflict from Wolverine's code generation dependency. This is a transitive dependency outside our control and does not affect functionality.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Solution compiles successfully with all 43 projects
- Shared kernel DDD base classes ready for Auth domain entities (Plan 01-03)
- Bootstrapper wired for Wolverine HTTP endpoints -- handlers will be auto-discovered
- Auth.Integration.Tests scaffold ready for TDD in Plan 01-03
- Audit.Unit.Tests scaffold ready for TDD in Plan 01-04
- All 9 module DbContexts scaffolded and registered in DI

## Self-Check: PASSED

All key files exist. Both task commits verified (94f406f, 39940cf). Solution builds with 0 errors.

---
*Phase: 01-foundation-infrastructure*
*Completed: 2026-02-28*
