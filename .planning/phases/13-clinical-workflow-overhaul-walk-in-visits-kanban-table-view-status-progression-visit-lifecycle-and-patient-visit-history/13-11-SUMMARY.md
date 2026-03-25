---
phase: 13-clinical-workflow-overhaul
plan: 11
subsystem: clinical-frontend
tags: [kanban, workflow, frontend, i18n]
dependency_graph:
  requires: [13-10]
  provides: [kanban-board-11-stage, api-hooks-workflow, conditional-columns]
  affects: [clinical-workflow-dashboard, patient-card, workflow-table-view]
tech_stack:
  added: []
  patterns: [conditional-column-visibility, track-status-based-filtering]
key_files:
  created: []
  modified:
    - frontend/src/features/clinical/api/clinical-api.ts
    - frontend/src/features/clinical/components/WorkflowDashboard.tsx
    - frontend/src/features/clinical/components/KanbanColumn.tsx
    - frontend/src/features/clinical/components/PatientCard.tsx
    - frontend/src/features/clinical/components/WorkflowTableView.tsx
    - frontend/public/locales/en/clinical.json
    - frontend/public/locales/vi/clinical.json
decisions:
  - "Removed CashierGlasses column to match backend WorkflowStage enum (11 stages, no CashierGlasses)"
  - "Done column has empty stages array, uses isCompleted flag for grouping"
  - "Conditional columns driven by drugTrackStatus/glassesTrackStatus from ActiveVisitDto"
metrics:
  duration: "4m 42s"
  completed: "2026-03-25T12:46:09Z"
  tasks_completed: 2
  tasks_total: 2
  files_modified: 7
---

# Phase 13 Plan 11: Frontend Kanban Board & API Hooks for 11-Stage Workflow Summary

Frontend kanban board redesigned for 11-stage branching workflow with conditional column visibility, new card anatomy with amber pulse dot and Vietnamese stage pills, and 10 new API mutation hooks for all workflow endpoints.

## Tasks Completed

### Task 1: Update API types and hooks for all new workflow endpoints
- **Commit:** bd6c5fd
- Added `drugTrackStatus`, `glassesTrackStatus`, `imagingRequested`, `refractionSkipped` to `ActiveVisitDto`
- Created 8 new command types: `SkipRefractionCommand`, `RequestImagingCommand`, `ConfirmVisitPaymentCommand`, `DispensePharmacyCommand`, `ConfirmOpticalOrderCommand`, `CompleteOpticalLabCommand`, `CompleteHandoffCommand`
- Added 10 mutation hooks following existing pattern with activeVisits invalidation
- Updated i18n: 11 stage labels in both EN and VI with proper Vietnamese diacritics
- Updated `ALLOWED_REVERSALS` comments for renamed stages

### Task 2: Redesign kanban board columns, card anatomy, and conditional visibility
- **Commit:** e922493
- Replaced 8-stage KANBAN_COLUMNS with 12-entry array (11 stages + Done), each with `alwaysVisible` flag
- Implemented `visibleColumns` useMemo: Pharmacy visible when `drugTrackStatus !== 0`, optical columns visible when `glassesTrackStatus !== 0`
- Redesigned PatientCard: amber pulse dot for elapsed wait, stage pill badges (green for signed/paid, amber for skipped)
- Forward shortcut button only on Reception (stage 0) with label "Chuyen tiep"
- All other stage cards show muted hint "Nhan de xem chi tiet" with entire card clickable
- Added `pink` and `indigo` accent colors to KanbanColumn
- Updated WorkflowTableView stage labels and MAX_STAGE for 11-stage model

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Removed CashierGlasses column from plan's KANBAN_COLUMNS**
- **Found during:** Task 2
- **Issue:** Plan's column definitions included `cashier-glasses` at stage 9, but backend `WorkflowStage.cs` has no CashierGlasses (removed in commit e061cd3). Plan's must_haves truth also states "NO CashierGlasses."
- **Fix:** Used 11-stage model matching backend enum: Reception(0) through ReturnGlasses(10) + Done(99), without CashierGlasses
- **Files modified:** WorkflowDashboard.tsx

## Verification

- TypeScript compiles without errors (only pre-existing baseUrl deprecation warning)
- All acceptance criteria grep checks pass
- Kanban has 12 column definitions (11 stages + Done)
- Conditional columns filter by track status
- Card shows amber pulse dot, stage pills, forward only on Reception

## Known Stubs

None - all components are wired to real data from ActiveVisitDto. The new mutation hooks call real API endpoints (which may not exist in the backend yet, as those are covered by plan 13-10).

## Self-Check: PASSED
