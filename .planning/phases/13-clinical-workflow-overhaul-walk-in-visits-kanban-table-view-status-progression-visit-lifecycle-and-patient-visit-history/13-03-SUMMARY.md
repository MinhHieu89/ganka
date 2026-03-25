---
phase: 13-clinical-workflow-overhaul
plan: 03
subsystem: ui
tags: [react, dnd-kit, kanban, stage-reversal, dialog, i18n, tanstack-query]

requires:
  - phase: 13-01
    provides: "Backend reverse-stage endpoint and allowed transitions"
  - phase: 13-02
    provides: "WorkflowDashboard kanban/table views, WorkflowTableView component"
provides:
  - "StageReversalDialog component for backward stage transitions"
  - "useReverseStage mutation hook"
  - "ALLOWED_REVERSALS constant and isReversalAllowed helper"
  - "Backward drag detection in kanban handleDragEnd"
  - "Backward arrow button in table view with multi-target dropdown"
affects: [13-04, 13-05, 13-06]

tech-stack:
  added: []
  patterns:
    - "Backward drag detection in kanban via stage comparison"
    - "Reversal dialog with mandatory reason for audit trail"
    - "Multi-target reversal dropdown for stages with multiple backward options"

key-files:
  created:
    - frontend/src/features/clinical/components/StageReversalDialog.tsx
  modified:
    - frontend/src/features/clinical/api/clinical-api.ts
    - frontend/src/features/clinical/components/WorkflowDashboard.tsx
    - frontend/src/features/clinical/components/WorkflowTableView.tsx
    - frontend/public/locales/en/clinical.json
    - frontend/public/locales/vi/clinical.json

key-decisions:
  - "Used Select dropdown for stages with multiple reversal targets instead of separate buttons"
  - "Reversal dialog uses standard Textarea (not AutoResizeTextarea) since reason field has fixed purpose"

patterns-established:
  - "Stage reversal pattern: detect backward movement, validate with ALLOWED_REVERSALS, open dialog, call mutation"

requirements-completed: [CLN-03]

duration: 3min
completed: 2026-03-25
---

# Phase 13 Plan 03: Stage Reversal Dialog Summary

**StageReversalDialog with mandatory 10-char reason, backward drag detection in kanban, and reversal button with multi-target dropdown in table view**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-25T06:41:20Z
- **Completed:** 2026-03-25T06:44:26Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments
- StageReversalDialog component with reason validation, mutation, and toast feedback
- Backward drag detection in kanban handleDragEnd opens reversal dialog for allowed transitions
- Table view backward arrow button with Select dropdown for multi-target stages (DoctorReads, Rx)
- ALLOWED_REVERSALS constant and isReversalAllowed helper exported from clinical-api
- Full English and Vietnamese i18n for all reversal dialog strings

## Task Commits

Each task was committed atomically:

1. **Task 1: StageReversalDialog component and useReverseStage mutation hook** - `122bf63` (feat)
2. **Task 2: Wire reversal into kanban handleDragEnd and table view actions** - `8cf905e` (feat)

## Files Created/Modified
- `frontend/src/features/clinical/components/StageReversalDialog.tsx` - Modal dialog for stage reversal with reason textarea and mutation
- `frontend/src/features/clinical/api/clinical-api.ts` - Added ALLOWED_REVERSALS, isReversalAllowed, reverseWorkflowStage, useReverseStage
- `frontend/src/features/clinical/components/WorkflowDashboard.tsx` - Added backward drag detection, reversalInfo state, StageReversalDialog rendering
- `frontend/src/features/clinical/components/WorkflowTableView.tsx` - Added backward arrow button with multi-target dropdown, onReverseStage prop
- `frontend/public/locales/en/clinical.json` - Added workflow.reversal.* keys
- `frontend/public/locales/vi/clinical.json` - Added workflow.reversal.* keys (Vietnamese)

## Decisions Made
- Used Select dropdown for stages with multiple reversal targets (DoctorReads->Diagnostics/DoctorExam, Rx->DoctorExam/DoctorReads) instead of always showing first target
- Used standard Textarea for reason input since it's a fixed-purpose field, not AutoResizeTextarea

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Known Stubs
None - all functionality is fully wired.

## Next Phase Readiness
- Stage reversal dialog and backward transitions fully functional
- Ready for plans 13-04 through 13-06 which build on the kanban/table foundation

---
*Phase: 13-clinical-workflow-overhaul*
*Completed: 2026-03-25*
