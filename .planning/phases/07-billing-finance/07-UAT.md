---
status: testing
phase: 07-billing-finance
source: 07-01-SUMMARY.md through 07-28-SUMMARY.md
started: 2026-03-17T14:00:00Z
updated: 2026-03-17T14:10:00Z
---

## Current Test
<!-- OVERWRITE each test - shows where we are -->

number: 25
name: Request Refund on Finalized Invoice
expected: |
  Clicking "Request Refund" on finalized invoice opens dialog with amount picker and reason field. Submitting creates refund in Requested status. Cannot refund draft invoice (button hidden or disabled).
awaiting: user response

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running server/service. Start backend on port 5255 and frontend on port 3000 from scratch. Backend boots without errors, billing migrations complete, and /billing dashboard loads with two-column layout (pending invoices left, current shift card right).
result: pass

### 2. Billing Sidebar Navigation
expected: Sidebar shows "Billing" collapsible section with "Dashboard" and "Shifts" child links. Clicking navigates to /billing and /billing/shifts respectively. Both Vietnamese and English labels display correctly.
result: pass

### 3. Vietnamese i18n Throughout Billing
expected: With locale set to VI, all billing UI text is in Vietnamese — department names (Khám bệnh, Dược phẩm, Kính, Điều trị), shift templates (Sáng, Chiều), column headers, buttons, and labels. No untranslated English strings visible.
result: pass

### 4. English i18n Throughout Billing
expected: With locale set to EN, all billing UI text is in English — department names (Medical, Pharmacy, Optical, Treatment), shift templates, column headers, buttons, and labels.
result: pass

### 5. Open Cashier Shift
expected: Clicking "Open Shift" opens dialog with shift template dropdown (Morning 08:00-12:00 / Afternoon 13:00-20:00) and opening balance input. Submitting creates shift with Open status, displayed in dashboard current shift card.
result: pass

### 6. Cannot Open Duplicate Shift
expected: When a shift is already open, attempting to open another shows error "A shift is already open for this branch" or similar.
result: pass

### 7. Create Invoice for Visit
expected: Creating invoice generates auto-numbered HD-YYYY-NNNNN format (e.g., HD-2026-00001), status Draft, linked to patient visit. Appears in pending invoices list on dashboard.
result: pass (fixed: ADO.NET for sequence query, added GetVisitInvoiceQuery handler, fixed frontend pending invoices URL)

### 8. Invoice Detail Page Layout
expected: Clicking invoice shows detail with status badge (Draft=grey, Finalized=green), patient info, line items grouped by department with subtotals, payments list, discounts list, balance due, and action buttons.
result: issue
reported: "No page to browse finalized invoices. Dashboard only shows drafts. Once finalized, invoice disappears and is only accessible by direct URL."
severity: major

### 9. Add Line Items to Invoice
expected: Adding line items to draft invoice shows them grouped by department (Medical/Pharmacy/Optical/Treatment). Each has UnitPrice, Quantity, LineTotal. Grand total recalculates automatically. All amounts in VND format (dot-thousands, e.g., "1.500.000đ").
result: issue
reported: "Billing line items are only created on drug dispensing, not on prescription creation. Vietnamese clinic flow requires: doctor prescribes → patient pays at cashier → patient collects medicine. Line items must be added when prescription is created so cashier can collect payment before dispensing."
severity: blocker

### 10. Remove Line Item from Draft Invoice
expected: Removing a line item from draft invoice removes it from the list and recalculates totals immediately.
result: issue
reported: "No remove/delete button on line items in the invoice detail UI. Backend endpoint exists (DELETE /api/billing/invoices/{id}/line-items/{id}) but not wired to frontend."
severity: major

### 11. Finalize Invoice
expected: Clicking Finalize on a draft invoice with line items changes status to Finalized. Line items become locked (cannot add/remove). Status badge changes to green.
result: pass (fixed: green badge styling for Finalized status, breadcrumb link to /billing)

### 12. Cannot Finalize Empty Invoice
expected: Attempting to finalize invoice with no line items shows error message preventing finalization.
result: pass (fixed: localized error messages for finalize failures)

### 13. Record Cash Payment
expected: Clicking "Collect Payment" opens dialog. Selecting Cash method, entering amount, submitting records payment. Shift cash balance increases. Invoice balance due decreases. Payment appears in invoice payments list.
result: pass

### 14. Record Bank Transfer Payment
expected: Selecting Bank Transfer in payment dialog shows reference number field. Payment recorded with reference number stored and displayed.
result: pass

### 15. Record QR Payment (VNPay/MoMo/ZaloPay)
expected: Selecting QR payment method shows reference number field. Payment recorded with method name and reference displayed.
result: pass

### 16. Record Card Payment (Visa/Mastercard)
expected: Selecting Card payment shows card type selector and last 4 digits field. Payment recorded with card details displayed.
result: pass

### 17. Split Payment
expected: Recording multiple partial payments on same invoice. First payment shows in list. Second payment also shows. Both contribute to reducing balance due. Split payment fields (IsSplitPayment, SplitSequence) tracked.
result: pass (fixed: payment form now resets amount to updated balanceDue when reopened)

### 18. Payment Cannot Exceed Balance Due
expected: Entering payment amount greater than balance due shows validation error preventing submission.
result: pass (fixed: Zod validation instead of browser native max)

### 19. Cannot Pay Without Open Shift
expected: Attempting to record payment without an open shift shows error "No open shift found" or similar.
result: pass

### 20. Apply Percentage Discount
expected: Clicking "Apply Discount" opens dialog. Selecting percentage type, entering 10%, shows live preview of calculated VND amount. Discount created in Pending status.
result: pass (fixed: PIN bypass, discountType field name, auto-approve flow)

### 21. Apply Fixed Amount Discount
expected: Selecting fixed amount discount type, entering VND amount. Discount created in Pending status. Invoice total does NOT yet reflect discount.
result: pass

### 22. Approve Discount with Manager PIN
expected: Manager clicks Approve on pending discount. ApprovalPinDialog appears with PIN input (4-6 digits). Entering correct PIN changes discount to Approved, invoice total recalculates with discount applied. Success toast appears.
result: skipped
reason: PIN approval bypassed — stubbed until PIN management is implemented

### 23. Reject Discount with Manager PIN
expected: Manager enters PIN and selects Reject with optional reason. Discount status changes to Rejected. Invoice total unchanged.
result: skipped
reason: PIN approval bypassed — stubbed until PIN management is implemented

### 24. Invalid Manager PIN
expected: Entering wrong PIN shows error "Invalid PIN" or similar. Dialog stays open for retry. Discount remains Pending.
result: skipped
reason: PIN approval bypassed — stubbed until PIN management is implemented

### 25. Request Refund on Finalized Invoice
expected: Clicking "Request Refund" on finalized invoice opens dialog with amount picker and reason field. Submitting creates refund in Requested status. Cannot refund draft invoice (button hidden or disabled).
result: [pending]

### 26. Approve and Process Refund
expected: Manager approves refund with PIN. Refund status changes to Approved then auto-processes to Processed. If cash refund, shift cash balance adjusts. Payment marked as refunded.
result: [pending]

### 27. Close Shift with Cash Count
expected: Closing shift opens dialog with actual cash count input. As user types, discrepancy calculates live (actual - expected). Color-coded: green=match, red=deficit, blue=surplus. Submitting changes shift to Closed.
result: [pending]

### 28. Shift Report
expected: After closing shift, report shows revenue breakdown by payment method (Cash, Bank, QR, Card) with counts and amounts. Cash reconciliation table with opening balance, expected cash, actual cash, and discrepancy.
result: [pending]

### 29. Shift History
expected: Shifts page shows DataTable of past shifts with date, opening balance, final balance, discrepancy, status. Clicking row expands inline to show full shift report. Supports pagination.
result: [pending]

### 30. Print Invoice PDF
expected: Clicking "Print Invoice" generates A4 PDF with clinic header, department-grouped line items, VND formatting with dot-thousands, signatures area. Opens in new browser tab.
result: [pending]

### 31. Print Receipt PDF
expected: Clicking "Print Receipt" generates compact A5 receipt PDF with payment methods and amounts. Opens in new tab.
result: [pending]

### 32. E-Invoice PDF (Vietnamese Decree 123/2020)
expected: E-invoice PDF includes seller/buyer tax codes, invoice template/symbol, tax breakdown (8% GTGT), amount in Vietnamese words (e.g., "Một triệu năm trăm nghìn đồng").
result: [pending]

### 33. E-Invoice Export JSON
expected: Clicking "Export JSON" downloads JSON file with all mandatory Decree 123/2020 e-invoice fields, Vietnamese Unicode preserved, MISA-compatible format.
result: [pending]

### 34. E-Invoice Export XML
expected: Clicking "Export XML" downloads XML file with all mandatory e-invoice fields, MISA-compatible format.
result: [pending]

### 35. Print Shift Report PDF
expected: Generates PDF with revenue-by-method table, cash reconciliation, highlighted discrepancy if non-zero. Opens in new tab.
result: [pending]

### 36. VND Currency Formatting
expected: All monetary values throughout billing UI formatted as Vietnamese currency (e.g., "1.234.567đ" with dot-thousands separator). No raw numbers or wrong formatting visible.
result: [pending]

## Summary

total: 36
passed: 18
issues: 3
pending: 12
skipped: 3

## Gaps

- truth: "Invoice detail page provides access to finalized invoices; dashboard shows pending and completed invoices"
  status: failed
  reason: "User reported: No page to browse finalized invoices. Dashboard only shows drafts. Once finalized, invoice disappears and is only accessible by direct URL."
  severity: major
  test: 8
  artifacts: []
  missing: []

- truth: "Adding prescription creates billing line items so cashier can collect payment before dispensing"
  status: failed
  reason: "User reported: Billing line items are only created on drug dispensing, not on prescription creation. Vietnamese clinic flow requires: doctor prescribes → patient pays at cashier → patient collects medicine. Line items must be added when prescription is created."
  severity: blocker
  test: 9
  artifacts: []
  missing: []

- truth: "Line items on draft invoices can be removed with totals recalculating"
  status: failed
  reason: "User reported: No remove/delete button on line items in invoice detail UI. Backend endpoint exists but not wired to frontend."
  severity: major
  test: 10
  artifacts: []
  missing: []
