---
status: complete
phase: 07-billing-finance
source: 07-32-SUMMARY.md, 07-33-SUMMARY.md
started: 2026-03-17T15:30:00Z
updated: 2026-03-17T15:50:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Invoice History Rows Clickable
expected: Go to Billing > Invoice History (All Invoices page). Click anywhere on an invoice row (not just the invoice number link). The click should navigate you to the invoice detail page at /billing/invoices/{id}.
result: pass

### 2. Invoice Detail Real-time Refresh on Prescription
expected: Open an invoice detail page for a draft invoice linked to a visit. In another tab/window, add a drug prescription to that visit (as a doctor). Switch back to the invoice detail page — the new prescription line items should appear automatically WITHOUT manually refreshing the browser.
result: pass

### 3. Prescription Line Items Not Removable by Cashier
expected: On an invoice detail page with prescription-sourced line items, those items should show a lock icon instead of a delete button. The cashier should NOT be able to remove prescription-linked line items. Non-prescription items (if any) should still have the delete button available.
result: pass

### 4. Doctor Removes Prescription Auto-Removes Billing Items
expected: With an invoice open that has prescription line items, have a doctor remove medicines from the visit prescription. The corresponding billing line items should automatically disappear from the invoice detail page in real-time (via SignalR), and the invoice totals should recalculate.
result: pass

## Summary

total: 4
passed: 4
issues: 0
pending: 0
skipped: 0

## Gaps

[none]
