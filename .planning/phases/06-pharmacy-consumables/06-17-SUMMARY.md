---
phase: 06-pharmacy-consumables
plan: 17
subsystem: api
tags: [minimal-api, wolverine, csharp, pharmacy, cross-module, endpoints]

# Dependency graph
requires:
  - phase: 06-09
    provides: IDispensingRepository interface with GetPendingPrescriptionsAsync
  - phase: 06-11
    provides: CreateSupplierCommand, GetSuppliersQuery, UpdateSupplierCommand handlers
  - phase: 06-12
    provides: CreateStockImportCommand, GetStockImportsQuery, ImportStockFromExcelCommand handlers
  - phase: 06-13
    provides: GetPendingPrescriptionsQuery in Clinical.Contracts
  - phase: 06-14
    provides: GetDrugBatchesQuery, AdjustStockCommand handlers
  - phase: 06-15
    provides: GetExpiryAlertsQuery, GetLowStockAlertsQuery handlers
  - phase: 06-16
    provides: GetDrugInventoryQuery handler
provides:
  - Pharmacy.Presentation project with ~15 API endpoints at /api/pharmacy
  - MapPharmacyApiEndpoints covering suppliers, inventory, stock import, alerts
  - UpdateDrugCatalogPricingCommand handler for pricing management
  - Cross-module GetPendingPrescriptions handler in Clinical.Application
  - ClinicalPendingPrescriptionDto in Clinical.Contracts for IMessageBus response
  - DispensingRepository.GetPendingPrescriptionsAsync via IMessageBus cross-module call
affects:
  - 06-18 (frontend pharmacy pages will consume these endpoints)
  - future dispensing workflow (pending prescriptions queue)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "MapXxxApiEndpoints private method groups for endpoint organization"
    - "IMessageBus cross-module query: Clinical returns DTO, Pharmacy maps to Pharmacy.Contracts"
    - "AsParameters binding class for query string params on GET endpoints"
    - "DisableAntiforgery() on multipart file upload endpoints"

key-files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/DrugCatalog/UpdateDrugCatalogPricing.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetPendingPrescriptions.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Presentation/PharmacyApiEndpoints.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/PendingPrescriptionQuery.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DispensingRepository.cs

key-decisions:
  - "Cross-module pending prescriptions: Clinical.Contracts defines ClinicalPendingPrescriptionDto, Clinical.Application handler returns it via IMessageBus, Pharmacy.Infrastructure maps to Pharmacy.Contracts.PendingPrescriptionDto"
  - "Pharmacy.Infrastructure references Clinical.Contracts for cross-module IMessageBus return type -- allowed by architecture test rules"
  - "DispensingRepository constructor injection of IMessageBus for cross-module query (added alongside existing PharmacyDbContext injection)"
  - "UpdateDrugCatalogPricingCommand separate from UpdateDrugCatalogItemCommand: pricing managed by pharmacist/manager role independently"
  - "IVisitRepository.GetPrescriptionsWithVisitsAsync returns domain tuples for flexibility -- handler does expiry calculation"
  - "Prescription validity window: 7 days from PrescribedAt for dispensing purposes"

patterns-established:
  - "Private MapXxxEndpoints(group) method groups for logical endpoint organization within a module"
  - "Cross-module IMessageBus pattern: query in Contracts, handler in Application, mapped in Infrastructure"

requirements-completed: [PHR-01, PHR-02, PHR-03, PHR-04]

# Metrics
duration: 12min
completed: 2026-03-06
---

# Phase 06 Plan 17: Pharmacy Presentation Endpoints Summary

**Minimal API HTTP surface for pharmacy module: 15 endpoints covering suppliers, inventory, stock import, alerts, and drug pricing via IMessageBus cross-module pattern**

## Performance

- **Duration:** 12 min
- **Started:** 2026-03-06T08:21:50Z
- **Completed:** 2026-03-06T08:33:50Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- Extended PharmacyApiEndpoints from 4 drug catalog routes to 15 total routes organized in 5 groups
- Created UpdateDrugCatalogPricingCommand handler for independent pricing management by pharmacist role
- Fixed pre-existing build error (DispensingRepository missing interface implementation) by implementing cross-module IMessageBus pattern

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Pharmacy.Presentation endpoints + fix cross-module dispensing** - `79556b4` (feat)
2. **Task 2: Add UpdateDrugCatalogPricing handler** - `c19b3b1` (feat)

**Plan metadata:** (docs commit to follow)

## Files Created/Modified
- `backend/src/Modules/Pharmacy/Pharmacy.Presentation/PharmacyApiEndpoints.cs` - Extended with 5 private method groups: suppliers, inventory, stock import, alerts, drug catalog
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/DrugCatalog/UpdateDrugCatalogPricing.cs` - New handler for PUT /inventory/{drugId}/pricing
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/PendingPrescriptionQuery.cs` - Added ClinicalPendingPrescriptionDto and ClinicalPendingPrescriptionItemDto return types
- `backend/src/Modules/Clinical/Clinical.Application/Features/GetPendingPrescriptions.cs` - New cross-module handler with 7-day expiry logic
- `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs` - Added GetPrescriptionsWithVisitsAsync method
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs` - Implemented GetPrescriptionsWithVisitsAsync with EF Core Join query
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj` - Added Clinical.Contracts project reference
- `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DispensingRepository.cs` - Implemented GetPendingPrescriptionsAsync via IMessageBus + dispensed-filter

## Decisions Made
- Cross-module pending prescriptions resolved via IMessageBus: Clinical handler returns `ClinicalPendingPrescriptionDto`, Pharmacy.Infrastructure maps to `Pharmacy.Contracts.PendingPrescriptionDto` after filtering already-dispensed IDs
- `UpdateDrugCatalogPricingCommand` kept separate from `UpdateDrugCatalogItemCommand` so pricing can be updated by pharmacist/manager role without catalog admin access
- Prescription validity window of 7 days (hardcoded) -- configurable in future via ClinicSettings

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed missing DispensingRepository.GetPendingPrescriptionsAsync implementation**
- **Found during:** Task 1 (solution-wide build verification)
- **Issue:** `DispensingRepository` was missing interface method `GetPendingPrescriptionsAsync` causing `CS0535` build error across entire solution
- **Fix:** Added `ClinicalPendingPrescriptionDto` to `Clinical.Contracts`, created `GetPendingPrescriptions` handler in `Clinical.Application`, added `GetPrescriptionsWithVisitsAsync` to `IVisitRepository` + `VisitRepository`, added `Clinical.Contracts` reference to `Pharmacy.Infrastructure`, implemented `GetPendingPrescriptionsAsync` in `DispensingRepository` via IMessageBus
- **Files modified:** 6 files (see above)
- **Verification:** Full solution builds with 0 errors; all 55 architecture tests pass
- **Committed in:** 79556b4 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (Rule 3 - blocking build error)
**Impact on plan:** Fix required for any downstream code to build. Cross-module dispensing queue now properly implemented. No scope creep.

## Issues Encountered
- `DispensingRepository` was created in Phase 06-09 with interface contract but implementation never completed -- the `GetPendingPrescriptionsAsync` method was listed in the interface but not in the class, causing a CS0535 build error across the full solution. Fixed by implementing the full cross-module IMessageBus pattern as designed.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All pharmacy API endpoints are now exposed at `/api/pharmacy` with RequireAuthorization
- Frontend pharmacy pages (plan 06-18 and beyond) can now call these endpoints
- Pending prescriptions dispensing queue works end-to-end via Clinical -> Pharmacy cross-module query
- 55/55 architecture tests pass

## Self-Check: PASSED

- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Presentation/PharmacyApiEndpoints.cs
- FOUND: backend/src/Modules/Pharmacy/Pharmacy.Application/Features/DrugCatalog/UpdateDrugCatalogPricing.cs
- FOUND: backend/src/Modules/Clinical/Clinical.Application/Features/GetPendingPrescriptions.cs
- FOUND: .planning/phases/06-pharmacy-consumables/06-17-SUMMARY.md
- FOUND commit 79556b4: feat(06-17): create Pharmacy.Presentation endpoints and fix cross-module dispensing
- FOUND commit c19b3b1: feat(06-17): add UpdateDrugCatalogPricing handler for independent pricing management

---
*Phase: 06-pharmacy-consumables*
*Completed: 2026-03-06*
