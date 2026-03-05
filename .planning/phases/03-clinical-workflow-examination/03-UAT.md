---
status: complete
phase: 03-clinical-workflow-examination
source: 03-01-SUMMARY.md, 03-02-SUMMARY.md, 03-03-SUMMARY.md, 03-04-SUMMARY.md, 03-05-SUMMARY.md, 03-06-SUMMARY.md, 03-07-SUMMARY.md, 03-08-SUMMARY.md, 03-09-SUMMARY.md
started: 2026-03-05T02:55:00Z
updated: 2026-03-05T03:10:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running backend/frontend servers. Start backend (port 5255) and frontend (port 3000) from scratch. Backend boots without errors, migrations apply, and navigating to http://localhost:3000/clinical loads without errors.
result: pass

### 2. Kanban Dashboard Renders
expected: Navigate to /clinical. Five columns render with Vietnamese labels (Tiếp nhận, Khám nghiệm, Bác sĩ, Xử lý, Hoàn tất). Sidebar shows "Clinical" nav item as active.
result: pass

### 3. Create Walk-in Visit
expected: Click the new visit button on the Kanban dashboard. A dialog opens with patient search and doctor selector. Search for an existing patient, select a doctor, submit. A new patient card appears in the Reception (Tiếp nhận) column.
result: pass

### 4. Patient Card Display
expected: A patient card on the Kanban shows: patient name, doctor name, visit time, exact stage badge, wait time badge, and allergy warning icon (if patient has allergies).
result: pass

### 5. Advance Stage via Button
expected: Click the "Chuyển tiếp" (Advance) button on a patient card. The card moves to the next column (e.g., from Reception to Testing). The stage badge updates accordingly.
result: pass

### 6. Navigate to Visit Detail
expected: Click on a patient card on the Kanban. Browser navigates to /visits/{visitId}. The visit detail page renders with 6 collapsible card sections (Patient Info, Refraction, Examination Notes, Diagnosis, Sign-off, Amendment History).
result: pass

### 7. Refraction Data Entry & Auto-save
expected: Open the Refraction section on visit detail. Enter values in OD fields (e.g., SPH: -2.50, CYL: -1.00, AXIS: 90). Click outside (blur). Data auto-saves. Refresh the page — entered values persist.
result: issue
reported: "Refraction data saves to database (API returns 200, odSph:-9 persisted), but after page reload the form fields are empty. Root cause: backend RefractionDto uses field name 'type' but frontend RefractionDto expects 'refractionType'. The getRefractionByType() lookup always returns undefined because r.refractionType is undefined (API sends 'type')."
severity: major

### 8. Refraction Tabs with Data Indicator
expected: In the Refraction section, three tabs are visible: Manifest, Autorefraction, Cycloplegic. After entering data in Manifest tab, a (*) indicator appears on that tab. Switching tabs shows empty forms for tabs without data.
result: issue
reported: "Three tabs render correctly, but (*) data indicator never appears because it depends on getRefractionByType() which always returns undefined due to the 'type' vs 'refractionType' field name mismatch (same root cause as Test 7)."
severity: major

### 9. Examination Notes Auto-save
expected: Open the Examination Notes section. Type notes in the textarea. Click outside (blur). Refresh the page — notes persist.
result: pass

### 10. ICD-10 Bilingual Search
expected: Open the Diagnosis section. Click the ICD-10 search combobox. Type a search term (e.g., "glaucoma" or Vietnamese equivalent). Results appear showing both Vietnamese and English descriptions.
result: pass

### 11. Add Diagnosis with Laterality
expected: Search for an ICD-10 code that requires laterality (e.g., an eye-specific code). After selecting, a laterality selector appears with MP (OD), MT (OS), 2M (OU) options. Select one and confirm. The diagnosis appears in the list with the correct laterality badge. If OU is selected, two records are created.
result: pass

### 12. Sign-Off Locks Record
expected: With refraction and diagnosis data entered, click Sign Off at the bottom. An AlertDialog confirmation appears. Confirm. All fields become read-only (refraction inputs disabled, diagnosis add/remove disabled, notes textarea disabled). Visit status shows "Signed".
result: pass

### 13. Amendment Workflow
expected: After sign-off, click "Amend" button. A dialog appears requiring a reason (minimum 10 characters). Enter a reason and submit. Fields unlock for editing again. Visit status shows "Amended". Amendment History section shows the new amendment entry with field-level changes captured.
result: pass

### 14. Error Toast on Mutation Failure
expected: If any clinical mutation fails (e.g., saving refraction with invalid data, or simulating a network error), an error toast notification appears informing the user of the failure.
result: skipped
reason: Could not reliably trigger a mutation failure via Playwright automation. Error toasts are wired in code (onError callbacks confirmed in RefractionForm and DiagnosisSection).

## Summary

total: 14
passed: 10
issues: 2
pending: 0
skipped: 2

## Gaps

- truth: "Refraction data persists and is visible after page reload"
  status: failed
  reason: "User reported: Refraction data saves to database (API returns 200, odSph:-9 persisted), but after page reload the form fields are empty. Root cause: backend RefractionDto uses field name 'type' but frontend RefractionDto expects 'refractionType'. The getRefractionByType() lookup always returns undefined because r.refractionType is undefined (API sends 'type')."
  severity: major
  test: 7
  root_cause: "Frontend RefractionDto interface defines 'refractionType: number' but backend RefractionDto record has 'Type' (serialized as 'type' in JSON). RefractionSection.getRefractionByType() calls refractions.find(r => r.refractionType === type) which never matches."
  artifacts:
    - path: "frontend/src/features/clinical/api/clinical-api.ts"
      issue: "RefractionDto.refractionType should be 'type' to match backend JSON"
    - path: "frontend/src/features/clinical/components/RefractionSection.tsx"
      issue: "getRefractionByType uses r.refractionType which is always undefined"
  missing:
    - "Rename refractionType to type in frontend RefractionDto, OR add JsonPropertyName on backend"
  debug_session: ""

- truth: "Refraction tab (*) indicator shows for tabs with data"
  status: failed
  reason: "User reported: Three tabs render correctly, but (*) data indicator never appears because it depends on getRefractionByType() which always returns undefined due to the 'type' vs 'refractionType' field name mismatch (same root cause as Test 7)."
  severity: major
  test: 8
  root_cause: "Same as Test 7 - frontend/backend RefractionDto field name mismatch (type vs refractionType)"
  artifacts:
    - path: "frontend/src/features/clinical/components/RefractionSection.tsx"
      issue: "hasRefractionData gets undefined data due to getRefractionByType returning undefined"
  missing:
    - "Fix field name mismatch (same fix as Test 7)"
  debug_session: ""

- truth: "IOP method Select should not produce controlled/uncontrolled React warning"
  status: failed
  reason: "Console shows 'Select is changing from uncontrolled to controlled' and vice versa warnings despite Plan 03-09 fix"
  severity: minor
  test: 7
  root_cause: "RefractionForm IOP method Select value prop returns undefined when iopMethod is null (correct), but when form resets or data changes, the value flips between undefined and a string, triggering the React warning"
  artifacts:
    - path: "frontend/src/features/clinical/components/RefractionForm.tsx"
      issue: "IOP Select controlled/uncontrolled warning persists"
  missing:
    - "Ensure Select always receives either undefined (uncontrolled) or a string value (controlled), never switching between them"
  debug_session: ""
