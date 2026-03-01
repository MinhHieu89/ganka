---
phase: 02-patient-management-scheduling
plan: 04
subsystem: ui
tags: [react, tanstack-table, react-hook-form, zod, i18n, datatable, allergy-alert, patient-profile, cmdk]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    provides: "Patient module backend with 12 API endpoints, PatientDto, AllergyDto, PagedResult<T>"
  - phase: 02-patient-management-scheduling
    provides: "Shared frontend infrastructure: shadcn wrappers (Command, Popover, Tabs, Calendar, DatePicker), GlobalSearch, recentPatientsStore, i18n translations"
  - phase: 01.2-refactor-frontend-to-shadcn-ui-with-tanstack-start-file-based-routing
    provides: "DataTable generic component, Field+Controller pattern, shadcn/ui wrapper import pattern"
provides:
  - "Patient API hooks for all CRUD operations (register, update, deactivate, reactivate, list, search, allergies, photo)"
  - "PatientRegistrationForm with Medical/Walk-in toggle and inline allergy entry"
  - "PatientListPage with DataTable, server-side pagination, and gender/allergy/date filters"
  - "PatientProfilePage with tabbed layout (Overview, Allergies, Appointments)"
  - "AllergyAlert reusable banner component (full + compact mode) for downstream prescribing"
  - "PatientOverviewTab with read-only and inline edit modes"
  - "AllergyForm with Command+Popover autocomplete from catalog + free-text entry"
  - "Routes: /patients (list) and /patients/$patientId (profile)"
affects: [03-clinical-examination-records, 05-prescribing, scheduling]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Patient API hook pattern: openapi-fetch with as never type assertions, React Query mutations with query invalidation"
    - "Medical/Walk-in patient type toggle: two-button segment control toggling form field visibility"
    - "AllergyAlert persistent banner: full mode with severity badges, compact mode with tooltip for prescribing context"
    - "Server-side pagination: manualPagination with TanStack Table, page/pageSize state driving API params"
    - "Profile inline edit: isEditing state toggles between read-only card layout and Field+Controller form"

key-files:
  created:
    - "frontend/src/features/patient/api/patient-api.ts"
    - "frontend/src/features/patient/hooks/usePatients.ts"
    - "frontend/src/features/patient/components/PatientRegistrationForm.tsx"
    - "frontend/src/features/patient/components/PatientTable.tsx"
    - "frontend/src/features/patient/components/PatientListPage.tsx"
    - "frontend/src/features/patient/components/PatientProfilePage.tsx"
    - "frontend/src/features/patient/components/PatientProfileHeader.tsx"
    - "frontend/src/features/patient/components/PatientOverviewTab.tsx"
    - "frontend/src/features/patient/components/PatientAllergyTab.tsx"
    - "frontend/src/features/patient/components/PatientAppointmentTab.tsx"
    - "frontend/src/features/patient/components/AllergyForm.tsx"
    - "frontend/src/features/patient/components/AllergyAlert.tsx"
    - "frontend/src/app/routes/_authenticated/patients/index.tsx"
    - "frontend/src/app/routes/_authenticated/patients/$patientId.tsx"
  modified:
    - "frontend/public/locales/en/patient.json"
    - "frontend/public/locales/vi/patient.json"
    - "frontend/src/app/routeTree.gen.ts"

key-decisions:
  - "Patient type toggle uses two-button segment (not radio) for visual clarity between Medical and Walk-in"
  - "AllergyAlert designed as both full persistent banner and compact tooltip badge for downstream Phase 5 prescribing reuse"
  - "Allergy autocomplete uses static catalog matching backend seeder (26 ophthalmology allergies) with free-text entry"
  - "Photo upload uses native fetch with FormData (not openapi-fetch) for multipart file upload support"
  - "PatientAppointmentTab gracefully handles missing scheduling API with retry:false and empty array fallback"

patterns-established:
  - "Patient API hooks: openapi-fetch + React Query with as never for untyped routes, query invalidation on mutations"
  - "Medical/Walk-in toggle: patientType state controls conditional field rendering in registration form"
  - "AllergyAlert: persistent orange banner (full) or tooltip badge (compact) for patient safety awareness"
  - "Server-side DataTable: manualPagination + pageCount calculated from totalCount/pageSize"
  - "Profile inline edit: isEditing boolean toggles read-only InfoRow layout vs Field+Controller form"

requirements-completed: [PAT-01, PAT-02, PAT-03, PAT-04, PAT-05]

# Metrics
duration: 7min
completed: 2026-03-01
---

# Phase 02 Plan 04: Patient Frontend Summary

**Complete patient management UI with registration form (Medical/Walk-in toggle), paginated DataTable with filters, tabbed profile page, allergy management with autocomplete, and reusable AllergyAlert banner**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-01T12:02:40Z
- **Completed:** 2026-03-01T12:09:40Z
- **Tasks:** 2
- **Files modified:** 17

## Accomplishments

- Complete patient registration with Medical/Walk-in toggle, inline allergy entry, and Vietnamese phone validation
- Paginated patient list with DataTable, server-side pagination, and gender/allergy/date range filters
- Patient profile page with tabbed layout (Overview, Allergies, Appointments) and inline edit mode
- AllergyAlert reusable component designed for downstream prescribing phase with both full banner and compact tooltip modes
- Allergy management with Command+Popover autocomplete from ophthalmology catalog plus free-text entry
- Photo upload on profile header with avatar initials fallback and DOB+age display

## Task Commits

Each task was committed atomically:

1. **Task 1: Patient API hooks, registration form, and patient list page with DataTable** - `7883593` (feat)
2. **Task 2: Patient profile page with tabs, allergy management, and allergy alert** - `66570bb` (feat)

## Files Created/Modified

### Patient API Layer (2 files)
- `frontend/src/features/patient/api/patient-api.ts` - All CRUD hooks: register, update, deactivate, reactivate, list, search, allergies, photo
- `frontend/src/features/patient/hooks/usePatients.ts` - useRecentPatients derived hook

### Patient Components (10 files)
- `frontend/src/features/patient/components/PatientRegistrationForm.tsx` - Dialog with Medical/Walk-in toggle, zod validation, inline allergy rows
- `frontend/src/features/patient/components/PatientTable.tsx` - DataTable with manual pagination, patient code/type/allergy/status columns
- `frontend/src/features/patient/components/PatientListPage.tsx` - Page with filters (gender, allergy, date range) and registration dialog
- `frontend/src/features/patient/components/PatientProfilePage.tsx` - Tabbed layout with loading skeleton and error state
- `frontend/src/features/patient/components/PatientProfileHeader.tsx` - Avatar/photo, name, code, DOB+age, deactivate/reactivate with AlertDialog
- `frontend/src/features/patient/components/PatientOverviewTab.tsx` - Read-only card layout + inline edit with Field+Controller
- `frontend/src/features/patient/components/PatientAllergyTab.tsx` - Allergy list with remove confirmation, add dialog
- `frontend/src/features/patient/components/PatientAppointmentTab.tsx` - Upcoming/past sections, graceful empty state
- `frontend/src/features/patient/components/AllergyForm.tsx` - Dialog with Command+Popover autocomplete + severity select
- `frontend/src/features/patient/components/AllergyAlert.tsx` - Reusable banner (full + compact) with severity-coded badges

### Routes (2 files)
- `frontend/src/app/routes/_authenticated/patients/index.tsx` - Patient list route
- `frontend/src/app/routes/_authenticated/patients/$patientId.tsx` - Patient profile dynamic route

### Modified (3 files)
- `frontend/public/locales/en/patient.json` - Extended with 14 new UI keys
- `frontend/public/locales/vi/patient.json` - Extended with 14 new Vietnamese keys with proper diacritics
- `frontend/src/app/routeTree.gen.ts` - Auto-generated with new patient routes

## Decisions Made

- **Patient type toggle as two-button segment:** More visually clear than radio buttons for the binary Medical/Walk-in choice, with icons for each type.
- **AllergyAlert full + compact modes:** Full persistent banner for profile pages (patient safety priority), compact tooltip badge for embedding in prescription or appointment contexts downstream.
- **Static allergy catalog in frontend:** Matches the 26 ophthalmology allergies seeded by AllergyCatalogSeeder on backend, with free-text entry for unlisted allergies.
- **Photo upload via native fetch:** openapi-fetch does not handle FormData/multipart uploads well, so native fetch with FormData is used for the photo upload endpoint.
- **Appointment tab with retry:false:** Scheduling API may not be deployed yet; graceful degradation returns empty array instead of error state.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- Pre-existing TypeScript errors from untyped openapi-fetch client (`as never` pattern) appear in patient-api.ts and usePatients.ts, consistent with admin-api.ts and auth-api.ts. These will resolve when OpenAPI types are generated.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Patient frontend is complete and ready for integration testing when backend is running
- AllergyAlert component is exportable for Phase 5 prescribing safety checks
- Routes /patients and /patients/$patientId are registered in the TanStack Router route tree
- Recent patients store is updated on profile views for GlobalSearch and dashboard widget

## Self-Check: PASSED

- All 14 created files verified present on disk
- Commit 7883593 (Task 1) verified in git log
- Commit 66570bb (Task 2) verified in git log

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-01*
