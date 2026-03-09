---
phase: 02-patient-management-scheduling
plan: 15
subsystem: ui
tags: [fullcalendar, moment-timezone, timezone, scheduling, validation]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    provides: "AppointmentCalendar and AppointmentBookingDialog components"
provides:
  - "FullCalendar with proper Asia/Ho_Chi_Minh timezone handling via moment-timezone plugin"
  - "Correct slot time pre-population using UTC-coerced date extraction"
  - "In-form validation error display for DOUBLE_BOOKING and VALIDATION_ERROR"
affects: [02-patient-management-scheduling]

# Tech tracking
tech-stack:
  added: ["@fullcalendar/moment-timezone", "moment-timezone"]
  patterns: ["UTC-coercion aware date extraction with getUTCHours/getUTCMinutes", "In-form error routing via setNonFieldError for domain-specific errors"]

key-files:
  created: []
  modified:
    - "frontend/src/features/scheduling/components/AppointmentCalendar.tsx"
    - "frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx"
    - "frontend/package.json"

key-decisions:
  - "Used @fullcalendar/moment-timezone plugin for timezone-aware calendar rendering instead of manual UTC offset conversion"
  - "Used getUTCHours/getUTCMinutes for FullCalendar UTC-coerced dates where UTC values represent wall-clock times"
  - "Routed DOUBLE_BOOKING and VALIDATION_ERROR to setNonFieldError for in-form display while keeping toast for success notifications"

patterns-established:
  - "UTC-coercion pattern: When FullCalendar uses named timezone, slot dates use UTC-coercion -- extract wall-clock time with getUTCHours/getUTCMinutes"
  - "Error routing pattern: Domain-specific validation errors (double booking, clinic hours) route to in-form ServerValidationAlert, not toast"

requirements-completed: [SCH-01, SCH-02]

# Metrics
duration: 2min
completed: 2026-03-09
---

# Phase 02 Plan 15: Calendar Booking Fixes Summary

**FullCalendar moment-timezone plugin for correct Asia/Ho_Chi_Minh display, UTC-coercion aware time extraction, and in-form validation error routing for booking conflicts**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-09T05:10:18Z
- **Completed:** 2026-03-09T05:12:22Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Installed and registered @fullcalendar/moment-timezone plugin so UTC ISO strings (e.g. 2026-03-10T06:00:00Z) display correctly at 13:00 Vietnam time on the calendar
- Fixed slot click time pre-population by using getUTCHours()/getUTCMinutes() instead of getHours()/getMinutes() to correctly extract wall-clock time from FullCalendar UTC-coerced dates
- Routed DOUBLE_BOOKING and VALIDATION_ERROR to ServerValidationAlert banner in the booking form instead of toast notifications

## Task Commits

Each task was committed atomically:

1. **Task 1: Install moment-timezone plugin and fix FullCalendar timezone handling** - `ed45c73` (fix)
2. **Task 2: Fix slot time pre-population and route validation errors to in-form display** - `e46ad4e` (fix)

## Files Created/Modified
- `frontend/package.json` - Added @fullcalendar/moment-timezone and moment-timezone dependencies
- `frontend/src/features/scheduling/components/AppointmentCalendar.tsx` - Imported and registered momentTimezonePlugin in plugins array
- `frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx` - Changed getHours/getMinutes to getUTCHours/getUTCMinutes in both defaultValues and useEffect reset; changed toast.error to setNonFieldError for DOUBLE_BOOKING and VALIDATION_ERROR

## Decisions Made
- Used @fullcalendar/moment-timezone plugin rather than manual UTC conversion in useAppointments.ts -- the plugin handles timezone conversion at the calendar rendering level, which is cleaner and more maintainable
- Kept toast import in AppointmentBookingDialog.tsx since toast.success is still used for the success callback

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Calendar timezone handling is now correct for Asia/Ho_Chi_Minh timezone
- Booking validation errors display in-form for better UX
- Plan 16 (remaining UAT gap closure) can proceed independently

## Self-Check: PASSED

All files exist and all commits verified.

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-09*
