---
phase: 02-patient-management-scheduling
plan: 03
subsystem: ui
tags: [fullcalendar, cmdk, shadcn, i18n, zustand, react-day-picker, global-search]

# Dependency graph
requires:
  - phase: 01.2-refactor-frontend-to-shadcn-ui-with-tanstack-start-file-based-routing
    provides: shadcn/ui wrapper pattern, SiteHeader, AppSidebar, dashboard, i18n setup
provides:
  - FullCalendar library installed and importable for appointment calendar
  - Command (cmdk) + Popover shadcn wrappers for autocomplete search patterns
  - Tabs + Calendar shadcn wrappers for patient profile and date picking
  - DatePicker form component combining Popover + Calendar + Button
  - GlobalSearch component in SiteHeader with recent patients and search
  - recentPatientsStore Zustand store with localStorage persistence
  - usePatientSearch React Query hook for patient search API
  - Patient and scheduling i18n translation files (en/vi)
  - Activated sidebar navigation for Patients and Appointments
  - Dashboard recent patients widget
affects: [02-04-patient-frontend, 02-05-scheduling-frontend]

# Tech tracking
tech-stack:
  added: ["@fullcalendar/core", "@fullcalendar/react", "@fullcalendar/timegrid", "@fullcalendar/daygrid", "@fullcalendar/interaction", "cmdk", "@radix-ui/react-popover", "@radix-ui/react-tabs", "react-day-picker", "date-fns"]
  patterns: [command-popover-search, zustand-persist-store, date-picker-form-component]

key-files:
  created:
    - frontend/src/shared/components/ui/command.tsx
    - frontend/src/shared/components/ui/calendar.tsx
    - frontend/src/shared/components/ui/popover.tsx
    - frontend/src/shared/components/ui/tabs.tsx
    - frontend/src/shared/components/Command.tsx
    - frontend/src/shared/components/Popover.tsx
    - frontend/src/shared/components/Tabs.tsx
    - frontend/src/shared/components/DatePicker.tsx
    - frontend/src/shared/components/GlobalSearch.tsx
    - frontend/src/shared/stores/recentPatientsStore.ts
    - frontend/src/features/patient/hooks/usePatientSearch.ts
    - frontend/public/locales/en/patient.json
    - frontend/public/locales/vi/patient.json
    - frontend/public/locales/en/scheduling.json
    - frontend/public/locales/vi/scheduling.json
  modified:
    - frontend/package.json
    - frontend/src/shared/components/ui/dialog.tsx
    - frontend/src/shared/components/SiteHeader.tsx
    - frontend/src/shared/components/AppSidebar.tsx
    - frontend/src/app/routes/_authenticated/dashboard.tsx
    - frontend/public/locales/en/common.json
    - frontend/public/locales/vi/common.json
    - frontend/src/shared/i18n/i18n.ts

key-decisions:
  - "DatePicker uses date-fns format with Vietnamese dd/MM/yyyy locale support"
  - "GlobalSearch uses type assertions for /patients routes not yet created (Plan 04)"
  - "usePatientSearch hook placed in features/patient/hooks/ with openapi-fetch and React Query"
  - "Restored hideCloseButton prop on dialog.tsx after shadcn overwrite"

patterns-established:
  - "Command+Popover search: GlobalSearch demonstrates cmdk+popover autocomplete pattern with recent items and deferred search"
  - "Zustand persist store: recentPatientsStore shows persist middleware with localStorage for client-side entity caching"
  - "DatePicker form component: combines Popover + Calendar + Button with locale-aware date formatting"

requirements-completed: [PAT-04]

# Metrics
duration: 8min
completed: 2026-03-01
---

# Phase 02 Plan 03: Shared Frontend Infrastructure Summary

**FullCalendar + cmdk + shadcn wrappers installed, GlobalSearch with recent patients in SiteHeader, dashboard widget, and patient/scheduling i18n translations for en/vi**

## Performance

- **Duration:** 8 min
- **Started:** 2026-03-01T11:41:59Z
- **Completed:** 2026-03-01T11:49:58Z
- **Tasks:** 2
- **Files modified:** 24

## Accomplishments
- Installed 10 new npm packages (FullCalendar 5 packages, cmdk, radix-popover, radix-tabs, react-day-picker, date-fns) and generated 4 shadcn/ui primitives with 4 wrapper components following the established re-export pattern
- Built GlobalSearch component with Command+Popover pattern, Ctrl+K keyboard shortcut, recent patients on focus, and debounced search via usePatientSearch hook
- Created recentPatientsStore with Zustand persist middleware for localStorage-backed recent patients tracking (deduplication, max 10)
- Added Recent Patients widget to dashboard with empty state and per-patient links
- Activated Patients and Appointments sidebar nav items (no longer disabled with "Coming soon" tooltip)
- Created comprehensive i18n translation files for patient and scheduling domains in both English and Vietnamese with proper diacritics

## Task Commits

Each task was committed atomically:

1. **Task 1: Install dependencies and create shadcn wrapper components** - `f98d752` (feat)
2. **Task 2: GlobalSearch, recent patients store, dashboard widget, sidebar activation, i18n** - `5276693` (feat)

## Files Created/Modified
- `frontend/package.json` - Added 10 new dependencies
- `frontend/src/shared/components/ui/command.tsx` - shadcn Command primitive (cmdk wrapper)
- `frontend/src/shared/components/ui/popover.tsx` - shadcn Popover primitive (radix)
- `frontend/src/shared/components/ui/tabs.tsx` - shadcn Tabs primitive (radix)
- `frontend/src/shared/components/ui/calendar.tsx` - shadcn Calendar primitive (react-day-picker)
- `frontend/src/shared/components/ui/dialog.tsx` - Restored hideCloseButton prop after shadcn overwrite
- `frontend/src/shared/components/Command.tsx` - Re-export wrapper for Command
- `frontend/src/shared/components/Popover.tsx` - Re-export wrapper for Popover
- `frontend/src/shared/components/Tabs.tsx` - Re-export wrapper for Tabs
- `frontend/src/shared/components/DatePicker.tsx` - Form-friendly date picker with locale support
- `frontend/src/shared/components/GlobalSearch.tsx` - Global search with recent patients and API search
- `frontend/src/shared/stores/recentPatientsStore.ts` - Zustand store with localStorage persistence
- `frontend/src/features/patient/hooks/usePatientSearch.ts` - React Query hook for patient search API
- `frontend/src/shared/components/SiteHeader.tsx` - Added GlobalSearch between breadcrumbs and controls
- `frontend/src/shared/components/AppSidebar.tsx` - Activated Patients and Appointments nav items
- `frontend/src/app/routes/_authenticated/dashboard.tsx` - Added Recent Patients widget
- `frontend/public/locales/en/patient.json` - English patient translations (46 keys)
- `frontend/public/locales/vi/patient.json` - Vietnamese patient translations with diacritics
- `frontend/public/locales/en/scheduling.json` - English scheduling translations (50+ keys)
- `frontend/public/locales/vi/scheduling.json` - Vietnamese scheduling translations with diacritics
- `frontend/public/locales/en/common.json` - Added search.noResults, search.placeholder
- `frontend/public/locales/vi/common.json` - Added search keys in Vietnamese
- `frontend/src/shared/i18n/i18n.ts` - Registered patient and scheduling namespaces

## Decisions Made
- Used date-fns Locale type import for DatePicker Vietnamese locale formatting (dd/MM/yyyy vs MM/dd/yyyy based on language)
- Used type assertions (`as string`, `as never`) for TanStack Router navigation to `/patients` and `/patients/$patientId` routes that don't exist yet -- they will be created in Plan 04
- Placed usePatientSearch in `features/patient/hooks/` following the established feature module pattern
- Restored hideCloseButton custom prop on dialog.tsx that was lost during shadcn CLI overwrite
- Added PopoverAnchor export to popover.tsx primitive for completeness

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Restored hideCloseButton prop on dialog.tsx**
- **Found during:** Task 1 (shadcn primitive generation)
- **Issue:** Running `npx shadcn@latest add command popover tabs calendar --overwrite` overwrote dialog.tsx and removed the custom hideCloseButton prop added in Phase 01.2
- **Fix:** Manually restored the hideCloseButton prop and conditional close button rendering
- **Files modified:** frontend/src/shared/components/ui/dialog.tsx
- **Verification:** TypeScript compilation passes, prop matches git HEAD version
- **Committed in:** f98d752 (Task 1 commit)

**2. [Rule 3 - Blocking] Fixed TypeScript Locale type import for DatePicker**
- **Found during:** Task 1 (DatePicker creation)
- **Issue:** `Locale` type used in `localeMap` Record type was not imported, causing TS2304
- **Fix:** Added `type Locale` import from date-fns
- **Files modified:** frontend/src/shared/components/DatePicker.tsx
- **Verification:** TypeScript compilation passes
- **Committed in:** f98d752 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (1 bug, 1 blocking)
**Impact on plan:** Both auto-fixes necessary for correctness. No scope creep.

## Issues Encountered
- shadcn CLI `--overwrite` flag was needed because existing dialog.tsx conflicted, which caused the hideCloseButton customization to be lost (resolved by manual restoration)
- TanStack Router typed routes cause type errors for `/patients` and `/patients/$patientId` since route files don't exist yet (resolved with type assertions that will be removed in Plan 04)

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All shared frontend infrastructure is ready for Plan 04 (Patient Frontend) and Plan 05 (Scheduling Frontend)
- FullCalendar packages are installed and importable
- GlobalSearch component is integrated in SiteHeader and ready to search patients once the backend API exists
- i18n translations cover all patient and scheduling UI text in both languages
- Recent patients store is functional and persists across sessions

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-01*
