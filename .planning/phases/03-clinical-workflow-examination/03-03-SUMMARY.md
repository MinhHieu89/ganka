---
phase: 03-clinical-workflow-examination
plan: 03
subsystem: ui
tags: [dnd-kit, kanban, drag-and-drop, tanstack-query, polling, i18n, clinical, workflow, shadcn]

# Dependency graph
requires:
  - phase: 03-clinical-workflow-examination
    plan: 02
    provides: "13 Clinical API endpoints under /api/clinical (active visits, visit lifecycle, refraction, diagnosis, ICD-10)"
  - phase: 01.2
    provides: "shadcn/ui wrappers (Card, Badge, Button, Tooltip, Skeleton, Dialog, etc.), AppSidebar, i18n setup"
  - phase: 02
    provides: "DoctorSelector component, GlobalSearch, patient search hooks, openapi-fetch client pattern"
provides:
  - "Clinical workflow Kanban dashboard at /clinical route with 5 grouped columns and @dnd-kit drag-and-drop"
  - "clinical-api.ts with 13 TanStack Query hooks and API functions for full clinical module"
  - "PatientCard with allergy warning, wait time badge, stage badge, and advance button"
  - "5-second polling via refetchInterval for real-time dashboard updates"
  - "NewVisitDialog for walk-in visit creation with patient search and doctor selector"
  - "Collapsible wrapper component for use in Plan 04 visit detail page"
  - "en/clinical.json and vi/clinical.json with Vietnamese diacritics for full clinical module"
affects: [03-04, 03-05]

# Tech tracking
tech-stack:
  added: ["@dnd-kit/core", "@dnd-kit/sortable", "@dnd-kit/utilities", "@radix-ui/react-collapsible (shadcn collapsible)"]
  patterns: ["Kanban column grouping (8 stages -> 5 columns)", "DndContext with PointerSensor+TouchSensor", "refetchInterval polling for real-time data", "useDroppable for column drop targets, useSortable for card drag handles"]

key-files:
  created:
    - frontend/src/features/clinical/api/clinical-api.ts
    - frontend/src/features/clinical/components/WorkflowDashboard.tsx
    - frontend/src/features/clinical/components/KanbanColumn.tsx
    - frontend/src/features/clinical/components/PatientCard.tsx
    - frontend/src/features/clinical/components/NewVisitDialog.tsx
    - frontend/src/app/routes/_authenticated/clinical/index.tsx
    - frontend/src/shared/components/Collapsible.tsx
    - frontend/src/shared/components/ui/collapsible.tsx
    - frontend/public/locales/en/clinical.json
    - frontend/public/locales/vi/clinical.json
  modified:
    - frontend/package.json
    - frontend/src/shared/i18n/i18n.ts
    - frontend/src/shared/components/AppSidebar.tsx
    - frontend/src/app/routeTree.gen.ts

key-decisions:
  - "5 Kanban columns grouping 8 stages: Reception[0], Testing[1], Doctor[2,3,4], Processing[5,6], Done[7]"
  - "PointerSensor distance:8 + TouchSensor delay:200 for desktop and tablet drag support"
  - "DragOverlay renders PatientCard clone for visual feedback during drag"
  - "NewVisitDialog inline patient search using existing usePatientSearch hook"
  - "Wait time badge changes to destructive variant at 60+ minutes for visual urgency"

patterns-established:
  - "Kanban grouping: KANBAN_COLUMNS constant maps column IDs to stage arrays, getColumnForStage helper for visit-to-column mapping"
  - "clinical-api.ts exports clinicalKeys factory, raw API functions, and useX hooks following scheduling-api pattern"
  - "Collapsible wrapper: pure re-export from ui/collapsible.tsx per established wrapper pattern"

requirements-completed: [CLN-03, CLN-04]

# Metrics
duration: 9min
completed: 2026-03-04
---

# Phase 03 Plan 03: Clinical Workflow Kanban Dashboard Summary

**Kanban workflow dashboard at /clinical with @dnd-kit drag-and-drop, 5 grouped columns, patient cards with allergy warnings and wait time, 5-second polling, and full clinical API hooks**

## Performance

- **Duration:** 9 min
- **Started:** 2026-03-04T10:27:35Z
- **Completed:** 2026-03-04T10:36:35Z
- **Tasks:** 2
- **Files modified:** 14

## Accomplishments
- Kanban dashboard renders 5 columns (Reception, Testing, Doctor, Processing, Done) grouping 8 workflow stages
- @dnd-kit drag-and-drop with PointerSensor and TouchSensor allows dragging patient cards between columns to advance stages
- PatientCard shows patient name, doctor name, visit time, exact stage badge, wait time badge, and allergy warning icon
- "Advance" action button on each card provides non-DnD alternative for stage advancement
- clinical-api.ts provides 13 TanStack Query hooks covering all clinical module API endpoints
- 5-second polling keeps dashboard data fresh without SignalR complexity
- NewVisitDialog enables walk-in visit creation with patient search and doctor selection
- Sidebar "Clinical" nav item enabled and links to /clinical route
- Vietnamese translations with proper diacritics for all clinical UI strings

## Task Commits

Each task was committed atomically:

1. **Task 1: Install dependencies, API hooks, Collapsible wrapper, and i18n translations** - `bedf113` (feat)
2. **Task 2: Kanban workflow dashboard with drag-and-drop and patient cards** - `05d8148` (feat)

## Files Created/Modified
- `frontend/src/features/clinical/api/clinical-api.ts` - 13 API functions + 13 TanStack Query hooks for clinical module
- `frontend/src/features/clinical/components/WorkflowDashboard.tsx` - Main Kanban board with DndContext, column grouping, drag-and-drop, new visit dialog
- `frontend/src/features/clinical/components/KanbanColumn.tsx` - Single column with useDroppable, SortableContext, color accent
- `frontend/src/features/clinical/components/PatientCard.tsx` - Draggable card with allergy warning, wait time, stage badge, advance button
- `frontend/src/features/clinical/components/NewVisitDialog.tsx` - Walk-in visit creation with patient search and doctor selector
- `frontend/src/app/routes/_authenticated/clinical/index.tsx` - Route page at /clinical
- `frontend/src/shared/components/Collapsible.tsx` - Re-export wrapper for shadcn Collapsible
- `frontend/src/shared/components/ui/collapsible.tsx` - shadcn Collapsible primitive
- `frontend/public/locales/en/clinical.json` - English translations for clinical module
- `frontend/public/locales/vi/clinical.json` - Vietnamese translations with proper diacritics
- `frontend/package.json` - Added @dnd-kit/core, @dnd-kit/sortable, @dnd-kit/utilities
- `frontend/src/shared/i18n/i18n.ts` - Added clinical namespace to i18n config
- `frontend/src/shared/components/AppSidebar.tsx` - Enabled Clinical nav item (removed disabled flag)
- `frontend/src/app/routeTree.gen.ts` - Auto-generated route tree update

## Decisions Made
- 5 Kanban columns grouping 8 stages for boutique 2-doctor clinic: reduces visual clutter while each card still shows exact stage
- PointerSensor distance:8 prevents accidental drags on click, TouchSensor delay:200 for tablet support
- DragOverlay renders PatientCard clone for smooth visual feedback
- Wait time badge uses destructive variant at 60+ minutes, secondary at 30+, outline below -- provides visual urgency cues
- NewVisitDialog reuses existing DoctorSelector and usePatientSearch for consistent patterns

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Kanban dashboard complete -- ready for visit detail page in Plan 04
- Collapsible wrapper created and available for collapsible card sections in visit detail page
- clinical-api.ts already exports all hooks needed by Plan 04 (useVisitById, useUpdateNotes, useUpdateRefraction, useAddDiagnosis, etc.)
- i18n translations cover both Kanban (Plan 03) and visit detail (Plan 04) strings

## Self-Check: PASSED
