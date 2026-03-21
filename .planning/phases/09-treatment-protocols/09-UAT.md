---
status: diagnosed
phase: 09-treatment-protocols
source: 09-01-SUMMARY.md, 09-02-SUMMARY.md, 09-03-SUMMARY.md, 09-04-SUMMARY.md, 09-05-SUMMARY.md, 09-06-SUMMARY.md, 09-07-SUMMARY.md, 09-08-SUMMARY.md, 09-09-SUMMARY.md, 09-10-SUMMARY.md, 09-11-SUMMARY.md, 09-12-SUMMARY.md, 09-13-SUMMARY.md, 09-14-SUMMARY.md, 09-15-SUMMARY.md, 09-16-SUMMARY.md, 09-17-SUMMARY.md, 09-18-SUMMARY.md, 09-19-SUMMARY.md, 09-20-SUMMARY.md, 09-21-SUMMARY.md, 09-22-SUMMARY.md, 09-23-SUMMARY.md, 09-24-SUMMARY.md, 09-25-SUMMARY.md, 09-26-SUMMARY.md, 09-27-SUMMARY.md, 09-29-SUMMARY.md, 09-30-SUMMARY.md, 09-31-SUMMARY.md, 09-32-SUMMARY.md, 09-33-SUMMARY.md, 09-34-SUMMARY.md, 09-35-SUMMARY.md
started: 2026-03-21T10:00:00Z
updated: 2026-03-21T11:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running server/service. Start the application from scratch. Server boots without errors, treatment database migration applies (6 tables in treatment schema), Treatment API endpoints are reachable under /api/treatments/*, and all endpoints require authorization.
result: pass

### 2. Sidebar navigation and routing
expected: Sidebar shows 'Treatments' menu with 3 sub-items: Treatments list (/treatments), Templates (/treatments/templates), and Approvals (/treatments/approvals). Clicking each navigates to the corresponding page. Both English and Vietnamese translations display correctly for sidebar items.
result: pass

### 3. Protocol template CRUD with i18n
expected: At /treatments/templates, user sees a DataTable of protocol templates with columns for name, treatment type (colored badge), sessions, pricing, interval, deduction %, and active status. User can create a new template via dialog with treatment-type-specific parameters (IPL: energy/pulse/spot size/zones; LLLT: wavelength/power/duration/area; LidCare: steps/products/duration). Validation enforces session count 1-6, deduction 10-20%, and non-negative prices. All labels display correctly in both English and Vietnamese.
result: pass

### 4. Treatment package creation with structured parameter fields
expected: From /treatments page, user opens 'Create Package' dialog, selects a protocol template, and form auto-populates with template defaults. User can search and select a patient via combobox. Treatment parameters are shown as structured input fields (not raw JSON textarea) matching the template type (IPL/LLLT/LidCare). On submit, the new package appears in the treatments list with Active status and 0/N sessions completed.
result: pass

### 5. Treatments list page with due-soon alerts
expected: At /treatments, user sees a DataTable of active treatment packages showing patient name, treatment type badge, status badge, progress bar (sessions completed/total), pricing, last session date, and next due date. Overdue packages are visually highlighted. A 'Due Soon' section shows packages eligible for their next session.
result: pass

### 6. Treatment package detail view with i18n
expected: At /treatments/:packageId, user sees package header with protocol name, type badge, status badge, patient link, progress bar, pricing, and dates. Session cards display in a grid. An OSDI trend chart shows score progression with colored severity bands. Action buttons (Record Session, Modify, Pause/Resume, Switch, Cancel) appear conditionally based on status. All labels display correctly in both English and Vietnamese.
result: pass

### 7. Record session with device parameters and OSDI card-based questionnaire
expected: From package detail, clicking 'Record Session' opens a dialog with type-specific device parameter fields (all with proper i18n labels). OSDI inline mode shows the full 12-question questionnaire using card-based UI with styled button answers (matching the public /osdi/:token page layout). Score auto-calculates severity (Normal/Mild/Moderate/Severe) with color-coded badge. Clinical notes textarea available. Consumable selector with quantity controls.
result: pass

### 8. OSDI patient self-fill via QR code
expected: In Record Session dialog, OSDI QR self-fill mode generates a QR code linking to a public OSDI page. The link works (no error page). Patient can complete the 12-question questionnaire on their device and submit. The score is captured back into the session form.
result: issue
reported: "The score was not captured back into the session form"
severity: major

### 9. Session interval warning (soft enforcement)
expected: If recording a session before the minimum interval days have elapsed since the last session, a yellow warning banner displays (e.g., "Last session was X days ago, minimum is Y days"). Recording is NOT blocked -- user can still submit the session.
result: issue
reported: "I don't see that warning - warning only appears after submit in API response, not proactively when opening the dialog"
severity: major

### 10. Package auto-completion on final session
expected: When the last remaining session of a treatment package is recorded, the package status automatically transitions to 'Completed'. The progress bar shows full completion (e.g., 4/4), and the Record Session button is no longer available.
result: pass

### 11. Mid-course modification with structured parameters and version history
expected: From package detail, clicking 'Modify' opens a dialog showing current values for total sessions, treatment parameters as structured input fields (IPL/LLLT/LidCare -- not raw JSON textarea), and min interval days. User can change values and must provide a reason. After saving, a version history entry is created. Version History dialog shows chronological modifications with version number, date, author, reason, and diff of changes. All labels in both English and Vietnamese.
result: issue
reported: "Version history missing localization - change description in English ('Session count changed from 4 to 6'), raw JSON diff not user friendly"
severity: minor

### 12. Pause and resume treatment package
expected: From an Active package, clicking 'Pause' transitions to Paused status (yellow badge). Record Session becomes unavailable. From Paused, clicking 'Resume' transitions back to Active (green badge). Record Session becomes available again.
result: issue
reported: "Paused badge not yellow. After pausing, package disappears from /treatments list (only shows Active). Pause button had no onClick handler (fixed during UAT)."
severity: major

### 13. Switch treatment type
expected: From package detail, clicking 'Switch Treatment Type' opens a dialog filtering templates to exclude current type. User selects new template and sees preview of close-and-create outcome. On confirm, old package marked as 'Switched', new package created with new template, user navigated to new package detail.
result: pass

### 14. Cancellation request with refund estimate
expected: From Active/Paused package, clicking 'Request Cancellation' opens dialog with package summary and refund estimate breakdown (remaining sessions, deduction %). On submit, package transitions to PendingCancellation status. Success toast displays.
result: issue
reported: "Missing localization - cancellation request status shows 'status.Requested' instead of translated text (fixed during UAT)"
severity: cosmetic

### 15. Manager approval queue and PIN verification
expected: At /treatments/approvals, manager sees DataTable of pending cancellations with patient, type, progress, requester, date, reason, and deduction. Approving requires manager PIN entry and allows adjusting deduction % (10-20%), with real-time refund calculation. Rejecting requires a reason and restores package to Active status.
result: issue
reported: "User with permission can approve without PIN input - PIN verification is bypassed"
severity: major

### 16. Patient profile treatments tab
expected: On a patient's profile page, a 'Treatments' tab appears. It displays the patient's treatment packages grouped by status: Active first, Completed in separate section, Cancelled/Switched collapsed by default. Each card shows type badge, template name, progress bar, dates. 'Create Package' button available.
result: issue
reported: "1) Create package from patient page still shows patient selector - should auto-fill and hide it. 2) Clicking 'View' on a treatment goes to /treatments/:id, but clicking back goes to /treatments instead of back to the patient's treatment tab."
severity: minor

### 17. Pharmacy consumable auto-deduction
expected: When a session is recorded with consumables, the pharmacy inventory is automatically updated. Stock balances decrease by the quantities used. If stock is insufficient, it deducts available amount without blocking the session.
result: pass

### 18. Vietnamese localization completeness
expected: Switch language to Vietnamese. Navigate through all treatment pages: templates list, create template dialog, treatments list, package detail, record session dialog, modify dialog, switch dialog, cancellation dialog, approvals page, patient treatments tab. All text displays in Vietnamese with proper diacritics. No hardcoded English strings or unaccented Vietnamese remain.
result: issue
reported: "Consumables selector shows both EN and VN names ('Anesthetic Eye Drops (Thuốc nhỏ tê)') instead of only the selected language name. Same rule should apply to entire app."
severity: minor

## Summary

total: 18
passed: 9
issues: 9
pending: 0
skipped: 0

## Gaps

- truth: "OSDI QR self-fill score should be captured back into the session form"
  status: failed
  reason: "User reported: The score was not captured back into the session form"
  severity: major
  test: 8
  root_cause: "Feature not implemented: SubmitOsdiQuestionnaireHandler saves score to DB but publishes no event. OsdiHub only supports visit-scoped groups (not token-scoped). SessionOsdiCapture has no listener (no SignalR, no polling) for score return."
  artifacts:
    - path: "backend/src/Modules/Clinical/Clinical.Application/Features/SubmitOsdiQuestionnaire.cs"
      issue: "Saves score but publishes no event for real-time notification"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Hubs/OsdiHub.cs"
      issue: "Only supports visit groups, not token groups"
    - path: "frontend/src/features/treatment/components/SessionOsdiCapture.tsx"
      issue: "No listener for score return after patient submits"
  missing:
    - "Extend SignalR hub to support token-scoped groups OR add polling endpoint"
    - "Publish event on OSDI submission for real-time notification"
    - "Add frontend listener (SignalR or polling) in SessionOsdiCapture"
  debug_session: ".planning/debug/osdi-qr-selffill-score-capture.md"

- truth: "Interval warning should display proactively when opening Record Session dialog if interval is too soon"
  status: failed
  reason: "User reported: I don't see that warning - warning only appears after submit in API response"
  severity: major
  test: 9
  root_cause: "TreatmentSessionForm receives no interval-related props (lastSessionDate, minIntervalDays). Parent TreatmentPackageDetail has the data in pkg but doesn't pass it. Warning only computed server-side in RecordTreatmentSessionHandler after form submission."
  artifacts:
    - path: "frontend/src/features/treatment/components/TreatmentSessionForm.tsx"
      issue: "Lines 310-311: intervalWarning set only from API response, never proactively"
    - path: "frontend/src/features/treatment/components/TreatmentPackageDetail.tsx"
      issue: "Lines 404-410: does not pass lastSessionDate or minIntervalDays to form"
  missing:
    - "Pass lastSessionDate and minIntervalDays as props from parent"
    - "Compute interval warning client-side in useEffect when dialog opens"
  debug_session: ".planning/debug/interval-warning-proactive.md"

- truth: "Version history should show localized change descriptions and user-friendly diff instead of raw JSON"
  status: failed
  reason: "User reported: Version history missing localization - change description in English, raw JSON diff not user friendly"
  severity: minor
  test: 11
  root_cause: "VersionHistoryDialog renders backend changeDescription string verbatim (English from ModifyTreatmentPackageHandler.BuildChangeDescription). JSON diff shows raw previousJson/currentJson snapshots."
  artifacts:
    - path: "frontend/src/features/treatment/components/VersionHistoryDialog.tsx"
      issue: "Renders changeDescription and JSON diff without translation or formatting"
  missing:
    - "Parse and translate change descriptions client-side from previousJson/currentJson diff"
    - "Replace raw JSON view with user-friendly field-by-field comparison"
  debug_session: ""

- truth: "Paused packages should show yellow badge and remain visible in /treatments list"
  status: failed
  reason: "User reported: Paused badge not yellow. After pausing, package disappears from /treatments list. Pause button had no onClick handler."
  severity: major
  test: 12
  root_cause: "1) TreatmentPackageDetail.tsx line 35 maps Paused to 'secondary' variant (gray). TreatmentsPage.tsx already has correct yellow styling. 2) TreatmentPackageRepository.GetActivePackagesAsync() filters Where(x => x.Status == PackageStatus.Active), excluding Paused/PendingCancellation."
  artifacts:
    - path: "frontend/src/features/treatment/components/TreatmentPackageDetail.tsx"
      issue: "Line 35: Paused mapped to 'secondary' (gray) instead of yellow"
    - path: "backend/src/Modules/Treatment/Treatment.Infrastructure/Repositories/TreatmentPackageRepository.cs"
      issue: "Line 56: .Where(x => x.Status == PackageStatus.Active) excludes Paused"
    - path: "backend/src/Modules/Treatment/Treatment.Application/Features/GetActiveTreatments.cs"
      issue: "Query semantics too restrictive"
  missing:
    - "Fix badge variant to use yellow styling matching TreatmentsPage"
    - "Broaden repository query to include Active, Paused, and PendingCancellation statuses"
  debug_session: ".planning/debug/pause-resume-issues.md"

- truth: "Manager approval should require PIN verification"
  status: failed
  reason: "User reported: User with permission can approve without PIN input"
  severity: major
  test: 15
  root_cause: "VerifyManagerPinHandler in Auth.Application is a stub that accepts any non-empty PIN. Frontend correctly collects and sends PIN. Backend ApproveCancellation correctly calls VerifyManagerPinQuery via IMessageBus. But the Auth handler just returns true for any non-empty string."
  artifacts:
    - path: "backend/src/Modules/Auth/Auth.Application/Features/VerifyManagerPin.cs"
      issue: "Stub implementation: returns true for any non-empty PIN"
  missing:
    - "Implement real PIN verification: inject user repository, retrieve stored hashed PIN, compare"
    - "Confirm User entity has ManagerPin field"
  debug_session: ".planning/debug/pin-bypass-and-consumable-lang.md"

- truth: "Create package from patient context should auto-fill patient and hide selector. Back button from treatment detail should return to patient page when navigated from there."
  status: failed
  reason: "User reported: Patient selector still shows. Back button goes to /treatments instead of patient treatment tab."
  severity: minor
  test: 16
  root_cause: "TreatmentPackageForm does not accept patientId/patientName props to pre-fill and hide selector. TreatmentPackageDetail back button hardcodes navigate({ to: '/treatments' }) instead of using browser history or referrer."
  artifacts:
    - path: "frontend/src/features/treatment/components/TreatmentPackageForm.tsx"
      issue: "No props for pre-filling patient context"
    - path: "frontend/src/features/treatment/components/TreatmentPackageDetail.tsx"
      issue: "Back button hardcodes /treatments instead of using history"
  missing:
    - "Accept optional patientId/patientName props to pre-fill and hide patient selector"
    - "Use navigate(-1) or pass referrer for back navigation"
  debug_session: ""

- truth: "Consumables selector should show only the name in the selected language, not both EN and VN"
  status: failed
  reason: "User reported: Shows both names like 'Anesthetic Eye Drops (Thuốc nhỏ tê)' instead of just the selected language"
  severity: minor
  test: 18
  root_cause: "ConsumableSelector.tsx hardcodes display of both names: renders item.name followed by (item.nameVi). Uses i18n for UI labels but not for selecting which consumable name to display."
  artifacts:
    - path: "frontend/src/features/treatment/components/ConsumableSelector.tsx"
      issue: "Lines 151-157: always renders both name and nameVi regardless of selected language"
  missing:
    - "Use i18n.language to conditionally pick item.name vs item.nameVi"
  debug_session: ".planning/debug/pin-bypass-and-consumable-lang.md"
