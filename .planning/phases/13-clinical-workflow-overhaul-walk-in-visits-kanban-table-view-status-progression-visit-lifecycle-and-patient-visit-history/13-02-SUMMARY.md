---
phase: 13-clinical-workflow-overhaul
plan: 02
subsystem: clinical-frontend
tags: [kanban, table-view, view-toggle, workflow-dashboard, i18n]
dependency_graph:
  requires: ["13-01"]
  provides: ["9-column-kanban", "workflow-table-view", "view-toggle-persistence"]
  affects: ["WorkflowDashboard", "KanbanColumn", "PatientCard"]
tech_stack:
  added: ["@radix-ui/react-toggle-group"]
  patterns: ["localStorage-persisted-view-mode", "9-column-kanban-with-done-filter"]
key_files:
  created:
    - frontend/src/features/clinical/components/WorkflowToolbar.tsx
    - frontend/src/features/clinical/components/ViewToggle.tsx
    - frontend/src/features/clinical/components/WorkflowTableView.tsx
    - frontend/src/shared/components/ToggleGroup.tsx
    - frontend/src/shared/components/Toggle.tsx
    - frontend/src/shared/components/ui/toggle-group.tsx
    - frontend/src/shared/components/ui/toggle.tsx
  modified:
    - frontend/src/features/clinical/components/WorkflowDashboard.tsx
    - frontend/src/features/clinical/components/KanbanColumn.tsx
    - frontend/src/features/clinical/components/PatientCard.tsx
    - frontend/src/features/clinical/api/clinical-api.ts
    - frontend/public/locales/en/clinical.json
    - frontend/public/locales/vi/clinical.json
    - frontend/package.json
decisions:
  - Built complete WorkflowTableView in Task 1 since it was needed for TypeScript compilation of the conditional rendering in WorkflowDashboard
  - Used shadcn ToggleGroup (installed via CLI) for view toggle instead of custom implementation
  - Kept legacy color mappings (gray, green) in KanbanColumn for backward compatibility
metrics:
  duration: 6min
  completed: "2026-03-25T06:36:36Z"
  tasks_completed: 2
  tasks_total: 2
  files_changed: 15
---

# Phase 13 Plan 02: Kanban 9-Column + Table View Summary

9-column kanban board with per-stage columns (Reception through Pharmacy/Optical + Done), horizontal scroll at 200px per column, WorkflowToolbar with patient count and view toggle, and WorkflowTableView with sorting/filtering/row-click navigation -- all with bilingual i18n.

## Changes Made

### Task 1: Expand kanban to 9 columns with toolbar and horizontal scroll
**Commit:** `7be3c8a`

- **WorkflowDashboard.tsx**: Replaced 5 grouped KANBAN_COLUMNS with 9 individual columns (1:1 stage mapping). Added `overflow-x-auto` horizontal scroll with `minWidth: 1800px`. Added `viewMode` state with localStorage persistence via `getStoredViewMode/storeViewMode`. Conditional rendering switches between kanban and table views.
- **KanbanColumn.tsx**: Updated accent color map with 9 new colors per UI-SPEC (stone, blue, emerald, cyan, teal, amber, orange, violet, muted). Changed column width from 220px to 200px. Added `isDone` prop to disable drag-and-drop for Done column.
- **PatientCard.tsx**: Added `isDone` prop to disable sortable/draggable behavior for completed visits.
- **WorkflowToolbar.tsx**: New component with page title, patient count badge, ViewToggle, and New Visit button.
- **ViewToggle.tsx**: New component using shadcn ToggleGroup with `ganka:workflow-view-mode` localStorage key. Exports `getStoredViewMode` and `storeViewMode` helpers.
- **clinical-api.ts**: Added `isCompleted: boolean` and `status: number` to `ActiveVisitDto`.
- **i18n**: Added all stage keys, view toggle labels, and empty state messages in both English and Vietnamese.

### Task 2: WorkflowTableView with sorting, filtering, and row actions
**Commit:** `7be3c8a` (built together with Task 1 for compilation)

- **WorkflowTableView.tsx**: Full sortable/filterable table with 7 columns (Patient, Doctor, Stage, Wait Time, Visit Time, Status, Actions). Stage filter via Select dropdown. Column header sorting with `aria-sort` attributes. Patient name links to `/patients/$patientId`. Row click navigates to `/visits/$visitId`. Forward arrow button to advance stage (disabled for done visits). Empty state when no visits match filters.
- **i18n**: All table-specific keys added in both languages.

### Dependency: shadcn toggle-group
**Commit:** `a8d35c0`

- Installed `@radix-ui/react-toggle-group` via shadcn CLI
- Created wrapper components at shared/components/ToggleGroup.tsx and Toggle.tsx

## Deviations from Plan

None - plan executed exactly as written.

## Decisions Made

1. **Combined Task 1 and Task 2 implementation**: WorkflowTableView was built as part of Task 1 because WorkflowDashboard's conditional rendering (`viewMode === "table"`) requires the import to compile. This is a practical consolidation, not a scope change.
2. **Kept legacy accent colors**: Added `gray` and `green` fallback entries in KanbanColumn's accent color map for backward compatibility in case any other code references old column definitions.

## Known Stubs

None. All components are fully wired to the data source (`useActiveVisits`) and render real data.

## Self-Check: PASSED
