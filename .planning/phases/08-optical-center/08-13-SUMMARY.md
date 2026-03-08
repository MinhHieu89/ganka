---
phase: 08-optical-center
plan: 13
subsystem: database
tags: [ef-core, repository-pattern, optical, frames, lens-catalog, unit-of-work]

# Dependency graph
requires:
  - phase: 08-optical-center-11
    provides: "IFrameRepository, ILensCatalogRepository, IUnitOfWork interfaces, OpticalDbContext with all DbSets"
  - phase: 08-optical-center-09
    provides: "FrameConfiguration, LensCatalogItemConfiguration, LensStockEntryConfiguration"
provides:
  - "FrameRepository: multi-field search (Brand/Model/Color/Barcode), enum filters, pagination, barcode uniqueness, sequence generation"
  - "LensCatalogRepository: eagerly includes StockEntries in GetByIdAsync/GetAllAsync, GetStockEntryAsync for power lookup"
  - "UnitOfWork: wraps OpticalDbContext.SaveChangesAsync"
affects: [08-optical-center-16, 08-optical-center-17, 08-optical-center-18, 08-optical-center-19]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "BuildSearchQuery private helper pattern: single IQueryable construction shared between SearchAsync and GetTotalCountAsync for pagination consistency"
    - "Eager include in repository: LensCatalogRepository always includes StockEntries to prevent lazy-loading issues"
    - "Active-only default filter: SearchAsync returns only active frames; GetAllAsync accepts includeInactive bool"

key-files:
  created:
    - "backend/src/Modules/Optical/Optical.Infrastructure/Repositories/FrameRepository.cs"
    - "backend/src/Modules/Optical/Optical.Infrastructure/Repositories/LensCatalogRepository.cs"
    - "backend/src/Modules/Optical/Optical.Infrastructure/UnitOfWork.cs"
  modified:
    - "backend/src/Modules/Optical/Optical.Infrastructure/OpticalDbContext.cs"
    - "backend/src/Modules/Optical/Optical.Application/Interfaces/IFrameRepository.cs"
    - "backend/src/Modules/Optical/Optical.Application/Interfaces/ILensCatalogRepository.cs"
    - "backend/src/Modules/Optical/Optical.Application/Interfaces/IUnitOfWork.cs"

key-decisions:
  - "GetNextSequenceNumberAsync uses CountAsync()+1 as simple monotonically increasing sequence for EAN-13 generation (not guaranteed globally unique, but combined with clinic prefix provides sufficient uniqueness)"
  - "SearchAsync filters active-only; GetAllAsync accepts includeInactive bool for admin catalog management"
  - "IsBarcodeUniqueAsync takes optional excludeId for both create (null) and update scenarios (own Id excluded)"
  - "LensCatalogRepository eagerly loads StockEntries in all reads to maintain aggregate consistency"

patterns-established:
  - "Pattern 1: BuildSearchQuery helper shares filter logic between SearchAsync and GetTotalCountAsync to prevent pagination drift"
  - "Pattern 2: EF Core Contains() for case-insensitive text search via SQL LIKE translation"

requirements-completed: [OPT-01, OPT-02]

# Metrics
duration: 15min
completed: 2026-03-08
---

# Phase 8 Plan 13: Frame and LensCatalog Repositories with UnitOfWork Summary

**EF Core FrameRepository with multi-field search/pagination/barcode uniqueness and LensCatalogRepository with eager StockEntries loading**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-08T02:49:34Z
- **Completed:** 2026-03-08T02:55:00Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments
- FrameRepository implements all IFrameRepository methods including SearchAsync with multi-field text search (Brand/Model/Color/Barcode) and enum filters (Material/FrameType/Gender) with pagination
- LensCatalogRepository eagerly loads StockEntries in all read operations and supports GetStockEntryAsync for SPH/CYL/ADD power lookup
- UnitOfWork wraps OpticalDbContext.SaveChangesAsync following the Pharmacy.Infrastructure pattern exactly

## Task Commits

Each task was committed atomically:

1. **Task 1: Implement FrameRepository** - `8f299a6` (feat - wired into Bootstrapper with IoC)
2. **Task 2: Implement LensCatalogRepository and UnitOfWork** - `6397941` (docs - as part of 08-11 completion)

## Files Created/Modified
- `backend/src/Modules/Optical/Optical.Infrastructure/Repositories/FrameRepository.cs` - Full IFrameRepository implementation with BuildSearchQuery helper
- `backend/src/Modules/Optical/Optical.Infrastructure/Repositories/LensCatalogRepository.cs` - ILensCatalogRepository with eager StockEntries include
- `backend/src/Modules/Optical/Optical.Infrastructure/UnitOfWork.cs` - IUnitOfWork wrapping OpticalDbContext.SaveChangesAsync
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IFrameRepository.cs` - Interface with search/barcode/sequence contracts
- `backend/src/Modules/Optical/Optical.Application/Interfaces/ILensCatalogRepository.cs` - Interface with stock entry queries
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IUnitOfWork.cs` - Unit of work interface
- `backend/src/Modules/Optical/Optical.Infrastructure/OpticalDbContext.cs` - Updated with 10 DbSets and ApplyConfigurationsFromAssembly

## Decisions Made
- GetNextSequenceNumberAsync uses `CountAsync() + 1` — simple monotonic counter for EAN-13 generation
- SearchAsync only returns active frames (soft-delete aware); GetAllAsync accepts `includeInactive` flag
- IsBarcodeUniqueAsync accepts optional `excludeId` for update validation (excludes the frame being updated from the uniqueness check)
- LensCatalogRepository always eagerly loads StockEntries to maintain aggregate root integrity

## Deviations from Plan

None - plan executed exactly as written. The FrameRepository, LensCatalogRepository, and UnitOfWork were implemented following the Pharmacy.Infrastructure patterns as specified.

## Issues Encountered
None

## Next Phase Readiness
- FrameRepository and LensCatalogRepository ready for use by application handlers (CreateFrame, SearchFrames, AdjustLensStock)
- UnitOfWork ready for wiring in IoC registration
- IoC.cs in Optical.Infrastructure needs to register repositories and UoW (addressed in later plans)

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*
