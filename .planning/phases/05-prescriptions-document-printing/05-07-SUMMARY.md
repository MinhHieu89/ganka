---
phase: 05-prescriptions-document-printing
plan: 07
subsystem: api
tags: [handler, optical-rx, prescription, tdd, wolverine, csharp]

# Dependency graph
requires:
  - phase: 05-04
    provides: OpticalPrescription domain entity with Create/Update methods and Visit.SetOpticalPrescription
  - phase: 05-05b
    provides: ClinicalDbContext OpticalPrescriptions DbSet, VisitRepository with Include, OpticalPrescriptionDto
provides:
  - AddOpticalPrescriptionHandler with FluentValidation and Result<Guid> return
  - UpdateOpticalPrescriptionHandler with field-level update and NotFound handling
  - AddOpticalPrescriptionCommand and UpdateOpticalPrescriptionCommand records in Contracts
  - AddOpticalPrescription/RemoveOpticalPrescriptions methods on IVisitRepository
  - 6 unit tests covering add, replace, signed visit guard, lens type, update, and not found
affects: [05-08, 05-09, 05-10]

# Tech tracking
tech-stack:
  added: []
  patterns: [set-and-replace-optical-rx, remove-then-add-ef-tracking]

key-files:
  created:
    - backend/src/Modules/Clinical/Clinical.Application/Features/AddOpticalPrescription.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/UpdateOpticalPrescription.cs
    - backend/tests/Clinical.Unit.Tests/Features/OpticalPrescriptionHandlerTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/CreateVisitCommand.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs

key-decisions:
  - "RemoveOpticalPrescriptions called before SetOpticalPrescription to sync EF Core change tracker with domain backing field clear"
  - "AddOpticalPrescription returns Result<Guid> (prescription ID) for frontend entity reference"

patterns-established:
  - "remove-then-add-ef-tracking: When domain method clears backing field, explicitly remove from DbContext before adding new entity"

requirements-completed: [RX-03]

# Metrics
duration: 3min
completed: 2026-03-05
---

# Phase 05 Plan 07: Optical Prescription Handlers Summary

**Add and Update optical prescription handlers with TDD, SetOpticalPrescription one-per-visit enforcement, and EF Core change tracker sync**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-05T16:44:25Z
- **Completed:** 2026-03-05T16:48:03Z
- **Tasks:** 1 (TDD: RED + GREEN)
- **Files modified:** 6

## Accomplishments
- AddOpticalPrescriptionHandler creates optical Rx via domain factory, replaces existing via SetOpticalPrescription, returns prescription ID
- UpdateOpticalPrescriptionHandler finds prescription by ID within visit collection, calls Update() with all fields
- TDD cycle completed: 6 failing tests written first, then handlers implemented to pass all tests
- EF Core change tracker correctly synced with domain backing field Clear() by calling RemoveOpticalPrescriptions before SetOpticalPrescription

## Task Commits

Each task was committed atomically (TDD red-green):

1. **Task 1 RED: Failing tests for optical prescription handlers** - `657f21b` (test)
2. **Task 1 GREEN: Implement optical prescription handlers** - `47ed3d4` (feat)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Application/Features/AddOpticalPrescription.cs` - Handler + validator for creating/replacing optical Rx
- `backend/src/Modules/Clinical/Clinical.Application/Features/UpdateOpticalPrescription.cs` - Handler + validator for updating existing optical Rx
- `backend/tests/Clinical.Unit.Tests/Features/OpticalPrescriptionHandlerTests.cs` - 6 unit tests covering all scenarios
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/CreateVisitCommand.cs` - Added AddOpticalPrescriptionCommand and UpdateOpticalPrescriptionCommand records
- `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs` - Added AddOpticalPrescription/RemoveOpticalPrescriptions methods
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs` - Implemented AddOpticalPrescription/RemoveOpticalPrescriptions

## Decisions Made
- RemoveOpticalPrescriptions called before SetOpticalPrescription to keep EF Core change tracker in sync with domain backing field Clear() operation
- AddOpticalPrescription returns Result<Guid> (prescription ID) so frontend can reference the created entity immediately
- LensType validated with Enum.IsDefined at command boundary (consistent with existing pattern)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added AddOpticalPrescription/RemoveOpticalPrescriptions to IVisitRepository**
- **Found during:** Task 1 (test setup)
- **Issue:** IVisitRepository lacked methods to explicitly track OpticalPrescription entities in EF Core (same pattern as AddRefraction, AddDiagnosis)
- **Fix:** Added AddOpticalPrescription and RemoveOpticalPrescriptions to interface and VisitRepository implementation
- **Files modified:** IVisitRepository.cs, VisitRepository.cs
- **Verification:** Build succeeds, all tests pass
- **Committed in:** 657f21b (RED phase commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Essential for EF Core change tracking. No scope creep.

## Issues Encountered
None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Optical prescription handlers ready for Minimal API endpoint wiring (Plan 05-08)
- Both Add and Update support all OD/OS distance + near Rx, far/near PD, and lens type fields
- One-per-visit enforcement works correctly via domain SetOpticalPrescription + EF tracker sync

## Self-Check: PASSED

- All 6 source files: FOUND
- Commit 657f21b (RED): FOUND
- Commit 47ed3d4 (GREEN): FOUND
- All 6 tests: PASSED
- Build: 0 errors, 0 warnings

---
*Phase: 05-prescriptions-document-printing*
*Completed: 2026-03-05*
