---
phase: 02-patient-management-scheduling
plan: 05
subsystem: ui
tags: [fullcalendar, react-query, scheduling-calendar, self-booking, i18n, shadcn, drag-drop, public-page]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    plan: 02
    provides: "Scheduling backend API endpoints (appointments CRUD, self-booking workflow, clinic schedule)"
  - phase: 02-patient-management-scheduling
    plan: 03
    provides: "FullCalendar packages, shadcn Command/Popover/Tabs/DatePicker wrappers, usePatientSearch hook, scheduling i18n translations"
provides:
  - "AppointmentCalendar component with FullCalendar weekly view, color-coded types, drag-drop rescheduling"
  - "AppointmentBookingDialog with patient search autocomplete and appointment type selection"
  - "AppointmentDetailDialog with cancel flow and mandatory reason selection"
  - "DoctorSelector dropdown component"
  - "PendingBookingsPanel with approve/reject workflow for self-booking requests"
  - "PublicBookingPage at /book with branded form, language toggle, clinic info footer"
  - "BookingStatusCheck at /book/status with pending/approved/rejected display"
  - "Public booking API client (no auth middleware) for patient self-service"
affects: [02-06]

# Tech tracking
tech-stack:
  added: []
  patterns: [public-api-client, calendar-event-mapping, fullcalendar-css-theming, public-route-outside-auth]

key-files:
  created:
    - frontend/src/features/scheduling/api/scheduling-api.ts
    - frontend/src/features/scheduling/hooks/useAppointments.ts
    - frontend/src/features/scheduling/hooks/useSelfBookings.ts
    - frontend/src/features/scheduling/components/AppointmentCalendar.tsx
    - frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx
    - frontend/src/features/scheduling/components/AppointmentDetailDialog.tsx
    - frontend/src/features/scheduling/components/DoctorSelector.tsx
    - frontend/src/features/scheduling/components/PendingBookingsPanel.tsx
    - frontend/src/features/booking/api/booking-api.ts
    - frontend/src/features/booking/hooks/usePublicBooking.ts
    - frontend/src/features/booking/components/PublicBookingPage.tsx
    - frontend/src/features/booking/components/BookingForm.tsx
    - frontend/src/features/booking/components/BookingConfirmation.tsx
    - frontend/src/features/booking/components/BookingStatusCheck.tsx
    - frontend/src/app/routes/_authenticated/appointments/index.tsx
    - frontend/src/app/routes/book/index.tsx
    - frontend/src/app/routes/book/status.tsx
  modified:
    - frontend/public/locales/en/scheduling.json
    - frontend/public/locales/vi/scheduling.json
    - frontend/src/app/routeTree.gen.ts

key-decisions:
  - "Separate public API client (publicApi) without auth middleware for patient self-booking endpoints"
  - "FullCalendar CSS themed via CSS variables matching shadcn/ui design tokens (--border, --primary, --muted)"
  - "Public /book and /book/status routes outside _authenticated layout group -- no auth redirect"
  - "PendingBookingsPanel included directly in Task 1 (not deferred) since appointments route references it"
  - "DoctorSelector queries auth/users endpoint filtered by Doctor role -- will need dedicated doctors endpoint later"
  - "Date picker on booking form restricts to next 30 days and only clinic open days"

patterns-established:
  - "Public API client pattern: separate openapi-fetch instance without auth middleware for unauthenticated endpoints"
  - "FullCalendar theming: CSS-in-JS style tag inside component using CSS variables from shadcn design system"
  - "Calendar event mapping: useAppointmentsForCalendar hook transforms API DTOs to FullCalendar EventInput format"
  - "Public route pattern: route files under /book/ directory (not under _authenticated/) render standalone branded pages"

requirements-completed: [SCH-01, SCH-02, SCH-03, SCH-04, SCH-05, SCH-06]

# Metrics
duration: 8min
completed: 2026-03-01
---

# Phase 02 Plan 05: Scheduling Frontend & Public Booking Summary

**FullCalendar weekly appointment calendar with drag-drop rescheduling, staff booking/cancel dialogs, public self-booking page at /book with branded design and status check, and pending bookings approval panel**

## Performance

- **Duration:** 8 min
- **Started:** 2026-03-01T12:02:48Z
- **Completed:** 2026-03-01T12:11:43Z
- **Tasks:** 2
- **Files modified:** 21

## Accomplishments
- Staff-facing appointment calendar at /appointments with FullCalendar weekly view, per-doctor filtering via DoctorSelector, color-coded appointment types (blue/green/orange/purple), drag-drop rescheduling with server validation, and click-to-book from empty slots
- Booking dialog with Command+Popover patient search autocomplete, appointment type selector with duration display, and datetime picker pre-filled from calendar slot clicks
- Appointment detail dialog with status badges, cancel flow with mandatory reason selection (4 cancellation reasons), and formatted date/time display with Vietnamese locale
- Public self-booking page at /book with Ganka28 branding, language toggle (VI/EN), Zod-validated booking form with Vietnamese phone regex, appointment type selection, date picker filtering by clinic open days, and confirmation page with prominent reference number
- Booking status check page at /book/status with pending (yellow), approved (green), and rejected (red) status display
- PendingBookingsPanel on appointments page with approve (assign doctor + time) and reject (with reason) dialogs for staff processing
- Extended i18n translations with 20+ new keys for booking status messages, clinic hours schedule, and doctor preference in both English and Vietnamese

## Task Commits

Each task was committed atomically:

1. **Task 1: Scheduling API hooks, appointment calendar with FullCalendar, booking dialog, and doctor selector** - `3f1f40e` (feat)
2. **Task 2: Public self-booking page, booking status check, and pending bookings management panel** - `aa5548b` (feat)

## Files Created/Modified

### Scheduling Feature (frontend/src/features/scheduling/)
- `api/scheduling-api.ts` - React Query hooks for all scheduling endpoints (10 hooks: book, cancel, reschedule, queries, self-booking management)
- `hooks/useAppointments.ts` - Calendar event mapping, business hours conversion, date range management for FullCalendar
- `hooks/useSelfBookings.ts` - Wrapper hook for pending self-bookings with count
- `components/AppointmentCalendar.tsx` - FullCalendar weekly view with timeGrid/dayGrid/interaction plugins, CSS-variable theming
- `components/AppointmentBookingDialog.tsx` - Staff booking dialog with patient search autocomplete and Zod validation
- `components/AppointmentDetailDialog.tsx` - Read-only appointment details with cancel flow and reason selection
- `components/DoctorSelector.tsx` - Doctor dropdown filtering by role from auth/users endpoint
- `components/PendingBookingsPanel.tsx` - Card list of pending self-bookings with approve/reject dialogs

### Booking Feature (frontend/src/features/booking/)
- `api/booking-api.ts` - Public API client (no auth) with React Query hooks for self-booking endpoints
- `hooks/usePublicBooking.ts` - Booking state management (form/confirmation transitions)
- `components/PublicBookingPage.tsx` - Branded standalone booking page with language toggle and clinic footer
- `components/BookingForm.tsx` - Zod-validated form with Vietnamese phone regex, date picker, appointment type selector
- `components/BookingConfirmation.tsx` - Reference number display with status check and book-another links
- `components/BookingStatusCheck.tsx` - Status lookup by reference number with color-coded result display

### Routes
- `app/routes/_authenticated/appointments/index.tsx` - Appointments page with calendar and pending tabs
- `app/routes/book/index.tsx` - Public booking route (no auth)
- `app/routes/book/status.tsx` - Public booking status check route (no auth)

### Modified
- `frontend/public/locales/en/scheduling.json` - Added 20+ booking/status translation keys
- `frontend/public/locales/vi/scheduling.json` - Added Vietnamese translations with proper diacritics
- `frontend/src/app/routeTree.gen.ts` - Auto-regenerated with new routes

## Decisions Made
- Created separate `publicApi` openapi-fetch client without auth middleware for /api/public/booking endpoints -- keeps auth middleware cleanly isolated from unauthenticated flows
- FullCalendar styled via CSS-in-JS `<style>` tag inside component using shadcn CSS variables -- avoids global CSS file and keeps theming co-located
- PendingBookingsPanel built as full component in Task 1 (not Task 2) because the appointments route page imports it directly -- avoids circular task dependency
- DoctorSelector queries /api/auth/users and filters by Doctor role client-side -- acceptable for boutique clinic, can be replaced with dedicated /api/doctors endpoint later
- Booking form date picker restricts to next 30 days and only clinic open days -- prevents booking on closed days without extra server roundtrip
- Used `as never` type assertions consistently for openapi-fetch calls since OpenAPI types are not yet generated -- matches existing project pattern

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Scheduling frontend is complete with all user-facing features (calendar, booking, self-booking, status check)
- Ready for Plan 02-06 integration testing or Phase 03 clinical module
- Public booking page can be accessed at /book without authentication
- Staff appointments page renders at /appointments with full calendar and pending bookings management
- FullCalendar events will display when backend API returns appointment data after database migration

## Self-Check: PASSED

All 17 created files verified present. Both task commits (3f1f40e, aa5548b) verified in git log.

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-01*
