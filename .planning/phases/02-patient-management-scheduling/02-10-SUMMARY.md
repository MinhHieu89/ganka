---
phase: 02-patient-management-scheduling
plan: 10
subsystem: api, ui, domain
tags: [field-validation, tdd, xunit, react-query, i18n, shadcn-alert]

# Dependency graph
requires:
  - phase: 02-01
    provides: Patient entity with Address/Cccd nullable fields
  - phase: 02-07
    provides: Patient inline edit, overview tab with edit toggle
provides:
  - FieldRequirementContext enum for downstream context-based field enforcement
  - PatientFieldValidator pure domain service (Validate method)
  - GET /api/patients/{id}/field-validation endpoint
  - PatientFieldWarning component for patient profile UI
  - shadcn Alert component wrapper
affects: [phase-03-clinical-examination, referral-workflows, legal-export, so-y-te-reporting]

# Tech tracking
tech-stack:
  added: [shadcn/ui Alert component]
  patterns: [context-based field validation, domain service for validation logic, field warning indicator pattern]

key-files:
  created:
    - backend/src/Modules/Patient/Patient.Domain/Enums/FieldRequirementContext.cs
    - backend/src/Modules/Patient/Patient.Domain/Services/PatientFieldValidator.cs
    - backend/src/Modules/Patient/Patient.Application/Features/ValidatePatientFields.cs
    - backend/tests/Patient.Unit.Tests/Patient.Unit.Tests.csproj
    - backend/tests/Patient.Unit.Tests/PatientFieldValidatorTests.cs
    - frontend/src/features/patient/components/PatientFieldWarning.tsx
    - frontend/src/shared/components/Alert.tsx
    - frontend/src/shared/components/ui/alert.tsx
  modified:
    - backend/src/Modules/Patient/Patient.Presentation/PatientApiEndpoints.cs
    - backend/Ganka28.slnx
    - frontend/src/features/patient/api/patient-api.ts
    - frontend/src/features/patient/components/PatientOverviewTab.tsx
    - frontend/src/features/booking/components/BookingForm.tsx
    - frontend/public/locales/en/patient.json
    - frontend/public/locales/vi/patient.json

key-decisions:
  - "PatientFieldValidationResult records placed in Domain.Services (not Contracts) because Domain cannot reference Contracts"
  - "Referral context used as strictest common downstream context for the validation endpoint"

patterns-established:
  - "Context-based field validation: PatientFieldValidator.Validate(address, cccd, context) for downstream enforcement"
  - "Field warning indicator: PatientFieldWarning component pattern for profile-level missing data alerts"

requirements-completed: [PAT-03, PAT-01, PAT-02, PAT-04, PAT-05, SCH-01, SCH-02, SCH-03, SCH-04, SCH-05, SCH-06]

# Metrics
duration: 9min
completed: 2026-03-02
---

# Phase 02 Plan 10: PAT-03 Field Validation Infrastructure Summary

**PatientFieldValidator domain service with TDD (8 tests), field-validation endpoint, frontend warning indicators on patient profile, and BookingForm placeholder cleanup**

## Performance

- **Duration:** 9 min
- **Started:** 2026-03-02T13:23:36Z
- **Completed:** 2026-03-02T13:32:36Z
- **Tasks:** 2
- **Files modified:** 15

## Accomplishments
- PatientFieldValidator pure domain service with 8 passing xUnit tests covering all context/field combinations
- FieldRequirementContext enum (Registration, Referral, LegalExport, SoYTeReporting) ready for Phase 3+ features
- GET /api/patients/{id}/field-validation endpoint wired in Presentation layer
- PatientFieldWarning amber banner component with i18n (English + Vietnamese) on patient profile
- BookingForm DatePicker redundant placeholder removed per CLAUDE.md guidelines

## Task Commits

Each task was committed atomically:

1. **Task 1 RED: Failing tests for PatientFieldValidator** - `db56ac9` (test)
2. **Task 1 GREEN: Implement PatientFieldValidator domain service** - `bf344e1` (feat)
3. **Task 1: Add field validation endpoint** - `6f51b5a` (feat)
4. **Task 2: Frontend warning indicators + BookingForm cleanup** - `9cbf9b5` (feat)

_Note: Task 1 followed TDD red-green cycle with separate commits_

## Files Created/Modified
- `backend/src/Modules/Patient/Patient.Domain/Enums/FieldRequirementContext.cs` - Enum defining contexts where field requirements differ
- `backend/src/Modules/Patient/Patient.Domain/Services/PatientFieldValidator.cs` - Pure domain service + result records for field validation
- `backend/src/Modules/Patient/Patient.Application/Features/ValidatePatientFields.cs` - Query handler loading patient and validating fields
- `backend/src/Modules/Patient/Patient.Presentation/PatientApiEndpoints.cs` - Added GET field-validation endpoint
- `backend/tests/Patient.Unit.Tests/Patient.Unit.Tests.csproj` - New test project for Patient domain
- `backend/tests/Patient.Unit.Tests/PatientFieldValidatorTests.cs` - 8 unit tests for validator
- `backend/Ganka28.slnx` - Added Patient.Unit.Tests to solution
- `frontend/src/features/patient/components/PatientFieldWarning.tsx` - Amber warning banner for missing fields
- `frontend/src/features/patient/api/patient-api.ts` - Added field validation types and usePatientFieldValidation hook
- `frontend/src/features/patient/components/PatientOverviewTab.tsx` - Integrated PatientFieldWarning in read mode
- `frontend/src/features/booking/components/BookingForm.tsx` - Removed redundant DatePicker placeholder
- `frontend/src/shared/components/Alert.tsx` - Wrapper for shadcn Alert component
- `frontend/src/shared/components/ui/alert.tsx` - shadcn Alert primitive
- `frontend/public/locales/en/patient.json` - Added fieldWarning i18n keys
- `frontend/public/locales/vi/patient.json` - Added fieldWarning Vietnamese i18n keys

## Decisions Made
- PatientFieldValidationResult and MissingFieldInfo records placed in Patient.Domain.Services namespace (not Patient.Contracts.Dtos) because Patient.Domain cannot reference Patient.Contracts (dependency flows the other way). The endpoint handler returns domain types directly since they are simple records suitable for API responses.
- Referral context used as the "strictest common" downstream context for the validation endpoint, since the UI just needs to know if fields will be needed.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Moved validation result types from Contracts to Domain**
- **Found during:** Task 1 GREEN phase (PatientFieldValidator implementation)
- **Issue:** Plan placed PatientFieldValidationResult in Patient.Contracts.Dtos, but Patient.Domain cannot reference Patient.Contracts (dependency flows Contracts -> Domain, not reverse)
- **Fix:** Defined PatientFieldValidationResult and MissingFieldInfo records in Patient.Domain.Services namespace alongside the validator
- **Files modified:** Patient.Domain/Services/PatientFieldValidator.cs (records added here), Patient.Contracts/Dtos/PatientFieldValidationResult.cs (deleted)
- **Verification:** dotnet build succeeds, all 8 tests pass
- **Committed in:** bf344e1 (Task 1 GREEN commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Necessary structural fix due to project dependency direction. No scope creep.

## Issues Encountered
None beyond the deviation documented above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- PAT-03 infrastructure complete: downstream Phase 3+ features (referrals, legal export, So Y Te reporting) can import FieldRequirementContext and call PatientFieldValidator.Validate() directly
- Phase 02 Patient Management & Scheduling is now fully complete (all 10 plans executed)
- Ready to proceed to Phase 03 (Clinical Examination) or verification/gap closure

## Self-Check: PASSED

All 9 created files verified present. All 4 commit hashes (db56ac9, bf344e1, 6f51b5a, 9cbf9b5) verified in git log.

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-02*
