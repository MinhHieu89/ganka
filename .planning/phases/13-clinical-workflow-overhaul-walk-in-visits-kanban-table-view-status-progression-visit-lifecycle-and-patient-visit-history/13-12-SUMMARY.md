---
phase: 13-clinical-workflow-overhaul
plan: 12
subsystem: clinical-stage-detail-views
tags: [react, shadcn-ui, stage-detail, skip-path, refraction, workflow-ux]
dependency_graph:
  requires: [workflow-api-endpoints, skip-refraction-handler, advance-stage-handler]
  provides: [stage-detail-shell, stage-bottom-bar, validation-message, stage2-refraction-view, skip-stage-modal, refraction-skip-banner, stage-route]
  affects: [clinical-kanban-frontend, visit-detail-page, workflow-navigation]
tech_stack:
  added: []
  patterns: [stage-detail-shell-wrapper, bottom-bar-layout, skip-path-ux-pattern, sph-field-validation]
key_files:
  created:
    - frontend/src/features/clinical/components/StageDetailShell.tsx
    - frontend/src/features/clinical/components/StageBottomBar.tsx
    - frontend/src/features/clinical/components/ValidationMessage.tsx
    - frontend/src/features/clinical/components/stage-views/Stage2RefractionView.tsx
    - frontend/src/features/clinical/components/stage-views/SkipStageModal.tsx
    - frontend/src/features/clinical/components/stage-views/RefractionSkipBanner.tsx
    - frontend/src/app/routes/_authenticated/clinical/visit.$visitId.stage.tsx
  modified:
    - frontend/src/features/clinical/api/clinical-api.ts
decisions:
  - "SkipStageModal designed as reusable component receiving reason chips as props rather than hardcoding refraction-specific reasons"
  - "Vietnamese labels stored as Unicode escape sequences in JS constants for reliable cross-platform encoding"
  - "Skip state tracked locally in Stage2RefractionView since backend VisitDetailDto does not yet expose StageSkip data"
metrics:
  duration: 6min
  completed: "2026-03-25T12:48:30Z"
  tasks_completed: 2
  tasks_total: 2
  files_created: 7
  files_modified: 1
---

# Phase 13 Plan 12: Stage Detail Views + Stage 2 Refraction Summary

Shared stage detail infrastructure (shell, bottom bar, validation message) and complete Stage 2 Refraction detail view with skip path UX, field validation, and routing.

## Commits

| # | Hash | Message |
|---|------|---------|
| 1 | 38c9410 | feat(13-12): create shared StageDetailShell, StageBottomBar, and ValidationMessage components |
| 2 | 1af536a | feat(13-12): implement Stage 2 refraction detail view with skip path and routing |

## Task Details

### Task 1: Shared stage detail components

- **StageDetailShell**: Wrapper with patient info header (name, doctor, date), stage pill badge (default/amber/green), scrollable content area, sticky bottom bar
- **StageBottomBar**: Consistent 3-button layout -- save draft (left), secondary action (middle), primary action (right) with validation message slot above
- **ValidationMessage**: Error (red), warning (amber), success (green) states with colored left border

### Task 2: Stage 2 Refraction detail view

- **Stage2RefractionView**: Full refraction form with two-column OD/OS layout, MP/MT labels, 3 measurement tabs (Thuong quy / May do tu dong / Liet dieu tiet)
- **SPH validation**: Red border + bg when empty, green border + bg when filled; only SPH fields highlighted
- **Validation messages**: Both empty = error, one filled = warning, both filled = success (Vietnamese text per spec)
- **Bottom bar**: "Luu nhap" (save draft), "Bo qua buoc nay" (skip, red border), "Hoan tat do, chuyen bac si" (advance, disabled until both SPH filled)
- **SkipStageModal**: Reusable modal with 4 preset reason chips (Tai kham/Tu choi do/Tong quat/Khac), optional free-text (200 char max), confirm disabled until chip selected
- **RefractionSkipBanner**: Amber banner with reason, actor, time; "Hoan tac" (undo) button
- **Post-skip state**: Form dimmed (opacity-50 pointer-events-none), amber stage pill, bottom bar simplified
- **Route file**: `/clinical/visit/:visitId/stage` renders stage-specific view by currentStage, Stage 2 implemented, others show placeholder
- **API hooks**: Added useSkipRefraction and useUndoRefractionSkip mutation hooks to clinical-api.ts

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing critical functionality] Added useSkipRefraction and useUndoRefractionSkip API hooks**
- **Found during:** Task 2
- **Issue:** clinical-api.ts had no hooks for skip-refraction and undo-refraction-skip backend endpoints
- **Fix:** Added skipRefraction/undoRefractionSkip API functions and corresponding React Query mutation hooks with proper cache invalidation
- **Files modified:** frontend/src/features/clinical/api/clinical-api.ts
- **Commit:** 1af536a

## Known Stubs

- Skip state is tracked locally in Stage2RefractionView (not from backend VisitDetailDto) -- will need backend StageSkip data exposure in a future plan for page-reload persistence

## Self-Check: PASSED
