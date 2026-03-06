---
status: diagnosed
phase: 07-billing-finance
source: 07-01-SUMMARY.md, 07-02-SUMMARY.md, 07-03-SUMMARY.md, 07-04-SUMMARY.md, 07-05-SUMMARY.md, 07-06-SUMMARY.md, 07-07-SUMMARY.md, 07-08-SUMMARY.md, 07-09-SUMMARY.md, 07-10-SUMMARY.md, 07-11-SUMMARY.md, 07-12-SUMMARY.md, 07-13-SUMMARY.md, 07-14-SUMMARY.md, 07-15-SUMMARY.md, 07-16-SUMMARY.md, 07-17-SUMMARY.md, 07-18-SUMMARY.md, 07-19-SUMMARY.md, 07-20-SUMMARY.md, 07-21-SUMMARY.md, 07-22-SUMMARY.md, 07-23-SUMMARY.md, 07-25-SUMMARY.md, 07-26-SUMMARY.md
started: 2026-03-06T00:00:00Z
updated: 2026-03-06T16:12:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running server/service. Start backend (port 5255) and frontend (port 3000) from scratch. Server boots without errors, migrations complete, and the app loads in the browser.
result: pass

### 2. Create Invoice
expected: Navigate to billing. Creating a new invoice generates an auto-numbered HD-YYYY-NNNNN invoice in Draft status.
result: pass

### 3. Add Line Item to Draft Invoice
expected: Adding a line item to a draft invoice shows the item in the invoice detail with department-based grouping (Medical/Pharmacy/Optical/Treatment) and totals recalculate.
result: issue
reported: "API returns 500 DbUpdateConcurrencyException when adding line items. The RowVersion concurrency token on Invoice entity causes optimistic concurrency failure on every write operation that modifies the invoice after initial creation."
severity: blocker

### 4. Remove Line Item from Draft Invoice
expected: Removing a line item updates the invoice — item disappears and totals recalculate automatically.
result: issue
reported: "Blocked by same DbUpdateConcurrencyException as Test 3. All operations that modify a loaded Invoice entity fail because RowVersion concurrency check fails."
severity: blocker

### 5. View Invoice Detail
expected: Invoice detail page shows grouped line items by department, payments list, discounts list, and balance due.
result: pass

### 6. View Pending Invoices List
expected: Billing dashboard displays all Draft status invoices with quick-view summary in the left column.
result: issue
reported: "API returns 500 on GET /api/billing/invoices/pending. The GetPendingInvoicesQuery has no Wolverine handler implementation — query is defined in Contracts but no handler exists in Application."
severity: blocker

### 7. Finalize Invoice
expected: Clicking Finalize changes invoice status from Draft to Finalized. Success notification appears. Finalized invoice cannot be edited.
result: issue
reported: "Finalize succeeds (HTTP 200) but allows finalizing invoices with zero balance and no line items. No validation prevents finalizing an empty invoice. Also, the finalize endpoint requires a JSON body with CashierShiftId but does not auto-populate from current shift."
severity: major

### 8. Record Cash Payment
expected: Recording a cash payment on a finalized invoice shows payment in payments list, updates balance due, and increments the cashier shift's CashReceived.
result: issue
reported: "Cannot fully test because adding line items fails (Test 3). On a zero-balance invoice, payment correctly returns 400 'Payment exceeds balance due'. The underlying payment handler logic passes all 45 unit tests, but integration is blocked by the concurrency issue."
severity: blocker

### 9. Record Bank Transfer Payment
expected: Recording a bank transfer payment with reference number shows it in payments list and shift non-cash revenue.
result: skipped
reason: Blocked by concurrency issue (Test 3) — cannot create invoices with balance to pay

### 10. Record QR Payment (VnPay/Momo/ZaloPay)
expected: Recording a QR payment with reference number shows it in payments list and shift non-cash revenue.
result: skipped
reason: Blocked by concurrency issue (Test 3)

### 11. Record Card Payment
expected: Recording a card payment with last-4 digits shows it in payments list and shift non-cash revenue.
result: skipped
reason: Blocked by concurrency issue (Test 3)

### 12. Record Split Payment for Treatment Package
expected: When recording payment for a treatment package, sequence selector appears. Payment records with TreatmentPackageId and SplitSequence, shown with split indicator.
result: skipped
reason: Blocked by concurrency issue (Test 3)

### 13. Record Payment Exceeding Balance Due
expected: Attempting to pay more than balance due shows error "Payment amount cannot exceed outstanding balance".
result: pass

### 14. Record Payment When No Open Shift
expected: Attempting to record payment without an open shift shows error "No open shift for this branch".
result: skipped
reason: Cannot test without a valid invoice with balance

### 15. Apply Percentage Discount to Invoice
expected: Applying a percentage discount shows it as Pending status with calculated VND amount displayed.
result: issue
reported: "API returns 500 DbUpdateConcurrencyException when applying discount (same root cause as Test 3). Additionally, the endpoint does not auto-populate RequestedById from ICurrentUser — the client must send it explicitly, causing validation error 'RequestedById must not be empty' when not provided."
severity: blocker

### 16. Apply Fixed-Amount Discount to Line Item
expected: Applying a fixed-amount discount to a specific line item shows the VND discount on that line item.
result: skipped
reason: Blocked by concurrency issue (Test 3) and missing RequestedById enrichment

### 17. Approve Discount with Manager PIN
expected: Entering valid manager PIN changes discount status from Pending to Approved. Invoice total recalculates excluding the discount.
result: skipped
reason: Blocked by discount creation failure (Test 15)

### 18. Reject Discount with Manager PIN
expected: Entering valid manager PIN to reject changes discount status to Rejected. Invoice total recalculates without the rejected discount.
result: skipped
reason: Blocked by discount creation failure (Test 15)

### 19. View Live Discount Preview
expected: As user types discount amount, the calculated VND amount updates in real-time before submitting.
result: skipped
reason: Frontend-only test, needs manual verification

### 20. Approve Discount with Invalid PIN
expected: Entering an invalid manager PIN shows error message "Invalid manager PIN".
result: skipped
reason: Blocked by discount creation failure (Test 15)

### 21. Request Refund on Finalized Invoice
expected: Requesting a refund on a finalized invoice creates refund in Requested status with reason displayed.
result: issue
reported: "Endpoint does not auto-populate RequestedById from ICurrentUser — client must send it explicitly. When RequestedById is provided, the refund request should work but is blocked by concurrency issue on the invoice entity."
severity: major

### 22. Request Refund Exceeding Invoice Total
expected: Attempting refund exceeding invoice total shows error "Refund amount cannot exceed invoice total".
result: skipped
reason: Blocked by concurrency issue

### 23. Approve Refund with Manager PIN
expected: Entering valid manager PIN approves refund — status changes to Approved.
result: skipped
reason: Blocked by refund creation failure (Test 21)

### 24. Process Full Refund
expected: Processing approved refund marks all payments as Refunded. Refund status changes to Processed. CashierShift CashRefunds increments.
result: skipped
reason: Blocked by refund creation failure (Test 21)

### 25. Request Refund on Draft Invoice
expected: Attempting refund on a draft invoice shows error "Refund can only be requested on finalized invoices".
result: pass

### 26. Open New Shift
expected: Opening a new shift shows shift card with selected template (Morning/Afternoon), opening balance, and Open status.
result: pass

### 27. Close Shift with Matching Cash
expected: Entering actual cash matching expected amount shows "Match" in green. Shift closes successfully (Open -> Locked -> Closed).
result: pass

### 28. Close Shift with Cash Surplus
expected: Entering actual cash higher than expected shows surplus amount in blue. Shift closes with surplus noted.
result: skipped
reason: Needs manual frontend verification

### 29. Close Shift with Cash Deficit
expected: Entering actual cash lower than expected shows deficit in red. AlertDialog asks for manager note before closing.
result: skipped
reason: Needs manual frontend verification

### 30. View Shift History
expected: Shift history table shows past shifts with open/closed dates, status, totals, and expandable rows for detailed report.
result: skipped
reason: Needs manual frontend verification

### 31. View Shift Report
expected: Expanding a shift shows revenue breakdown by payment method (Cash, Bank, QR, Cards) and cash reconciliation (expected vs actual).
result: pass

### 32. Print Shift Report PDF
expected: Clicking print on shift report downloads PDF with shift date, revenue table, and cash reconciliation.
result: skipped
reason: PDF download needs manual verification

### 33. Cannot Open Multiple Shifts Per Branch
expected: Attempting to open a second shift while one is already open shows error "Only one open shift allowed per branch at a time".
result: pass

### 34. Print Invoice PDF
expected: Clicking print on invoice downloads PDF showing invoice number, patient info, department-grouped line items, totals, and payments.
result: pass

### 35. Print Payment Receipt PDF
expected: Clicking print receipt downloads A5 PDF with payment method, amount, reference number, and cashier name.
result: pass

### 36. Print E-Invoice PDF
expected: Exporting e-invoice as PDF opens in new tab with Vietnamese Decree 123/2020 fields, tax codes, and 8% tax calculation.
result: pass

### 37. Export E-Invoice as JSON
expected: Exporting e-invoice as JSON downloads file with MISA-compatible structure.
result: pass

### 38. Export E-Invoice as XML
expected: Exporting e-invoice as XML downloads file with all mandatory e-invoice fields.
result: pass

### 39. Billing Dashboard Layout
expected: Billing page shows two-column layout: pending draft invoices on left, current shift status on right.
result: pass

### 40. Action Buttons Context-Sensitive
expected: Invoice action buttons (Collect Payment, Apply Discount, Request Refund, Print, Finalize) show/hide based on invoice status (Draft vs Finalized).
result: pass

### 41. VND Currency Formatting
expected: All currency amounts display with Vietnamese locale using dot thousands separator (e.g., 1.000.000 VND).
result: pass

### 42. Payment Method Selector UI
expected: Payment form shows 7 payment method cards with icons and labels. Clicking a method highlights it with primary color.
result: pass

### 43. E-Invoice Export Dropdown
expected: E-invoice export menu shows PDF (opens new tab), JSON (file download), XML (file download) options.
result: pass

### 44. Shift Management Page Loads
expected: Shift management page shows current shift card and shift history table with expandable rows.
result: pass

### 45. Shift Open Dialog
expected: Open shift dialog shows template selector dropdown, opening balance input, and "Open Shift" button.
result: pass

### 46. Shift Close Dialog
expected: Close shift dialog shows actual cash count input with live discrepancy calculation updating as user types.
result: pass

### 47. Payment Form Dialog
expected: Payment form shows amount input (validates against balance due), method selector, and split payment toggle for treatment packages.
result: pass

### 48. Manager PIN Verification
expected: Discount/Refund approval dialogs show PIN input. Valid PIN from manager account processes the action. Invalid PIN shows error.
result: pass

## Summary

total: 48
passed: 24
issues: 6
pending: 0
skipped: 18

## Gaps

- truth: "Adding line item to draft invoice updates totals and shows department grouping"
  status: failed
  reason: "User reported: API returns 500 DbUpdateConcurrencyException when adding line items. The RowVersion concurrency token on Invoice entity causes optimistic concurrency failure on every write operation that modifies the invoice after initial creation."
  severity: blocker
  test: 3
  root_cause: "InvoiceRepository.Update() calls context.Invoices.Update(invoice) which marks ALL properties Modified including RowVersion. For SQL Server rowversion columns (auto-generated, read-only), EF tries to include RowVersion in SET clause which SQL Server rejects. Handlers that don't call Update() still fail because child collection changes may not trigger invoice entity to be marked Modified."
  artifacts:
    - path: "backend/src/Modules/Billing/Billing.Infrastructure/Repositories/InvoiceRepository.cs"
      issue: "Line 96: context.Invoices.Update(invoice) marks RowVersion as Modified"
    - path: "backend/src/Modules/Billing/Billing.Application/Features/RecordPayment.cs"
      issue: "Line 143: unnecessary invoiceRepository.Update(invoice) call"
    - path: "backend/src/Modules/Billing/Billing.Application/Features/ApplyDiscount.cs"
      issue: "Line 100: unnecessary invoiceRepository.Update(invoice) call"
    - path: "backend/src/Modules/Billing/Billing.Application/Features/ApproveDiscount.cs"
      issue: "Line 64: unnecessary invoiceRepository.Update(invoice) call"
    - path: "backend/src/Modules/Billing/Billing.Application/Features/RejectDiscount.cs"
      issue: "Line 68: unnecessary invoiceRepository.Update(invoice) call"
    - path: "backend/src/Modules/Billing/Billing.Application/Features/RequestRefund.cs"
      issue: "Line 93: unnecessary invoiceRepository.Update(invoice) call"
    - path: "backend/src/Modules/Billing/Billing.Application/Features/ApproveRefund.cs"
      issue: "Line 67: unnecessary invoiceRepository.Update(invoice) call"
    - path: "backend/src/Modules/Billing/Billing.Application/Features/ProcessRefund.cs"
      issue: "Line 93: unnecessary invoiceRepository.Update(invoice) call"
  missing:
    - "Remove all invoiceRepository.Update(invoice) calls — entities are already tracked by DbContext via GetByIdAsync"
  debug_session: ".planning/debug/invoice-rowversion-concurrency.md"

- truth: "Billing dashboard displays all Draft status invoices with quick-view summary"
  status: failed
  reason: "User reported: API returns 500 on GET /api/billing/invoices/pending. The GetPendingInvoicesQuery has no Wolverine handler implementation."
  severity: blocker
  test: 6
  root_cause: "GetPendingInvoicesQuery is defined in Billing.Contracts and dispatched by the API endpoint, but no Wolverine handler exists in Billing.Application to process it. Also IInvoiceRepository lacks a GetPendingAsync method."
  artifacts:
    - path: "backend/src/Modules/Billing/Billing.Contracts/Queries/GetVisitInvoiceQuery.cs"
      issue: "Line 14: query defined but no handler exists"
    - path: "backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs"
      issue: "Lines 72-78: endpoint dispatches query that has no handler"
  missing:
    - "Create GetPendingInvoices.cs handler in Billing.Application/Features/"
    - "Add GetPendingAsync(Guid? cashierShiftId, CancellationToken ct) to IInvoiceRepository"
    - "Implement GetPendingAsync in InvoiceRepository filtering by Status == Draft"
  debug_session: ".planning/debug/pending-invoices-500.md"

- truth: "Finalize validates invoice has line items and balance before finalizing"
  status: failed
  reason: "User reported: Finalize succeeds (HTTP 200) but allows finalizing invoices with zero balance and no line items. No validation prevents finalizing an empty invoice."
  severity: major
  test: 7
  root_cause: "Invoice.Finalize() only checks EnsureDraft() and IsFullyPaid. Empty invoice has BalanceDue=0 (TotalAmount-PaidAmount=0-0), so IsFullyPaid=true (BalanceDue<=0). No check for line items count or positive total amount."
  artifacts:
    - path: "backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs"
      issue: "Lines 200-215: Finalize() missing line item and total amount validation"
    - path: "backend/src/Modules/Billing/Billing.Application/Features/FinalizeInvoice.cs"
      issue: "Lines 20-46: handler delegates entirely to domain, no additional guards"
  missing:
    - "Add guard in Invoice.Finalize(): if (_lineItems.Count == 0) throw InvalidOperationException"
    - "Add guard in Invoice.Finalize(): if (TotalAmount <= 0) throw InvalidOperationException"
    - "Add unit tests for finalize-empty-invoice scenarios"
  debug_session: ".planning/debug/finalize-invoice-zero-balance.md"

- truth: "Apply discount endpoint auto-populates RequestedById from current user"
  status: failed
  reason: "User reported: Endpoint does not auto-populate RequestedById from ICurrentUser — client must send it explicitly. Also blocked by DbUpdateConcurrencyException."
  severity: blocker
  test: 15
  root_cause: "ApplyDiscountHandler does not inject ICurrentUser. It exposes RequestedById as required Guid on command record, forcing client to send it. Every other billing handler injects ICurrentUser and reads currentUser.UserId server-side."
  artifacts:
    - path: "backend/src/Modules/Billing/Billing.Application/Features/ApplyDiscount.cs"
      issue: "Lines 45-50: Handle() missing ICurrentUser parameter; line 78 uses command.RequestedById"
  missing:
    - "Remove RequestedById from ApplyDiscountCommand record"
    - "Remove RuleFor(x => x.RequestedById).NotEmpty() validator"
    - "Add ICurrentUser currentUser to Handle() parameters"
    - "Replace command.RequestedById with currentUser.UserId"
  debug_session: ".planning/debug/billing-requestedbyid-not-auto-populated.md"

- truth: "Record payment against finalized invoice with balance"
  status: failed
  reason: "User reported: Cannot create invoices with balance to pay because adding line items fails due to concurrency issue."
  severity: blocker
  test: 8
  root_cause: "Blocked by the same DbUpdateConcurrencyException root cause as Test 3. Once concurrency fix is applied, payments should work (45 unit tests pass for payment logic)."
  artifacts:
    - path: "backend/src/Modules/Billing/Billing.Infrastructure/Repositories/InvoiceRepository.cs"
      issue: "Line 96: same Update() issue as Test 3"
  missing:
    - "Fix concurrency issue (Test 3 fix) unblocks this"
  debug_session: ".planning/debug/invoice-rowversion-concurrency.md"

- truth: "Request refund endpoint auto-populates RequestedById from current user"
  status: failed
  reason: "User reported: Endpoint does not auto-populate RequestedById from ICurrentUser — client must send it explicitly."
  severity: major
  test: 21
  root_cause: "RequestRefundHandler does not inject ICurrentUser. Same pattern issue as ApplyDiscountHandler (Test 15)."
  artifacts:
    - path: "backend/src/Modules/Billing/Billing.Application/Features/RequestRefund.cs"
      issue: "Lines 41-46: Handle() missing ICurrentUser parameter; line 89 uses command.RequestedById"
  missing:
    - "Remove RequestedById from RequestRefundCommand record"
    - "Remove RuleFor(x => x.RequestedById).NotEmpty() validator"
    - "Add ICurrentUser currentUser to Handle() parameters"
    - "Replace command.RequestedById with currentUser.UserId"
  debug_session: ".planning/debug/billing-requestedbyid-not-auto-populated.md"
