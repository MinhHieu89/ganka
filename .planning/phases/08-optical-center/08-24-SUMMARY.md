---
phase: 08-optical-center
plan: 24
subsystem: infra
tags: [dotnet, entityframeworkcore, optical, ioc, migration, dependency-injection]

# Dependency graph
requires:
  - phase: 08-optical-center/08-22
    provides: Optical module IoC registrations and supplier seeder plan
  - phase: 08-optical-center/08-23
    provides: Optical domain models and entity configurations
  - phase: 07-billing
    provides: Billing.Contracts with GetVisitInvoiceQuery for OPT-04 payment gate
  - phase: 05-clinical
    provides: Clinical.Contracts for OPT-08 prescription history cross-module query
provides:
  - Optical module fully registered in Bootstrapper (AddOptical* + MapOptical*/MapWarranty*/MapStocktaking*)
  - Optical.Presentation project with all endpoint stubs wired
  - Cross-module references to Billing.Contracts and Clinical.Contracts in Optical.Application
  - ZXing.Net 0.16.11 in Optical.Infrastructure for barcode label PDF generation
  - EF Core migration AddOpticalEntities creating all 10 optical schema tables
affects: [09-frontend-integration, all optical endpoint plans]

# Tech tracking
tech-stack:
  added:
    - ZXing.Net 0.16.11 (barcode generation for label printing)
  patterns:
    - Optical.Presentation project created following Pharmacy.Presentation pattern
    - IoC extension methods AddOpticalApplication/Infrastructure/Presentation
    - MapOpticalApiEndpoints/MapWarrantyApiEndpoints/MapStocktakingApiEndpoints in Program.cs

key-files:
  created:
    - backend/src/Modules/Optical/Optical.Application/IoC.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/IoC.cs
    - backend/src/Modules/Optical/Optical.Presentation/Optical.Presentation.csproj
    - backend/src/Modules/Optical/Optical.Presentation/IoC.cs
    - backend/src/Modules/Optical/Optical.Presentation/OpticalApiEndpoints.cs
    - backend/src/Modules/Optical/Optical.Presentation/WarrantyApiEndpoints.cs
    - backend/src/Modules/Optical/Optical.Presentation/StocktakingApiEndpoints.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Migrations/20260308025509_AddOpticalEntities.cs
  modified:
    - backend/src/Bootstrapper/Program.cs
    - backend/src/Bootstrapper/Bootstrapper.csproj
    - backend/src/Modules/Optical/Optical.Application/Optical.Application.csproj
    - backend/src/Modules/Optical/Optical.Infrastructure/Optical.Infrastructure.csproj
    - backend/Directory.Packages.props

key-decisions:
  - "OPT-05: Contact lenses prescribed via Clinical module (no Optical module code needed); documented in Program.cs comment"
  - "Optical.Presentation project follows Pharmacy.Presentation pattern with separate endpoint files per feature area"
  - "ZXing.Net added via central package management (Directory.Packages.props) for future barcode label PDF generation"
  - "Cross-module references use Contracts-only dependencies (Billing.Contracts, Clinical.Contracts) not Infrastructure references"

patterns-established:
  - "AddOptical*/MapOptical* follows existing module IoC pattern"
  - "Optical schema tables all in optical.* schema with ValueGeneratedNever for client-side Guid IDs"

requirements-completed: [OPT-01, OPT-02, OPT-03, OPT-05]

# Metrics
duration: 24min
completed: 2026-03-08
---

# Phase 08 Plan 24: Bootstrapper Wiring and EF Core Migration Summary

**Optical module wired in Bootstrapper with IoC + endpoints and AddOpticalEntities migration creating 10 optical schema tables**

## Performance

- **Duration:** 24 min
- **Started:** 2026-03-08T02:49:23Z
- **Completed:** 2026-03-08T03:12:58Z
- **Tasks:** 2
- **Files modified:** 11

## Accomplishments
- Created Optical.Presentation project with IoC, OpticalApiEndpoints, WarrantyApiEndpoints, StocktakingApiEndpoints
- Wired AddOpticalApplication/Infrastructure/Presentation and MapOptical*/MapWarranty*/MapStocktaking* in Program.cs
- Added Billing.Contracts and Clinical.Contracts cross-module references to Optical.Application
- Added ZXing.Net 0.16.11 to Optical.Infrastructure via central package management
- Created and applied EF Core migration AddOpticalEntities: Frames, LensCatalogItems, LensStockEntries, LensOrders, GlassesOrders, GlassesOrderItems, ComboPackages, WarrantyClaims, StocktakingSessions, StocktakingItems in optical schema

## Task Commits

Each task was committed atomically:

1. **Task 1: Update Bootstrapper Program.cs with Optical module wiring** - `8f299a6` (feat)
2. **Task 2: Update .csproj references and create migration** - Pre-committed in `39d4513` by prior agent sessions; verified applied

**Plan metadata:** Created with SUMMARY.md and state updates

## Files Created/Modified
- `backend/src/Modules/Optical/Optical.Application/IoC.cs` - AddOpticalApplication extension with FluentValidation
- `backend/src/Modules/Optical/Optical.Infrastructure/IoC.cs` - AddOpticalInfrastructure with all repository registrations
- `backend/src/Modules/Optical/Optical.Presentation/Optical.Presentation.csproj` - New project following Pharmacy pattern
- `backend/src/Modules/Optical/Optical.Presentation/IoC.cs` - AddOpticalPresentation extension
- `backend/src/Modules/Optical/Optical.Presentation/OpticalApiEndpoints.cs` - Full frame/lens/order/combo/prescription API endpoints
- `backend/src/Modules/Optical/Optical.Presentation/WarrantyApiEndpoints.cs` - Warranty claim CRUD and document upload endpoints
- `backend/src/Modules/Optical/Optical.Presentation/StocktakingApiEndpoints.cs` - Stocktaking session endpoints
- `backend/src/Bootstrapper/Program.cs` - Optical IoC + endpoint registrations + OPT-05 comment
- `backend/src/Bootstrapper/Bootstrapper.csproj` - Optical.Presentation project reference
- `backend/src/Modules/Optical/Optical.Application/Optical.Application.csproj` - Billing.Contracts + Clinical.Contracts references
- `backend/src/Modules/Optical/Optical.Infrastructure/Optical.Infrastructure.csproj` - ZXing.Net PackageReference
- `backend/src/Modules/Optical/Optical.Infrastructure/Migrations/20260308025509_AddOpticalEntities.cs` - EF Core migration

## Decisions Made
- OPT-05 (contact lenses via HIS): No Optical module code needed -- comment added in Program.cs as documentation
- Used Pharmacy.Presentation as exact structural template for Optical.Presentation
- ValueComparer added to WarrantyClaimConfiguration.DocumentUrls (IReadOnlyList<string>) to fix EF Core change tracking warning

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Created missing IoC extension methods and Optical.Presentation project**
- **Found during:** Task 1 (Update Bootstrapper Program.cs)
- **Issue:** Plan referenced AddOpticalApplication(), AddOpticalInfrastructure(), AddOpticalPresentation() and MapOptical*/MapWarranty*/MapStocktaking* methods that did not exist
- **Fix:** Created IoC.cs in Application and Infrastructure, created Optical.Presentation project with IoC.cs and all endpoint files
- **Files modified:** See Files Created/Modified above
- **Verification:** dotnet build Bootstrapper succeeded with 0 errors
- **Committed in:** 8f299a6 (Task 1 commit)

**2. [Rule 2 - Missing Critical] Added ValueComparer to WarrantyClaimConfiguration**
- **Found during:** Task 2 (Create EF Core migration)
- **Issue:** EF Core warned that List<string>/IReadOnlyList<string> DocumentUrls had value converter but no value comparer
- **Fix:** Added ValueComparer<IReadOnlyList<string>> with SequenceEqual comparison and ToList() snapshot
- **Files modified:** backend/src/Modules/Optical/Optical.Infrastructure/Configurations/WarrantyClaimConfiguration.cs
- **Verification:** Build succeeded with 0 warnings, migration created without warnings
- **Committed in:** Pre-committed in 39d4513 by prior agent session

---

**Total deviations:** 2 auto-fixed (1 blocking, 1 missing critical)
**Impact on plan:** Both auto-fixes essential for compilation and EF Core correctness. No scope creep.

## Issues Encountered
- Many Task 2 changes (Billing.Contracts reference, ZXing.Net, migration, WarrantyClaimConfiguration fix, repository IoC registrations) were discovered to have been pre-committed in `39d4513` by a prior concurrent agent session. Verified all changes are present in HEAD and migration was applied to the database.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Optical module fully wired in Bootstrapper
- All 10 optical database tables created (Frames, LensCatalogItems, LensStockEntries, LensOrders, GlassesOrders, GlassesOrderItems, ComboPackages, WarrantyClaims, StocktakingSessions, StocktakingItems)
- Cross-module dependencies established (Billing.Contracts for OPT-04, Clinical.Contracts for OPT-08)
- Ready for Application layer handler implementation (frames, lenses, orders in subsequent plans)

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*
