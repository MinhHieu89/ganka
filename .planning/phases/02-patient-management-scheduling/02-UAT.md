---
status: diagnosed
phase: 02-patient-management-scheduling
source: 02-01-SUMMARY.md, 02-02-SUMMARY.md, 02-03-SUMMARY.md, 02-04-SUMMARY.md, 02-05-SUMMARY.md, 02-06-SUMMARY.md, 02-07-SUMMARY.md, 02-08-SUMMARY.md, 02-09-SUMMARY.md, 02-10-SUMMARY.md
started: 2026-03-02T14:00:00Z
updated: 2026-03-02T14:20:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Sidebar Navigation Active
expected: Patients and Appointments items in the sidebar are clickable and navigate to their pages (not disabled with "Coming soon" tooltip).
result: pass

### 2. Patient Registration (Medical)
expected: On /patients page, clicking "Register" opens a dialog with sticky header/footer. Medical tab is selected by default. Filling in name, DOB (date picker has dropdown month/year selection), gender, phone, and optional allergy (autocomplete with Vietnamese categories and free-text entry). Submitting creates the patient and navigates to the new patient's profile page (not a 404 or [object Object] URL). The patient has a GK-YYYY-NNNN code.
result: issue
reported: "broken date picker in patient registration form, month and year dropdown and chevrons are not align correctly, it must match original component from shadcn/ui 100%. Make sure it works in all form. After submit patient registration form, user is redirected to patients/undefined. allergy input is not autocomplete with Vietnamese categories and free-text entry, kind of broken, you should delete current autocomplete component and istall a new one. When there is validation error from server, Show validation error in under the field that has an error. Make this is reusable pattern across the app. User friendly exception should show about the first field on the form (top of the form without tab, if the form has tab, show under the tab when in a dialog. If form is not in dialog, User friendly exception should show in Alert dialog."
severity: blocker

### 3. Patient List Search & Pagination
expected: /patients shows a DataTable with patient columns. Pagination row is always visible (buttons disabled when only one page). Typing in the search box filters patients by name, phone, or patient code (Vietnamese diacritics-insensitive).
result: issue
reported: "search box does not match patient code and phone by pattern. '0001' should match 'GK-2026-0001', '6543' should match 0987654321"
severity: major

### 4. Global Search (Ctrl+K)
expected: Pressing Ctrl+K opens the GlobalSearch overlay. Recently viewed patients appear on focus. Typing a patient name shows matching results with patient codes and phone numbers. Clicking a result navigates to the patient profile.
result: issue
reported: "same string match issue with test 3"
severity: major

### 5. Patient Profile Breadcrumb & Tabs
expected: Clicking a patient opens /patients/$patientId. The breadcrumb shows the patient's full name (not a raw UUID). Profile header shows avatar/initials, name, GK code, DOB with calculated age. Three tabs are visible: Overview, Allergies, Appointments.
result: issue
reported: "Profile header show correctly. But you should use frontend design skill to make it more elegant and clean. It's kind of boring now."
severity: minor

### 6. Patient Inline Edit
expected: On the Overview tab, clicking Edit switches to inline form. The DOB date picker shows the patient's current date of birth (not blank). Editing a field and clicking Save updates successfully (no 500 error). The view returns to read-only mode with updated values.
result: issue
reported: "after update, user is redirect to a page show Không tìm thấy bệnh nhân Đã xảy ra lỗi. there was 401 code error, it only work after refresh page."
severity: blocker

### 7. Allergy Management & Alert Banner
expected: On the Allergies tab, clicking "Add Allergy" opens a dialog with shadcn/ui autocomplete. Categories display in the current language (Vietnamese if VI selected). Typing filters the catalog. Typing a custom name shows "Add custom: {value}" option. Selecting an allergy with severity and saving shows it in the list with a severity badge. An orange AllergyAlert banner appears on the profile header when allergies exist.
result: issue
reported: "500 Internal Server Error when submit form"
severity: blocker

### 8. Patient Deactivate/Reactivate
expected: On the profile header, clicking Deactivate shows an AlertDialog confirmation. Confirming deactivates the patient (status changes). A Reactivate button then appears to restore the patient.
result: pass

### 9. Appointment Calendar View
expected: /appointments shows a FullCalendar weekly view with time slots. A DoctorSelector dropdown is visible and populated with users who have the Doctor role. Existing appointments are color-coded by type.
result: issue
reported: "400 Bad Request when approve appointment with payload {doctorId: '1b0fe92e-64a0-4e35-9299-969dd0f82ab2', doctorName: 'Dr. Test User', patientName: 'Pham Van Public', startTime: '2026-03-04T07:00:00.000Z'}"
severity: blocker

### 10. Book Appointment (Staff)
expected: Clicking an empty time slot opens a booking dialog. The dialog has patient search autocomplete, appointment type selector with duration, and a shadcn DatePicker + time Select combo (not native datetime-local). Textarea fields have rounded corners. Submitting creates the appointment on the calendar.
result: issue
reported: "submit form does not call api, no error show in form. When click on a time slot, the form should pre-populate with the time that was clicked on"
severity: blocker

### 11. Public Self-Booking Page
expected: Navigating to /book (no login required) shows a branded Ganka28 booking page with language toggle (VI/EN). Form fields with labels do NOT have redundant placeholders. The DatePicker header (arrows, month, year) is properly aligned. Submitting shows a confirmation page with a reference number (BK-YYMMDD-NNNN format).
result: issue
reported: "date picker has the same issue as test 2. Submitting shows a confirmation page with a reference number correctly"
severity: minor

### 12. Booking Status Check
expected: Navigating to /book/status shows a reference number input. Entering a valid reference number displays the booking status with color coding (yellow=pending, green=approved, red=rejected).
result: issue
reported: "work ok but lack of round corner."
severity: cosmetic

### 13. Pending Bookings Approval Panel
expected: On /appointments, a Pending Bookings panel shows self-booking requests. Approving opens a dialog with doctor dropdown (populated), DatePicker + time Select, and Field/FieldLabel wrappers with proper spacing. Rejecting opens a styled dialog with reason textarea. Both dialogs use consistent shadcn/ui styling.
result: issue
reported: "400 code error when approve. reject works. But both form has no whitespace between dialog header and the form (dialog body), no whitespace between the form (dialog body) and the action button at the bottom. This is a global issue, you need to come up with re-usable components or pattern to fix it across the app."
severity: blocker

### 14. Dashboard Recent Patients Widget
expected: On the dashboard, a "Recent Patients" widget shows recently viewed patients. If no patients have been viewed, an empty state message is shown.
result: pass

### 15. Patient Field Warning Indicators
expected: On a patient profile where address or CCCD fields are empty, an amber warning banner appears in the Overview tab indicating which fields are missing for downstream workflows (referrals, legal export).
result: pass

## Summary

total: 15
passed: 4
issues: 11
pending: 0
skipped: 0

## Gaps

- truth: "Patient registration form works end-to-end: date picker dropdown month/year aligned like original shadcn/ui, allergy autocomplete with Vietnamese categories and free-text, form submits and navigates to patient profile, server validation errors shown under fields"
  status: failed
  reason: "User reported: broken date picker in patient registration form, month and year dropdown and chevrons are not align correctly, it must match original component from shadcn/ui 100%. Make sure it works in all form. After submit patient registration form, user is redirected to patients/undefined. allergy input is not autocomplete with Vietnamese categories and free-text entry, kind of broken, you should delete current autocomplete component and istall a new one. When there is validation error from server, Show validation error in under the field that has an error. Make this is reusable pattern across the app. User friendly exception should show about the first field on the form (top of the form without tab, if the form has tab, show under the tab when in a dialog. If form is not in dialog, User friendly exception should show in Alert dialog."
  severity: blocker
  test: 2
  root_cause: "1) calendar.tsx drifted from official shadcn/ui — custom classNames merge with rdp defaults causing nav/dropdown overlap when captionLayout=dropdown. 2) registerPatient extracts (res.data as {Id:string}).Id but backend returns camelCase {id:...} — returns undefined. 3) AllergyRow in PatientRegistrationForm has no category grouping, no free-text entry, no shouldFilter={false}. 4) Backend joins FluentValidation errors into flat string losing PropertyName; frontend only shows toast.error."
  artifacts:
    - path: "frontend/src/shared/components/ui/calendar.tsx"
      issue: "Lines 50-53: nav classNames merge custom + rdp defaults causing misalignment with dropdown caption"
    - path: "frontend/src/features/patient/api/patient-api.ts"
      issue: "Line 183: (res.data as {Id:string}).Id uses PascalCase but backend returns camelCase {id:...}"
    - path: "frontend/src/features/patient/components/PatientRegistrationForm.tsx"
      issue: "Lines 342-458: AllergyRow missing categories, free-text, shouldFilter={false}; Lines 137-141: only toast.error for server errors"
    - path: "backend/src/Modules/Patient/Patient.Application/Features/RegisterPatient.cs"
      issue: "Line 61: joins validation errors into flat string, losing PropertyName"
    - path: "backend/src/Shared/Shared.Presentation/ResultExtensions.cs"
      issue: "Lines 59-62: returns flat detail string, no errors dictionary"
  missing:
    - "Replace calendar.tsx with official shadcn/ui version (npx shadcn@latest add calendar)"
    - "Fix registerPatient to extract .id (camelCase) from response"
    - "Replace AllergyRow with proper combobox supporting categories, free-text, shouldFilter={false}"
    - "Backend: return structured validation errors preserving field names"
    - "Frontend: create reusable server validation error handler that maps to form.setError()"
  debug_session: ".planning/debug/uat-test2-patient-registration-medical.md"
- truth: "Patient list search matches by patient code substring and phone substring"
  status: failed
  reason: "User reported: search box does not match patient code and phone by pattern. '0001' should match 'GK-2026-0001', '6543' should match 0987654321"
  severity: major
  test: 3
  root_cause: "PatientRepository.cs uses == for PatientCode (exact match) and StartsWith for Phone (prefix only). Both need Contains for substring matching."
  artifacts:
    - path: "backend/src/Modules/Patient/Patient.Infrastructure/Repositories/PatientRepository.cs"
      issue: "Lines 41-42 (SearchAsync) and 69-70 (GetPagedAsync): PatientCode uses == and Phone uses StartsWith"
  missing:
    - "Change p.PatientCode == term to p.PatientCode.Contains(term) in both methods"
    - "Change p.Phone.StartsWith(term) to p.Phone.Contains(term) in both methods"
  debug_session: ".planning/debug/uat-t3-t4-search-substring-match.md"
- truth: "Global search matches by patient code substring and phone substring"
  status: failed
  reason: "User reported: same string match issue with test 3 — search does not match patient code and phone by substring pattern"
  severity: major
  test: 4
  root_cause: "Same as test 3 — PatientRepository.SearchAsync uses == for PatientCode and StartsWith for Phone"
  artifacts:
    - path: "backend/src/Modules/Patient/Patient.Infrastructure/Repositories/PatientRepository.cs"
      issue: "Lines 41-42: same operators as GetPagedAsync"
  missing:
    - "Fix applied in test 3 covers this (same file, same methods)"
  debug_session: ".planning/debug/uat-t3-t4-search-substring-match.md"
- truth: "Patient profile header is visually polished and elegant"
  status: failed
  reason: "User reported: Profile header show correctly. But you should use frontend design skill to make it more elegant and clean. It's kind of boring now."
  severity: minor
  test: 5
  root_cause: "PatientProfileHeader uses bare div with space-y-4, no Card/shadow/border. Basic avatar, flat metadata layout. AllergyAlert missing rounded corners."
  artifacts:
    - path: "frontend/src/features/patient/components/PatientProfileHeader.tsx"
      issue: "Line 112: no visual container (Card, shadow, border), plain layout"
    - path: "frontend/src/features/patient/components/AllergyAlert.tsx"
      issue: "Line 53: border without rounded-* class"
  missing:
    - "Redesign PatientProfileHeader using frontend-design skill for polished look"
    - "Add rounded corners to AllergyAlert"
  debug_session: ".planning/debug/uat-5-11-12-cosmetic-issues.md"
- truth: "Patient inline edit saves successfully and stays on patient profile page"
  status: failed
  reason: "User reported: after update, user is redirect to a page show Không tìm thấy bệnh nhân Đã xảy ra lỗi. there was 401 code error, it only work after refresh page."
  severity: blocker
  test: 6
  root_cause: "JWT refresh timer created on LoginForm's useRef, destroyed on unmount when navigating to dashboard. No 401 interceptor in API client. After 15min token expires, all API calls return 401. PatientProfilePage shows 'not found' for auth errors."
  artifacts:
    - path: "frontend/src/features/auth/hooks/useAuth.ts"
      issue: "Lines 20-57: scheduleRefresh timer lost on component unmount"
    - path: "frontend/src/shared/lib/api-client.ts"
      issue: "Lines 7-15: auth middleware has no onResponse 401 interceptor"
    - path: "frontend/src/features/patient/components/PatientProfilePage.tsx"
      issue: "Line 59: treats 401 same as 404"
  missing:
    - "Add onResponse 401 interceptor to api-client.ts that silently refreshes token and retries"
    - "Move token refresh scheduling to a persistent component (AppShell) or interceptor"
    - "Differentiate error types in PatientProfilePage"
  debug_session: ".planning/debug/uat6-inline-edit-401.md"
- truth: "Adding an allergy via the Allergies tab form submits successfully"
  status: failed
  reason: "User reported: 500 Internal Server Error when submit form"
  severity: blocker
  test: 7
  root_cause: "addAllergy sends severity as string ('Mild') but backend expects integer (0). No severityToInt conversion applied unlike registerPatient which uses denormalizeForApi()."
  artifacts:
    - path: "frontend/src/features/patient/api/patient-api.ts"
      issue: "Lines 259-271: addAllergy sends severity string directly, missing severityToInt conversion"
  missing:
    - "Apply severityToInt[data.severity] in addAllergy before sending to backend"
  debug_session: ".planning/debug/uat-test7-allergy-500-error.md"
- truth: "Approving a self-booking appointment succeeds and creates appointment on calendar"
  status: failed
  reason: "User reported: 400 Bad Request when approve appointment with payload {doctorId: '1b0fe92e-64a0-4e35-9299-969dd0f82ab2', doctorName: 'Dr. Test User', patientName: 'Pham Van Public', startTime: '2026-03-04T07:00:00.000Z'}"
  severity: blocker
  test: 9
  root_cause: "UTC/local timezone mismatch: frontend sends UTC time (07:00Z for 14:00 Vietnam), backend compares UTC TimeOfDay against local-time schedule (13:00-20:00). 07:00 < 13:00 fails validation."
  artifacts:
    - path: "backend/src/Modules/Scheduling/Scheduling.Application/Features/ApproveSelfBooking.cs"
      issue: "Lines 64-66: compares command.StartTime.TimeOfDay (UTC) against local-time schedule"
    - path: "backend/src/Modules/Scheduling/Scheduling.Application/Features/BookAppointment.cs"
      issue: "Lines 57-59: same timezone bug"
    - path: "backend/src/Modules/Scheduling/Scheduling.Domain/Entities/ClinicSchedule.cs"
      issue: "Lines 32-38: IsWithinHours is timezone-unaware"
  missing:
    - "Convert command.StartTime from UTC to Asia/Ho_Chi_Minh before extracting TimeOfDay and DayOfWeek"
  debug_session: ".planning/debug/uat-tests-9-10-13.md"
- truth: "Staff booking form submits successfully and pre-populates time from calendar slot click"
  status: failed
  reason: "User reported: submit form does not call api, no error show in form. When click on a time slot, the form should pre-populate with the time that was clicked on"
  severity: blocker
  test: 10
  root_cause: "1) doctorName form field always empty on dialog open — DoctorSelector onChange only passes ID, not name. Silent Zod validation failure with no visible error. 2) Calendar 15-min slots vs form 30-min options causes time mismatch. 3) Time slot click data not pre-populating form."
  artifacts:
    - path: "frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx"
      issue: "Lines 113-128: resets doctorName to ''; Line 258: sets doctorName to UUID; No FieldError for doctorName"
    - path: "frontend/src/features/scheduling/components/DoctorSelector.tsx"
      issue: "Lines 38-42: onChange only passes doctorId, not name"
    - path: "frontend/src/features/scheduling/components/AppointmentCalendar.tsx"
      issue: "15-min slotDuration vs 30-min form options"
  missing:
    - "DoctorSelector onChange should provide both ID and name"
    - "Form reset should resolve doctor name from defaultDoctorId"
    - "Add FieldError for doctorName to prevent silent validation failures"
    - "Align calendar slot duration with form time options"
    - "Pre-populate date and time from calendar slot click"
  debug_session: ".planning/debug/uat-tests-9-10-13.md"
- truth: "DatePicker dropdown month/year and chevrons are properly aligned in public booking page"
  status: failed
  reason: "User reported: date picker has the same issue as test 2 — month/year dropdown and chevrons misaligned"
  severity: minor
  test: 11
  root_cause: "Same calendar.tsx issue as test 2 — shared component fix resolves both"
  artifacts:
    - path: "frontend/src/shared/components/ui/calendar.tsx"
      issue: "Same as test 2"
  missing:
    - "Fix in test 2 (replace calendar.tsx) covers this"
  debug_session: ".planning/debug/uat-5-11-12-cosmetic-issues.md"
- truth: "Booking status check page elements have rounded corners"
  status: failed
  reason: "User reported: work ok but lack of round corner."
  severity: cosmetic
  test: 12
  root_cause: "BookingStatusCheck has 5 hand-written container divs using 'border' without any rounded-* class"
  artifacts:
    - path: "frontend/src/features/booking/components/BookingStatusCheck.tsx"
      issue: "Lines 86, 97, 111, 130, 154: divs missing rounded-lg"
  missing:
    - "Add rounded-lg to all 5 container divs"
  debug_session: ".planning/debug/uat-5-11-12-cosmetic-issues.md"
- truth: "Approving a pending booking succeeds and dialog forms have proper spacing between header/body/footer"
  status: failed
  reason: "User reported: 400 code error when approve. reject works. But both form has no whitespace between dialog header and the form (dialog body), no whitespace between the form (dialog body) and the action button at the bottom. This is a global issue, you need to come up with re-usable components or pattern to fix it across the app."
  severity: blocker
  test: 13
  root_cause: "1) Same 400 approve bug as test 9 (UTC timezone mismatch). 2) DialogContent has gap-4 but no flex flex-col — gap has no effect on block-level element. Global issue affecting all dialogs."
  artifacts:
    - path: "frontend/src/shared/components/ui/dialog.tsx"
      issue: "Line 41: gap-4 without flex flex-col, gap has no effect"
    - path: "backend/src/Modules/Scheduling/Scheduling.Application/Features/ApproveSelfBooking.cs"
      issue: "Same timezone bug as test 9"
  missing:
    - "Add flex flex-col to DialogContent so gap-4 takes effect"
    - "Timezone fix from test 9 covers the 400 error"
  debug_session: ".planning/debug/uat-tests-9-10-13.md"
