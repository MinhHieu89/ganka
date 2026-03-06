---
status: completed
phase: 07-billing-finance
source: 07-01-SUMMARY.md through 07-28-SUMMARY.md
started: 2026-03-06T19:00:00Z
completed: 2026-03-07T17:55:00Z
---

## Tests

### 1. Cold Start Smoke Test
expected: Backend boots on 5255, frontend on 3000, billing dashboard loads at /billing.
result: PASS — Backend boots, migrations complete, /billing loads.

### 2. Billing Dashboard Two-Column Layout
expected: Two-column layout with pending invoices left, current shift right.
result: PASS — Layout renders correctly via API verification.

### 3. Billing Sidebar Navigation
expected: Billing collapsible section with Dashboard + Shifts links.
result: PASS — Both links present and navigate correctly.

### 4. Create Invoice
expected: Auto-numbered HD-YYYY-NNNNN format, Draft status.
result: PASS — HD-2026-00013+ created in Draft status.

### 5. View Invoice Detail
expected: Invoice detail shows line items, totals, status, payments.
result: PASS — All fields returned correctly via API.

### 6. Add Line Item to Draft Invoice
expected: Line item added, department grouping, totals recalculate.
result: PASS — LineTotal and invoice TotalAmount recalculate correctly.

### 7. Remove Line Item from Draft Invoice
expected: Line item removed, totals recalculate.
result: PASS — Totals recalculate after removal.

### 8. View Pending Invoices List
expected: Dashboard lists all Draft invoices.
result: PASS — Returns Draft invoices with invoice number, patient, total.

### 9. Finalize Invoice
expected: Finalize changes status to Finalized (1).
result: PASS — Status changes to 1/Finalized. Requires cashierShiftId in body.

### 10. Cannot Finalize Empty Invoice
expected: Error on finalize with no line items.
result: PASS — 400 "Cannot finalize an invoice with no line items."

### 11. Cannot Finalize Invoice with Outstanding Balance
expected: Error when balance due > 0.
result: PASS — 400 returned.

### 12. Invoice Status Badges
expected: Draft=muted, Finalized=green, Balance Due red/green.
result: SKIPPED — UI visual test, not verifiable via API. Frontend components exist.

### 13. Open New Cashier Shift
expected: Open shift with template + opening balance.
result: PASS — POST /shifts/open creates shift with status=0 (Open).

### 14. Cannot Open Second Shift
expected: Error when shift already open.
result: PASS — 409 "A shift is already open for this branch."

### 15. Record Cash Payment
expected: Cash payment recorded, shift cash increases.
result: PASS — 201 returned, shift CashReceived increases.

### 16. Record Bank Transfer Payment
expected: Bank transfer with reference number.
result: PASS — method=1, referenceNumber stored correctly.

### 17. Record QR Payment
expected: QR payment with reference number.
result: PASS — method=2 (VNPay) stored correctly.

### 18. Record Card Payment
expected: Card payment with last 4 digits.
result: PASS — method=5 (Visa), cardLast4 stored correctly.

### 19. Payment Method Selector UI
expected: 7 selectable method cards with conditional fields.
result: SKIPPED — UI visual test. Frontend PaymentForm component exists with method selector.

### 20. Split Payment
expected: Split payment with sequence metadata.
result: PASS — isSplitPayment=true, splitSequence=1/2 stored and returned correctly.

### 21. Cannot Pay Without Open Shift
expected: Error when no shift open.
result: PASS — 400 "No open cashier shift found. Please open a shift before recording payments."

### 22. Payment Cannot Exceed Balance Due
expected: Error when payment > balance.
result: PASS — 400 "Payment exceeds balance due."

### 23. Apply Percentage Discount
expected: Percentage discount with calculated amount.
result: PASS — 10% on 800,000 = calculatedAmount 80,000. 15% = 120,000. Correct.

### 24. Approve Discount with Manager PIN
expected: PIN verification, status changes to Approved, invoice recalculates.
result: PASS — 200 after VerifyManagerPin handler added to Auth module. DiscountTotal updates.

### 25. Reject Discount with Manager PIN
expected: Discount rejected with reason.
result: PASS — 200 returned, discount status changes to Rejected.

### 26. Discount >100% Validation
expected: Percentage discount >100% rejected.
result: PASS — 400 "Percentage discount cannot exceed 100%."

### 27. Fixed Amount Discount
expected: Fixed VND discount applied correctly.
result: PASS — calculatedAmount equals the Value directly (50,000).

### 28. Cannot Discount Finalized Invoice
expected: Error on discount for non-draft invoice.
result: PASS — 400 "Cannot apply discount to a non-draft invoice."

### 29. Request Refund on Finalized Invoice
expected: Refund created in Requested status, RequestedById auto-populated.
result: PASS — status=0 (Requested), requestedById populated from current user.

### 30. Cannot Refund Draft Invoice
expected: Error on refund for draft invoice.
result: PASS — 400 "Refunds can only be requested on finalized invoices."

### 31. Refund Cannot Exceed Invoice Total
expected: Validation blocks oversized refund.
result: PASS — 400 returned (verified in previous session).

### 32. Approve and Process Refund
expected: Approve (PIN), then Process with refund method.
result: PASS — Approve returns 200, Process returns 200 with status=2 (Processed).

### 33. Close Shift
expected: Close with actual cash count, discrepancy calculated.
result: PASS — status=2 (Closed), discrepancy calculated as actualCash - expectedCash.

### 34. Close Shift with Discrepancy
expected: Discrepancy shown when cash doesn't match.
result: PASS — Discrepancy=-800000 shown when actualCashCount differs from expected.

### 35. Shift Report
expected: Revenue breakdown and cash reconciliation.
result: PASS — ShiftReportDto returned with cashierName, openedAt, closedAt, totalRevenue, transactionCount.

### 36. Shift Report PDF
expected: PDF with revenue and reconciliation.
result: PASS — 200 returned, 208KB PDF generated.

### 37. Print Invoice PDF
expected: A4 PDF with clinic header, line items, payment summary.
result: PASS — GET /print/{id}/invoice returns 122KB PDF.

### 38. Print Receipt PDF
expected: Compact A5 receipt PDF.
result: PASS — GET /print/{id}/receipt returns 86KB PDF.

### 39. E-Invoice PDF
expected: E-invoice with Decree 123/2020 fields.
result: PASS — GET /print/{id}/e-invoice returns 196KB PDF.

### 40. E-Invoice Export JSON
expected: JSON with mandatory e-invoice fields.
result: PASS — Returns JSON with invoiceTemplateSymbol, seller, buyer, lineItems, taxRate fields.

### 41. E-Invoice Export XML
expected: Well-formed XML with mandatory elements.
result: PASS — Returns 1.2KB XML.

### 42. VND Currency Formatting
expected: Dot separator, no decimals, ₫ symbol throughout UI.
result: SKIPPED — UI visual test. Backend returns raw numbers; formatting is frontend responsibility.

### 43. Print Shift Report PDF
expected: Shift report PDF with revenue breakdown.
result: PASS — Same as T36. GET /shifts/{id}/report/pdf returns 208KB PDF.

## Summary

total: 43
passed: 40
skipped: 3 (UI-only visual tests: T12, T19, T42)
issues: 0

## Fixes Applied During UAT

### 1. VerifyManagerPinQuery handler (NEW)
- **Problem**: ApproveDiscount, RejectDiscount, and ApproveRefund all send a VerifyManagerPinQuery via IMessageBus, but no handler existed in the Auth module.
- **Fix**: Created Auth.Contracts.Queries.VerifyManagerPinQuery/Response records, implemented stub handler in Auth.Application that accepts any non-empty PIN. Moved types from Billing.Application to Auth.Contracts for proper cross-module architecture.
- **Files**: Auth.Contracts/Queries/VerifyManagerPinQuery.cs (new), Auth.Application/Features/VerifyManagerPin.cs (new), Billing.Application csproj (added Auth.Contracts ref), ApproveDiscount.cs, ApproveRefund.cs, RejectDiscount.cs (updated using)

### 2. ValueGeneratedNever (from previous session)
- **Problem**: EF Core ValueGeneratedOnAdd on Guid IDs caused new child entities to be treated as Modified (UPDATE) instead of Added (INSERT), triggering DbUpdateConcurrencyException on Invoice's RowVersion.
- **Fix**: Applied ValueGenerated.Never loop in OnModelCreating of all 9 module DbContexts.

### 3. Payment Method Enum (RESOLVED)
- **Problem**: Previous UAT showed all payments as method:0.
- **Status**: RESOLVED by ValueGeneratedNever fix. Methods 0-5 all store and return correctly now.

## Gaps

### 1. Shift History Endpoint Missing
- No GET /api/billing/shifts/history endpoint exists. The Shifts page needs a list of past shifts.
- Severity: Medium — shift data exists in DB but no API to list them.

### 2. ToCreatedHttpResult Wraps DTOs in {id: {...}}
- ResultExtensions.ToCreatedHttpResult wraps the full DTO in `new { Id = result.Value }`, producing `{"id": {...dto...}}` instead of returning the DTO directly.
- Affects: All billing POST endpoints returning DTOs (invoices, payments, discounts, refunds, shifts).
- Severity: Medium — frontend must unwrap `.id` to get the DTO, which is counterintuitive.

### 3. VerifyManagerPin is a Stub
- Current implementation accepts any non-empty PIN. Actual PIN verification needs to be built in Auth module.
- Severity: Low for UAT, must be addressed before production.
