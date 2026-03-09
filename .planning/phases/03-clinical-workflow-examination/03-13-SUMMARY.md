---
phase: 03-clinical-workflow-examination
plan: 13
subsystem: clinical
tags: [amendment, diff, sign-off, audit-trail, visit-lifecycle]

# Dependency graph
requires:
  - phase: 03-clinical-workflow-examination
    provides: "Visit amendment workflow, sign-off lifecycle"
provides:
  - "Accurate field-level diff in amendment history (baseline-at-initiation, diff-at-resign)"
  - "UpdateFieldChanges method on VisitAmendment entity"
  - "SignOffVisitCommand accepts optional FieldChangesJson for re-sign"
affects: [03-clinical-workflow-examination]

# Tech tracking
tech-stack:
  added: []
  patterns: ["baseline-snapshot-then-diff pattern for amendment audit trail"]

key-files:
  created: []
  modified:
    - "backend/src/Modules/Clinical/Clinical.Domain/Entities/VisitAmendment.cs"
    - "backend/src/Modules/Clinical/Clinical.Application/Features/SignOffVisit.cs"
    - "backend/src/Modules/Clinical/Clinical.Contracts/Dtos/CreateVisitCommand.cs"
    - "backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs"
    - "frontend/src/features/clinical/components/AmendmentDialog.tsx"
    - "frontend/src/features/clinical/components/SignOffSection.tsx"
    - "frontend/src/features/clinical/api/clinical-api.ts"
    - "backend/tests/Clinical.Unit.Tests/Features/SignOffVisitHandlerTests.cs"

key-decisions:
  - "Compute diff at re-sign time by comparing stored baseline to current state, not at amendment initiation"
  - "Store baseline as VisitBaseline JSON object (not array), replacing it with actual diff array at re-sign"
  - "Use 'field' property name consistently (matching VisitAmendmentHistory's FieldChange interface)"
  - "Backward-compatible: old array-format fieldChangesJson skips diff computation"

patterns-established:
  - "baseline-snapshot-then-diff: capture state at action start, compute diff at action end"

requirements-completed: [CLN-02, CLN-04]

# Metrics
duration: 6min
completed: 2026-03-09
---

# Phase 03 Plan 13: Amendment Field-Level Diff Summary

**Fix amendment diff to compute actual before/after changes at re-sign time, replacing placeholder "pending_amendment" values with accurate field-level diffs**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-09T08:11:37Z
- **Completed:** 2026-03-09T08:17:32Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- Fixed amendment history to show only fields that actually changed (not all non-empty fields)
- Replaced "pending_amendment" placeholder values with accurate old/new values
- Fixed property name mismatch ("fieldName" -> "field") for correct display in amendment history table
- Added 3 new test cases for amendment re-sign scenarios (8 total SignOffVisit tests, all pass)
- Maintained backward compatibility with old-format fieldChangesJson arrays

## Task Commits

Each task was committed atomically:

1. **Task 1: Add backend support for updating amendment field changes at re-sign** - `631ed65` (feat)
2. **Task 2: Fix frontend amendment diff lifecycle and property name consistency** - `f58f844` (fix)

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Domain/Entities/VisitAmendment.cs` - Added UpdateFieldChanges method
- `backend/src/Modules/Clinical/Clinical.Application/Features/SignOffVisit.cs` - Handler updates latest amendment on re-sign
- `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/CreateVisitCommand.cs` - SignOffVisitCommand with optional FieldChangesJson
- `backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs` - Sign-off endpoint accepts optional body
- `frontend/src/features/clinical/components/AmendmentDialog.tsx` - Captures baseline snapshot only (not diff)
- `frontend/src/features/clinical/components/SignOffSection.tsx` - Computes actual diff at re-sign time
- `frontend/src/features/clinical/api/clinical-api.ts` - signOffVisit/useSignOffVisit accept optional fieldChangesJson
- `backend/tests/Clinical.Unit.Tests/Features/SignOffVisitHandlerTests.cs` - 8 tests including 3 new amendment scenarios

## Decisions Made
- Compute diff at re-sign time by comparing stored baseline to current state (architectural fix for the root cause)
- Store baseline as VisitBaseline JSON object; replaced with diff array at re-sign (allows distinguishing baseline from final diff)
- Use "field" property name consistently to match VisitAmendmentHistory's FieldChange interface
- Added backward compatibility check: if fieldChangesJson is already an array, skip diff computation

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Amendment workflow now produces accurate diffs with correct property names
- UAT Test 13 should pass: editing 1 field shows exactly 1 row in amendment history
- No "pending_amendment" text will appear in amendment history

## Self-Check: PASSED

- All 9 files verified present on disk
- Both task commits (631ed65, f58f844) verified in git history
- 124/124 clinical unit tests pass (0 regressions)

---
*Phase: 03-clinical-workflow-examination*
*Completed: 2026-03-09*
