---
phase: 02-patient-management-scheduling
plan: 07
subsystem: ui, api
tags: [react, tanstack-router, react-hook-form, efcore, patient, breadcrumb, pagination, search]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    provides: Patient CRUD endpoints, registration form, overview tab, patient table, breadcrumb system
provides:
  - Working patient registration with correct API response extraction
  - Patient inline edit with DOB display and concurrency handling
  - Always-visible pagination on patient list
  - Patient name in breadcrumbs via recentPatientsStore
  - Backend patient list search with Vietnamese collation support
affects: [02-patient-management-scheduling]

# Tech tracking
tech-stack:
  added: [Microsoft.EntityFrameworkCore in Patient.Application for DbUpdateConcurrencyException]
  patterns: [useEffect form reset on prop change, recentPatientsStore breadcrumb lookup, debounced search, AllergyRow autocomplete with bilingual catalog]

key-files:
  created: []
  modified:
    - frontend/src/features/patient/api/patient-api.ts
    - frontend/src/shared/components/ui/dialog.tsx
    - frontend/src/features/patient/components/PatientRegistrationForm.tsx
    - frontend/src/features/patient/components/PatientTable.tsx
    - frontend/src/shared/components/SiteHeader.tsx
    - frontend/src/features/patient/components/PatientOverviewTab.tsx
    - frontend/src/features/patient/components/PatientListPage.tsx
    - frontend/src/shared/components/DatePicker.tsx
    - backend/src/Modules/Patient/Patient.Application/Features/GetPatientList.cs
    - backend/src/Modules/Patient/Patient.Application/Interfaces/IPatientRepository.cs
    - backend/src/Modules/Patient/Patient.Infrastructure/Repositories/PatientRepository.cs
    - backend/src/Modules/Patient/Patient.Application/Features/UpdatePatient.cs
    - backend/src/Modules/Patient/Patient.Application/Patient.Application.csproj

key-decisions:
  - "Microsoft.EntityFrameworkCore added to Patient.Application for DbUpdateConcurrencyException (follows Scheduling.Application precedent)"
  - "recentPatientsStore used for breadcrumb patient name lookup (no extra API call needed)"
  - "AllergyRow refactored as separate component with Popover/Command autocomplete storing English canonical key"
  - "Vietnamese_CI_AI collation for patient name search in backend"

patterns-established:
  - "useEffect form reset on patient.id change for syncing async data into react-hook-form"
  - "recentPatientsStore breadcrumb lookup pattern for /patients/{uuid} routes"

requirements-completed: [PAT-01, PAT-02, PAT-03]

# Metrics
duration: 9min
completed: 2026-03-02
---

# Phase 02 Plan 07: Patient Module Gap Closure Summary

**Fixed patient registration 404, inline edit 500, pagination visibility, breadcrumb names, and backend search with Vietnamese collation**

## Performance

- **Duration:** 9 min
- **Started:** 2026-03-02T12:16:37Z
- **Completed:** 2026-03-02T12:25:45Z
- **Tasks:** 2
- **Files modified:** 16

## Accomplishments
- Patient registration form now correctly extracts `.Id` from backend response, eliminating the 404 navigation to `/patients/[object Object]`
- Patient inline edit properly resets form on patient data load (DOB displays correctly) and strips patientId from PUT body
- Pagination row always visible on patient list with disabled buttons when single page
- Breadcrumb shows patient full name (from recentPatientsStore) instead of UUID on patient profile pages
- Backend patient list search with Vietnamese diacritics-insensitive collation, phone prefix, and patient code matching
- UpdatePatientHandler catches DbUpdateConcurrencyException for optimistic concurrency conflict handling

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix patient registration 404 and dialog layout (UAT Tests 2, 3, 5)** - `e2cf346` (fix)
2. **Task 2: Fix patient inline edit DOB and save 500 error (UAT Test 6)** - `00202b8` (fix)

**Plan metadata:** `eebbe2a` (docs: complete plan)

## Files Created/Modified
- `frontend/src/features/patient/api/patient-api.ts` - registerPatient extracts .Id; updatePatient strips patientId from body
- `frontend/src/shared/components/ui/dialog.tsx` - Removed grid from DialogContent base class for flex layout compatibility
- `frontend/src/features/patient/components/PatientRegistrationForm.tsx` - Refactored AllergyRow with autocomplete, Tabs for patient type, sticky header/footer layout
- `frontend/src/features/patient/components/PatientTable.tsx` - Pagination always visible (no pageCount > 1 conditional)
- `frontend/src/shared/components/SiteHeader.tsx` - Breadcrumb patient name lookup via recentPatientsStore
- `frontend/src/features/patient/components/PatientOverviewTab.tsx` - useEffect form reset on patient.id change, relaxed phone regex
- `frontend/src/features/patient/components/PatientListPage.tsx` - Debounced search with 300ms delay, reset pagination on search
- `frontend/src/shared/components/DatePicker.tsx` - Dropdown month/year caption, configurable fromDate/toDate
- `frontend/public/locales/en/common.json` - Added sidebar.detail translation
- `frontend/public/locales/vi/common.json` - Added sidebar.detail translation (Chi tiet)
- `backend/src/Modules/Patient/Patient.Application/Features/GetPatientList.cs` - Search param in correct position
- `backend/src/Modules/Patient/Patient.Application/Interfaces/IPatientRepository.cs` - search before CancellationToken
- `backend/src/Modules/Patient/Patient.Infrastructure/Repositories/PatientRepository.cs` - Search implementation with Vietnamese_CI_AI collation
- `backend/src/Modules/Patient/Patient.Application/Features/UpdatePatient.cs` - DbUpdateConcurrencyException catch with conflict error
- `backend/src/Modules/Patient/Patient.Application/Patient.Application.csproj` - Added Microsoft.EntityFrameworkCore reference

## Decisions Made
- Microsoft.EntityFrameworkCore added to Patient.Application.csproj for DbUpdateConcurrencyException type access (follows established Scheduling.Application precedent from Phase 02)
- recentPatientsStore used for breadcrumb patient name lookup -- avoids extra API call, leverages existing patient visit tracking
- AllergyRow refactored as separate component with Popover/Command autocomplete, always stores English canonical key for backend consistency
- Vietnamese_CI_AI collation used for patient name search (accent-insensitive, case-insensitive)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added Microsoft.EntityFrameworkCore to Patient.Application.csproj**
- **Found during:** Task 2 (UpdatePatient concurrency handling)
- **Issue:** Plan specified adding `using Microsoft.EntityFrameworkCore` but Patient.Application.csproj had no EF Core package reference, causing CS0234 build error
- **Fix:** Added `<PackageReference Include="Microsoft.EntityFrameworkCore" />` to Patient.Application.csproj (follows Scheduling.Application pattern)
- **Files modified:** backend/src/Modules/Patient/Patient.Application/Patient.Application.csproj
- **Verification:** `dotnet build` passes with 0 errors
- **Committed in:** 00202b8 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Auto-fix was necessary for build to pass. No scope creep.

## Issues Encountered
- Backend build initially failed due to file locks from a running Bootstrapper process. Killed the process and rebuild succeeded.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- UAT Tests 2, 3, 5, and 6 gaps are now addressed
- Patient registration, inline edit, pagination, and breadcrumb functionality complete
- Ready for remaining gap closure plan (02-09) to complete Phase 02

## Self-Check: PASSED

All 16 created/modified files verified present. Both task commits (e2cf346, 00202b8) verified in git log.

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-02*
