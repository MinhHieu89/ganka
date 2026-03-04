---
phase: 03-clinical-workflow-examination
plan: 04
subsystem: ui
tags: [react, tanstack-query, shadcn, popover, command, collapsible, icd10, refraction, sign-off, amendment, zod, i18n]

# Dependency graph
requires:
  - phase: 03-clinical-workflow-examination
    plan: 03
    provides: "clinical-api.ts with 13 TanStack Query hooks, Collapsible wrapper, i18n clinical namespace"
  - phase: 01.2
    provides: "shadcn/ui wrappers (Card, Badge, Button, Tabs, Popover, Command, AlertDialog, Dialog, Select, etc.)"
  - phase: 02
    provides: "AllergyForm Popover+Command pattern reference, Field/FieldLabel/FieldError components"
provides:
  - "Visit detail page at /visits/{visitId} with 6 collapsible card sections"
  - "VisitSection reusable collapsible card component for consistent section layout"
  - "RefractionSection with OD/OS side-by-side layout and Manifest/Auto/Cycloplegic tabs"
  - "Icd10Combobox with bilingual search, per-doctor favorites, laterality enforcement"
  - "SignOffSection with AlertDialog confirmation and read-only immutability"
  - "AmendmentDialog with mandatory reason (min 10 chars) and useAmendVisit mutation"
  - "VisitAmendmentHistory with reverse-chronological amendments and field-level diff table"
affects: [03-05]

# Tech tracking
tech-stack:
  added: []
  patterns: ["VisitSection collapsible card pattern (Collapsible+Card+CardHeader trigger)", "RefractionForm OD/OS side-by-side grid with debounced auto-save on blur", "Icd10Combobox Popover+Command with inline laterality selector", "SignOff AlertDialog confirmation -> read-only immutability"]

key-files:
  created:
    - frontend/src/features/clinical/components/VisitDetailPage.tsx
    - frontend/src/features/clinical/components/VisitSection.tsx
    - frontend/src/features/clinical/components/PatientInfoSection.tsx
    - frontend/src/features/clinical/components/RefractionSection.tsx
    - frontend/src/features/clinical/components/RefractionForm.tsx
    - frontend/src/features/clinical/components/ExaminationNotesSection.tsx
    - frontend/src/features/clinical/components/DiagnosisSection.tsx
    - frontend/src/features/clinical/components/Icd10Combobox.tsx
    - frontend/src/features/clinical/components/SignOffSection.tsx
    - frontend/src/features/clinical/components/AmendmentDialog.tsx
    - frontend/src/features/clinical/components/VisitAmendmentHistory.tsx
    - frontend/src/app/routes/_authenticated/visits/$visitId.tsx
  modified:
    - frontend/public/locales/en/clinical.json
    - frontend/public/locales/vi/clinical.json
    - frontend/src/app/routeTree.gen.ts

key-decisions:
  - "VisitSection as reusable collapsible card wrapper with defaultOpen prop and headerExtra slot"
  - "RefractionForm debounced auto-save on blur (500ms) rather than explicit save button"
  - "Icd10Combobox uses Button trigger (not Input) with div wrapper to avoid click-to-toggle anti-pattern"
  - "Laterality enforcement as inline selector within Popover (not separate dialog)"
  - "ExaminationNotes debounced auto-save on blur (1s) for longer text editing"
  - "SignOff uses AlertDialog (not Dialog) for non-dismissible confirmation pattern"

patterns-established:
  - "VisitSection: Collapsible + Card pattern with CardHeader as CollapsibleTrigger, chevron rotation via data-state CSS"
  - "RefractionForm: 3-column grid (label + OD + OS) for eye-specific data entry, shared fields below separator"
  - "Icd10Combobox: Popover+Command with shouldFilter=false, debounced API search, inline laterality selector for RequiresLaterality codes"
  - "AmendmentDialog: Zod validation inside component function for i18n access to error messages"

requirements-completed: [CLN-01, CLN-02, REF-01, REF-02, REF-03, DX-01, DX-02]

# Metrics
duration: 7min
completed: 2026-03-04
---

# Phase 03 Plan 04: Visit Detail Page with Refraction, ICD-10 Diagnosis, Sign-off, and Amendment Summary

**Complete visit detail page with OD/OS refraction tabs, bilingual ICD-10 combobox with laterality enforcement and per-doctor favorites, sign-off immutability, and amendment workflow**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-04T10:41:09Z
- **Completed:** 2026-03-04T10:48:14Z
- **Tasks:** 2
- **Files modified:** 15

## Accomplishments
- Visit detail page at /visits/{visitId} renders 6 collapsible card sections in single scrollable layout
- RefractionSection with Manifest/Autorefraction/Cycloplegic tabs, (*) indicator for tabs with data, OD/OS side-by-side form with all refraction fields (SPH, CYL, AXIS, ADD, PD, UCVA, BCVA, IOP, Axial Length)
- Icd10Combobox with bilingual Vietnamese/English search, per-doctor favorites with star toggle, category grouping, inline laterality enforcement (OD/OS/OU) for RequiresLaterality codes
- SignOffSection with AlertDialog confirmation that locks the record, making all fields read-only
- AmendmentDialog with mandatory reason (minimum 10 characters), opens fields for editing again
- VisitAmendmentHistory showing reverse-chronological amendments with field-level diff table
- Auto-save on blur for refraction (500ms debounce) and examination notes (1s debounce)
- Full i18n translations for all new UI (en + vi with proper Vietnamese diacritics)

## Task Commits

Each task was committed atomically:

1. **Task 1: Visit detail page layout, refraction section, and examination notes** - `be2dbc7` (feat)
2. **Task 2: ICD-10 diagnosis combobox, sign-off section, and amendment workflow** - `65d694d` (feat)

## Files Created/Modified
- `frontend/src/features/clinical/components/VisitDetailPage.tsx` - Main scrollable visit page with header, status badge, and 6 collapsible sections
- `frontend/src/features/clinical/components/VisitSection.tsx` - Reusable collapsible card wrapper with Collapsible+Card pattern
- `frontend/src/features/clinical/components/PatientInfoSection.tsx` - Read-only patient/doctor/date/stage display
- `frontend/src/features/clinical/components/RefractionSection.tsx` - Tabs for manifest/auto/cycloplegic with (*) data indicator
- `frontend/src/features/clinical/components/RefractionForm.tsx` - OD/OS side-by-side grid with number inputs, Zod validation, debounced auto-save
- `frontend/src/features/clinical/components/ExaminationNotesSection.tsx` - Free-text textarea with debounced auto-save on blur
- `frontend/src/features/clinical/components/DiagnosisSection.tsx` - Diagnosis list with role/laterality badges, add/remove, Icd10Combobox
- `frontend/src/features/clinical/components/Icd10Combobox.tsx` - Popover+Command combobox with bilingual search, favorites, laterality enforcement
- `frontend/src/features/clinical/components/SignOffSection.tsx` - Sign-off button with AlertDialog, signed badge, amend button
- `frontend/src/features/clinical/components/AmendmentDialog.tsx` - Dialog with mandatory reason textarea, Zod min 10 chars validation
- `frontend/src/features/clinical/components/VisitAmendmentHistory.tsx` - Reverse-chronological amendment list with field-level diff table
- `frontend/src/app/routes/_authenticated/visits/$visitId.tsx` - TanStack Router file-based route
- `frontend/src/app/routeTree.gen.ts` - Auto-generated route tree with visits/$visitId
- `frontend/public/locales/en/clinical.json` - Extended English translations for visit detail
- `frontend/public/locales/vi/clinical.json` - Extended Vietnamese translations with proper diacritics

## Decisions Made
- VisitSection as reusable collapsible card wrapper -- consistent pattern for all 6 visit sections, with defaultOpen prop and headerExtra slot
- RefractionForm uses debounced auto-save on blur (500ms) rather than explicit save button -- reduces friction for rapid data entry
- Icd10Combobox uses Button trigger (not Input) with div wrapper -- follows AllergyForm pattern, avoids click-to-toggle anti-pattern
- Laterality enforcement implemented as inline selector within Popover -- doctor sees OD/OS/OU buttons immediately after selecting a RequiresLaterality code, no separate dialog needed
- ExaminationNotes uses 1s debounce (longer than refraction 500ms) because text editing benefits from less frequent saves
- SignOff uses AlertDialog (non-dismissible) for confirmation -- matches established pattern from session warning dialog

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Visit detail page complete -- ready for Plan 05 (if any remaining clinical phase work)
- All clinical components use hooks from clinical-api.ts established in Plan 03
- Collapsible card pattern (VisitSection) available for reuse in future detail pages
- i18n translations comprehensive for clinical module

## Self-Check: PASSED
