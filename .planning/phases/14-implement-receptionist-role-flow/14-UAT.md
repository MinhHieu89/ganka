---
status: partial
phase: 14-implement-receptionist-role-flow
source: 14-01-SUMMARY.md through 14-09-SUMMARY.md
started: 2026-03-28T00:00:00Z
updated: 2026-03-28T02:48:00Z
---

## Current Test

[testing paused — additional integration issues found]

## Tests

### 1. Cold Start Smoke Test
expected: Backend boots, login page loads, receptionist login succeeds.
result: pass

### 2. Receptionist Role-Based Dashboard Routing
expected: Receptionist sees dedicated dashboard with KPIs, filters, search, action buttons.
result: pass

### 3. Dashboard Status Filter and Search
expected: Filter pills with counts, search input, refresh button present.
result: pass

### 4. Patient Intake Form - New Patient Registration
expected: 4 sections, all fields, save creates patient with auto-generated code.
result: pass
notes: Gender sent as numeric enum (0/1/2). Patient "Test Patient UAT" created with code GK-2026-0005.

### 5. Patient Intake Form - Phone Duplicate Detection
expected: Amber warning bar with "Mo ho so cu" link when duplicate phone entered.
result: pass
notes: PhoneFieldWithDuplicateCheck component verified with 500ms debounce.

### 6. Patient Intake Form - Save and Advance to Walk-in
expected: "Luu & Chuyen tien kham" saves patient AND creates walk-in visit.
result: [pending]

### 7. Patient Intake Form - Edit Existing Patient
expected: ?patientId=X pre-fills form for editing.
result: [pending]

### 8. Appointment Booking - Existing Patient
expected: Patient search, doctor selector, calendar with slots, confirmation bar, booking succeeds.
result: issue
reported: "Booking page works structurally — patient search finds patients, doctor selector shows doctors (after fix to /api/appointments/doctors endpoint), calendar shows correct slots (Sang 08:00-11:30 Sat, Mon disabled), confirmation bar shows details. However booking creates appointment via 409 Conflict (double booking) but appointment does NOT appear on receptionist dashboard queue. Dashboard query (GetReceptionistDashboard) may filter differently than expected."
severity: major

### 9. Appointment Booking - Guest Patient
expected: Yellow warning bar with guest fields when patient not found.
result: [pending]

### 10. Check-in - Complete Patient
expected: Check-in dialog with patient info, status change.
result: blocked
blocked_by: prior-phase
reason: "Booked appointment doesn't appear on dashboard, cannot test check-in"

### 11. Check-in - Incomplete Patient Warning
expected: Amber warning for incomplete profile.
result: blocked
blocked_by: prior-phase

### 12. Walk-in Visit Creation
expected: Walk-in dialog from dashboard search.
result: [pending]

### 13. Action Menu - Status-Dependent Options
expected: Different menu items per status.
result: blocked
blocked_by: prior-phase

### 14. No-Show Marking
expected: Amber dialog with note and rebook checkbox.
result: blocked
blocked_by: prior-phase

### 15. Cancel Appointment
expected: Red warning dialog with reason dropdown.
result: blocked
blocked_by: prior-phase

### 16. Reschedule Appointment
expected: Dialog with old schedule strikethrough, new slot selection.
result: blocked
blocked_by: prior-phase

### 17. Cancel Visit with Reason
expected: Dialog with 5 reasons and rebook checkbox.
result: blocked
blocked_by: prior-phase

### 18. API Permission Protection
expected: Correct 401/403/200 behavior per role.
result: pass

## Summary

total: 18
passed: 6
issues: 1
pending: 4
skipped: 0
blocked: 7

## Gaps

- truth: "Booked appointment appears on receptionist dashboard queue"
  status: failed
  reason: "Appointment booked via POST /api/appointments (confirmed by 409 duplicate check) but does not appear in receptionist dashboard. GetReceptionistDashboard query may not be finding the appointment."
  severity: major
  test: 8
  root_cause: "GetReceptionistDashboard query likely filters by today's date using different timezone logic or filters out appointments without certain fields populated."
  artifacts:
    - path: "backend/src/Modules/Scheduling/Scheduling.Application/Features/GetReceptionistDashboard.cs"
      issue: "Dashboard query may not match booked appointments"
  missing:
    - "Debug GetReceptionistDashboard to understand why booked appointments don't show"

## Additional Fixes Applied During Testing

1. **AuthDataSeeder** — Fixed to add missing roles individually instead of skipping all
2. **Gender enum** — Frontend sends numeric (0/1/2) matching backend enum pattern
3. **Slots endpoint** — Fixed InvokeAsync return type (Result<List<>> instead of List<>)
4. **Booking calendar** — Added closed-day disabling via useClinicSchedule
5. **Doctors endpoint** — Added /api/appointments/doctors with Scheduling.View permission (receptionist accessible)
6. **AppointmentTypeId** — Fixed to use seeded "00000000-0000-0000-0000-000000000101" (NewPatient type)
7. **Guest booking default type** — Fixed same wrong appointment type ID
