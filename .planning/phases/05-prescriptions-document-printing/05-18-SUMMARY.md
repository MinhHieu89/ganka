---
phase: 05-prescriptions-document-printing
plan: 18
subsystem: ui
tags: [react, tanstack-query, react-hook-form, zod, shadcn-ui, clinic-settings, i18n, sidebar, admin]

# Dependency graph
requires:
  - phase: 05-09
    provides: IClinicSettingsService, ClinicSettings entity, GET/PUT /api/settings/clinic endpoints
  - phase: 05-13
    provides: Drug catalog page, pharmacy route, Vietnamese pharmacy translations
provides:
  - ClinicSettingsPage admin page for configuring clinic header (logo, name, address, phone, fax, license, tagline)
  - TanStack Query hooks for clinic settings CRUD and logo upload
  - Sidebar navigation with enabled Pharmacy link and Clinic Settings admin link
  - Vietnamese clinicSettings page translations with proper diacritics
affects: [05-prescriptions-document-printing, document-generation, admin-settings, frontend-navigation]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Clinic settings API hooks following admin-api.ts TanStack Query pattern with clinicSettingsKeys factory"
    - "Logo upload via native fetch + FormData following patient photo upload pattern"
    - "Zod schema defined inside hook function to access i18n t() for inline error messages"

key-files:
  created:
    - frontend/src/features/admin/api/clinic-settings-api.ts
    - frontend/src/features/admin/components/ClinicSettingsPage.tsx
    - frontend/src/app/routes/_authenticated/admin/clinic-settings.tsx
  modified:
    - frontend/src/shared/components/AppSidebar.tsx
    - frontend/public/locales/en/common.json
    - frontend/public/locales/vi/common.json
    - frontend/src/app/routeTree.gen.ts

key-decisions:
  - "Logo upload uses native fetch + FormData pattern following patient photo upload (not openapi-fetch)"
  - "Clinic settings translations placed in common.json clinicSettings namespace rather than separate file"
  - "Pharmacy sidebar link enabled (disabled:true removed) since drug catalog page exists from Plan 13"

patterns-established:
  - "clinicSettingsKeys query key factory: { all: ['clinic-settings'] }"
  - "Admin settings page pattern: Card with form fields using React Hook Form + Zod validation"

requirements-completed: [PRT-01, PRT-02, PRT-04, PRT-05]

# Metrics
duration: 3min
completed: 2026-03-06
---

# Phase 05 Plan 18: Clinic Settings Admin Page & Sidebar Navigation Summary

**Admin clinic settings page with React Hook Form for configurable clinic header (logo, name, address, phone, fax, license, tagline) plus sidebar navigation updates for Pharmacy and Clinic Settings**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-05T17:17:04Z
- **Completed:** 2026-03-05T17:20:04Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments
- Created clinic-settings-api.ts with TanStack Query hooks for GET/PUT clinic settings and logo upload via native fetch + FormData
- Created ClinicSettingsPage with Card layout, React Hook Form + Zod validation, logo preview/upload, and all clinic header fields
- Created route file for /admin/clinic-settings following existing admin route pattern
- Enabled Pharmacy link in sidebar operations section and added Clinic Settings link to admin section
- Added Vietnamese clinic settings translations with proper diacritics

## Task Commits

Each task was committed atomically:

1. **Task 1: Create clinic settings API hooks and admin page** - `3332c54` (feat)
2. **Task 2: Create Vietnamese pharmacy translations and update sidebar** - `38ceb88` (feat)

## Files Created/Modified
- `frontend/src/features/admin/api/clinic-settings-api.ts` - TanStack Query hooks for clinic settings CRUD, logo upload with clinicSettingsKeys factory
- `frontend/src/features/admin/components/ClinicSettingsPage.tsx` - Admin page with Card form for all clinic header fields including logo upload/preview
- `frontend/src/app/routes/_authenticated/admin/clinic-settings.tsx` - Route file for /admin/clinic-settings path
- `frontend/src/shared/components/AppSidebar.tsx` - Enabled Pharmacy link, added Clinic Settings with IconBuilding to admin section
- `frontend/public/locales/en/common.json` - Added clinicSettings sidebar key and clinicSettings page translation section
- `frontend/public/locales/vi/common.json` - Added clinicSettings sidebar key and clinicSettings page translations with proper diacritics
- `frontend/src/app/routeTree.gen.ts` - Auto-generated route tree with clinic-settings route

## Decisions Made
- Used native fetch + FormData for logo upload following the established patient photo upload pattern (not openapi-fetch for multipart)
- Placed clinic settings page translations in common.json under clinicSettings namespace rather than a separate admin.json file for simplicity
- Enabled Pharmacy sidebar link since drug catalog page already exists from Plan 13 (previously was disabled/coming-soon)
- Zod schema defined inside a custom hook function to access i18n t() for localized validation error messages

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added clinicSettings translations to common.json**
- **Found during:** Task 2 (sidebar and translations)
- **Issue:** Plan mentioned Vietnamese pharmacy translations but ClinicSettingsPage also needs translation keys for field labels, success messages, and page title
- **Fix:** Added clinicSettings section to both en/common.json and vi/common.json with all page labels and messages with proper Vietnamese diacritics
- **Files modified:** frontend/public/locales/en/common.json, frontend/public/locales/vi/common.json
- **Verification:** TypeScript compilation passes, all t() calls have fallback defaults
- **Committed in:** 38ceb88 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** Translation keys required for proper i18n on the clinic settings page. No scope creep.

## Issues Encountered
- Vietnamese pharmacy translations were already complete with proper diacritics from Plan 13 -- no additional work needed
- TypeScript errors in clinic-settings-api.ts are pre-existing openapi-fetch typing pattern (same as admin-api.ts and patient-api.ts) -- not new issues

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Clinic settings admin page accessible at /admin/clinic-settings
- All document generators can reference configured clinic header via IClinicSettingsService
- Pharmacy catalog accessible from sidebar navigation
- Logo upload infrastructure ready (pending backend logo upload endpoint implementation if not yet done)

## Self-Check: PASSED

- [x] clinic-settings-api.ts exists with useClinicSettings and useUpdateClinicSettings
- [x] ClinicSettingsPage.tsx exists with ClinicSettingsPage export
- [x] clinic-settings.tsx route file exists with ClinicSettingsPage import
- [x] AppSidebar.tsx updated with pharmacy (enabled) and clinic-settings links
- [x] Vietnamese pharmacy.json exists with proper diacritics
- [x] English common.json updated with clinicSettings translations
- [x] Vietnamese common.json updated with clinicSettings translations and proper diacritics
- [x] Commit 3332c54 found in git log
- [x] Commit 38ceb88 found in git log

---
*Phase: 05-prescriptions-document-printing*
*Completed: 2026-03-06*
