---
phase: 08-optical-center
plan: 11
subsystem: database
tags: [efcore, optical, dbcontext, repository-interfaces, unit-of-work]

# Dependency graph
requires:
  - phase: 08-optical-center/08-09
    provides: EF Core configurations for Frame, LensCatalogItem, LensOrder
  - phase: 08-optical-center/08-10
    provides: EF Core configurations for GlassesOrder, ComboPackage, WarrantyClaim, StocktakingSession

provides:
  - OpticalDbContext with all 10 entity DbSets and ApplyConfigurationsFromAssembly
  - IFrameRepository interface with barcode lookup and paginated search
  - ILensCatalogRepository interface with stock entry queries
  - IUnitOfWork interface for Optical module persistence coordination

affects: [08-optical-center, optical-infrastructure, optical-application]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - DbSet registration as expression-bodied properties using Set<T>()
    - ApplyConfigurationsFromAssembly for auto-discovering IEntityTypeConfiguration implementations
    - Repository interface pattern with async methods and CancellationToken
    - Unit of Work abstraction over DbContext.SaveChangesAsync

key-files:
  created:
    - backend/src/Modules/Optical/Optical.Application/Interfaces/IFrameRepository.cs
    - backend/src/Modules/Optical/Optical.Application/Interfaces/ILensCatalogRepository.cs
    - backend/src/Modules/Optical/Optical.Application/Interfaces/IUnitOfWork.cs
  modified:
    - backend/src/Modules/Optical/Optical.Infrastructure/OpticalDbContext.cs

key-decisions:
  - "All 10 DbSets registered as expression-bodied properties following PharmacyDbContext pattern"
  - "ApplyConfigurationsFromAssembly added to OnModelCreating for auto-discovery of IEntityTypeConfiguration implementations"
  - "IFrameRepository includes GetNextSequenceNumberAsync for EAN-13 barcode generation workflow"
  - "IFrameRepository.SearchAsync accepts int? parameters for enum filters (Material, FrameType, Gender) matching handler pattern"
  - "ILensCatalogRepository.GetByIdAsync eagerly includes StockEntries for full aggregate loading"
  - "IUnitOfWork has default parameter CancellationToken cancellationToken = default matching Pharmacy pattern"

patterns-established:
  - "Pattern: Repository interface defines void Add() for change tracking, async Get methods for queries"
  - "Pattern: IUnitOfWork.SaveChangesAsync as the single persistence point per handler"

requirements-completed: [OPT-01, OPT-02]

# Metrics
duration: 10min
completed: 2026-03-08
---

# Phase 08 Plan 11: OpticalDbContext and Repository Interfaces Summary

**OpticalDbContext updated with 10 entity DbSets and ApplyConfigurationsFromAssembly; IFrameRepository, ILensCatalogRepository, and IUnitOfWork interfaces created for handler data access contracts**

## Performance

- **Duration:** 10 min
- **Started:** 2026-03-08T02:48:36Z
- **Completed:** 2026-03-08T02:58:00Z
- **Tasks:** 2/2
- **Files modified:** 4 (3 created + 1 modified)

## Accomplishments

- OpticalDbContext updated with all 10 entity DbSets (Frame, LensCatalogItem, LensStockEntry, LensOrder, GlassesOrder, GlassesOrderItem, ComboPackage, WarrantyClaim, StocktakingSession, StocktakingItem) following PharmacyDbContext pattern
- `modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpticalDbContext).Assembly)` added to OnModelCreating for auto-discovery of all IEntityTypeConfiguration implementations
- IFrameRepository interface created with 8 methods: GetByIdAsync, GetByBarcodeAsync, GetAllAsync, SearchAsync (with Material/FrameType/Gender filters + pagination), GetTotalCountAsync, GetNextSequenceNumberAsync (EAN-13 barcode generation), IsBarcodeUniqueAsync, and void Add
- ILensCatalogRepository interface created with 4 methods: GetByIdAsync (includes StockEntries), GetAllAsync, GetStockEntryAsync (finds specific SPH/CYL/ADD stock entry), and void Add
- IUnitOfWork interface created matching Pharmacy.Application.Interfaces.IUnitOfWork pattern

## Task Commits

Work was completed as part of earlier out-of-order execution:

1. **Task 1: Update OpticalDbContext with all DbSets** - Committed in `8f299a6` (feat(08-24): wire Optical module in Bootstrapper with IoC + endpoint mapping)
2. **Task 2: Create repository interfaces** - Committed in `8f299a6` (feat(08-24): wire Optical module in Bootstrapper with IoC + endpoint mapping)

## Files Created/Modified

- `backend/src/Modules/Optical/Optical.Infrastructure/OpticalDbContext.cs` - Added 10 DbSets, using Optical.Domain.Entities, ApplyConfigurationsFromAssembly
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IFrameRepository.cs` - Frame repository with barcode lookup, paginated search, sequence generation
- `backend/src/Modules/Optical/Optical.Application/Interfaces/ILensCatalogRepository.cs` - Lens catalog repository with stock entry query
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IUnitOfWork.cs` - Unit of work with SaveChangesAsync

## Decisions Made

- DbSets registered as expression-bodied properties (`DbSet<T> Frames => Set<Frame>()`) matching PharmacyDbContext pattern
- Frame search filters accept `int?` for Material/FrameType/Gender to match the handler command parameter style (enums passed as int from API layer)
- GetNextSequenceNumberAsync included in IFrameRepository to support EAN-13 barcode auto-generation workflow (handler calls this, then calls Ean13Generator.Generate())
- ILensCatalogRepository.GetByIdAsync eagerly includes StockEntries since aggregate handlers always need the full aggregate

## Deviations from Plan

None - plan executed exactly as written. Both objectives (OpticalDbContext DbSets + repository interfaces) were pre-completed as part of plan 08-24 execution which wired the full Optical module into the Bootstrapper.

## Issues Encountered

- All work was already completed in an earlier out-of-order execution (plan 08-24). Verification confirmed both Optical.Infrastructure and Optical.Application compile with 0 errors.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- OpticalDbContext has all 10 DbSets and ApplyConfigurationsFromAssembly ready for migration generation
- Repository interfaces define contracts for all feature handlers
- Ready for plans implementing concrete repository classes (FrameRepository, LensCatalogRepository, etc.)

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*

## Self-Check: PASSED

**Files verified:**
- `backend/src/Modules/Optical/Optical.Infrastructure/OpticalDbContext.cs` - FOUND, contains 10 DbSets + ApplyConfigurationsFromAssembly
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IFrameRepository.cs` - FOUND, 8 methods including barcode/search/sequence
- `backend/src/Modules/Optical/Optical.Application/Interfaces/ILensCatalogRepository.cs` - FOUND, 4 methods including GetStockEntryAsync
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IUnitOfWork.cs` - FOUND, SaveChangesAsync

**Build verification:**
- Optical.Infrastructure: Build succeeded, 0 Warning(s), 0 Error(s)
- Optical.Application: Build succeeded, 0 Warning(s), 0 Error(s)
