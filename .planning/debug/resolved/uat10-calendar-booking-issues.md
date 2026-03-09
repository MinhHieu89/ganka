---
status: resolved
trigger: "Investigate 3 issues from UAT test 10: time slot pre-populate, validation errors in toast, UTC vs local timezone"
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:05:00Z
---

## Current Focus

hypothesis: All three issues confirmed with root causes identified
test: Code analysis complete
expecting: N/A - diagnosis phase
next_action: Return diagnosis results

## Symptoms

expected: (1) Clicking 13:00 slot pre-populates time field (2) Validation errors show in-form (3) Saved appointments display on calendar
actual: (1) Time field empty or wrong time (2) Errors show in toast (3) Appointments not visible at correct time
errors: N/A - behavioral issues
reproduction: Book appointment via calendar slot click, submit invalid time, view saved appointments
started: Since feature implementation

## Eliminated

(none)

## Evidence

- timestamp: 2026-03-09T00:01:00Z
  checked: FullCalendar timeZone config and installed plugins
  found: timeZone="Asia/Ho_Chi_Minh" is set but no timezone plugin (@fullcalendar/moment-timezone) is installed. FullCalendar uses "UTC-coercion" -- all dates in API use UTC where UTC values represent wall-clock times.
  implication: getHours() returns wrong value; ISO strings with Z offset are misinterpreted

- timestamp: 2026-03-09T00:02:00Z
  checked: AppointmentBookingDialog lines 109-111 and 128-130
  found: Uses defaultStartTime.getHours() and getMinutes() to extract time. With UTC-coercion, getUTCHours()/getUTCMinutes() is needed.
  implication: Issue 1 root cause -- wrong time extracted from UTC-coerced Date

- timestamp: 2026-03-09T00:03:00Z
  checked: AppointmentBookingDialog onError handler lines 167-178
  found: DOUBLE_BOOKING and VALIDATION_ERROR are caught by message string checks and shown via toast.error(). Only the else branch calls handleServerValidationError() which would set in-form errors.
  implication: Issue 2 root cause -- known error types bypass the in-form error path

- timestamp: 2026-03-09T00:04:00Z
  checked: useAppointments.ts appointmentToEvent function and backend GetAppointmentsByDoctor
  found: Backend stores/returns UTC DateTimes (e.g. "2026-03-10T06:00:00Z" for 13:00 Vietnam). Frontend passes apt.startTime/endTime directly to FullCalendar events. With UTC-coercion, FullCalendar strips the Z and displays 06:00 instead of 13:00.
  implication: Issue 3 root cause -- UTC times fed to UTC-coerced calendar, time offset lost

## Resolution

root_cause: |
  Issue 1: UTC-coercion causes getHours() to return wrong value for UTC-coerced Dates
  Issue 2: DOUBLE_BOOKING and VALIDATION_ERROR bypass handleServerValidationError, go to toast
  Issue 3: Backend returns UTC ISO strings, FullCalendar UTC-coercion strips offset, displays wrong time
fix: Pending
verification: Pending
files_changed: []
