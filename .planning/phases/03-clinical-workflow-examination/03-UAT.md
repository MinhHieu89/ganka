---
status: diagnosed
phase: 03-clinical-workflow-examination
source: 03-01-SUMMARY.md, 03-02-SUMMARY.md, 03-03-SUMMARY.md, 03-04-SUMMARY.md, 03-05-SUMMARY.md, 03-06-SUMMARY.md, 03-07-SUMMARY.md, 03-08-SUMMARY.md, 03-09-SUMMARY.md, 03-10-SUMMARY.md
started: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:30:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running backend/frontend servers. Start backend (port 5255) and frontend (port 3000) from scratch. Backend boots without errors, migrations apply, and navigating to http://localhost:3000/clinical loads without errors.
result: pass

### 2. Kanban Dashboard Renders
expected: Navigate to /clinical. Five columns render with Vietnamese labels (Tiep nhan, Kham nghiem, Bac si, Xu ly, Hoan tat). Sidebar shows "Clinical" nav item as active.
result: issue
reported: "it shows 'Không có bệnh nhân đang trong quy trình'"
severity: major

### 3. Create Walk-in Visit
expected: Click the new visit button on the Kanban dashboard. A dialog opens with patient search and doctor selector. Search for an existing patient, select a doctor, submit. A new patient card appears in the Reception column.
result: pass

### 4. Patient Card Display
expected: A patient card on the Kanban shows: patient name, doctor name, visit time, exact stage badge, wait time badge, and allergy warning icon (if patient has allergies).
result: pass

### 5. Advance Stage via Button
expected: Click the "Chuyen tiep" (Advance) button on a patient card. The card moves to the next column (e.g., from Reception to Testing). The stage badge updates accordingly.
result: pass

### 6. Navigate to Visit Detail
expected: Click on a patient card on the Kanban. Browser navigates to /visits/{visitId}. The visit detail page renders with 6 collapsible card sections (Patient Info, Refraction, Examination Notes, Diagnosis, Sign-off, Amendment History).
result: issue
reported: "there is no Amendment History"
severity: major

### 7. Refraction Data Entry & Auto-save
expected: Open the Refraction section on visit detail. Enter values in OD fields (e.g., SPH: -2.50, CYL: -1.00, AXIS: 90). Click outside (blur). Data auto-saves. Refresh the page — entered values persist and are visible in the form fields.
result: issue
reported: "there was bad request error { 'type': 'https://tools.ietf.org/html/rfc9110#section-15.5.1', 'title': 'One or more validation errors occurred.', 'status': 400, 'errors': { 'UcvaOd.Value': [ 'VA must be between 0.01 and 2.0.' ] } }. validation result should show under the field that has error."
severity: major

### 8. Refraction Tabs with Data Indicator
expected: In the Refraction section, three tabs are visible: Manifest, Autorefraction, Cycloplegic. After entering data in Manifest tab, a (*) indicator appears on that tab. Switching tabs shows empty forms for tabs without data.
result: pass

### 9. Examination Notes Auto-save
expected: Open the Examination Notes section. Type notes in the textarea. Click outside (blur). Refresh the page — notes persist.
result: pass

### 10. ICD-10 Bilingual Search
expected: Open the Diagnosis section. Click the ICD-10 search combobox. Type a search term (e.g., "glaucoma" or Vietnamese equivalent). Results appear showing both Vietnamese and English descriptions.
result: issue
reported: "it shows Unaccented Vietnamese, the seed to add data with accented Vietnamese"
severity: minor

### 11. Add Diagnosis with Laterality
expected: Search for an ICD-10 code that requires laterality (e.g., an eye-specific code). After selecting, a laterality selector appears with MP (OD), MT (OS), 2M (OU) options. Select one and confirm. The diagnosis appears in the list with the correct laterality badge. If OU is selected, two records are created.
result: pass

### 12. Sign-Off Locks Record
expected: With refraction and diagnosis data entered, click Sign Off at the bottom. An AlertDialog confirmation appears. Confirm. All fields become read-only (refraction inputs disabled, diagnosis add/remove disabled, notes textarea disabled). Visit status shows "Signed".
result: pass

### 13. Amendment Workflow
expected: After sign-off, click "Amend" button. A dialog appears requiring a reason (minimum 10 characters). Enter a reason and submit. Fields unlock for editing again. Visit status shows "Amended". Amendment History section shows the new amendment entry with field-level changes captured.
result: issue
reported: "New Value shows pending_amendment. I edited only on field but it shows 3 rows"
severity: major

### 14. Error Toast on Mutation Failure
expected: If any clinical mutation fails (e.g., saving refraction with invalid data, or simulating a network error), an error toast notification appears informing the user of the failure.
result: pass

## Summary

total: 14
passed: 9
issues: 5
pending: 0
skipped: 0

## Gaps

- truth: "Five columns render with Vietnamese labels on the Kanban dashboard"
  status: failed
  reason: "User reported: it shows 'Không có bệnh nhân đang trong quy trình'"
  severity: major
  test: 2
  root_cause: "WorkflowDashboard.tsx lines 217-250: mutually exclusive conditional render — columns only render when totalPatients > 0, empty state replaces columns entirely"
  artifacts:
    - path: "frontend/src/features/clinical/components/WorkflowDashboard.tsx"
      issue: "Kanban columns hidden behind totalPatients > 0 conditional"
  missing:
    - "Always render DndContext and 5 KanbanColumn components regardless of totalPatients"
  debug_session: ".planning/debug/kanban-empty-state.md"

- truth: "Visit detail page renders 6 collapsible card sections including Amendment History"
  status: failed
  reason: "User reported: there is no Amendment History"
  severity: major
  test: 6
  root_cause: "VisitDetailPage.tsx line 131: VisitAmendmentHistory wrapped in {visit.amendments.length > 0 && ...} conditional, hiding section when no amendments exist"
  artifacts:
    - path: "frontend/src/features/clinical/components/VisitDetailPage.tsx"
      issue: "Conditional render hides Amendment History when amendments array is empty"
  missing:
    - "Remove conditional wrapper so VisitAmendmentHistory always renders (matches all other sections)"
  debug_session: ".planning/debug/amendment-history-missing.md"

- truth: "Refraction auto-save on blur with validation errors shown under the field"
  status: failed
  reason: "User reported: bad request error with UcvaOd validation, validation result should show under the field that has error"
  severity: major
  test: 7
  root_cause: "Three compounding gaps: (1) onError discards error param entirely, only shows generic toast; (2) renderNumberInput has no error message rendering; (3) backend field names have .Value suffix (UcvaOd.Value) that doesn't map to form field names"
  artifacts:
    - path: "frontend/src/features/clinical/components/RefractionForm.tsx"
      issue: "onError ignores error param; renderNumberInput has no error display"
    - path: "frontend/src/shared/lib/server-validation.ts"
      issue: "No .Value suffix stripping for FluentValidation nullable property names"
  missing:
    - "Accept error in onError, call handleServerValidationError with fieldMap"
    - "Add error message rendering under each input in renderNumberInput"
    - "Strip .Value suffix in server-validation.ts or provide fieldMap"
  debug_session: ".planning/debug/refraction-validation-errors-not-shown.md"

- truth: "ICD-10 search results show accented Vietnamese descriptions"
  status: failed
  reason: "User reported: it shows Unaccented Vietnamese, the seed to add data with accented Vietnamese"
  severity: minor
  test: 10
  root_cause: "Seed data file icd10-ophthalmology.json contains all 130 descriptionVi values in unaccented ASCII Vietnamese. Seeder is insert-only (skips existing codes) so fixing JSON alone won't update DB."
  artifacts:
    - path: "backend/src/Modules/Audit/Audit.Infrastructure/Seeding/icd10-ophthalmology.json"
      issue: "All 130 descriptionVi values lack Vietnamese diacritical marks"
    - path: "backend/src/Modules/Audit/Audit.Infrastructure/Seeding/Icd10Seeder.cs"
      issue: "Insert-only seeder skips existing codes, won't update fixed descriptions"
  missing:
    - "Rewrite all 130 descriptionVi values with proper Vietnamese diacritics"
    - "Change seeder to upsert or create migration to update existing records"
  debug_session: ".planning/debug/icd10-unaccented-vietnamese.md"

- truth: "Amendment history shows accurate field-level changes for only the edited fields"
  status: failed
  reason: "User reported: New Value shows pending_amendment. I edited only on field but it shows 3 rows"
  severity: major
  test: 13
  root_cause: "Three bugs: (1) buildFieldChangesSnapshot runs at amendment initiation (before edits) with 'pending_amendment' placeholder; (2) snapshot includes all non-empty fields, not just changed ones; (3) property name mismatch — AmendmentDialog uses 'fieldName' but VisitAmendmentHistory reads 'field'"
  artifacts:
    - path: "frontend/src/features/clinical/components/AmendmentDialog.tsx"
      issue: "buildFieldChangesSnapshot captures diff at wrong time with placeholder values and wrong property name"
    - path: "frontend/src/features/clinical/components/VisitAmendmentHistory.tsx"
      issue: "Reads 'field' property but snapshot sends 'fieldName'"
    - path: "backend/src/Modules/Clinical/Clinical.Application/Features/AmendVisit.cs"
      issue: "Stores snapshot without computing actual diff"
  missing:
    - "Compute diff at re-sign time comparing baseline to current state"
    - "Only include actually changed fields in the diff"
    - "Fix property name consistency (fieldName vs field)"
  debug_session: ".planning/debug/amendment-history-wrong-diff.md"
