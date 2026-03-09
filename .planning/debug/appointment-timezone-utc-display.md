---
status: awaiting_human_verify
trigger: "When adding a new appointment, time 14:00 in UTC+7 is saved as 7:00 UTC but displayed as 7:00 instead of converting back to 14:00"
created: 2026-03-09T00:00:00+07:00
updated: 2026-03-09T00:00:07+07:00
---

## Current Focus

hypothesis: CONFIRMED - Backend returns DateTime with DateTimeKind.Unspecified, causing FullCalendar to misinterpret UTC times as local times
test: Applied DateTime.SpecifyKind(a.StartTime, DateTimeKind.Utc) in both query handlers
expecting: API will serialize with 'Z' suffix, FullCalendar will convert UTC to Asia/Ho_Chi_Minh correctly
next_action: Human verification - run app and test the appointment display

## Symptoms

expected: User selects 14:00 in UTC+7 timezone. The appointment should display at the 14:00 slot in the calendar/schedule UI.
actual: The appointment is saved as 7:00 UTC in the database and displayed as 7:00 in the UI, not converted back to local time.
errors: No error messages - logic/timezone conversion bug.
reproduction: 1) Be in UTC+7 timezone. 2) Go to appointments page. 3) Add new appointment selecting 14:00. 4) Appointment appears at 7:00 instead of 14:00.
started: Ongoing issue with how appointment times are handled.

## Eliminated

## Evidence

- timestamp: 2026-03-09T00:00:01
  checked: Frontend submission (AppointmentBookingDialog.tsx line 158)
  found: Frontend calls `startDateTime.toISOString()` which produces UTC string like "2026-03-09T07:00:00.000Z" when user selects 14:00 in UTC+7
  implication: The time is correctly converted to UTC before sending to backend

- timestamp: 2026-03-09T00:00:02
  checked: Backend BookAppointmentHandler (BookAppointment.cs lines 80-88)
  found: Handler stores `command.StartTime` directly (which is the UTC value). It converts to local only for schedule validation.
  implication: Database stores UTC time correctly (07:00 for a 14:00 local selection)

- timestamp: 2026-03-09T00:00:03
  checked: Backend GetAppointmentsByDoctorHandler (GetAppointmentsByDoctor.cs lines 28-44)
  found: Handler maps `a.StartTime` and `a.EndTime` directly into AppointmentDto without any timezone conversion
  implication: The DTO contains the raw UTC DateTime values from the database

- timestamp: 2026-03-09T00:00:04
  checked: Frontend event mapping (useAppointments.ts lines 54-58)
  found: `appointmentToEvent` maps `apt.startTime` and `apt.endTime` directly as `start` and `end` for FullCalendar EventInput
  implication: The raw strings from API response are passed directly to FullCalendar

- timestamp: 2026-03-09T00:00:05
  checked: FullCalendar timezone config (AppointmentCalendar.tsx line 100)
  found: Calendar uses `timeZone="Asia/Ho_Chi_Minh"` with momentTimezonePlugin
  implication: If FullCalendar receives a proper UTC datetime (with Z suffix), it WOULD convert to Asia/Ho_Chi_Minh. If it receives an unqualified datetime, it treats it as already being in the calendar's timezone.

- timestamp: 2026-03-09T00:00:06
  checked: ASP.NET DateTime serialization behavior (no custom JsonSerializerOptions for scheduling)
  found: System.Text.Json default behavior for DateTime: If DateTimeKind is Unspecified (default from EF Core), it serializes WITHOUT 'Z' suffix (e.g., "2026-03-09T07:00:00"). EF Core reads DateTime from SQL Server as DateTimeKind.Unspecified by default.
  implication: The API returns "2026-03-09T07:00:00" (no Z), so FullCalendar treats 07:00 as local Vietnam time and displays it at 07:00 slot instead of converting from UTC.

- timestamp: 2026-03-09T00:00:07
  checked: TDD fix applied - both GetAppointmentsByDoctor and GetAppointmentsByPatient handlers
  found: Added DateTime.SpecifyKind(a.StartTime, DateTimeKind.Utc) and DateTime.SpecifyKind(a.EndTime, DateTimeKind.Utc) in DTO mapping
  implication: API responses will now serialize as "2026-03-09T07:00:00Z" (with Z), causing FullCalendar to correctly convert to local time (14:00 in UTC+7)

- timestamp: 2026-03-09T00:00:07
  checked: Unit tests
  found: All 9 scheduling unit tests pass including 2 new tests that verify DateTimeKind.Utc on DTO StartTime/EndTime
  implication: Fix is correct and no regressions in existing tests

## Resolution

root_cause: The backend stores appointment times in UTC but returns them via the API as DateTime values with DateTimeKind.Unspecified (no 'Z' suffix in JSON). FullCalendar with timeZone="Asia/Ho_Chi_Minh" receives "2026-03-09T07:00:00" and treats it as already local time, displaying at 07:00 instead of converting from UTC to 14:00.
fix: Added DateTime.SpecifyKind(..., DateTimeKind.Utc) in GetAppointmentsByDoctorHandler, GetAppointmentsByPatientHandler, and CheckBookingStatusHandler when building DTOs. This ensures System.Text.Json serializes the DateTime values with 'Z' suffix, allowing FullCalendar (with momentTimezonePlugin and timeZone="Asia/Ho_Chi_Minh") to correctly convert UTC to local time.
verification: 9/9 unit tests pass (including 2 new regression tests for DateTimeKind.Utc). Awaiting human verification via UI.
files_changed:
  - backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByDoctor.cs
  - backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByPatient.cs
  - backend/src/Modules/Scheduling/Scheduling.Application/Features/CheckBookingStatus.cs
  - backend/tests/Scheduling.Unit.Tests/Features/GetAppointmentsByDoctorHandlerTests.cs
  - backend/tests/Scheduling.Unit.Tests/Features/GetAppointmentsByPatientHandlerTests.cs
