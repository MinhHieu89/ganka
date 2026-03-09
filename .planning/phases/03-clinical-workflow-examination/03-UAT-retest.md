---
status: complete
phase: 03-clinical-workflow-examination
source: 03-11-SUMMARY.md, 03-12-SUMMARY.md, 03-13-SUMMARY.md
started: 2026-03-09T08:30:00Z
updated: 2026-03-09T09:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Kanban Dashboard Empty State
expected: Navigate to /clinical. Five columns render with Vietnamese labels regardless of whether active visits exist. No "Không có bệnh nhân đang trong quy trình" message blocking the columns.
result: pass

### 2. Amendment History Always Visible
expected: Navigate to any visit detail page (/visits/{visitId}). The Amendment History section is visible as a collapsible card — even if no amendments exist. It should show "No amendments" or an empty table, not be completely hidden.
result: pass

### 3. Refraction Validation Error Display
expected: Open a visit's Refraction section. Enter an invalid value (e.g., VA of 5.0 which exceeds the 0.01-2.0 range). Blur the field. A validation error message appears under the specific field with red border styling. The error clears when you correct the value.
result: issue
reported: "error message shows but not localized. input value does not display decimal, cannot enter 5.0, only 5 is accepted"
severity: major

### 4. ICD-10 Vietnamese Diacritics
expected: Open Diagnosis section on a visit. Search for an ICD-10 code (e.g., "glaucoma" or "viêm"). Vietnamese descriptions display with proper diacritical marks (e.g., "Glaucoma góc mở nguyên phát" not "Glaucoma goc mo nguyen phat").
result: issue
reported: "search for 'viem' does not return 'viêm'"
severity: major

### 5. Amendment Field-Level Diff
expected: On a signed visit, click Amend, enter a reason. Edit exactly ONE field (e.g., change SPH value). Re-sign the visit. In Amendment History, exactly 1 row appears showing the field name, old value, and new value — no "pending_amendment" text, no extra rows for unchanged fields.
result: issue
reported: "it show (none) and (added) for old value and new value. Should show exact value. field name is not localized"
severity: major

## Summary

total: 5
passed: 2
issues: 3
pending: 0
skipped: 0

## Gaps

- truth: "Refraction validation errors display under the specific field with localized messages and decimal input works"
  status: failed
  reason: "User reported: error message shows but not localized. input value does not display decimal, cannot enter 5.0, only 5 is accepted"
  severity: major
  test: 3
  root_cause: ""
  artifacts: []
  missing: []
  debug_session: ""

- truth: "Amendment history shows accurate old/new values and localized field names"
  status: failed
  reason: "User reported: it show (none) and (added) for old value and new value. Should show exact value. field name is not localized"
  severity: major
  test: 5
  root_cause: ""
  artifacts: []
  missing: []
  debug_session: ""

- truth: "ICD-10 search matches unaccented input to accented Vietnamese descriptions"
  status: failed
  reason: "User reported: search for 'viem' does not return 'viêm'"
  severity: major
  test: 4
  root_cause: ""
  artifacts: []
  missing: []
  debug_session: ""
