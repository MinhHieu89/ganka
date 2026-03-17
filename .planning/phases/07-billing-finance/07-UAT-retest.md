---
status: complete
phase: 07-billing-finance
source: Re-verification of UAT gaps fixed by plans 07-29, 07-30, 07-31
started: 2026-03-17T16:00:00Z
updated: 2026-03-17T16:15:00Z
---

## Current Test
<!-- OVERWRITE each test - shows where we are -->

[testing complete]

## Tests

### 1. Browse Finalized Invoices (Invoice History Page)
expected: Navigate to /billing/invoices (or click "View All Invoices" from dashboard). Page shows DataTable with status tabs (All/Draft/Finalized/Voided). Finalized invoices appear in the list and can be clicked to view details. Search and pagination work.
result: issue
reported: "click on a row should go to details page"
severity: major

### 2. Prescription Creates Billing Line Items
expected: When a doctor adds a drug prescription to a visit, billing line items are created immediately (on prescription, not on dispensing). Cashier can see pharmacy line items on the invoice and collect payment BEFORE the patient goes to collect medicine.
result: issue
reported: "cashier need to refresh page manually"
severity: major

### 3. Remove Line Item from Draft Invoice
expected: On a draft invoice detail page, each line item has a delete/trash button. Clicking it shows a confirmation dialog. Confirming removes the line item and totals recalculate immediately.
result: issue
reported: "why it's allowed to remove line items. What happened if doctor give patient a medicine, but cashier remove it from invoice. Prescription-linked line items should not be removable by cashier."
severity: major

### 4. Remove Prescription Removes Invoice Line Items
expected: When a doctor removes medicines from a visit prescription, the corresponding billing line items should also be removed from the invoice automatically.
result: issue
reported: "when doctor remove medicines from a visit, it still in the invoice"
severity: major

## Summary

total: 4
passed: 0
issues: 4
pending: 0
skipped: 0

## Gaps

- truth: "Clicking an invoice row in the Invoice History page navigates to the invoice detail page"
  status: failed
  reason: "User reported: click on a row should go to details page"
  severity: major
  test: 1
  artifacts: []
  missing: []

- truth: "Prescription billing line items appear on cashier's invoice automatically without manual refresh"
  status: failed
  reason: "User reported: cashier need to refresh page manually"
  severity: major
  test: 2
  artifacts: []
  missing: []

- truth: "Prescription-linked line items should not be removable by cashier to prevent revenue loss"
  status: failed
  reason: "User reported: why it's allowed to remove line items. What happened if doctor give patient a medicine, but cashier remove it from invoice. Prescription-linked line items should not be removable by cashier."
  severity: major
  test: 3
  artifacts: []
  missing: []

- truth: "Removing medicines from a visit prescription should automatically remove corresponding invoice line items"
  status: failed
  reason: "User reported: when doctor remove medicines from a visit, it still in the invoice"
  severity: major
  test: 4
  artifacts: []
  missing: []
