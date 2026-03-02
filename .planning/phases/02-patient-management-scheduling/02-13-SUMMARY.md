---
phase: 02-patient-management-scheduling
plan: 13
subsystem: ui, scheduling, patient
tags: [doctor-selector, booking-form, allergy-combobox, fullcalendar, category-grouping, free-text]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    provides: "Shared infrastructure fixes (dialog spacing, 401 interceptor, allergy severity int conversion)"
provides:
  - "DoctorSelector onChange with both doctorId and doctorName"
  - "Working booking form with pre-populated slot data and correct doctorName"
  - "Calendar 30-min slot duration matching form time options"
  - "Category-aware allergy combobox in patient registration with free-text support"
affects: [02-14, all-scheduling-ui]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "DoctorSelector 2-arg onChange pattern: (doctorId, doctorName) for denormalized name capture"
    - "Category-grouped Popover+Command combobox with shouldFilter={false} and free-text entry"
    - "Button trigger wrapped in div inside PopoverTrigger to avoid click-to-toggle anti-pattern"

key-files:
  created: []
  modified:
    - "frontend/src/features/scheduling/components/DoctorSelector.tsx"
    - "frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx"
    - "frontend/src/features/scheduling/components/AppointmentCalendar.tsx"
    - "frontend/src/features/scheduling/components/PendingBookingsPanel.tsx"
    - "frontend/src/app/routes/_authenticated/appointments/index.tsx"
    - "frontend/src/features/patient/components/PatientRegistrationForm.tsx"

key-decisions:
  - "ApproveBookingDialog does not exist as standalone component -- approve functionality is inline in PendingBookingsPanel"
  - "PendingBookingsPanel already resolves doctorName from useDoctors() query, only needed onChange signature update"
  - "Allergy combobox uses Button trigger (not Input) to match AllergyForm.tsx pattern and avoid click-to-toggle issues"

patterns-established:
  - "DoctorSelector 2-arg onChange: all consumers must destructure (id, name) or (id) with explicit arrow"
  - "Category-grouped allergy combobox: reusable pattern with categoryKeyMap, shouldFilter={false}, free-text CommandItem"

requirements-completed: [SCH-01, SCH-04, SCH-05, PAT-01]

# Metrics
duration: 4min
completed: 2026-03-02
---

# Phase 02 Plan 13: Staff Booking Form and Allergy Autocomplete Fix Summary

**DoctorSelector providing name+ID, booking form pre-populating from calendar slot clicks, 30-min slot alignment, and category-aware allergy combobox with free-text in patient registration**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-02T15:37:29Z
- **Completed:** 2026-03-02T15:42:17Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments
- DoctorSelector now passes both doctorId and doctorName to all consumers, fixing silent Zod validation failure on booking form
- Booking form correctly pre-populates date and time when user clicks a calendar time slot
- Calendar slot duration aligned to 30 minutes to match form's generateTimeSlots() output
- Allergy autocomplete in registration form completely replaced with category-grouped combobox supporting free-text entry

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix DoctorSelector, booking form, and calendar slot alignment** - `80b22e4` (feat)
2. **Task 2: Replace allergy autocomplete in PatientRegistrationForm** - `1242b5a` (feat)

## Files Created/Modified
- `frontend/src/features/scheduling/components/DoctorSelector.tsx` - onChange now provides (doctorId, doctorName), auto-select passes both
- `frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx` - Uses useDoctors() for doctorName resolution, fixed onChange handler, added doctorName FieldError
- `frontend/src/features/scheduling/components/AppointmentCalendar.tsx` - slotDuration changed from 15min to 30min
- `frontend/src/features/scheduling/components/PendingBookingsPanel.tsx` - DoctorSelector onChange updated for 2-arg signature
- `frontend/src/app/routes/_authenticated/appointments/index.tsx` - DoctorSelector onChange updated for 2-arg signature
- `frontend/src/features/patient/components/PatientRegistrationForm.tsx` - AllergyRow rewritten with Popover+Command, category grouping, shouldFilter={false}, free-text support

## Decisions Made
- ApproveBookingDialog does not exist as a standalone file -- the approve functionality is inline within PendingBookingsPanel.tsx. Updated DoctorSelector usage there instead.
- PendingBookingsPanel already resolves doctorName from useDoctors() query for the approve mutation, so only the onChange signature needed updating.
- Used Button trigger (not Input) for allergy combobox to match the working AllergyForm.tsx pattern and avoid the click-to-toggle anti-pattern.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] ApproveBookingDialog.tsx does not exist**
- **Found during:** Task 1 (plan referenced ApproveBookingDialog.tsx)
- **Issue:** Plan specified updating ApproveBookingDialog.tsx but no such file exists. The approve functionality lives inline in PendingBookingsPanel.tsx.
- **Fix:** Updated DoctorSelector onChange in PendingBookingsPanel.tsx instead, plus the appointments page index.tsx.
- **Files modified:** PendingBookingsPanel.tsx, appointments/index.tsx
- **Verification:** TypeScript compilation passes, all DoctorSelector consumers use correct 2-arg signature.
- **Committed in:** 80b22e4 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Correct file identified and updated. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Staff booking flow is fully functional: slot click -> pre-populate -> select patient -> submit
- Allergy registration autocomplete works with categories, Vietnamese labels, and free-text entry
- Plan 14 can proceed for remaining UAT gap closure items

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-02*

## Self-Check: PASSED
- All 6 modified files verified present on disk
- Both task commits (80b22e4, 1242b5a) verified in git log
