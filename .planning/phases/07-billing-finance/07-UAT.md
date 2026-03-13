---
status: testing
phase: 07-billing-finance
source: 07-01-SUMMARY.md through 07-28-SUMMARY.md
started: 2026-03-13T03:21:33Z
updated: 2026-03-13T04:05:36Z
---

## Current Test
<!-- OVERWRITE each test - shows where we are -->

number: 12
name: Record Cash Payment
expected: |
  Selecting Cash payment method, entering amount, submitting records payment. Shift cash balance increases. Invoice balance due decreases.
awaiting: user response (BLOCKED - need to fix AddLineItem 500 first)

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running server/service. Start backend on port 5255 and frontend on port 3000 from scratch. Backend boots without errors, billing migrations complete, and /billing dashboard loads.
result: pass

### 2. Billing Sidebar Navigation
expected: Sidebar shows "Billing" collapsible section with "Dashboard" and "Shifts" child links. Clicking navigates to /billing and /billing/shifts respectively.
result: pass

### 3. Billing Dashboard Layout
expected: Dashboard displays two-column layout — pending invoices list on left, current shift summary on right.
result: pass

### 4. Create Invoice for Visit
expected: Creating invoice generates auto-numbered HD-YYYY-NNNNN format (e.g., HD-2026-00001), status Draft, linked to patient visit. Appears in pending invoices list.
result: skipped
reason: No UI to create invoices from billing dashboard. Per FIN-01, invoices are per-visit — creation flow belongs in clinical workflow.

### 5. Invoice Detail Page
expected: Clicking invoice shows detail with status badge (Draft=muted, Finalized=green), patient info, line items grouped by department (Medical/Pharmacy/Optical/Treatment) with subtotals, payments list, discounts list, and action buttons (Collect Payment, Apply Discount, Request Refund, Print, Finalize, E-Invoice Export).
result: skipped
reason: Depends on Test 4 (invoice creation)

### 6. Add Line Items to Invoice
expected: Adding line items to draft invoice shows them grouped by department. Each has UnitPrice, Quantity, LineTotal. Grand total recalculates automatically.
result: skipped
reason: Depends on Test 4 (invoice creation)

### 7. Remove Line Item from Invoice
expected: Removing a line item from draft invoice removes it from the list and recalculates totals.
result: skipped
reason: Depends on Test 4 (invoice creation)

### 8. Finalize Invoice
expected: Clicking Finalize on a draft invoice with line items and full payment changes status to Finalized. Line items become locked.
result: skipped
reason: Depends on Test 4 (invoice creation)

### 9. Cannot Finalize Empty Invoice
expected: Attempting to finalize invoice with no line items shows error "Cannot finalize an invoice with no line items."
result: skipped
reason: Depends on Test 4 (invoice creation)

### 10. Open Cashier Shift
expected: Opening a new shift shows shift template selector (Morning 08:00-12:00 / Afternoon 13:00-20:00), enter opening balance. Shift created with Open status, displayed in dashboard current shift card.
result: pass

### 11. Cannot Open Duplicate Shift
expected: When a shift is already open, attempting to open another shows error "A shift is already open for this branch."
result: pass

### 12. Record Cash Payment
expected: Selecting Cash payment method, entering amount, submitting records payment. Shift cash balance increases. Invoice balance due decreases.
result: [pending]

### 13. Record Bank Transfer Payment
expected: Selecting Bank Transfer shows reference number field. Payment recorded with reference number stored.
result: [pending]

### 14. Record QR Payment (VNPay/MoMo/ZaloPay)
expected: Selecting QR payment method shows reference number field. Payment recorded with method and reference.
result: [pending]

### 15. Record Card Payment (Visa/Mastercard)
expected: Selecting Card payment shows card type selector and last 4 digits field. Payment recorded with card details.
result: [pending]

### 16. Split Payment
expected: Recording split payment marks first payment with IsSplitPayment=true, SplitSequence=1. Second payment gets SplitSequence=2. Both appear in payment list.
result: [pending]

### 17. Payment Cannot Exceed Balance Due
expected: Entering payment amount greater than balance due shows error "Payment exceeds balance due" or similar validation.
result: [pending]

### 18. Cannot Pay Without Open Shift
expected: Attempting to record payment without an open shift shows error "No open cashier shift found."
result: [pending]

### 19. Apply Percentage Discount
expected: Applying percentage discount shows live preview of calculated VND amount. Discount created in Pending status awaiting manager approval.
result: [pending]

### 20. Apply Fixed Amount Discount
expected: Applying fixed VND discount to invoice or line item. Discount created in Pending status.
result: [pending]

### 21. Approve Discount with Manager PIN
expected: Manager enters PIN, discount status changes to Approved, invoice total recalculates with discount applied.
result: [pending]

### 22. Reject Discount with Manager PIN
expected: Manager enters PIN and rejection reason. Discount status changes to Rejected. Invoice total unchanged.
result: [pending]

### 23. Request Refund on Finalized Invoice
expected: Requesting refund on finalized invoice creates refund in Requested status with reason and amount. Cannot refund draft invoice.
result: [pending]

### 24. Approve and Process Refund
expected: Manager approves refund with PIN. Processing refund with cash method deducts from shift balance. Payment marked as refunded.
result: [pending]

### 25. Close Shift with Cash Count
expected: Entering actual cash count calculates discrepancy (actual - expected). Discrepancy shown color-coded (green=match, red=deficit, blue=surplus). Shift status changes to Closed.
result: [pending]

### 26. Shift Report
expected: Shift report shows revenue breakdown by payment method (Cash, Bank, QR, Card) with counts and amounts. Cash reconciliation table with opening/closing balances and discrepancy.
result: [pending]

### 27. Shift History
expected: Shifts page shows DataTable of past shifts with date, opening balance, final balance, discrepancy. Clicking row expands inline report.
result: [pending]

### 28. Print Invoice PDF
expected: Generates A4 PDF with clinic header, department-grouped line items, VND formatting with dot-thousands, signatures area. Opens in new tab.
result: [pending]

### 29. Print Receipt PDF
expected: Generates compact A5 receipt PDF with payment methods and amounts.
result: [pending]

### 30. E-Invoice PDF (Vietnamese Decree 123/2020)
expected: E-invoice PDF includes seller/buyer tax codes, invoice template/symbol, tax breakdown (8% GTGT), amount in Vietnamese words.
result: [pending]

### 31. E-Invoice Export JSON
expected: JSON export with all mandatory e-invoice fields, Vietnamese Unicode preserved, MISA-compatible format.
result: [pending]

### 32. E-Invoice Export XML
expected: XML export with all mandatory e-invoice fields, MISA-compatible format.
result: [pending]

### 33. Print Shift Report PDF
expected: PDF with revenue-by-method table, cash reconciliation, highlighted discrepancy if non-zero.
result: [pending]

### 34. VND Currency Formatting
expected: All monetary values throughout billing UI formatted as Vietnamese currency (e.g., "1.234.567 ₫" with dot-thousands separator).
result: [pending]

### 35. VND Amount in Vietnamese Words
expected: Invoice displays amount in Vietnamese words (e.g., 1,000,000 → "Một triệu đồng").
result: [pending]

## Summary

total: 35
passed: 5
issues: 0
pending: 24
skipped: 6

## Gaps

[none yet]
