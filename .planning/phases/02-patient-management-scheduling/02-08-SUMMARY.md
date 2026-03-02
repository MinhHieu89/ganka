---
phase: 02-patient-management-scheduling
plan: 08
subsystem: ui
tags: [shadcn, textarea, datepicker, calendar, scheduling, react, i18n]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    provides: "Scheduling module with appointment booking, pending bookings, and calendar components"
  - phase: 01.2-refactor-frontend-to-shadcn-ui-with-tanstack-start-file-based-routing
    provides: "shadcn/ui component wrappers and Field/FieldLabel patterns"
provides:
  - "DoctorSelector fetching from /api/admin/users with paginated envelope unwrap"
  - "shadcn Textarea primitive with rounded-md and re-export wrapper"
  - "DatePicker + time Select pattern for appointment scheduling"
  - "Field/FieldLabel consistency in approve/reject dialogs"
  - "Calendar dropdown nav alignment fix"
affects: [scheduling, booking, patient-management]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "DatePicker + time Select for datetime input (replacing native datetime-local)"
    - "Textarea re-export wrapper following established component pattern"
    - "Paginated API envelope unwrap pattern for admin user listing"

key-files:
  created:
    - "frontend/src/shared/components/ui/textarea.tsx"
    - "frontend/src/shared/components/Textarea.tsx"
  modified:
    - "frontend/src/features/scheduling/components/DoctorSelector.tsx"
    - "frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx"
    - "frontend/src/features/scheduling/components/PendingBookingsPanel.tsx"
    - "frontend/src/features/scheduling/components/AppointmentDetailDialog.tsx"
    - "frontend/src/features/booking/components/BookingForm.tsx"
    - "frontend/src/shared/components/ui/calendar.tsx"
    - "frontend/public/locales/en/scheduling.json"
    - "frontend/public/locales/vi/scheduling.json"

key-decisions:
  - "DoctorSelector auto-select moved to useEffect to avoid React state-update-during-render anti-pattern"
  - "30-minute time slots from 08:00 to 19:30 for clinic hours in generateTimeSlots helper"
  - "Separate startDate (Date) + startTime (string) form fields replacing single datetime-local string"

patterns-established:
  - "DatePicker + time Select combo: split datetime into date and time fields, combine in handleSubmit"
  - "Textarea wrapper: re-export from ui/textarea.tsx following established shadcn wrapper convention"

requirements-completed: [SCH-01, SCH-02, SCH-03, SCH-04, SCH-05, SCH-06]

# Metrics
duration: 6min
completed: 2026-03-02
---

# Phase 02 Plan 08: Scheduling UX Gap Closure Summary

**Fixed doctor dropdown API endpoint, replaced native datetime-local with shadcn DatePicker+Select, created Textarea component with rounded corners, and aligned calendar dropdown navigation**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-02T12:06:25Z
- **Completed:** 2026-03-02T12:13:12Z
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments
- DoctorSelector fetches from correct /api/admin/users endpoint with paginated envelope unwrap
- All native datetime-local inputs replaced with shadcn DatePicker + time Select combo across booking and approve dialogs
- Created shadcn Textarea primitive with rounded-md and re-export wrapper for consistent textarea styling
- All raw textarea elements replaced with Textarea component (AppointmentBookingDialog, PendingBookingsPanel, AppointmentDetailDialog, BookingForm)
- Calendar dropdown nav alignment fixed by removing absolute positioning
- Public booking form cleaned up: no redundant placeholders on labeled fields
- Approve/reject dialogs restyled with proper Field/FieldLabel wrappers

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix DoctorSelector, create Textarea, fix calendar alignment** - `e3449df` (fix)
2. **Task 2: Fix booking dialog datetime picker and pending bookings dialog styling** - `c8461cf` (fix)

**Plan metadata:** `b487b6d` (docs: complete plan)

## Files Created/Modified
- `frontend/src/shared/components/ui/textarea.tsx` - shadcn Textarea primitive with rounded-md class
- `frontend/src/shared/components/Textarea.tsx` - Textarea re-export wrapper
- `frontend/src/features/scheduling/components/DoctorSelector.tsx` - Fixed API endpoint to /api/admin/users, envelope unwrap, useEffect auto-select
- `frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx` - DatePicker + time Select replacing datetime-local, Textarea replacing raw textarea
- `frontend/src/features/scheduling/components/PendingBookingsPanel.tsx` - Field/FieldLabel in approve/reject dialogs, DatePicker + time Select, Textarea
- `frontend/src/features/scheduling/components/AppointmentDetailDialog.tsx` - Textarea replacing raw textarea
- `frontend/src/features/booking/components/BookingForm.tsx` - Removed redundant placeholders, Textarea replacing raw textarea
- `frontend/src/shared/components/ui/calendar.tsx` - Removed absolute positioning from nav for dropdown alignment
- `frontend/public/locales/en/scheduling.json` - Added selectTime key
- `frontend/public/locales/vi/scheduling.json` - Added selectTime key

## Decisions Made
- Moved DoctorSelector auto-select from render body to useEffect to fix React state-update-during-render anti-pattern
- Used 30-minute time slots from 08:00 to 19:30 for generateTimeSlots helper function
- Split startTime into separate startDate (z.date) and startTime (z.string) form fields, combined in handleSubmit before API call
- AppointmentDetailDialog textarea keeps min-h-[60px] override via className prop on Textarea component

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Task 2 file changes were already committed in a previous session's metadata commit (c8461cf). The changes were verified to be correctly in place.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- UAT Tests 9, 10, 11, and 13 gap closures complete
- All scheduling UX issues addressed: doctor dropdown, datetime picker, textarea corners, dialog spacing, booking form, calendar alignment
- Ready for remaining gap closure plans (02-07, 02-09)

## Self-Check: PASSED

All 9 created/modified files verified present. Both task commits (e3449df, c8461cf) verified in git log. Metadata commit: b487b6d.

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-02*
