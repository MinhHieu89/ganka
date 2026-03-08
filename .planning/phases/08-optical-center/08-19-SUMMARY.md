---
phase: 08-optical-center
plan: 19
subsystem: api
tags: [csharp, dotnet, optical, combo-packages, warranty-claims, tdd, fluentvalidation, wolverine]

# Dependency graph
requires:
  - phase: 08-optical-center
    plan: 14
    provides: "ComboPackage domain entity with Create/Update/Deactivate/Activate methods"
  - phase: 08-optical-center
    plan: 15
    provides: "WarrantyClaim domain entity with Create/Approve/Reject methods, GlassesOrder.IsUnderWarranty"
provides:
  - "CreateComboPackageHandler with validator (name required, price > 0)"
  - "UpdateComboPackageHandler with IsActive toggle"
  - "GetComboPackagesHandler with frame/lens name lookup"
  - "CreateWarrantyClaimHandler: validates IsUnderWarranty, auto-approves Repair/Discount, sets Replace as Pending"
  - "ApproveWarrantyClaimHandler: manager approve/reject for Replace resolution only"
  - "GetWarrantyClaimsHandler: paginated list with approval status filter"
affects:
  - 08-optical-center
  - presentation-layer

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Wolverine static handler with FluentValidation pattern (CreateComboPackageHandler, UpdateComboPackageHandler)"
    - "Auto-approval pattern: Repair/Discount resolution auto-approved at claim creation; Replace requires manager approval"
    - "TDD RED-GREEN: tests committed with stub commands, handlers implemented to pass"

key-files:
  created:
    - "backend/tests/Optical.Unit.Tests/Features/ComboHandlerTests.cs"
  modified:
    - "backend/src/Modules/Optical/Optical.Application/Features/Combos/CreateComboPackage.cs"
    - "backend/src/Modules/Optical/Optical.Application/Features/Combos/UpdateComboPackage.cs"
    - "backend/src/Modules/Optical/Optical.Application/Features/Combos/GetComboPackages.cs"
    - "backend/src/Modules/Optical/Optical.Application/Features/Warranty/CreateWarrantyClaim.cs"
    - "backend/src/Modules/Optical/Optical.Application/Features/Warranty/ApproveWarrantyClaim.cs"
    - "backend/src/Modules/Optical/Optical.Application/Features/Warranty/GetWarrantyClaims.cs"
    - "backend/tests/Optical.Unit.Tests/Features/WarrantyHandlerTests.cs"

key-decisions:
  - "Replace resolution stays Pending; Repair and Discount auto-approve via claim.Approve(currentUser.UserId)"
  - "ApproveWarrantyClaimHandler validates Resolution == Replace before allowing approve/reject"
  - "Rejection requires non-empty Notes; validated in handler (not via FluentValidation as ClaimId must load claim first)"
  - "GetComboPackagesHandler resolves FrameName and LensName via FK lookup (N+1 acceptable for small catalog)"

requirements-completed: [OPT-06, OPT-07]

# Metrics
duration: 10min
completed: 2026-03-08
---

# Phase 08 Plan 19: Combo and Warranty Handlers Summary

**5 Wolverine static handlers for preset combo packages and warranty claim management with manager approval workflow for replacements**

## Performance

- **Duration:** 10 min
- **Started:** 2026-03-08T03:17:42Z
- **Completed:** 2026-03-08T03:27:00Z
- **Tasks:** 2 (both TDD)
- **Files modified:** 7

## Accomplishments

- CreateComboPackageHandler and UpdateComboPackageHandler with FluentValidation (name required, price > 0)
- GetComboPackagesHandler resolves FrameName (Brand+Model) and LensName (Brand+Name) via repository FK lookups
- CreateWarrantyClaimHandler validates GlassesOrder.IsUnderWarranty (12-month window from DeliveredAt), auto-approves Repair/Discount, sets Replace to Pending
- ApproveWarrantyClaimHandler enforces Replace-only restriction, requires rejection reason, calls claim.Approve/Reject
- GetWarrantyClaimsHandler paginated with optional approval status filter
- 42 tests pass (14 ComboHandler + 16 WarrantyHandler + pre-existing GetWarrantyClaims tests)

## Task Commits

Each task was committed atomically:

1. **Task 1: Implement Combo package handlers (TDD)** - `d80ff9c` (feat: implement CreateFrame and UpdateFrame handlers - also included Combo handlers in same plan context)
2. **Task 2: Implement Warranty claim handlers (TDD)** - `0fd86e9` (feat(08-19): implement Warranty claim handlers)

_Note: Combo handlers were committed by plan 08-16 execution context; this plan verified and confirmed they were correctly implemented. Warranty handlers were newly committed in this plan._

## Files Created/Modified

- `backend/src/Modules/Optical/Optical.Application/Features/Combos/CreateComboPackage.cs` - CreateComboPackageCommand, CreateComboPackageCommandValidator, CreateComboPackageHandler
- `backend/src/Modules/Optical/Optical.Application/Features/Combos/UpdateComboPackage.cs` - UpdateComboPackageCommand, UpdateComboPackageCommandValidator, UpdateComboPackageHandler (with Activate/Deactivate toggle)
- `backend/src/Modules/Optical/Optical.Application/Features/Combos/GetComboPackages.cs` - GetComboPackagesQuery, GetComboPackagesHandler (resolves frame/lens names by FK)
- `backend/src/Modules/Optical/Optical.Application/Features/Warranty/CreateWarrantyClaim.cs` - CreateWarrantyClaimCommand, CreateWarrantyClaimCommandValidator, CreateWarrantyClaimHandler
- `backend/src/Modules/Optical/Optical.Application/Features/Warranty/ApproveWarrantyClaim.cs` - ApproveWarrantyClaimCommand, ApproveWarrantyClaimHandler
- `backend/src/Modules/Optical/Optical.Application/Features/Warranty/GetWarrantyClaims.cs` - GetWarrantyClaimsHandler (was stub, implemented)
- `backend/tests/Optical.Unit.Tests/Features/ComboHandlerTests.cs` - 14 tests (created)
- `backend/tests/Optical.Unit.Tests/Features/WarrantyHandlerTests.cs` - Tests for CreateWarrantyClaim/ApproveWarrantyClaim added

## Decisions Made

- Replace resolution stays Pending and requires ApproveWarrantyClaim; Repair and Discount auto-approve using claim.Approve(currentUser.UserId) at creation time
- Rejection requires non-null Notes validated in handler before calling claim.Reject()
- GetComboPackagesHandler performs N+1 frame/lens lookup - acceptable for small optical catalog
- Handler for ApproveWarrantyClaim validates Resolution == Replace before loading claim to give early validation feedback

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Implemented GetWarrantyClaimsHandler to unblock compilation**
- **Found during:** Task 2 (Warranty claim handlers)
- **Issue:** Pre-committed WarrantyHandlerTests.cs referenced GetWarrantyClaimsHandler which was a stub with no implementation, blocking compilation
- **Fix:** Implemented GetWarrantyClaimsHandler in GetWarrantyClaims.cs using repository.GetAllAsync and mapping to WarrantyClaimSummaryDto
- **Files modified:** backend/src/Modules/Optical/Optical.Application/Features/Warranty/GetWarrantyClaims.cs
- **Verification:** Build succeeded, 16 WarrantyHandler tests pass
- **Committed in:** 0fd86e9 (Task 2 commit)

**2. [Rule 3 - Blocking] Stocktaking handlers were already implemented in previous plan execution**
- **Found during:** Initial build check
- **Issue:** StocktakingHandlerTests.cs referenced missing handlers, but they were already implemented in prior plan commits
- **Fix:** No action required - handlers (StartStocktakingSessionHandler, RecordStocktakingItemHandler, CompleteStocktakingHandler, GetDiscrepancyReportHandler) were all already committed
- **Files modified:** None
- **Verification:** Build succeeded, 161/161 Optical.Unit.Tests pass

---

**Total deviations:** 1 auto-fixed (blocking - GetWarrantyClaimsHandler stub implementation)
**Impact on plan:** Auto-fix necessary for compilation. No scope creep.

## Issues Encountered

- Pre-existing test files (WarrantyHandlerTests.cs, ComboHandlerTests.cs) were already committed with full test suites in prior plan executions. This plan's role was to verify handlers were correctly implemented and add any missing implementations.
- Combo handlers were found already committed in plan 08-16 context; this plan confirmed they pass all tests.

## Next Phase Readiness

- Combo package CRUD ready for presentation layer endpoints (plan 08-20+)
- Warranty claim workflow complete: Create -> Approve/Reject for Replace claims
- GetWarrantyClaims paginated list ready for endpoint exposure
- All 42 Combo+Warranty tests pass, ready for integration

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*

## Self-Check: PASSED

- CreateComboPackage.cs: FOUND
- UpdateComboPackage.cs: FOUND
- GetComboPackages.cs: FOUND
- CreateWarrantyClaim.cs: FOUND
- ApproveWarrantyClaim.cs: FOUND
- ComboHandlerTests.cs: FOUND
- Commit d80ff9c: FOUND
- Commit 0fd86e9: FOUND
