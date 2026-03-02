---
status: diagnosed
trigger: "Diagnose root causes for Phase 02 UAT Tests 9, 10, and 13"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: Three distinct root causes identified for all three UAT failures
test: Code tracing and contract analysis
expecting: N/A - diagnosis complete
next_action: Report findings

## Symptoms

expected: Test 9/13 approve should succeed (201); Test 10 form submit should call API and time slots should pre-populate; All dialogs should have proper spacing
actual: Test 9/13 approve returns 400; Test 10 submit does nothing, no errors shown; Dialog spacing missing globally
errors: 400 Bad Request on approve
reproduction: Approve any pending booking; click time slot and try submitting booking form; open any dialog
started: Since initial implementation

## Eliminated

- hypothesis: JSON case mismatch causing deserialization failure
  evidence: ASP.NET Minimal APIs use JsonSerializerDefaults.Web (case-insensitive, camelCase) by default
  timestamp: 2026-03-02

- hypothesis: openapi-fetch not sending body
  evidence: Reject endpoint uses same api.POST pattern and works; body is sent correctly
  timestamp: 2026-03-02

## Evidence

- timestamp: 2026-03-02
  checked: ClinicScheduleSeeder.cs (seed data)
  found: Wednesday schedule is 13:00-20:00 (Vietnam local time). All weekday schedules use local times.
  implication: Schedule hours are stored in Vietnam local time (UTC+7)

- timestamp: 2026-03-02
  checked: ApproveSelfBooking.cs handler lines 64-66, BookAppointment.cs handler lines 57-59
  found: Both use command.StartTime.TimeOfDay to compare against clinic schedule. Frontend sends UTC ISO string (toISOString()), which is 7 hours behind Vietnam time.
  implication: 14:00 Vietnam -> 07:00 UTC. TimeOfDay=07:00 compared against OpenTime=13:00. 07:00 < 13:00 = FAIL. This is the 400 error cause.

- timestamp: 2026-03-02
  checked: AppointmentBookingDialog.tsx lines 113-128 (useEffect form reset)
  found: Form resets with doctorId=defaultDoctorId but doctorName="". DoctorSelector only provides onChange(id), no name.
  implication: When dialog opens with pre-selected doctor, doctorName stays empty string.

- timestamp: 2026-03-02
  checked: AppointmentBookingDialog.tsx line 258
  found: form.setValue("doctorName", id) sets doctorName to UUID instead of actual name
  implication: Even when user manually changes doctor, doctorName is UUID not real name. But this passes min(1) validation.

- timestamp: 2026-03-02
  checked: DoctorSelector.tsx lines 49-53 (auto-select effect)
  found: Auto-select only fires when !value. When defaultDoctorId is provided, value is truthy, so onChange never fires.
  implication: When opening dialog from slot click (which has defaultDoctorId from page), onChange never fires, doctorName stays "".

- timestamp: 2026-03-02
  checked: Zod schema line 85 and form JSX
  found: doctorName requires min(1) but there is no FieldError display for doctorName in the JSX
  implication: Form validation silently fails - handleSubmit never calls onSubmit, no error shown to user.

- timestamp: 2026-03-02
  checked: dialog.tsx DialogContent className (line 41)
  found: Has "gap-4" but no flex/grid display class. Element defaults to display:block where gap has no effect.
  implication: No spacing between DialogHeader, body content, and DialogFooter in ALL dialogs globally.

- timestamp: 2026-03-02
  checked: AppointmentCalendar.tsx line 86-87
  found: slotDuration="00:15:00" but booking form generates 30-minute slots only
  implication: Clicking 15-minute-aligned slots (e.g., 14:15) produces time values not in the form's Select options, causing visual mismatch.

## Resolution

root_cause: See diagnosis report (3 distinct root causes)
fix: (diagnosis only - not applied)
verification: (diagnosis only)
files_changed: []
