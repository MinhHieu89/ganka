---
phase: 13-clinical-workflow-overhaul
plan: 07
subsystem: clinical
tags: [workflow, kanban, enum, bugfix, dnd-kit]

requires:
  - phase: 13-clinical-workflow-overhaul
    provides: "WorkflowStage enum, Kanban board, GetActiveVisits handler"
provides:
  - "Done=8 enum value in WorkflowStage"
  - "Correct IsCompleted logic (Done stage only, not PharmacyOptical)"
  - "Drag-to-Done support in kanban board"
  - "Advance button from PharmacyOptical in table view"
affects: [13-clinical-workflow-overhaul]

tech-stack:
  added: []
  patterns: ["explicit terminal stage enum for workflow completion"]

key-files:
  created:
    - backend/tests/Clinical.Unit.Tests/Domain/VisitTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/WorkflowStage.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetActiveVisits.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetActiveVisitsHandlerTests.cs
    - frontend/src/features/clinical/components/WorkflowDashboard.tsx
    - frontend/src/features/clinical/components/WorkflowTableView.tsx
    - frontend/src/features/clinical/components/PatientCard.tsx

key-decisions:
  - "Done=8 requires no DB migration since EF Core stores enums as integers"
  - "SignOffVisit auto-advance caps at PharmacyOptical, leaving Done as explicit manual step"

patterns-established:
  - "Terminal workflow stage: explicit Done enum value separates working stages from completion"

requirements-completed: [CLN-03, CLN-04]

duration: 5min
completed: 2026-03-25
---

# Phase 13 Plan 07: PharmacyOptical Done Bug Fix Summary

**Added Done=8 workflow stage to fix PharmacyOptical auto-complete bug -- visits now stay in PharmacyOptical until explicitly advanced to Done**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-25T10:01:23Z
- **Completed:** 2026-03-25T10:06:20Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- Added `Done = 8` to WorkflowStage enum, separating PharmacyOptical (working) from Done (completed)
- Fixed GetActiveVisits IsCompleted check and VisitRepository done-today filter to use WorkflowStage.Done
- Updated frontend kanban Done column to map to stage 8, enabled drag-to-Done, updated MAX_STAGE to 8
- All 211 backend unit tests pass including new tests for Done stage behavior

## Task Commits

Each task was committed atomically:

1. **Task 1 RED: Failing tests for Done=8** - `9d93361` (test)
2. **Task 1 GREEN: Add Done=8 enum and fix backend logic** - `127186a` (feat)
3. **Task 2: Update frontend kanban and table for Done=8** - `be3530c` (feat)

_Note: TDD task 1 has separate RED and GREEN commits_

## Files Created/Modified
- `backend/src/Modules/Clinical/Clinical.Domain/Enums/WorkflowStage.cs` - Added Done = 8 enum value
- `backend/src/Modules/Clinical/Clinical.Application/Features/GetActiveVisits.cs` - IsCompleted now checks WorkflowStage.Done
- `backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs` - Done-today filter uses WorkflowStage.Done
- `backend/tests/Clinical.Unit.Tests/Features/GetActiveVisitsHandlerTests.cs` - Updated/added tests for Done stage
- `backend/tests/Clinical.Unit.Tests/Domain/VisitTests.cs` - New: AdvanceStage to Done tests
- `frontend/src/features/clinical/components/WorkflowDashboard.tsx` - Done column stages=[8], removed isCompleted routing, enabled drag-to-Done
- `frontend/src/features/clinical/components/WorkflowTableView.tsx` - MAX_STAGE=8, added stage 8 label
- `frontend/src/features/clinical/components/PatientCard.tsx` - MAX_STAGE=8, added stage 8 label

## Decisions Made
- Done=8 requires no DB migration since EF Core stores enums as integers -- existing PharmacyOptical visits (int 7) remain correctly at stage 7
- SignOffVisit.cs auto-advance intentionally caps at PharmacyOptical -- Done requires explicit staff action (drag or advance button)
- AdvanceStage allows skipping stages (e.g., Cashier to Done) since domain only validates forward movement

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Known Stubs
None - all data paths are fully wired.

## Next Phase Readiness
- PharmacyOptical bug fixed, visits stay visible in PharmacyOptical column until explicitly moved to Done
- Ready for remaining Phase 13 plans

---
*Phase: 13-clinical-workflow-overhaul*
*Completed: 2026-03-25*
