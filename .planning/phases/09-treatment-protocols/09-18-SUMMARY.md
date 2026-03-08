---
phase: 09-treatment-protocols
plan: 18
subsystem: pharmacy
tags: [wolverine, domain-events, consumables, fefo, cross-module, tdd]

# Dependency graph
requires:
  - phase: 09-treatment-protocols (plan 13)
    provides: "TreatmentSessionCompletedEvent with ConsumableUsage list"
provides:
  - "DeductTreatmentConsumablesHandler: Wolverine handler for auto-deducting consumable stock on session completion"
  - "FEFO batch deduction for ExpiryTracked consumables"
  - "Graceful handling of missing items and insufficient stock"
affects: [pharmacy-inventory, treatment-sessions]

# Tech tracking
tech-stack:
  added: []
  patterns: [cross-module-event-handler, fefo-consumable-deduction, graceful-degradation]

key-files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/DeductTreatmentConsumables.cs
    - backend/tests/Pharmacy.Unit.Tests/Features/DeductTreatmentConsumablesTests.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Pharmacy.Application.csproj
    - backend/tests/Pharmacy.Unit.Tests/Pharmacy.Unit.Tests.csproj

key-decisions:
  - "Referenced Treatment.Domain instead of Treatment.Contracts since TreatmentSessionCompletedEvent lives in Domain"
  - "Graceful degradation: deduct available stock when insufficient rather than failing entire deduction"
  - "FEFO deduction reuses GetBatchesAsync which returns batches pre-sorted by ExpiryDate ASC"

patterns-established:
  - "Cross-module event handler: static Wolverine handler in Application layer responding to Domain event from another module"
  - "Partial deduction: when stock is insufficient, deduct what is available instead of all-or-nothing"

requirements-completed: [TRT-11]

# Metrics
duration: 3min
completed: 2026-03-08
---

# Phase 09 Plan 18: Cross-Module Consumable Deduction Summary

**Wolverine event handler auto-deducting consumable stock on treatment session completion with FEFO batch support and graceful degradation**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-08T07:21:30Z
- **Completed:** 2026-03-08T07:24:12Z
- **Tasks:** 2 (TDD RED + GREEN)
- **Files modified:** 4

## Accomplishments
- DeductTreatmentConsumablesHandler responds to TreatmentSessionCompletedEvent via Wolverine
- SimpleStock items deducted directly; ExpiryTracked items use FEFO batch deduction
- Graceful handling: missing items skipped, insufficient stock deducts available amount
- 7 new tests covering all behavior scenarios, 91 total Pharmacy tests pass

## Task Commits

Each task was committed atomically:

1. **Task 1: Write failing tests (RED)** - `f50a311` (test)
2. **Task 2: Implement handler + csproj updates (GREEN)** - `a8660cf` (feat)

## Files Created/Modified
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/DeductTreatmentConsumables.cs` - Wolverine handler deducting consumable stock on TreatmentSessionCompletedEvent
- `backend/src/Modules/Pharmacy/Pharmacy.Application/Pharmacy.Application.csproj` - Added Treatment.Domain project reference
- `backend/tests/Pharmacy.Unit.Tests/Features/DeductTreatmentConsumablesTests.cs` - 7 unit tests for handler behavior
- `backend/tests/Pharmacy.Unit.Tests/Pharmacy.Unit.Tests.csproj` - Added Treatment.Domain project reference for test compilation

## Decisions Made
- **Treatment.Domain reference instead of Treatment.Contracts**: The TreatmentSessionCompletedEvent is defined in Treatment.Domain (not Contracts). The plan suggested Treatment.Contracts but the event is not there. Referenced Treatment.Domain directly for pragmatic cross-module access.
- **Graceful degradation over all-or-nothing**: When stock is insufficient, handler deducts whatever is available rather than failing the entire deduction. This prevents treatment session recording from being blocked by inventory issues.
- **Reused repository FEFO ordering**: GetBatchesAsync already returns batches ordered by ExpiryDate ASC, so no additional FEFO allocator was needed for consumables.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Referenced Treatment.Domain instead of Treatment.Contracts**
- **Found during:** Task 1 (test writing)
- **Issue:** Plan specified Treatment.Contracts reference, but TreatmentSessionCompletedEvent lives in Treatment.Domain
- **Fix:** Referenced Treatment.Domain directly in both Pharmacy.Application.csproj and Pharmacy.Unit.Tests.csproj
- **Files modified:** Pharmacy.Application.csproj, Pharmacy.Unit.Tests.csproj
- **Verification:** Build succeeds, all tests pass
- **Committed in:** f50a311 (test csproj), a8660cf (application csproj)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Necessary for compilation since the event type is in Treatment.Domain. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Consumable deduction handler is ready for integration with session recording workflow
- When RecordTreatmentSession publishes TreatmentSessionCompletedEvent, Wolverine auto-routes to this handler
- Pharmacy inventory will be automatically updated on session completion

## Self-Check: PASSED

All files verified present, all commit hashes verified in git log.

---
*Phase: 09-treatment-protocols*
*Completed: 2026-03-08*
