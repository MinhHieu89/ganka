---
status: testing
phase: 07-billing-finance
source: 07-01-SUMMARY.md, 07-02-SUMMARY.md, 07-03-SUMMARY.md, 07-04-SUMMARY.md, 07-05-SUMMARY.md, 07-06-SUMMARY.md, 07-07-SUMMARY.md, 07-08-SUMMARY.md, 07-09-SUMMARY.md, 07-10-SUMMARY.md, 07-11-SUMMARY.md, 07-12-SUMMARY.md, 07-13-SUMMARY.md, 07-14-SUMMARY.md, 07-15-SUMMARY.md, 07-16-SUMMARY.md, 07-17-SUMMARY.md, 07-18-SUMMARY.md, 07-19-SUMMARY.md, 07-20-SUMMARY.md, 07-21-SUMMARY.md, 07-22-SUMMARY.md, 07-23-SUMMARY.md, 07-25-SUMMARY.md, 07-26-SUMMARY.md, 07-27-SUMMARY.md, 07-28-SUMMARY.md
started: 2026-03-06T19:00:00Z
updated: 2026-03-06T19:00:00Z
---

## Current Test
<!-- OVERWRITE each test - shows where we are -->

number: 1
name: Cold Start Smoke Test
expected: |
  Kill any running server/service. Start backend (port 5255) and frontend (port 3000) from scratch. Server boots without errors, migrations complete, and the billing dashboard loads in the browser at /billing.
awaiting: user response

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running server/service. Start backend (port 5255) and frontend (port 3000) from scratch. Server boots without errors, migrations complete, and the billing dashboard loads at /billing.
result: [pending]

### 2. Billing Dashboard Two-Column Layout
expected: The billing dashboard (/billing) renders a two-column layout. Left column shows pending (Draft) invoices list. Right column shows current shift status card. If no shift is open, an "Open Shift" button appears instead.
result: [pending]

### 3. Billing Sidebar Navigation
expected: The sidebar has a "Billing" collapsible section with two child links: "Billing Dashboard" (/billing) and "Shifts" (/billing/shifts). Both links navigate correctly.
result: [pending]

### 4. Create Invoice
expected: Creating a new invoice generates an auto-numbered invoice in HD-YYYY-NNNNN format (e.g., HD-2026-00001) in Draft status. The invoice appears in the pending invoices list on the dashboard.
result: [pending]

### 5. View Invoice Detail
expected: Navigating to an invoice detail page shows: invoice number, patient info, status badge, line items grouped by department, payment history, discount list, balance due, and breadcrumb navigation back to dashboard.
result: [pending]

### 6. Add Line Item to Draft Invoice
expected: Adding a line item to a Draft invoice shows the item in the line items table grouped under its department section (Kham benh / Duoc pham / Kinh mat / Dieu tri). Section subtotals and grand total update immediately.
result: [pending]

### 7. Remove Line Item from Draft Invoice
expected: Removing a line item from a Draft invoice removes it from the table. Department section subtotal and invoice grand total recalculate. Remove button is hidden/disabled on Finalized invoices.
result: [pending]

### 8. View Pending Invoices List
expected: The billing dashboard left panel lists all Draft invoices. Each entry shows invoice number, patient name, and total in VND. Clicking an entry navigates to invoice detail.
result: [pending]

### 9. Finalize Invoice
expected: Clicking Finalize on a Draft invoice with line items and fully paid balance triggers an AlertDialog confirmation. Confirming changes status to Finalized. Action buttons switch to show Print/Export/Refund instead of Payment/Discount/Finalize.
result: [pending]

### 10. Cannot Finalize Empty Invoice
expected: Attempting to finalize an invoice with no line items or zero total returns an error. The invoice remains in Draft status.
result: [pending]

### 11. Cannot Finalize Invoice with Outstanding Balance
expected: Attempting to finalize an invoice where balance due > 0 returns a validation error. The invoice remains in Draft.
result: [pending]

### 12. Invoice Status Badges
expected: Draft invoices show a muted/secondary status badge. Finalized invoices show a green/primary badge. Balance Due shows red when outstanding (> 0) and green when fully paid (0).
result: [pending]

### 13. Open New Cashier Shift
expected: On the Shifts page, clicking "Open Shift" shows a dialog with shift template dropdown (Morning 08:00-12:00, Afternoon 13:00-20:00) and opening balance input. Confirming creates an Open shift displayed on the shift card.
result: [pending]

### 14. Cannot Open Second Shift
expected: Attempting to open a new shift while one is already Open returns an error "Only one open shift allowed per branch at a time".
result: [pending]

### 15. Record Cash Payment
expected: On a Draft invoice with balance due, opening the payment dialog, selecting "Cash", entering an amount, and submitting records the payment. Payment appears in the invoice's payment history. Shift Cash Received increases.
result: [pending]

### 16. Record Bank Transfer Payment
expected: Selecting "Bank Transfer" reveals a Reference Number field. Submitting with reference records the payment. Payment appears in payment list. Shift non-cash revenue increases.
result: [pending]

### 17. Record QR Payment (VNPay/MoMo/ZaloPay)
expected: Selecting a QR method reveals a Reference Number field. Submitting records the payment. Shift non-cash revenue increases.
result: [pending]

### 18. Record Card Payment (Visa/Mastercard)
expected: Selecting a Card method reveals a "Last 4 Digits" field. Submitting records the payment. Payment appears in payment list.
result: [pending]

### 19. Payment Method Selector UI
expected: Payment form shows 7 selectable method cards in a grid: Cash, Bank Transfer, QR VNPay, QR MoMo, QR ZaloPay, Card Visa, Card Mastercard. Selecting one highlights it with primary color. Method-specific fields (Reference Number / Last 4 Digits) appear conditionally.
result: [pending]

### 20. Split Payment for Treatment Package
expected: When a treatment package is involved, a split payment toggle appears. Selecting it shows a sequence selector (Lan 1, Lan 2). Payment records with split metadata and shows split indicator in payment history.
result: [pending]

### 21. Cannot Pay Without Open Shift
expected: Attempting to record a payment without an open cashier shift returns an error "No open shift for this branch".
result: [pending]

### 22. Payment Cannot Exceed Balance Due
expected: Attempting to pay more than the outstanding balance shows error "Payment amount cannot exceed outstanding balance".
result: [pending]

### 23. Apply Percentage Discount with Live Preview
expected: Opening the Discount dialog, selecting Percentage type, entering a value (e.g., 10%) shows a live VND preview that updates instantly as you type. Submitting creates a Pending discount in the invoice's discount list. RequestedById is auto-populated (no need to send it).
result: [pending]

### 24. Apply Fixed-Amount Discount
expected: Selecting Fixed Amount type, entering a VND amount, and submitting creates a Pending discount. The discount appears in the invoice discount list.
result: [pending]

### 25. Approve Discount with Manager PIN
expected: Clicking Approve on a Pending discount opens a PIN dialog. Entering a valid 4-6 digit manager PIN approves it. Status changes to Approved. Invoice total and balance due recalculate to reflect the discount.
result: [pending]

### 26. Reject Discount with Manager PIN
expected: Choosing Reject, entering a valid manager PIN, changes discount status to Rejected. Invoice total recalculates to exclude the rejected discount.
result: [pending]

### 27. Invalid Manager PIN Shows Error
expected: Entering an incorrect PIN in the Manager PIN dialog shows an error. The discount/refund remains unchanged.
result: [pending]

### 28. Cannot Apply Discount to Finalized Invoice
expected: The "Apply Discount" button is hidden/disabled on Finalized invoices. Attempting via API returns an error.
result: [pending]

### 29. Request Refund on Finalized Invoice
expected: Opening the Refund dialog on a Finalized invoice, entering an amount, and submitting creates a refund in Requested status. RequestedById is auto-populated from current user. The refund appears in the invoice's refund list.
result: [pending]

### 30. Cannot Refund Draft Invoice
expected: The "Request Refund" button is hidden/disabled on Draft invoices. Attempting via API returns error "Refund can only be requested on finalized invoices".
result: [pending]

### 31. Refund Cannot Exceed Invoice Total
expected: Entering a refund amount greater than the invoice total is blocked by validation with an error message.
result: [pending]

### 32. Approve and Process Refund
expected: Approving a Requested refund with valid manager PIN moves it to Approved status. For cash refunds, the shift's Cash Refunds total increases. The refund status transitions to Processed.
result: [pending]

### 33. Close Shift with Matching Cash
expected: Entering actual cash count matching the expected amount shows discrepancy as 0 (green). Confirming closes the shift. No AlertDialog confirmation needed when matching.
result: [pending]

### 34. Close Shift with Discrepancy
expected: Entering actual cash different from expected shows live discrepancy in red (deficit) or blue (surplus). Confirming shows AlertDialog. Manager Note field is highlighted when discrepancy exists.
result: [pending]

### 35. View Shift History with Expandable Rows
expected: Shifts page shows a table of past shifts with date, cashier name, status, opening balance, and totals. Clicking a row expands it inline to show the full shift report (no page navigation).
result: [pending]

### 36. View Shift Report
expected: Expanded shift report shows revenue breakdown by payment method (Cash, Bank, QR VNPay, QR MoMo, QR ZaloPay, Visa, Mastercard) and cash reconciliation (Opening Balance, Cash Received, Cash Refunds, Expected Cash, Actual Cash, Discrepancy).
result: [pending]

### 37. Print Invoice PDF
expected: Clicking "Print Invoice" on a Finalized invoice generates an A4 PDF with clinic header, MST tax code, patient info, department-grouped line items in VND, payment summary, and cashier signature area.
result: [pending]

### 38. Print Receipt PDF (A5)
expected: Clicking "Print Receipt" generates a compact A5 PDF receipt with payment methods, amounts in VND, and reference numbers.
result: [pending]

### 39. E-Invoice PDF (Decree 123/2020)
expected: Clicking "E-Invoice PDF" opens a new browser tab with the e-invoice. It includes: invoice template/symbol, seller and buyer tax codes, pre-tax amount, 8% GTGT tax rate, tax amount, total with tax, and total amount written in Vietnamese words (e.g., "Hai trieu dong").
result: [pending]

### 40. E-Invoice Export JSON
expected: Clicking "Export JSON" downloads a JSON file with all Decree 123/2020 mandatory fields in Vietnamese (Unicode preserved) in MISA-compatible format.
result: [pending]

### 41. E-Invoice Export XML
expected: Clicking "Export XML" downloads a well-formed XML file with all mandatory e-invoice elements in MISA-compatible format.
result: [pending]

### 42. VND Currency Formatting Throughout
expected: All monetary amounts across the billing UI use Vietnamese locale: dot (.) thousands separator, no decimal places, "₫" symbol (e.g., 1.500.000 ₫). No fractional VND anywhere.
result: [pending]

### 43. Print Shift Report PDF
expected: Clicking print on the shift report downloads/opens a PDF with clinic header, revenue-by-payment-method table, and cash reconciliation section.
result: [pending]

## Summary

total: 43
passed: 0
issues: 0
pending: 43
skipped: 0

## Gaps

[none yet]
