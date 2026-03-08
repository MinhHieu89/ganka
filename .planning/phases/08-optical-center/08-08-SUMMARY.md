---
phase: 08-optical-center
plan: 08
subsystem: api
tags: [dotnet, contracts, dto, cross-module, optical, warranty, stocktaking]

# Dependency graph
requires:
  - phase: 08-optical-center
    provides: Optical.Contracts project foundation with enums and frame/lens DTOs (08-01 through 08-07)
  - phase: 07-billing
    provides: VisitChargeDto and GetVisitChargesQuery cross-module query pattern (Billing.Contracts)
provides:
  - WarrantyClaimDto with approval status, document URLs, and RequiresApproval flag
  - StocktakingSessionDto, StocktakingItemDto, DiscrepancyReportDto for stocktaking reports
  - GetPatientOpticalPrescriptionsQuery and OpticalPrescriptionHistoryDto for Clinical integration
  - GetOpticalSuppliersQuery for Pharmacy integration
affects:
  - 08-optical-center (Application handlers for warranty and stocktaking)
  - 09-clinical (must implement handler for GetPatientOpticalPrescriptionsQuery)
  - 10-pharmacy (must implement handler for GetOpticalSuppliersQuery)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Cross-module queries in Contracts layer enable inter-module communication without direct project references"
    - "int-serialized enums in DTOs for API serialization following Billing.Contracts pattern"
    - "Sealed records for all DTOs following immutable contract pattern"

key-files:
  created:
    - backend/src/Modules/Optical/Optical.Contracts/Dtos/WarrantyClaimDto.cs
    - backend/src/Modules/Optical/Optical.Contracts/Dtos/StocktakingReportDto.cs
    - backend/src/Modules/Optical/Optical.Contracts/Queries/GetOpticalChargesQuery.cs
  modified: []

key-decisions:
  - "GetOpticalChargesQuery.cs contains queries Optical sends TO other modules (Clinical, Pharmacy), not a query Optical responds to - follows cross-module design where Optical.Application handles Billing.Contracts.GetVisitChargesQuery"
  - "WarrantyClaimSummaryDto added as lightweight list view variant following InvoiceSummaryDto pattern from Billing"
  - "OpticalPrescriptionHistoryDto includes full binocular fields (OD/OS) with nullable decimals for partial prescriptions"

patterns-established:
  - "Cross-module query pattern: Contracts layer defines what module needs from others, Application layer defines what others can query from this module"
  - "DiscrepancyReportDto aggregates StocktakingItemDto list with summary counts (over/under/missing) for efficient report rendering"

requirements-completed: [OPT-07, OPT-08, OPT-09]

# Metrics
duration: 8min
completed: 2026-03-08
---

# Phase 08 Plan 08: Remaining Contracts DTOs Summary

**Warranty claim DTO with approval workflow fields, stocktaking session/item/discrepancy DTOs, and cross-module queries for Clinical prescription history and Pharmacy supplier data in Optical.Contracts**

## Performance

- **Duration:** 8 min
- **Started:** 2026-03-08T02:47:02Z
- **Completed:** 2026-03-08T02:55:00Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- WarrantyClaimDto with RequiresApproval boolean, ApprovalStatus, DocumentUrls list, and full approver tracking
- Three stocktaking DTOs: StocktakingSessionDto (session metadata), StocktakingItemDto (per-item scan data), DiscrepancyReportDto (summary with over/under/missing counts)
- Cross-module queries: GetPatientOpticalPrescriptionsQuery for Clinical integration and GetOpticalSuppliersQuery for Pharmacy integration
- Optical.Contracts builds successfully with 0 warnings

## Task Commits

Each task was committed atomically:

1. **Task 1: Create WarrantyClaim and StocktakingReport DTOs** - `3f907b0` (feat)
2. **Task 2: Create cross-module query records** - `04d6c62` (feat)

**Plan metadata:** (see final commit)

## Files Created/Modified
- `backend/src/Modules/Optical/Optical.Contracts/Dtos/WarrantyClaimDto.cs` - WarrantyClaimDto (full) and WarrantyClaimSummaryDto (list view)
- `backend/src/Modules/Optical/Optical.Contracts/Dtos/StocktakingReportDto.cs` - StocktakingSessionDto, StocktakingItemDto, DiscrepancyReportDto
- `backend/src/Modules/Optical/Optical.Contracts/Queries/GetOpticalChargesQuery.cs` - GetPatientOpticalPrescriptionsQuery, OpticalPrescriptionHistoryDto, GetOpticalSuppliersQuery

## Decisions Made
- GetOpticalChargesQuery.cs file name retained from plan but contains cross-module queries that Optical sends to other modules (not a query others send to Optical). Optical.Application will handle Billing.Contracts.GetVisitChargesQuery separately.
- Added WarrantyClaimSummaryDto as a lightweight variant following the pattern established by InvoiceSummaryDto in Billing.Contracts.
- OpticalPrescriptionHistoryDto fields are nullable to support partial prescriptions (e.g., monocular corrections).

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Optical.Contracts is now complete with all DTOs and cross-module queries
- Application layer (08-09+) can reference these contracts to implement handlers
- Clinical module (phase 09) needs to implement handler for GetPatientOpticalPrescriptionsQuery
- Pharmacy module (phase 10) needs to implement handler for GetOpticalSuppliersQuery

## Self-Check

- [x] `backend/src/Modules/Optical/Optical.Contracts/Dtos/WarrantyClaimDto.cs` - EXISTS
- [x] `backend/src/Modules/Optical/Optical.Contracts/Dtos/StocktakingReportDto.cs` - EXISTS
- [x] `backend/src/Modules/Optical/Optical.Contracts/Queries/GetOpticalChargesQuery.cs` - EXISTS
- [x] Commit `3f907b0` - EXISTS
- [x] Commit `04d6c62` - EXISTS
- [x] Optical.Contracts builds: 0 errors, 0 warnings

## Self-Check: PASSED

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*
