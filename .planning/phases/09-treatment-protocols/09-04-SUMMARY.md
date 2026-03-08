---
phase: 09-treatment-protocols
plan: 04
subsystem: domain
tags: [ddd, aggregate-root, entity, treatment, session, consumable, status-machine]

# Dependency graph
requires:
  - phase: 09-treatment-protocols
    plan: 01
    provides: "Treatment.Domain project scaffolding with enums (TreatmentType, PricingMode, PackageStatus, SessionStatus)"
  - phase: 09-treatment-protocols
    plan: 02
    provides: "ValueObjects (IplParameters, LlltParameters, LidCareParameters)"
provides:
  - "TreatmentPackage aggregate root with session management and status machine"
  - "TreatmentSession child entity with OSDI score and clinical notes"
  - "SessionConsumable child entity for consumable tracking"
  - "CancellationRequestStatus enum for cancellation workflow"
affects: [09-treatment-protocols]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Aggregate root with backing fields and IReadOnlyList exposures"
    - "Status machine with guard methods (EnsureActive/EnsureModifiable)"
    - "Auto-completion via computed IsComplete property triggering status transition"
    - "Refund calculation supporting both PerSession and PerPackage pricing modes"

key-files:
  created:
    - backend/src/Modules/Treatment/Treatment.Domain/Entities/TreatmentPackage.cs
    - backend/src/Modules/Treatment/Treatment.Domain/Entities/TreatmentSession.cs
    - backend/src/Modules/Treatment/Treatment.Domain/Entities/SessionConsumable.cs
    - backend/src/Modules/Treatment/Treatment.Domain/Enums/CancellationRequestStatus.cs
  modified: []

key-decisions:
  - "RecordSession auto-completes session inline (Complete() called within RecordSession) rather than requiring separate completion step"
  - "ProtocolVersion snapshots use JSON serialization for previous/current state comparison"
  - "CancellationRequest is a nullable backing field on TreatmentPackage rather than a separate collection"

patterns-established:
  - "Treatment aggregate uses backing fields pattern from GlassesOrder/Invoice for _sessions, _versions, _cancellationRequest"
  - "Domain events raised from aggregate root (TreatmentSessionCompletedEvent, TreatmentPackageCompletedEvent)"

requirements-completed: [TRT-01, TRT-02, TRT-03, TRT-04, TRT-06, TRT-11]

# Metrics
duration: 4min
completed: 2026-03-08
---

# Phase 09 Plan 04: TreatmentPackage Aggregate Summary

**TreatmentPackage aggregate root with session tracking, auto-completion, status machine, cancellation workflow, and dual-mode refund calculation**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-08T06:46:55Z
- **Completed:** 2026-03-08T06:51:25Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- TreatmentPackage aggregate root with full status machine (Active/Paused/PendingCancellation/Cancelled/Switched/Completed)
- RecordSession method with auto-completion logic that transitions package to Completed when all sessions are done (TRT-04)
- SessionsCompleted/SessionsRemaining computed properties for progress tracking (TRT-02)
- TreatmentSession child entity recording OSDI score, severity, clinical notes, device parameters (TRT-03)
- SessionConsumable child entity for consumable tracking linked to Pharmacy module (TRT-11)
- CalculateRefundAmount supporting both PerSession and PerPackage pricing modes
- Modify method with ProtocolVersion snapshots for mid-course changes (TRT-07 preparation)
- Cancellation workflow: RequestCancellation/ApproveCancellation/RejectCancellation (TRT-09 preparation)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create TreatmentPackage aggregate root** - `b31f948` (feat)
2. **Task 2: Create TreatmentSession and SessionConsumable child entities** - `c0472dc` (feat)

## Files Created/Modified
- `backend/src/Modules/Treatment/Treatment.Domain/Entities/TreatmentPackage.cs` - Aggregate root with session management, status machine, auto-completion, refund calculation
- `backend/src/Modules/Treatment/Treatment.Domain/Entities/TreatmentSession.cs` - Child entity recording session details (OSDI, notes, parameters, consumables)
- `backend/src/Modules/Treatment/Treatment.Domain/Entities/SessionConsumable.cs` - Child entity linking session consumables to Pharmacy.ConsumableItem
- `backend/src/Modules/Treatment/Treatment.Domain/Enums/CancellationRequestStatus.cs` - Enum for cancellation request workflow (Requested/Approved/Rejected)

## Decisions Made
- RecordSession auto-completes the session inline (calls Complete() within RecordSession) rather than requiring a separate completion step, matching the clinical workflow where recording a session and completing it are a single action
- ProtocolVersion uses JSON serialization for previous/current state snapshots, aligning with the linter-modified ProtocolVersion entity that uses PreviousJson/CurrentJson fields
- CancellationRequest is a nullable backing field (`_cancellationRequest`) on TreatmentPackage rather than a list, since only one active cancellation request is allowed per package

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Created CancellationRequestStatus enum**
- **Found during:** Task 1 (TreatmentPackage aggregate root)
- **Issue:** CancellationRequest entity references CancellationRequestStatus enum which did not exist
- **Fix:** Created CancellationRequestStatus.cs with Requested/Approved/Rejected values
- **Files modified:** backend/src/Modules/Treatment/Treatment.Domain/Enums/CancellationRequestStatus.cs
- **Verification:** Build succeeds
- **Committed in:** b31f948 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Enum was a necessary missing piece for the CancellationRequest entity. No scope creep.

## Issues Encountered
- TreatmentPackage could not compile without TreatmentSession (Task 2) since it references TreatmentSession as a backing field type. Both tasks were implemented before running the build verification, then committed individually.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- TreatmentPackage aggregate ready for EF Core configuration in subsequent plans
- Domain events (TreatmentSessionCompletedEvent, TreatmentPackageCompletedEvent) ready for cross-module handlers
- All domain logic for treatment tracking, auto-completion, and cancellation workflow in place

## Self-Check: PASSED

All 4 created files verified on disk. Both task commits (b31f948, c0472dc) verified in git history.

---
*Phase: 09-treatment-protocols*
*Completed: 2026-03-08*
