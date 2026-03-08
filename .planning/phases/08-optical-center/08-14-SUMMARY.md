---
phase: 08-optical-center
plan: 14
subsystem: database
tags: [efcore, repository-pattern, optical, glasses-order, warranty, stocktaking]

# Dependency graph
requires:
  - phase: 08-optical-center
    provides: "Domain entities (GlassesOrder, ComboPackage, WarrantyClaim, StocktakingSession), repository interfaces (IGlassesOrderRepository, IComboPackageRepository, IWarrantyClaimRepository, IStocktakingRepository), EF Core configurations"
provides:
  - "GlassesOrderRepository with overdue order detection"
  - "ComboPackageRepository with active filter"
  - "WarrantyClaimRepository with approval status filter"
  - "StocktakingRepository with current InProgress session lookup"
  - "Missing EF Core configurations: WarrantyClaimConfiguration (JSON DocumentUrls), StocktakingSessionConfiguration (unique barcode index)"
affects: [08-optical-center, application-handlers, di-registration]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Repository pattern with eager loading of child collections via Include()"
    - "Status filter as nullable int cast to enum"
    - "GetCurrentSessionAsync: WhereStatus==InProgress + OrderByDescending(CreatedAt) + FirstOrDefaultAsync"
    - "GetOverdueOrdersAsync: EstimatedDeliveryDate < UtcNow AND Status != Delivered"
    - "JSON column conversion for List<string> using System.Text.Json"

key-files:
  created:
    - backend/src/Modules/Optical/Optical.Infrastructure/Repositories/GlassesOrderRepository.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Repositories/ComboPackageRepository.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Repositories/WarrantyClaimRepository.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Repositories/StocktakingRepository.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Configurations/WarrantyClaimConfiguration.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Configurations/StocktakingSessionConfiguration.cs
  modified: []

key-decisions:
  - "StocktakingRepository.GetCurrentSessionAsync uses OrderByDescending(CreatedAt).FirstOrDefaultAsync for deterministic result when multiple InProgress sessions exist"
  - "GlassesOrderRepository.GetOverdueOrdersAsync captures UtcNow once before query to prevent non-deterministic LINQ translation"
  - "WarrantyClaimConfiguration stores DocumentUrls as JSON nvarchar(max) using System.Text.Json value converter"
  - "StocktakingSessionConfiguration adds unique (SessionId, Barcode) index on StocktakingItems table to enforce upsert at DB level"

patterns-established:
  - "Optical repository pattern: constructor injection of OpticalDbContext, sealed class, primary constructor syntax"
  - "Paginated list query: AsNoTracking + filter + OrderByDescending + Skip/Take"
  - "Child collection eager loading: Include(x => x.Items) on all GetByIdAsync calls"

requirements-completed: [OPT-03, OPT-06, OPT-07, OPT-09]

# Metrics
duration: 25min
completed: 2026-03-08
---

# Phase 08 Plan 14: Remaining Optical Repositories Summary

**EF Core repository implementations for GlassesOrder, ComboPackage, WarrantyClaim, and Stocktaking with overdue order detection and current session lookup**

## Performance

- **Duration:** ~25 min
- **Started:** 2026-03-08T02:30:00Z
- **Completed:** 2026-03-08T02:55:30Z
- **Tasks:** 2
- **Files modified:** 6 (4 repositories + 2 EF configurations)

## Accomplishments
- GlassesOrderRepository with overdue order detection comparing EstimatedDeliveryDate with UtcNow and Status != Delivered
- StocktakingRepository.GetCurrentSessionAsync returning InProgress session ordered by CreatedAt desc
- WarrantyClaimRepository with approval status filter for manager approval workflow
- ComboPackageRepository with active-only filter for order creation dropdowns
- All repositories eagerly load child collections (GlassesOrder.Items, StocktakingSession.Items)

## Task Commits

Each task was committed atomically:

1. **Task 1: GlassesOrderRepository and ComboPackageRepository** - `cf75646` (feat)
2. **Task 2: WarrantyClaimRepository and StocktakingRepository** - `eb9ce15` (feat)

**Plan metadata:** committed with SUMMARY.md and STATE.md

## Files Created/Modified
- `backend/src/Modules/Optical/Optical.Infrastructure/Repositories/GlassesOrderRepository.cs` - Order data access with status filter, overdue query, and eager Items loading
- `backend/src/Modules/Optical/Optical.Infrastructure/Repositories/ComboPackageRepository.cs` - Combo package data access with active/inactive filter
- `backend/src/Modules/Optical/Optical.Infrastructure/Repositories/WarrantyClaimRepository.cs` - Warranty claim data access with approval status filter
- `backend/src/Modules/Optical/Optical.Infrastructure/Repositories/StocktakingRepository.cs` - Stocktaking session data access with current InProgress session query
- `backend/src/Modules/Optical/Optical.Infrastructure/Configurations/WarrantyClaimConfiguration.cs` - EF config with JSON DocumentUrls storage
- `backend/src/Modules/Optical/Optical.Infrastructure/Configurations/StocktakingSessionConfiguration.cs` - EF config with unique (SessionId, Barcode) index

## Decisions Made
- GetCurrentSessionAsync returns most recently created InProgress session (OrderByDescending CreatedAt) when multiple sessions exist (defensive behavior)
- GetOverdueOrdersAsync captures DateTime.UtcNow once before LINQ query to avoid non-deterministic translation warnings
- WarrantyClaim DocumentUrls stored as JSON nvarchar(max) using System.Text.Json value converter (EF Core standard approach)
- StocktakingItem unique composite index (StocktakingSessionId, Barcode) enforces upsert pattern at database level per RESEARCH.md pitfall 5

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Created missing WarrantyClaimConfiguration and StocktakingSessionConfiguration**
- **Found during:** Task 1 (repository implementation)
- **Issue:** Plans 08-09 and 08-10 (EF configurations for WarrantyClaim and StocktakingSession) had not been executed, missing required configurations
- **Fix:** Created WarrantyClaimConfiguration with JSON DocumentUrls storage and StocktakingSessionConfiguration with unique (SessionId, Barcode) index on StocktakingItems
- **Files modified:** Optical.Infrastructure/Configurations/WarrantyClaimConfiguration.cs, StocktakingSessionConfiguration.cs
- **Verification:** dotnet build Optical.Infrastructure.csproj succeeds with 0 errors
- **Committed in:** cf75646 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (blocking prerequisite)
**Impact on plan:** Required to unblock repository implementation. No scope creep.

## Issues Encountered
- Plans 08-09, 08-10, and 08-12 (EF configurations and repository interfaces) were already completed but not tracked with SUMMARY.md files. All required artifacts existed in the codebase.

## Next Phase Readiness
- All 4 repository implementations ready for injection into application handlers
- IoC.cs registration for these repositories deferred to later plans (08-25 through 08-31) per original plan comment
- UnitOfWork implementation also deferred to later plans

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*
