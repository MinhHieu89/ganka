---
phase: 08-optical-center
plan: 09
subsystem: database
tags: [efcore, sql-server, optical, frames, lenses, configurations]

# Dependency graph
requires:
  - phase: 08-optical-center/08-03
    provides: Frame aggregate root entity
  - phase: 08-optical-center/08-04
    provides: LensCatalogItem, LensStockEntry, LensOrder entities

provides:
  - EF Core configuration for Frame with unique filtered barcode index
  - EF Core configuration for LensCatalogItem with backing field for StockEntries
  - EF Core configuration for LensStockEntry with unique power combo index
  - EF Core configuration for LensOrder with prescription decimal columns

affects: [08-optical-center, migrations, optical-infrastructure]

# Tech tracking
tech-stack:
  added: [FluentValidation.DependencyInjectionExtensions (fixed missing dep in Optical.Application)]
  patterns:
    - Unique filtered index on nullable Barcode column ([Barcode] IS NOT NULL)
    - Backing field pattern for aggregate child collections (HasField + UsePropertyAccessMode)
    - Flags enum stored as combined int via HasConversion<int>
    - BranchId value object conversion via HasConversion with lambda pair

key-files:
  created:
    - backend/src/Modules/Optical/Optical.Infrastructure/Configurations/FrameConfiguration.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Configurations/LensCatalogItemConfiguration.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Configurations/LensOrderConfiguration.cs
  modified:
    - backend/src/Modules/Optical/Optical.Application/Optical.Application.csproj

key-decisions:
  - "FrameType column renamed to FrameType via HasColumnName to avoid SQL keyword conflict with Type"
  - "Barcode unique index filtered on IS NOT NULL to allow multiple null barcodes (frames awaiting barcode assignment)"
  - "LensStockEntry unique composite index on (LensCatalogItemId, Sph, Cyl, Add) prevents duplicate power entries"
  - "LensOrder prescription decimals use decimal(5,2) - sufficient for clinical range -99.99 to +99.99"
  - "RequestedCoatings stored as int for bitwise flags combination"

patterns-established:
  - "Pattern: Unique filtered index on nullable columns - HasIndex().IsUnique().HasFilter('[Col] IS NOT NULL')"
  - "Pattern: Backing field collection mapping - Navigation().HasField('_fieldName').UsePropertyAccessMode(PropertyAccessMode.Field)"

requirements-completed: [OPT-01, OPT-02]

# Metrics
duration: 15min
completed: 2026-03-08
---

# Phase 08 Plan 09: EF Core Configurations for Frame, LensCatalogItem, LensOrder Summary

**Three EF Core configurations mapping optical inventory entities to SQL Server with unique filtered barcode index on Frame, backing field collection for LensStockEntries, and decimal(5,2) precision for prescription parameters**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-08T02:36:00Z
- **Completed:** 2026-03-08T02:51:52Z
- **Tasks:** 2
- **Files modified:** 4 (3 created + 1 csproj fixed)

## Accomplishments
- FrameConfiguration maps all frame attributes with unique filtered barcode index preventing duplicate barcodes from mixed manufacturer/clinic-generated sources
- LensCatalogItemConfiguration uses backing field pattern for StockEntries collection with unique composite index on power combinations
- LensOrderConfiguration maps all prescription parameters (Sph, Cyl, Add, Axis) with decimal(5,2) precision and GlassesOrderId index
- Fixed missing FluentValidation.DependencyInjectionExtensions package in Optical.Application.csproj (was blocking compilation)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Frame EF Core configuration** - `d3e7165` (feat)
2. **Task 2: Create LensCatalogItem and LensOrder EF Core configurations** - `309e17d` (feat)

## Files Created/Modified
- `backend/src/Modules/Optical/Optical.Infrastructure/Configurations/FrameConfiguration.cs` - Frame entity mapping with unique filtered barcode index, enum conversions, BranchId value object
- `backend/src/Modules/Optical/Optical.Infrastructure/Configurations/LensCatalogItemConfiguration.cs` - LensCatalogItem + LensStockEntry configurations with backing field and composite unique index
- `backend/src/Modules/Optical/Optical.Infrastructure/Configurations/LensOrderConfiguration.cs` - LensOrder configuration mapping all prescription parameters and supplier reference
- `backend/src/Modules/Optical/Optical.Application/Optical.Application.csproj` - Added FluentValidation.DependencyInjectionExtensions package reference

## Decisions Made
- Barcode unique index filtered with `[Barcode] IS NOT NULL` to allow frames awaiting barcode assignment without violating uniqueness
- FrameType column renamed to `FrameType` via HasColumnName to avoid potential SQL keyword conflict
- LensCoating and FrameMaterial/FrameType/FrameGender enums all stored as int via HasConversion<int>()
- LensStockEntry power combo unique index covers (LensCatalogItemId, Sph, Cyl, Add) to prevent duplicate power entries

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed missing FluentValidation.DependencyInjectionExtensions in Optical.Application.csproj**
- **Found during:** Task 1 (build verification after creating FrameConfiguration)
- **Issue:** Optical.Infrastructure depends on Optical.Application which uses `AddValidatorsFromAssembly` from FluentValidation.DependencyInjectionExtensions but the package was not referenced in Optical.Application.csproj
- **Fix:** Added `<PackageReference Include="FluentValidation.DependencyInjectionExtensions" />` to Optical.Application.csproj, following Pharmacy.Application pattern
- **Files modified:** `backend/src/Modules/Optical/Optical.Application/Optical.Application.csproj`
- **Verification:** `dotnet build Optical.Infrastructure.csproj` succeeds with 0 errors
- **Committed in:** d3e7165 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking dependency fix)
**Impact on plan:** Essential fix - build was blocked without it. No scope creep.

## Issues Encountered
- Domain entity files were already created by earlier plans (08-03, 08-04) even though git log did not show their commits. The entities existed at the expected paths and the Glob tool missed them due to path search. Proceeded with EF Core configuration as planned once entities were confirmed present.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 3 EF Core configuration files exist and Optical.Infrastructure compiles
- OpticalDbContext needs DbSet registrations and ApplyConfigurationsFromAssembly call before migrations can be run
- Ready for plan 08-10 (GlassesOrder, ComboPackage, WarrantyClaim, StocktakingSession configurations)

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*

## Self-Check: PASSED

- FOUND: backend/src/Modules/Optical/Optical.Infrastructure/Configurations/FrameConfiguration.cs
- FOUND: backend/src/Modules/Optical/Optical.Infrastructure/Configurations/LensCatalogItemConfiguration.cs
- FOUND: backend/src/Modules/Optical/Optical.Infrastructure/Configurations/LensOrderConfiguration.cs
- FOUND: .planning/phases/08-optical-center/08-09-SUMMARY.md
- FOUND commit: d3e7165 (Task 1 - Frame configuration)
- FOUND commit: 309e17d (Task 2 - LensCatalogItem + LensOrder configurations)
- Build: Optical.Infrastructure.csproj succeeds with 0 errors
