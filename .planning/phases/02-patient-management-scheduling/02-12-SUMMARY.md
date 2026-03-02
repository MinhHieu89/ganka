---
phase: 02-patient-management-scheduling
plan: 12
subsystem: ui, api, auth
tags: [shadcn-ui, dialog, calendar, 401-interceptor, token-refresh, allergy, rounded-corners]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    provides: "Patient registration, allergy management, booking status check, API client"
provides:
  - "DialogContent with flex flex-col for proper gap-4 spacing across all dialogs"
  - "401 response interceptor with silent token refresh and automatic retry"
  - "Patient registration returns correct camelCase id for redirect"
  - "Allergy severity sent as integer to backend (no 500 error)"
  - "Rounded corners on BookingStatusCheck and AllergyAlert"
  - "Auth error vs not-found differentiation on PatientProfilePage"
affects: [02-13, 02-14, all-future-frontend-plans]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "401 interceptor with shared refresh promise to prevent concurrent refresh attempts"
    - "Auth error differentiation pattern in error states"

key-files:
  created: []
  modified:
    - "frontend/src/shared/components/ui/dialog.tsx"
    - "frontend/src/shared/lib/api-client.ts"
    - "frontend/src/features/patient/api/patient-api.ts"
    - "frontend/src/features/patient/components/PatientProfilePage.tsx"
    - "frontend/src/features/booking/components/BookingStatusCheck.tsx"
    - "frontend/src/features/patient/components/AllergyAlert.tsx"
    - "frontend/public/locales/en/common.json"
    - "frontend/public/locales/vi/common.json"

key-decisions:
  - "Calendar already matched official shadcn/ui -- skipped CLI overwrite to avoid risk of breaking existing imports"
  - "AlertDialog uses grid (not flex) which already handles gap-4 correctly -- no fix needed"
  - "401 interceptor skips refresh endpoint to avoid infinite loop"
  - "Retry uses new Request constructor instead of request.clone() for reliability with already-consumed bodies"

patterns-established:
  - "401 interceptor pattern: shared refreshPromise prevents concurrent refresh, skips auth endpoints"
  - "Auth error vs data error differentiation in page error states"

requirements-completed: [PAT-01, PAT-05, AUTH-04, SCH-06, UI-01]

# Metrics
duration: 4min
completed: 2026-03-02
---

# Phase 02 Plan 12: Shared Frontend Infrastructure Fixes Summary

**Dialog flex spacing, 401 token refresh interceptor, patient registration camelCase fix, allergy severity integer conversion, and rounded corners on booking/allergy components**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-02T15:14:11Z
- **Completed:** 2026-03-02T15:18:37Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- All dialogs now have proper spacing between header, body, and footer via flex flex-col + gap-4
- 401 responses are silently intercepted, token refreshed via HTTP-only cookie, and original request retried
- Patient registration redirect uses correct camelCase .id from backend response
- Allergy severity is converted from string to integer before sending to backend (no more 500 errors)
- BookingStatusCheck has rounded corners on all 5 container divs
- AllergyAlert full banner has rounded corners
- PatientProfilePage differentiates auth errors from not-found errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Replace calendar.tsx, fix dialog spacing, add rounded corners** - `f4f96be` (feat)
2. **Task 2: Add 401 interceptor, fix patient registration redirect, fix allergy severity** - `ea82824` (feat)

## Files Created/Modified
- `frontend/src/shared/components/ui/dialog.tsx` - Added flex flex-col to DialogContent for gap-4 spacing
- `frontend/src/shared/lib/api-client.ts` - Added 401 onResponse interceptor with silent refresh and retry
- `frontend/src/features/patient/api/patient-api.ts` - Fixed .Id to .id, added severityToInt conversion in addAllergy
- `frontend/src/features/patient/components/PatientProfilePage.tsx` - Auth vs not-found error differentiation
- `frontend/src/features/booking/components/BookingStatusCheck.tsx` - Added rounded-lg to 5 containers
- `frontend/src/features/patient/components/AllergyAlert.tsx` - Added rounded-lg to banner container
- `frontend/public/locales/en/common.json` - Added sessionExpired, sessionExpiredDetail keys
- `frontend/public/locales/vi/common.json` - Added sessionExpired, sessionExpiredDetail keys (Vietnamese)

## Decisions Made
- Calendar component already matched official shadcn/ui version (exports Calendar + CalendarDayButton, has buttonVariant prop, getDefaultClassNames) -- skipped CLI overwrite to avoid breaking DatePicker imports
- AlertDialog uses `grid` which already handles gap-4 correctly, unlike Dialog's missing display property -- no fix needed
- 401 interceptor explicitly skips /api/auth/refresh endpoint to prevent infinite loop when refresh token itself is expired
- Used new Request() constructor for retry instead of request.clone() to handle cases where request body was already consumed by openapi-fetch

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added translation keys for auth error differentiation**
- **Found during:** Task 2 (PatientProfilePage error state)
- **Issue:** The sessionExpired and sessionExpiredDetail translation keys did not exist in common.json
- **Fix:** Added keys to both en/common.json and vi/common.json with proper Vietnamese diacritics
- **Files modified:** frontend/public/locales/en/common.json, frontend/public/locales/vi/common.json
- **Verification:** TypeScript compilation passes, keys referenced correctly
- **Committed in:** ea82824 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** Translation keys were necessary for the error differentiation feature to work. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All shared frontend infrastructure fixes complete
- Dialog spacing, 401 resilience, and API fixes unblock UAT tests 2, 6, 7, 11, 12, 13
- Plans 13 and 14 can proceed with these foundations in place

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-02*

## Self-Check: PASSED
- All 8 modified files verified present on disk
- Both task commits (f4f96be, ea82824) verified in git log
