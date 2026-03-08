---
phase: 09-treatment-protocols
plan: 05
subsystem: domain
tags: [entity, ddd, versioning, cancellation, approval-workflow]

# Dependency graph
requires:
  - phase: 09-01
    provides: "Treatment module scaffolding and domain project structure"
provides:
  - "ProtocolVersion entity for mid-course modification audit trail"
  - "CancellationRequest entity with approval/rejection workflow"
  - "CancellationStatus enum (Requested, Approved, Rejected)"
affects: [09-treatment-protocols, treatment-package-aggregate, treatment-infrastructure]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Child entity with static Create factory method", "Approval workflow pattern (Requested/Approved/Rejected)", "JSON state snapshots for version history"]

key-files:
  created:
    - backend/src/Modules/Treatment/Treatment.Domain/Entities/ProtocolVersion.cs
    - backend/src/Modules/Treatment/Treatment.Domain/Entities/CancellationRequest.cs
    - backend/src/Modules/Treatment/Treatment.Domain/Enums/CancellationStatus.cs
  modified: []

key-decisions:
  - "CancellationStatus placed in separate Enums/ file for consistency with existing Treatment enums"
  - "ProtocolVersion stores full JSON snapshots (PreviousJson + CurrentJson) rather than field-level diffs for simplicity"
  - "CancellationRequest follows Billing Refund entity pattern for approval workflow consistency"

patterns-established:
  - "Approval workflow: Create -> Approve/Reject with status guards and InvalidOperationException"
  - "Version snapshots: before/after JSON plus human-readable ChangeDescription"

requirements-completed: [TRT-07, TRT-09]

# Metrics
duration: 2min
completed: 2026-03-08
---

# Phase 09 Plan 05: Protocol Version & Cancellation Request Summary

**ProtocolVersion entity with JSON state snapshots and CancellationRequest with manager-approval workflow following Refund pattern**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-08T06:46:22Z
- **Completed:** 2026-03-08T06:48:46Z
- **Tasks:** 1
- **Files modified:** 3

## Accomplishments
- ProtocolVersion entity stores before/after JSON snapshots with change description and reason for full audit trail
- CancellationRequest entity with Approve/Reject methods, deduction percentage, and refund amount calculation
- CancellationStatus enum (Requested=0, Approved=1, Rejected=2) consistent with Billing RefundStatus

## Task Commits

Each task was committed atomically:

1. **Task 1: Create ProtocolVersion and CancellationRequest entities** - `c51246d` (feat)

**Plan metadata:** (pending)

## Files Created/Modified
- `backend/src/Modules/Treatment/Treatment.Domain/Entities/ProtocolVersion.cs` - Version snapshot entity with PreviousJson/CurrentJson, ChangeDescription, and sequential versioning
- `backend/src/Modules/Treatment/Treatment.Domain/Entities/CancellationRequest.cs` - Cancellation request entity with Create/Approve/Reject workflow and deduction percentage
- `backend/src/Modules/Treatment/Treatment.Domain/Enums/CancellationStatus.cs` - Enum: Requested, Approved, Rejected

## Decisions Made
- CancellationStatus placed as separate file in Enums/ directory (consistent with existing PackageStatus, SessionStatus pattern)
- ProtocolVersion stores full JSON snapshots (PreviousJson + CurrentJson) for complete state capture rather than field-level diffs
- CancellationRequest follows Billing.Domain Refund entity approval workflow pattern for consistency across modules
- Both entities inherit from Entity base class (providing Id, CreatedAt, UpdatedAt, IsDeleted)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- ProtocolVersion and CancellationRequest entities ready for aggregate root integration (TreatmentPackage)
- Entities ready for EF Core configuration in Treatment.Infrastructure
- Approval workflow methods (Approve/Reject) ready for handler implementation

## Self-Check: PASSED

All 3 created files verified on disk. Task commit c51246d verified in git log.

---
*Phase: 09-treatment-protocols*
*Completed: 2026-03-08*
