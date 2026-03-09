---
status: verified
phase: 02-patient-management-scheduling
source: 02-01-SUMMARY.md, 02-02-SUMMARY.md, 02-03-SUMMARY.md, 02-04-SUMMARY.md, 02-05-SUMMARY.md, 02-06-SUMMARY.md, 02-07-SUMMARY.md, 02-08-SUMMARY.md, 02-09-SUMMARY.md, 02-10-SUMMARY.md, 02-11-SUMMARY.md, 02-12-SUMMARY.md, 02-13-SUMMARY.md, 02-14-SUMMARY.md
started: 2026-03-09T10:00:00Z
updated: 2026-03-09T16:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running server/service. Start the backend from scratch. Server boots without errors, all seeders complete (allergy catalog, appointment types, clinic schedule), and a health check or basic API call returns successfully. Start the frontend — it builds and loads without errors.
result: pass

### 2. Sidebar Navigation Active
expected: Patients and Appointments items in the sidebar are clickable and navigate to their respective pages (not disabled with "Coming soon" tooltip).
result: pass

### 3. Patient Registration (Medical)
expected: On /patients, clicking "Register" opens a dialog with sticky header/footer. Medical tab selected by default. Fill in name, DOB (date picker dropdown month/year and chevrons properly aligned matching shadcn/ui), gender, phone, and optional allergy (autocomplete with Vietnamese categories, free-text entry via "Add custom" option). Submit creates the patient and redirects to the new patient's profile page (not /patients/undefined). Patient has GK-YYYY-NNNN code. If server validation errors occur, field-level errors appear under the respective input fields, and a ServerValidationAlert shows near the top of the form.
result: pass

### 4. Patient List Search & Pagination
expected: /patients shows a DataTable with patient columns (code, name, type as readable string, status). Pagination row always visible. Search box matches patients by name (Vietnamese diacritics-insensitive), phone substring ('6543' matches '0987654321'), and patient code substring ('0001' matches 'GK-2026-0001'). Filters for gender, allergy, and date range are available.
result: pass

### 5. Global Search (Ctrl+K)
expected: Pressing Ctrl+K opens GlobalSearch overlay. Recently viewed patients appear on focus without typing. Typing a patient name, phone substring, or patient code substring shows matching results with patient codes and phone numbers. Clicking a result navigates to the patient profile.
result: pass

### 6. Patient Profile Header & Breadcrumb
expected: Patient profile page shows breadcrumb with patient full name (not UUID). Profile header uses Card layout with large avatar (initials fallback), icon-labeled metadata (phone, DOB with calculated age, gender as readable string). Three tabs visible: Overview, Allergies, Appointments. AllergyAlert banner with rounded corners appears if patient has allergies.
result: pass

### 7. Patient Inline Edit
expected: On Overview tab, clicking Edit switches to inline form. DOB date picker shows the patient's current date of birth (not blank). Editing a field and clicking Save updates successfully (no 401 or 500 error). View returns to read-only mode with updated values — stays on the patient profile page.
result: pass

### 8. Allergy Management & Alert
expected: On Allergies tab, clicking "Add Allergy" opens a dialog with autocomplete showing Vietnamese categories and free-text entry. Selecting an allergy with severity and saving succeeds (no 500 error). Allergy appears in list with color-coded severity badge (Mild/Moderate/Severe). Removing an allergy shows confirmation and deletes it. AllergyAlert banner updates accordingly.
result: pass

### 9. Patient Deactivate/Reactivate & Field Warnings
expected: Profile header has Deactivate button. Clicking shows AlertDialog confirmation. Confirming deactivates patient (status changes). Reactivate button appears to restore. On Overview tab, if address or CCCD fields are empty, an amber warning banner indicates missing fields.
result: pass

### 10. Appointment Calendar & Staff Booking
expected: /appointments shows FullCalendar weekly view with 30-min time slots. DoctorSelector dropdown shows doctors. Clicking an empty time slot opens booking dialog pre-populated with clicked date/time. Dialog has patient search autocomplete, appointment type selector (4 types with durations), DatePicker + time Select combo (not native datetime-local). Textareas have rounded corners. Submitting creates the appointment on the calendar.
result: issue
reported: "when click on 13:00 slot, the time in the form should show 13:00 by default. when select the time outside of working hour and click submit, it showing error in toast. I want to show error in the top of the form. When I submit an appointment at 13:00, it saved successfully but does not show in the calendar. I suspect when displaying it using UTC+0 timezone, but not local timezone, so it hide under non-working hour."
severity: blocker

### 11. Appointment Reschedule, Cancel & Double-Booking
expected: Clicking a calendar appointment shows detail dialog with patient/doctor/type/status. Cancel button opens cancellation reason selector (4 reasons). Dragging appointment to new time slot confirms reschedule. Attempting to book overlapping appointment for same doctor returns 409 Conflict error.
result: issue
reported: "detail dialog show info, but appointment type is showing in English, but not Vietnamese although user is using Vietnamese. Attempting to book overlapping appointment for same doctor showing error in toast, but I want to show in the top of the form. This should be a pattern accrossing the app."
severity: major

### 12. Public Self-Booking & Language Toggle
expected: /book (no login required) shows branded Ganka28 booking page with language toggle (VI/EN). Labels translate correctly. DatePicker header (arrows, month, year) is properly aligned. Form fields with labels do NOT have redundant placeholders. Submitting shows confirmation page with reference number (BK-YYMMDD-NNNN format).
result: issue
reported: "There are still redundant placeholders in Loại lịch hẹn, Ngày mong muốn, Giờ mong muốn"
severity: minor

### 13. Booking Status Check
expected: /book/status shows reference number input with rounded corners. Entering valid reference number displays booking status with color coding (yellow=pending, green=approved, red=rejected). All container elements have rounded corners.
result: pass

### 14. Pending Bookings Approval & Rejection
expected: On /appointments, Pending Bookings panel shows self-booking requests. Approving opens dialog with doctor dropdown, DatePicker + time Select — submitting succeeds (no 400 error). Rejecting opens dialog with reason textarea. Both dialogs have proper whitespace between header, body, and footer sections.
result: pass

### 15. Auth Token Refresh & Session Handling
expected: After logging in and waiting 15+ minutes (or manually expiring token), navigating or submitting a form does NOT show 401 errors. Token refreshes silently. If session truly expires, PatientProfilePage shows "Session Expired" message (not "Patient not found").
result: pass

### 16. Server Validation & Dialog Spacing
expected: Submitting a form with invalid data shows field-level error messages below the respective input fields (reusable pattern). Non-field errors show in ServerValidationAlert banner near top of form. All dialog forms across the app have consistent whitespace between DialogHeader, content body, and DialogFooter (flex flex-col with gap).
result: pass

### 17. Dashboard Recent Patients Widget
expected: Dashboard shows "Recent Patients" widget with clickable patient links. If no patients viewed, empty state message shown. Widget updates when viewing patient profiles.
result: pass

## Summary

total: 17
passed: 14
issues: 3
pending: 0
skipped: 0

## Gaps

- truth: "Clicking a time slot pre-populates the form time, validation errors show in-form not toast, and saved appointments display correctly on the calendar at local timezone"
  status: resolved
  reason: "User reported: when click on 13:00 slot, the time in the form should show 13:00 by default. when select the time outside of working hour and click submit, it showing error in toast. I want to show error in the top of the form. When I submit an appointment at 13:00, it saved successfully but does not show in the calendar. I suspect when displaying it using UTC+0 timezone, but not local timezone, so it hide under non-working hour."
  severity: blocker
  test: 10
  root_cause: "1) FullCalendar uses UTC-coercion (timeZone='Asia/Ho_Chi_Minh' without timezone plugin). Slot click Date has UTC values representing wall-clock time, but AppointmentBookingDialog extracts via getHours()/getMinutes() (local-time) instead of getUTCHours()/getUTCMinutes(). 2) onError handler routes DOUBLE_BOOKING and VALIDATION_ERROR to toast.error() instead of setNonFieldError(). 3) Appointments fetched as UTC ISO strings — FullCalendar strips Z offset under UTC-coercion, placing 13:00 Vietnam time at 06:00 (before slotMinTime), hiding it."
  artifacts:
    - path: "frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx"
      issue: "Lines 109-111, 128-130: uses getHours()/getMinutes() instead of getUTCHours()/getUTCMinutes()"
    - path: "frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx"
      issue: "Lines 168-172: DOUBLE_BOOKING and VALIDATION_ERROR routed to toast.error() instead of setNonFieldError()"
    - path: "frontend/src/features/scheduling/hooks/useAppointments.ts"
      issue: "Lines 57-58: passes raw UTC ISO strings to FullCalendar which strips Z offset under UTC-coercion"
    - path: "frontend/src/features/scheduling/components/AppointmentCalendar.tsx"
      issue: "Line 99: timeZone='Asia/Ho_Chi_Minh' without @fullcalendar/moment-timezone plugin"
  missing:
    - "Install @fullcalendar/moment-timezone and moment-timezone, register plugin in AppointmentCalendar"
    - "Or: use getUTCHours()/getUTCMinutes() in AppointmentBookingDialog and convert UTC ISO to wall-clock strings in useAppointments"
    - "Replace toast.error() with setNonFieldError() for DOUBLE_BOOKING and VALIDATION_ERROR"
  debug_session: ".planning/debug/uat10-calendar-booking-issues.md"
- truth: "Appointment detail dialog shows appointment type in user's selected language, and double-booking error shows in-form not toast"
  status: resolved
  reason: "User reported: detail dialog show info, but appointment type is showing in English, but not Vietnamese although user is using Vietnamese. Attempting to book overlapping appointment for same doctor showing error in toast, but I want to show in the top of the form. This should be a pattern accrossing the app."
  severity: major
  test: 11
  root_cause: "1) Backend GetAppointmentsByDoctor/ByPatient handlers only map appointmentType?.Name (English), ignoring NameVi. AppointmentDto lacks AppointmentTypeNameVi field. Frontend renders raw string without locale check. 2) Same toast.error() vs setNonFieldError() issue as test 10."
  artifacts:
    - path: "backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/AppointmentDto.cs"
      issue: "Line 15: only has AppointmentTypeName, no AppointmentTypeNameVi"
    - path: "backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByDoctor.cs"
      issue: "Line 40: maps only appointmentType?.Name (English)"
    - path: "backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByPatient.cs"
      issue: "Line 38: same English-only mapping"
    - path: "frontend/src/features/scheduling/components/AppointmentDetailDialog.tsx"
      issue: "Line 159: renders appointmentTypeName raw without locale selection"
    - path: "frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx"
      issue: "Line 309: hardcodes type.nameVi in dropdown — always Vietnamese even in English locale"
  missing:
    - "Add AppointmentTypeNameVi to AppointmentDto"
    - "Map NameVi in GetAppointmentsByDoctor and GetAppointmentsByPatient handlers"
    - "Frontend: select Name vs NameVi based on i18n.language"
    - "Fix AppointmentBookingDialog dropdown to use locale-aware type name"
  debug_session: ".planning/debug/uat11-appt-type-and-double-booking.md"
- truth: "Form fields with labels do NOT have redundant placeholders"
  status: resolved
  reason: "User reported: There are still redundant placeholders in Loại lịch hẹn, Ngày mong muốn, Giờ mong muốn"
  severity: minor
  test: 12
  root_cause: "BookingForm.tsx line 178: SelectValue has placeholder={t('appointmentType')} duplicating FieldLabel. Line 230: SelectValue has placeholder={t('selfBooking.preferredTime')} duplicating FieldLabel. DatePicker defaults to t('buttons.search') = 'Tìm kiếm' as placeholder when none provided."
  artifacts:
    - path: "frontend/src/features/booking/components/BookingForm.tsx"
      issue: "Line 178: redundant placeholder on appointment type SelectValue"
    - path: "frontend/src/features/booking/components/BookingForm.tsx"
      issue: "Line 230: redundant placeholder on preferred time SelectValue"
    - path: "frontend/src/shared/components/DatePicker.tsx"
      issue: "Line 40: default fallback placeholder 'Tìm kiếm' shown on booking date picker"
  missing:
    - "Remove placeholder prop from SelectValue on lines 178 and 230"
    - "Pass placeholder='' from BookingForm to DatePicker, or change DatePicker default"
  debug_session: ".planning/debug/uat12-redundant-placeholders-booking.md"
