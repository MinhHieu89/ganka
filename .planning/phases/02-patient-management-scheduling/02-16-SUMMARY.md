---
phase: 02-patient-management-scheduling
plan: 16
subsystem: scheduling, ui
tags: [i18n, locale, appointment-type, dto, placeholder, react, csharp]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    provides: "Scheduling module with AppointmentDto, GetAppointmentsByDoctor/Patient handlers, appointment calendar UI"
provides:
  - "AppointmentDto with AppointmentTypeNameVi field for locale-aware display"
  - "Frontend locale-aware appointment type name rendering in detail and booking dialogs"
  - "Clean booking form with no redundant placeholders"
affects: [scheduling, booking]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "i18n.language === 'vi' pattern for locale-aware data-driven field display"

key-files:
  created:
    - "backend/tests/Scheduling.Unit.Tests/Features/GetAppointmentsByDoctorHandlerTests.cs"
    - "backend/tests/Scheduling.Unit.Tests/Features/GetAppointmentsByPatientHandlerTests.cs"
  modified:
    - "backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/AppointmentDto.cs"
    - "backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByDoctor.cs"
    - "backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByPatient.cs"
    - "frontend/src/features/scheduling/api/scheduling-api.ts"
    - "frontend/src/features/scheduling/hooks/useAppointments.ts"
    - "frontend/src/features/scheduling/components/AppointmentDetailDialog.tsx"
    - "frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx"
    - "frontend/src/app/routes/_authenticated/appointments/index.tsx"
    - "frontend/src/features/booking/components/BookingForm.tsx"

key-decisions:
  - "Add NameVi as a separate field to AppointmentDto rather than using translation keys, keeping data-driven i18n approach consistent"
  - "Leave PendingBookingsPanel as-is since SelfBookingRequestDto is a different DTO from a different flow"

patterns-established:
  - "i18n locale check: Use i18n.language === 'vi' ? viField : enField for data-driven bilingual fields"

requirements-completed: [SCH-01, SCH-03, SCH-05]

# Metrics
duration: 7min
completed: 2026-03-09
---

# Phase 02 Plan 16: Appointment Type i18n and Placeholder Cleanup Summary

**Backend AppointmentDto extended with NameVi field; frontend displays locale-aware appointment type names; booking form redundant placeholders removed**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-09T05:14:26Z
- **Completed:** 2026-03-09T05:21:31Z
- **Tasks:** 2
- **Files modified:** 11

## Accomplishments
- Backend AppointmentDto now includes AppointmentTypeNameVi field, mapped from AppointmentType.NameVi in both GetAppointmentsByDoctor and GetAppointmentsByPatient handlers
- 4 new unit tests verify correct NameVi mapping and "Unknown" fallback behavior
- AppointmentDetailDialog displays Vietnamese or English type name based on user locale
- AppointmentBookingDialog dropdown shows locale-aware type names (was hardcoded to Vietnamese)
- BookingForm cleaned up: removed 2 redundant placeholder props and fixed DatePicker default

## Task Commits

Each task was committed atomically:

1. **Task 1 RED: Add failing tests for AppointmentTypeNameVi** - `35e9cfd` (test)
2. **Task 1 GREEN: Add AppointmentTypeNameVi to backend DTO and handlers** - `121d952` (feat)
3. **Task 2: Frontend locale-aware type names and placeholder cleanup** - `2375d56` (feat)

## Files Created/Modified
- `backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/AppointmentDto.cs` - Added AppointmentTypeNameVi field to record
- `backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByDoctor.cs` - Maps NameVi from AppointmentType entity
- `backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByPatient.cs` - Maps NameVi from AppointmentType entity
- `backend/tests/Scheduling.Unit.Tests/Features/GetAppointmentsByDoctorHandlerTests.cs` - Tests for NameVi mapping and fallback
- `backend/tests/Scheduling.Unit.Tests/Features/GetAppointmentsByPatientHandlerTests.cs` - Tests for NameVi mapping and fallback
- `frontend/src/features/scheduling/api/scheduling-api.ts` - Added appointmentTypeNameVi to AppointmentDto interface
- `frontend/src/features/scheduling/hooks/useAppointments.ts` - Pass appointmentTypeNameVi through extendedProps
- `frontend/src/features/scheduling/components/AppointmentDetailDialog.tsx` - Locale-aware type name display
- `frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx` - Locale-aware dropdown type names
- `frontend/src/app/routes/_authenticated/appointments/index.tsx` - AppointmentInfo interface + handleEventClick updated
- `frontend/src/features/booking/components/BookingForm.tsx` - Removed redundant placeholders

## Decisions Made
- Added NameVi as a separate field in AppointmentDto rather than using i18n translation keys, keeping the data-driven approach consistent with how AppointmentTypeDto already works
- Left PendingBookingsPanel unchanged since it uses SelfBookingRequestDto (different DTO, different data flow)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Backend solution build failed due to file locks from a running Bootstrapper process (unrelated to changes). Individual scheduling project builds and all unit tests confirmed successful.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All UAT Test 11 (appointment type i18n) and UAT Test 12 (redundant placeholders) issues resolved
- Scheduling module ready for final verification

## Self-Check: PASSED

All 11 modified/created files verified present. All 3 task commits (35e9cfd, 121d952, 2375d56) verified in git log.

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-09*
