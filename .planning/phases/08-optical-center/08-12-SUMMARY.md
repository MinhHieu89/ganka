---
phase: 08-optical-center
plan: 12
subsystem: api
tags: [csharp, dotnet, repository-pattern, optical, glasses-orders, warranty, stocktaking]

# Dependency graph
requires:
  - phase: 08-09
    provides: EF Core configurations for Frame, LensCatalogItem, LensOrder
  - phase: 08-10
    provides: EF Core configurations for GlassesOrder, ComboPackage, WarrantyClaim, StocktakingSession
provides:
  - IGlassesOrderRepository interface with overdue order query and status/patient/visit filtering
  - IComboPackageRepository interface with active-only filter toggle
  - IWarrantyClaimRepository interface with approval status filter and order lookup
  - IStocktakingRepository interface with current session query for active stocktaking
affects: [08-13, 08-14, 08-15, 08-16, 08-17, 08-18, 08-19, 08-20, 08-21]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - Repository interface pattern following Pharmacy.Application.Interfaces structure
    - Separate GetTotalCountAsync for pagination metadata
    - GetCurrentSessionAsync for singleton-like active session query

key-files:
  created:
    - backend/src/Modules/Optical/Optical.Application/Interfaces/IGlassesOrderRepository.cs
    - backend/src/Modules/Optical/Optical.Application/Interfaces/IComboPackageRepository.cs
    - backend/src/Modules/Optical/Optical.Application/Interfaces/IWarrantyClaimRepository.cs
    - backend/src/Modules/Optical/Optical.Application/Interfaces/IStocktakingRepository.cs
  modified: []

key-decisions:
  - "IGlassesOrderRepository.GetAllAsync takes int? statusFilter instead of enum to match Pharmacy pattern and avoid casts in handlers"
  - "IComboPackageRepository.GetAllAsync uses includeInactive bool flag for clean active-only filtering"
  - "IStocktakingRepository.GetCurrentSessionAsync returns null when no active session (single active session assumption)"
  - "IWarrantyClaimRepository.GetByOrderIdAsync supports multiple claims per order (history view)"

patterns-established:
  - "Pattern: Repository interfaces live in Optical.Application.Interfaces namespace with domain entity types as parameters"
  - "Pattern: Paginated queries split into GetAllAsync(page, pageSize) + GetTotalCountAsync for metadata"
  - "Pattern: void Add(entity) for change tracking, caller invokes IUnitOfWork.SaveChangesAsync"

requirements-completed: [OPT-03, OPT-06, OPT-07, OPT-09]

# Metrics
duration: 4min
completed: 2026-03-08
---

# Phase 08 Plan 12: Repository Interfaces for GlassesOrder, ComboPackage, Warranty, and Stocktaking Summary

**4 repository interfaces defining data access contracts for glasses order lifecycle, combo pricing, warranty claim workflow, and barcode-based stocktaking**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-08T02:48:33Z
- **Completed:** 2026-03-08T02:52:47Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- IGlassesOrderRepository with overdue order query, status/patient/visit filtering, and pagination
- IComboPackageRepository with active-only filter for order creation workflow
- IWarrantyClaimRepository with approval status filter and per-order lookup for warranty history
- IStocktakingRepository with GetCurrentSessionAsync for active InProgress session detection

## Task Commits

Each task was committed atomically:

1. **Task 1: Create GlassesOrder and ComboPackage repository interfaces** - `9da24c8` (feat)
2. **Task 2: Create WarrantyClaim and Stocktaking repository interfaces** - `a47efbd` (feat)

## Files Created/Modified
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IGlassesOrderRepository.cs` - GetByIdAsync (with items), GetAllAsync (paginated, status filter), GetByPatientIdAsync, GetByVisitIdAsync, GetOverdueOrdersAsync, GetTotalCountAsync, Add
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IComboPackageRepository.cs` - GetByIdAsync, GetAllAsync (includeInactive flag), Add
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IWarrantyClaimRepository.cs` - GetByIdAsync, GetByOrderIdAsync, GetAllAsync (approval status filter, paginated), GetTotalCountAsync, Add
- `backend/src/Modules/Optical/Optical.Application/Interfaces/IStocktakingRepository.cs` - GetByIdAsync (with items), GetCurrentSessionAsync, GetAllAsync (paginated), GetTotalCountAsync, Add

## Decisions Made
- statusFilter uses `int?` (not enum) in GetAllAsync methods to match established Pharmacy pattern and avoid type casting in handlers
- includeInactive bool flag for ComboPackage is more expressive than a nullable status filter
- GetCurrentSessionAsync assumes at most one InProgress session at a time (per business logic)
- GetByOrderIdAsync on IWarrantyClaimRepository returns a list (not single) to support multiple claims per order

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 4 repository interfaces are defined and Optical.Application compiles
- Application layer handlers (plans 08-13 through 08-21) can now implement commands using these interfaces
- Infrastructure layer can implement these interfaces in concrete repository classes

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*
