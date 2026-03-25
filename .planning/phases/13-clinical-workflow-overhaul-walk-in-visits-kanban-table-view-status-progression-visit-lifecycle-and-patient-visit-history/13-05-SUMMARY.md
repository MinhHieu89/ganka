---
phase: 13-clinical-workflow-overhaul
plan: 05
subsystem: ui
tags: [react, tanstack-router, i18n, clinical-workflow, signalr]

requires:
  - phase: 13-01
    provides: "Kanban board with PatientCard, VisitDetailPage, OsdiSection components"
provides:
  - "Patient name links in kanban cards and visit detail page header"
  - "Optical prescription auto-expand when data exists"
  - "Verified OSDI answers display and realtime updates already implemented"
affects: [clinical-workflow]

tech-stack:
  added: []
  patterns:
    - "Link with stopPropagation on draggable cards to separate click-navigate from drag"
    - "Conditional initial state for collapsible sections based on data presence"

key-files:
  created: []
  modified:
    - frontend/src/features/clinical/components/PatientCard.tsx
    - frontend/src/features/clinical/components/VisitDetailPage.tsx
    - frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx
    - frontend/public/locales/en/clinical.json
    - frontend/public/locales/vi/clinical.json

key-decisions:
  - "Used stopPropagation on Link click in PatientCard to prevent card navigation handler from firing"
  - "Split visit.title i18n key into titlePrefix + Link for patient name in VisitDetailPage header"
  - "Changed OpticalPrescriptionSection initial open state from always-true to data-dependent"

patterns-established:
  - "Patient name link pattern: Link to /patients/$patientId with text-primary hover:underline"

requirements-completed: [CLN-03, CLN-04]

duration: 2min
completed: 2026-03-25
---

# Phase 13 Plan 05: Folded Todos Summary

**Patient name links to profile in kanban cards and visit detail, optical Rx auto-expand on data, OSDI answers and realtime updates verified already complete**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-25T06:30:17Z
- **Completed:** 2026-03-25T06:32:29Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Patient name in kanban PatientCard now links to patient profile with stopPropagation to avoid drag interference (D-17)
- Patient name in VisitDetailPage header now links to patient profile (D-17)
- Optical prescription section auto-expands when prescriptions exist, stays collapsed when empty (D-19)
- Verified OSDI answers display (D-18) already implemented via OsdiAnswersSection in OsdiSection
- Verified realtime OSDI score updates (D-20) already implemented via useOsdiHub SignalR invalidation

## Task Commits

Each task was committed atomically:

1. **Task 1: Patient name links (D-17) + OSDI answers verification (D-18)** - `8828701` (feat)
2. **Task 2: Optical Rx auto-expand (D-19) + realtime OSDI (D-20) verification** - `464713d` (feat)

## Files Created/Modified
- `frontend/src/features/clinical/components/PatientCard.tsx` - Added Link import, wrapped patient name in Link to /patients/$patientId with stopPropagation
- `frontend/src/features/clinical/components/VisitDetailPage.tsx` - Wrapped patient name in header with Link to patient profile
- `frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx` - Changed initial sectionOpen state from true to prescriptions.length > 0
- `frontend/public/locales/en/clinical.json` - Added visit.titlePrefix key
- `frontend/public/locales/vi/clinical.json` - Added visit.titlePrefix key

## Decisions Made
- Used stopPropagation on Link click in PatientCard to prevent the card's onClick handler from navigating to the visit page when clicking the patient name link
- Split the visit.title i18n template into titlePrefix + inline Link to make patient name a clickable link while keeping i18n support
- Changed OpticalPrescriptionSection from always-open to data-dependent initial state (prescriptions.length > 0)

## Deviations from Plan

None - plan executed exactly as written. D-18 and D-20 were confirmed already implemented as the plan anticipated.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 4 folded todos (D-17, D-18, D-19, D-20) addressed
- Ready for subsequent plans in phase 13

---
*Phase: 13-clinical-workflow-overhaul*
*Completed: 2026-03-25*
