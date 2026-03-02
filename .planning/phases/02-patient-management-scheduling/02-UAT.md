---
status: diagnosed
phase: 02-patient-management-scheduling
source: 02-01-SUMMARY.md, 02-02-SUMMARY.md, 02-03-SUMMARY.md, 02-04-SUMMARY.md, 02-05-SUMMARY.md, 02-06-SUMMARY.md
started: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:10:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Sidebar Navigation Active
expected: Patients and Appointments items in the sidebar are clickable and navigate to their pages (not disabled with "Coming soon" tooltip).
result: pass

### 2. Patient Registration (Medical)
expected: On /patients page, clicking "Register" opens a dialog. Filling in a Medical patient form (name, DOB, gender, phone, optional allergies) and submitting creates the patient. The new patient appears in the list with a GK-YYYY-NNNN code.
result: issue
reported: "Date picker can not select year and month quickly. Use tabs in to switch between tab instead of button and fixed at the top. Allergy input should be autocomplete. Dialog title should fixed at top and action buttons should be fixed at bottom. When submit form there is 404 code error"
severity: blocker

### 3. Patient List with Pagination & Filters
expected: /patients shows a DataTable with patient columns (code, name, type, status). Pagination controls work (next/previous page). Gender, allergy, and date range filters are visible and functional.
result: issue
reported: "I don't see next/previous page button, may be not enough patients. Search box does not work."
severity: major

### 4. Global Search (Ctrl+K)
expected: Pressing Ctrl+K opens the GlobalSearch overlay in the header. Typing a patient name shows matching results with patient codes and phone numbers. Clicking a result navigates to the patient profile.
result: pass

### 5. Patient Profile with Tabs
expected: Clicking a patient from the list opens /patients/$patientId with a profile header (avatar/initials, name, GK code, DOB with age) and three tabs: Overview, Allergies, Appointments.
result: issue
reported: "the breadcrum should show name of patient instead of id string"
severity: cosmetic

### 6. Patient Inline Edit
expected: On the Overview tab, clicking an Edit button switches to an inline form with editable fields. Saving updates the patient details and returns to read-only view.
result: issue
reported: "When click on DOB, the date picker does not show the selected value. Click save return 500 code error"
severity: blocker

### 7. Allergy Management & Alert Banner
expected: On the Allergies tab, clicking "Add Allergy" opens a dialog with autocomplete search from the ophthalmology catalog (26 items) plus free-text entry. Selecting an allergy and severity, then saving, shows it in the list with a severity badge. An orange AllergyAlert banner appears on the profile when allergies exist.
result: issue
reported: "allergy select is too bad. Replace it with shadcn/ui component. Cannot enter free text. All options are in English althrough user set vn language."
severity: major

### 8. Patient Deactivate/Reactivate
expected: On the profile header, clicking Deactivate shows an AlertDialog confirmation. Confirming deactivates the patient (status changes). A Reactivate button then appears to restore the patient.
result: pass

### 9. Appointment Calendar View
expected: /appointments shows a FullCalendar weekly view with time slots. A DoctorSelector dropdown is visible to filter by doctor. Existing appointments are color-coded by type (blue/green/orange/purple).
result: issue
reported: "There is no option in doctor dropdown although there are user with doctor role"
severity: major

### 10. Book Appointment (Staff)
expected: Clicking an empty time slot on the calendar opens a booking dialog. The dialog has patient search autocomplete, appointment type selector with duration, and date/time picker. Submitting creates the appointment on the calendar.
result: issue
reported: "There is no doctor in dropdown. Date time picker styling feel off, make sure it's using shadcn/ui standard component. Textarea does not have round corner, you need to fix this globally"
severity: major

### 11. Public Self-Booking Page
expected: Navigating to /book (no login required) shows a branded Ganka28 booking page with language toggle (VI/EN). The form has fields for name, phone, appointment type, preferred date, and notes. Submitting shows a confirmation page with a reference number (BK-YYMMDD-NNNN format).
result: issue
reported: "phone input should not have placeholder. Same for other fields becuase we already has labels. The datepick header (arrow, month, year) are not align properly."
severity: minor

### 12. Booking Status Check
expected: Navigating to /book/status shows a reference number input. Entering a valid reference number displays the booking status with color coding (yellow=pending, green=approved, red=rejected).
result: pass

### 13. Pending Bookings Approval Panel
expected: On the /appointments page, a Pending Bookings panel shows self-booking requests with Approve and Reject buttons. Approving opens a dialog to assign doctor and time. Rejecting opens a dialog for a reason.
result: issue
reported: "approving does not load any doctors, and form styling feel off, lack of spacing. reject form styling feel off too, make it better"
severity: major

### 14. Dashboard Recent Patients Widget
expected: On the dashboard, a "Recent Patients" widget shows recently viewed patients. If no patients have been viewed, an empty state message is shown.
result: pass

## Summary

total: 14
passed: 5
issues: 9
pending: 0
skipped: 0

## Gaps

- truth: "Patient registration form works end-to-end: date picker allows quick year/month selection, tabs switch via tabs not buttons, allergy input has autocomplete, dialog title fixed at top with action buttons at bottom, form submits successfully"
  status: failed
  reason: "User reported: Date picker can not select year and month quickly. Use tabs in to switch between tab instead of button and fixed at the top. Allergy input should be autocomplete. Dialog title should fixed at top and action buttons should be fixed at bottom. When submit form there is 404 code error"
  severity: blocker
  test: 2
  root_cause: "1) registerPatient() returns res.data as string but backend returns {Id: guid} object — navigate goes to /patients/[object Object] causing 404. 2) DialogContent base grid class conflicts with flex-col sticky layout. 3) AllergyRow onSelect sets display label instead of canonical English key."
  artifacts:
    - path: "frontend/src/features/patient/api/patient-api.ts"
      issue: "Line 171: res.data as string should extract .Id from response object"
    - path: "frontend/src/shared/components/ui/dialog.tsx"
      issue: "Line 41: base grid class conflicts with flex flex-col override for sticky header/footer"
    - path: "frontend/src/features/patient/components/PatientRegistrationForm.tsx"
      issue: "AllergyRow onSelect sets display label instead of item.en canonical key"
  missing:
    - "Extract .Id from registerPatient response: (res.data as {Id: string}).Id"
    - "Fix DialogContent base class to not conflict with flex-col layout"
    - "AllergyRow onSelect should set item.en as value, not display label"
  debug_session: ".planning/debug/patient-registration-uat-blockers.md"
- truth: "Patient list search box filters patients and pagination buttons are visible"
  status: failed
  reason: "User reported: I don't see next/previous page button, may be not enough patients. Search box does not work."
  severity: major
  test: 3
  root_cause: "PatientTable.tsx line 161 wraps pagination in pageCount > 1 guard — hidden when <=20 patients. Search is wired correctly end-to-end but insufficient test data makes it appear broken. Backend IPatientRepository has search param after CancellationToken (code smell)."
  artifacts:
    - path: "frontend/src/features/patient/components/PatientTable.tsx"
      issue: "Line 161: pagination controls hidden when pageCount <= 1"
    - path: "backend/src/Modules/Patient/Patient.Application/Interfaces/IPatientRepository.cs"
      issue: "Lines 13-20: search param placed after CancellationToken — violates .NET convention"
  missing:
    - "Always show pagination row with buttons disabled when pageCount <= 1"
    - "Move search param before CancellationToken in IPatientRepository.GetPagedAsync"
  debug_session: ""
- truth: "Patient profile breadcrumb shows patient name instead of raw UUID"
  status: failed
  reason: "User reported: the breadcrum should show name of patient instead of id string"
  severity: cosmetic
  test: 5
  root_cause: "SiteHeader.tsx breadcrumb logic detects UUID segments but labels them with static t('sidebar.detail') instead of looking up patient name. Patient name is available in recentPatientsStore (Zustand) but SiteHeader doesn't import it."
  artifacts:
    - path: "frontend/src/shared/components/SiteHeader.tsx"
      issue: "Lines 78-80: UUID segments replaced with static 'Detail' label instead of entity name"
  missing:
    - "Import useRecentPatientsStore and look up patient name by ID when previous segment is 'patients'"
  debug_session: ".planning/debug/breadcrumb-shows-uuid-instead-of-patient-name.md"
- truth: "Patient inline edit saves successfully with DOB date picker showing current value"
  status: failed
  reason: "User reported: When click on DOB, the date picker does not show the selected value. Click save return 500 code error"
  severity: blocker
  test: 6
  root_cause: "1) useForm defaultValues set on first mount but not reset when patient prop changes — DOB picker shows stale/blank value. 2) updatePatient sends patientId in both URL and body; if body deserialization fails it causes 500. Also missing DbUpdateConcurrencyException handling."
  artifacts:
    - path: "frontend/src/features/patient/components/PatientOverviewTab.tsx"
      issue: "Missing useEffect to reset form when patient prop changes"
    - path: "frontend/src/features/patient/api/patient-api.ts"
      issue: "updatePatient includes patientId in body (from denormalizeForApi) — redundant and risky"
    - path: "backend/src/Modules/Patient/Patient.Application/Features/UpdatePatient.cs"
      issue: "UpdatePatientHandler does not catch DbUpdateConcurrencyException"
  missing:
    - "Add useEffect to reset form on patient prop change"
    - "Strip patientId from PUT request body since it's in URL"
    - "Handle concurrency exception in UpdatePatientHandler"
  debug_session: ".planning/debug/uat6-patient-inline-edit.md"
- truth: "Allergy form uses proper shadcn/ui autocomplete with free-text entry and Vietnamese translations"
  status: failed
  reason: "User reported: allergy select is too bad. Replace it with shadcn/ui component. Cannot enter free text. All options are in English althrough user set vn language."
  severity: major
  test: 7
  root_cause: "1) Input wrapped in PopoverTrigger causes click-to-toggle popover collapse. 2) No 'Add custom' CommandItem when filtered is empty — free text impossible. 3) Category labels are hardcoded English strings with no i18n. 4) cmdk internal filtering conflicts with manual filtered array."
  artifacts:
    - path: "frontend/src/features/patient/components/AllergyForm.tsx"
      issue: "Input-as-PopoverTrigger anti-pattern; no free-text confirmation item; category without i18n"
    - path: "frontend/src/features/patient/api/patient-api.ts"
      issue: "ALLERGY_CATALOG_BILINGUAL has English-only category field"
    - path: "frontend/public/locales/vi/patient.json"
      issue: "Missing allergyCategory translation keys"
  missing:
    - "Rewrite allergy field: Input outside PopoverTrigger, add dynamic 'Add custom' CommandItem"
    - "Add allergyCategory i18n keys to both locale files"
    - "Pass shouldFilter={false} to Command to disable cmdk internal filtering"
  debug_session: ".planning/debug/allergy-form-ux-issues.md"
- truth: "Doctor dropdown in appointment calendar shows users with doctor role"
  status: failed
  reason: "User reported: There is no option in doctor dropdown although there are user with doctor role"
  severity: major
  test: 9
  root_cause: "DoctorSelector.tsx calls /api/auth/users which does not exist — correct endpoint is /api/admin/users. Also response is paginated envelope {data:[...]} but code casts as bare array."
  artifacts:
    - path: "frontend/src/features/scheduling/components/DoctorSelector.tsx"
      issue: "Line 23: calls /api/auth/users (non-existent); line 26: casts response as bare array but actual shape is {data: UserDto[]} envelope"
  missing:
    - "Fix URL to /api/admin/users and unwrap .data from paginated envelope before filtering by role"
  debug_session: ".planning/debug/uat-tests-9-10-13-doctor-dropdown.md"
- truth: "Book appointment dialog has working doctor dropdown, proper shadcn/ui date/time picker, and rounded textarea corners"
  status: failed
  reason: "User reported: There is no doctor in dropdown. Date time picker styling feel off, make sure it's using shadcn/ui standard component. Textarea does not have round corner, you need to fix this globally"
  severity: major
  test: 10
  root_cause: "Same DoctorSelector bug as test 9. AppointmentBookingDialog uses native <Input type='datetime-local'> instead of shadcn Calendar+time. Raw <textarea> elements missing rounded-md globally."
  artifacts:
    - path: "frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx"
      issue: "Line 290: native datetime-local input instead of shadcn DatePicker; Line 302: raw textarea missing rounded-md"
    - path: "frontend/src/features/scheduling/components/PendingBookingsPanel.tsx"
      issue: "Raw textarea in reject dialog missing rounded-md"
    - path: "frontend/src/features/scheduling/components/AppointmentDetailDialog.tsx"
      issue: "Raw textarea for cancel note missing rounded-md"
  missing:
    - "Replace native datetime-local with shadcn DatePicker + time selector"
    - "Add global textarea rounded-md via globals.css or create shared Textarea component"
  debug_session: ".planning/debug/uat-tests-9-10-13-doctor-dropdown.md"
- truth: "Public booking page has no redundant placeholders when labels exist, and datepicker header elements are properly aligned"
  status: failed
  reason: "User reported: phone input should not have placeholder. Same for other fields becuase we already has labels. The datepick header (arrow, month, year) are not align properly."
  severity: minor
  test: 11
  root_cause: "BookingForm.tsx has placeholder attributes on Input components despite having visible FieldLabel elements. calendar.tsx nav uses position:absolute overlaying caption row — when captionLayout='dropdown', arrows and month/year dropdowns misalign."
  artifacts:
    - path: "frontend/src/features/booking/components/BookingForm.tsx"
      issue: "Lines 123,135,149,199,242: placeholder props duplicate label text"
    - path: "frontend/src/shared/components/ui/calendar.tsx"
      issue: "Lines 50-53: nav absolute positioning causes misalignment with dropdown caption"
  missing:
    - "Remove placeholder attributes from BookingForm fields that have labels"
    - "Fix calendar.tsx: restructure nav+caption as proper flex row instead of absolute overlay"
  debug_session: ""
- truth: "Pending bookings approval dialog loads doctors, has proper spacing; reject dialog has polished styling"
  status: failed
  reason: "User reported: approving does not load any doctors, and form styling feel off, lack of spacing. reject form styling feel off too, make it better"
  severity: major
  test: 13
  root_cause: "Same DoctorSelector bug as test 9. Approve/reject dialogs use raw <label> elements instead of Field/FieldLabel wrappers — inconsistent styling and spacing."
  artifacts:
    - path: "frontend/src/features/scheduling/components/PendingBookingsPanel.tsx"
      issue: "Lines 207-223 (approve) and 257-266 (reject): raw labels instead of Field/FieldLabel; missing consistent spacing"
  missing:
    - "Use Field/FieldLabel wrappers in approve and reject dialogs for consistent styling"
    - "DoctorSelector fix (same as test 9) resolves doctor loading"
  debug_session: ".planning/debug/uat-tests-9-10-13-doctor-dropdown.md"
