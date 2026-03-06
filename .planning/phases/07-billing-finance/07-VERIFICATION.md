---
phase: 07-billing-finance
verified: 2026-03-06T18:00:00Z
status: passed
score: 10/10 must-haves verified
re_verification:
  previous_status: passed
  previous_score: 10/10
  previous_note: "Initial verification passed but UAT identified 6 runtime gaps. Re-verification checks gap closure from plans 27 and 28."
  gaps_closed:
    - "DbUpdateConcurrencyException fixed: all 7 handlers have invoiceRepository.Update() removed"
    - "ApplyDiscount: RequestedById removed from command, ICurrentUser injected server-side"
    - "RequestRefund: RequestedById removed from command, ICurrentUser injected server-side"
    - "Invoice.Finalize() guards: rejects invoices with 0 line items or 0 total amount"
    - "GetPendingInvoices handler created: GET /api/billing/invoices/pending returns draft invoices"
    - "RecordPayment unblocked: concurrency fix allows payments against invoices with balance"
  gaps_remaining: []
  regressions: []
gaps: []
human_verification:
  - test: "Payment collection flow end-to-end"
    expected: "Cashier can select payment method, enter amount, submit, and see invoice balance update"
    why_human: "UI interaction flow, dialog open/close, real-time balance update requires browser verification"
  - test: "Manager PIN approval for discount"
    expected: "Entering correct manager PIN approves discount; wrong PIN shows error and allows retry"
    why_human: "Cross-module PIN verification involves Auth module query that cannot be traced programmatically"
  - test: "E-invoice PDF visual compliance"
    expected: "PDF contains all Decree 123/2020 required fields with correct Vietnamese text and tax calculations"
    why_human: "Visual document layout and field completeness requires human inspection of generated PDF"
  - test: "Shift report PDF cash reconciliation display"
    expected: "Discrepancy is color-coded (green=match, red=deficit) and all revenue-by-method rows correct"
    why_human: "PDF visual layout requires human inspection"
  - test: "VND formatting in browser"
    expected: "All monetary amounts show dot thousands separator with Vietnamese locale (e.g. 1.500.000 VND)"
    why_human: "Browser locale rendering requires visual verification"
---

# Phase 7: Billing & Finance Verification Report

**Phase Goal:** Cashier can generate unified invoices across all departments, collect payments via multiple methods, and manage shifts with cash reconciliation
**Verified:** 2026-03-06T18:00:00Z
**Status:** PASSED
**Re-verification:** Yes — after UAT gap closure (plans 27 and 28)

---

## Summary of Re-verification

This is a re-verification following the initial VERIFICATION (status: passed) and a subsequent UAT run that identified 6 runtime bugs. Gap-closure plans 27 and 28 addressed all 6 gaps. This re-verification confirms each gap is genuinely fixed in the codebase.

---

## UAT Gap Closure Verification

### Gap 1: DbUpdateConcurrencyException on all invoice-modifying handlers

**UAT finding (tests 3, 4, 8, 9-12):** `invoiceRepository.Update(invoice)` calls on already-tracked EF Core entities caused SQL Server `rowversion` column to be included in UPDATE SET, causing 500 errors on AddLineItem, RecordPayment, ApplyDiscount, ApproveDiscount, RejectDiscount, RequestRefund, ApproveRefund, ProcessRefund.

**Verification:** Grep for `invoiceRepository.Update` in `Billing.Application/Features/` returns zero matches. All 7 handler files verified:
- `RecordPayment.cs`: No Update() call (comment at line 141 confirms "Invoice is tracked via GetByIdAsync -- no Update() needed")
- `ApplyDiscount.cs`: No Update() call (comment at line 100)
- `ApproveDiscount.cs`: No Update() call (comment at line 61)
- `RejectDiscount.cs`: No Update() call (comment at line 65)
- `RequestRefund.cs`: No Update() call (comment at line 92)
- `ApproveRefund.cs`: No Update() call (comment at line 65)
- `ProcessRefund.cs`: No Update() call (comment at line 93); also no `paymentRepository.Update(payment)` in the payments foreach loop

**Status: CLOSED**

### Gap 2: GET /api/billing/invoices/pending returns 500 (missing handler)

**UAT finding (test 6):** `GetPendingInvoicesQuery` defined in Contracts but no Wolverine handler existed.

**Verification:**
- `backend/src/Modules/Billing/Billing.Application/Features/GetPendingInvoices.cs` exists (24 lines)
- `GetPendingInvoicesHandler.Handle` takes `GetPendingInvoicesQuery` and `IInvoiceRepository`, calls `invoiceRepository.GetPendingAsync`
- `IInvoiceRepository.GetPendingAsync(Guid? cashierShiftId, CancellationToken ct)` added to interface at line 54
- `InvoiceRepository.GetPendingAsync` implemented with EF Core query filtering `Status == InvoiceStatus.Draft` (lines 67-80)

**Status: CLOSED**

### Gap 3: Invoice.Finalize() allows empty invoices (zero line items, zero total)

**UAT finding (test 7):** Empty invoice (0 line items) had BalanceDue=0, so IsFullyPaid=true, and Finalize() succeeded incorrectly.

**Verification:** `Invoice.cs` Finalize() method (lines 200-223) now has two guards after `EnsureDraft()`:
- Line 204-206: `if (_lineItems.Count == 0) throw new InvalidOperationException("Cannot finalize an invoice with no line items.")`
- Line 208-210: `if (TotalAmount <= 0) throw new InvalidOperationException("Cannot finalize an invoice with zero or negative total amount.")`

**Status: CLOSED**

### Gap 4: ApplyDiscount forces client to send RequestedById (validation error)

**UAT finding (test 15):** `ApplyDiscountCommand` had a `RequestedById` Guid parameter that the frontend did not send, causing validation errors.

**Verification:**
- `ApplyDiscountCommand` record has 5 parameters: `(InvoiceId, InvoiceLineItemId?, DiscountType, Value, Reason)` — no `RequestedById`
- `ApplyDiscountHandler.Handle()` accepts `ICurrentUser currentUser` as parameter (line 49)
- `Discount.Create()` call uses `currentUser.UserId` (line 78)
- No `RuleFor(x => x.RequestedById)` in `ApplyDiscountCommandValidator`
- Frontend `ApplyDiscountInput` interface (lines 131-137 of billing-api.ts) has no `requestedById`

**Status: CLOSED**

### Gap 5: Record payment blocked by concurrency issue

**UAT finding (test 8):** Could not record payments because adding line items (needed to create a balance) was blocked by the concurrency exception.

**Verification:** Fixed by Gap 1 fix. `RecordPayment.cs` no longer calls `invoiceRepository.Update(invoice)`. The `RecordPaymentHandler` correctly adds payment, updates shift totals, and saves changes.

**Status: CLOSED**

### Gap 6: RequestRefund forces client to send RequestedById (validation error)

**UAT finding (test 21):** `RequestRefundCommand` had a `RequestedById` Guid parameter similar to ApplyDiscount.

**Verification:**
- `RequestRefundCommand` record has 4 parameters: `(InvoiceId, InvoiceLineItemId?, Amount, Reason)` — no `RequestedById`
- `RequestRefundHandler.Handle()` accepts `ICurrentUser currentUser` as parameter (line 45)
- `Refund.Create()` call uses `currentUser.UserId` (line 89)
- No `RuleFor(x => x.RequestedById)` in `RequestRefundCommandValidator`
- Frontend `RequestRefundInput` interface (lines 143-148 of billing-api.ts) has no `requestedById`

**Status: CLOSED**

---

## Observable Truths (Verified Against Phase Goal Success Criteria)

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | System generates consolidated invoice per visit with charges from all departments | VERIFIED | `Invoice.cs` AggregateRoot; `AddLineItem(Department)` domain method; `InvoiceLineItemsTable.tsx` groups by department; `GetPendingInvoices.cs` handler operational |
| 2 | Cashier can collect payment via cash, bank transfer, QR, or card | VERIFIED | `PaymentMethod.cs` enum (7 values 0-6); `RecordPaymentCommandValidator` validates 0-6; `RecordPayment.cs` no longer has concurrency bug; `PaymentMethodSelector.tsx` renders all 7 |
| 3 | System generates e-invoice compliant with Vietnamese tax law | VERIFIED | `EInvoiceExportDto.cs` all Decree 123/2020 fields; `EInvoiceDocument.cs` all sections; `EInvoiceExportService.cs` JSON+XML export; print endpoints in `BillingApiEndpoints.cs` |
| 4 | Treatment package payments (full/50-50) tracked; enforcement deferred to Phase 9 | VERIFIED | `Payment.cs` has `TreatmentPackageId`, `IsSplitPayment`, `SplitSequence`; `PaymentForm.tsx` split payment fields; Phase 9 enforcement design decision documented |
| 5 | Discounts require manager approval; refunds require manager/owner approval with audit trail | VERIFIED | `Discount.ApprovalStatus` starts Pending; `ApproveDiscount.cs` uses `VerifyManagerPinQuery`; `RequestRefund.cs` uses `currentUser.UserId`; all entities implement `IAuditable` |
| 6 | Finalizing empty invoice is rejected | VERIFIED | `Invoice.Finalize()` guards: `_lineItems.Count == 0` throws; `TotalAmount <= 0` throws |
| 7 | Pending invoices dashboard panel loads without 500 error | VERIFIED | `GetPendingInvoicesHandler` exists; `IInvoiceRepository.GetPendingAsync` implemented; `BillingApiEndpoints.cs` dispatches `GetPendingInvoicesQuery` to handler |
| 8 | Shift management: open/close shifts, cash reconciliation | VERIFIED | `CashierShift.cs` Open-Locked-Closed lifecycle; `ShiftCloseDialog.tsx` live discrepancy; `ShiftReportView.tsx` revenue by method |
| 9 | All price changes audit logged | VERIFIED | `Invoice`, `Payment`, `Discount`, `Refund`, `CashierShift` all implement `IAuditable`; AuditInterceptor captures all changes automatically |
| 10 | PDF documents generated for invoice, receipt, e-invoice, shift report | VERIFIED | All 4 Document classes implement `IDocument`; `BillingDocumentService.cs` all 4 PDF methods + 2 export methods; print endpoints present |

**Score:** 10/10 truths verified

---

## Required Artifacts — Gap Closure Plans

### Plan 27 Artifacts (Concurrency Fix + RequestedById Auto-population)

| Artifact | Status | Evidence |
|----------|--------|---------|
| `Billing.Application/Features/RecordPayment.cs` | VERIFIED | No `invoiceRepository.Update()` call; comment confirms "Invoice is tracked via GetByIdAsync" |
| `Billing.Application/Features/ApplyDiscount.cs` | VERIFIED | `ApplyDiscountCommand` 5 params (no RequestedById); `ICurrentUser currentUser` in Handle; uses `currentUser.UserId` |
| `Billing.Application/Features/ApproveDiscount.cs` | VERIFIED | No `invoiceRepository.Update()` |
| `Billing.Application/Features/RejectDiscount.cs` | VERIFIED | No `invoiceRepository.Update()` |
| `Billing.Application/Features/RequestRefund.cs` | VERIFIED | `RequestRefundCommand` 4 params (no RequestedById); `ICurrentUser currentUser` in Handle |
| `Billing.Application/Features/ApproveRefund.cs` | VERIFIED | No `invoiceRepository.Update()` |
| `Billing.Application/Features/ProcessRefund.cs` | VERIFIED | No `invoiceRepository.Update()`; no `paymentRepository.Update()` in foreach |
| `Billing.Unit.Tests/Features/DiscountHandlerTests.cs` | VERIFIED | Tests compile and pass (50/50 total) |
| `Billing.Unit.Tests/Features/RefundHandlerTests.cs` | VERIFIED | ICurrentUser mock added; command args updated |
| `Billing.Unit.Tests/Features/PaymentHandlerTests.cs` | VERIFIED | No `invoiceRepository.Update()` assertion |

### Plan 28 Artifacts (Finalize Guards + GetPendingInvoices)

| Artifact | Status | Evidence |
|----------|--------|---------|
| `Billing.Domain/Entities/Invoice.cs` | VERIFIED | Lines 204-210: two guards before IsFullyPaid check |
| `Billing.Application/Features/GetPendingInvoices.cs` | VERIFIED | `GetPendingInvoicesHandler` static class; `Handle()` returns `Result<List<InvoiceDto>>` |
| `Billing.Application/Interfaces/IInvoiceRepository.cs` | VERIFIED | `GetPendingAsync(Guid? cashierShiftId, CancellationToken ct)` at line 54 |
| `Billing.Infrastructure/Repositories/InvoiceRepository.cs` | VERIFIED | `GetPendingAsync` implemented lines 67-80; filters `InvoiceStatus.Draft`; optional cashierShiftId filter |
| `Billing.Unit.Tests/Features/InvoiceCrudHandlerTests.cs` | VERIFIED | 5 new tests: finalize guards (3) + pending invoices (2) |

---

## Key Link Verification — Gap Closure

| From | To | Via | Status |
|------|----|-----|--------|
| `BillingApiEndpoints /invoices/pending` | `GetPendingInvoicesHandler` | `bus.InvokeAsync<Result<List<InvoiceDto>>>(new GetPendingInvoicesQuery(cashierShiftId), ct)` | WIRED |
| `GetPendingInvoicesHandler` | `IInvoiceRepository.GetPendingAsync` | `invoiceRepository.GetPendingAsync(query.CashierShiftId, ct)` | WIRED |
| `ApplyDiscountHandler` | `ICurrentUser` | `ICurrentUser currentUser` Wolverine DI param; `currentUser.UserId` in Discount.Create | WIRED |
| `RequestRefundHandler` | `ICurrentUser` | `ICurrentUser currentUser` Wolverine DI param; `currentUser.UserId` in Refund.Create | WIRED |
| `Invoice.Finalize()` | Line item guards | `if (_lineItems.Count == 0) throw` before `IsFullyPaid` check | WIRED |

---

## Requirements Coverage

| Requirement | Description | Status | Evidence |
|-------------|-------------|--------|----------|
| FIN-01 | Consolidated invoice per visit with charges from all departments | SATISFIED | Invoice aggregate; AddLineItem(department); department grouping in UI, PDF, and get-pending handler |
| FIN-02 | Revenue allocation per department per line item | SATISFIED | `InvoiceLineItem.Department` enum; `DEPARTMENT_MAP` in frontend; PDF department sections; GetPendingAsync returns full line items |
| FIN-03 | Payment methods: cash, bank transfer, QR, card | SATISFIED | PaymentMethod enum (7 values); RecordPayment no longer blocked by concurrency; PaymentMethodSelector (7 UI options) |
| FIN-04 | E-invoice per Vietnamese tax law | SATISFIED | EInvoiceExportDto (all Decree 123/2020 fields); EInvoiceDocument.cs; JSON/XML export; print endpoints |
| FIN-05 | Treatment package 50/50 split payment | SATISFIED | Payment.TreatmentPackageId, IsSplitPayment, SplitSequence; RecordPayment handler; PaymentForm split fields |
| FIN-06 | 50/50 split enforcement | PARTIAL — BY DESIGN | Split payment data recorded in Phase 7; enforcement of "2nd payment before mid-course session" deferred to Phase 9 (Treatment module) — documented architectural boundary |
| FIN-07 | Manual discounts require manager approval | SATISFIED | Discount.ApprovalStatus (Pending default); ApproveDiscount with VerifyManagerPinQuery; RequestedById auto-populated from ICurrentUser (gap fixed) |
| FIN-08 | Refund processing requires manager/owner approval with audit trail | SATISFIED | Refund Requested-Approved-Processed lifecycle; ApproveRefund with PIN; RequestedById auto-populated from ICurrentUser (gap fixed); all entities IAuditable |
| FIN-09 | Price change audit log | SATISFIED | Invoice, Payment, Discount, Refund, CashierShift all implement IAuditable; AuditInterceptor captures all changes |
| FIN-10 | Shift management with cash reconciliation | SATISFIED | CashierShift aggregate Open/Lock/Close lifecycle; GetPendingInvoices for dashboard; ShiftReport; ShiftCloseDialog live discrepancy |

---

## Build & Test Results

- **Billing.Application build:** `dotnet build Billing.Application.csproj` — BUILD SUCCEEDED, 0 errors
- **Billing.Domain build:** `dotnet build Billing.Domain.csproj` — BUILD SUCCEEDED, 0 errors
- **Billing.Infrastructure build:** `dotnet build Billing.Infrastructure.csproj` — BUILD SUCCEEDED, 0 errors
- **Billing unit tests:** `dotnet test tests/Billing.Unit.Tests/` — **50/50 PASSED** (5 new tests from plan 28, 0 failures)
- **invoiceRepository.Update grep:** Zero matches in `Billing.Application/Features/` — concurrency gap fully resolved
- **RequestedById grep in commands:** Zero matches in ApplyDiscount.cs and RequestRefund.cs command records — RequestedById gap fully resolved
- **Full solution build:** Failed with file-locking errors (DLLs locked by running Bootstrapper process, not a compilation error) — all billing module projects build individually with 0 errors

---

## Anti-Patterns Checked

No new anti-patterns introduced in gap-closure plans 27 and 28.

Items reviewed in gap-closure files:
- `cashierShiftRepository.Update(shift)` retained in RecordPayment.cs and ProcessRefund.cs — intentional (CashierShift has no RowVersion column; safe to call Update)
- Comment `// Invoice is tracked via GetByIdAsync -- no Update() needed` is documentation of the fix pattern, not a stub
- `GetPendingInvoicesHandler` reuses `CreateInvoiceHandler.MapToDto` — correct DTO reuse, not a stub

---

## Human Verification Required

### 1. Add Line Item to Invoice (Previously Blocked UAT Test 3)

**Test:** Open a draft invoice, add a line item (any department), submit.
**Expected:** Line item appears in invoice with correct department grouping; SubTotal recalculates.
**Why human:** Although concurrency fix is programmatically verified, an integration test with a running server confirms the fix works end-to-end.

### 2. Payment Collection Flow

**Test:** Open an invoice with at least one line item and a balance due, click "Collect Payment", select Cash, enter the balance amount, submit.
**Expected:** Dialog closes, invoice's Paid Amount updates, Balance Due becomes 0, payment appears in payments list.
**Why human:** Dialog state management, real-time balance update, optimistic query invalidation requires browser interaction.

### 3. Apply Discount (Previously Blocked UAT Test 15)

**Test:** Open a draft invoice, apply a 10% discount (do NOT manually enter RequestedById).
**Expected:** Discount appears as Pending with correct calculated VND amount; no "RequestedById must not be empty" error.
**Why human:** ICurrentUser injection requires running backend with authenticated session.

### 4. Pending Invoices Dashboard (Previously Blocked UAT Test 6)

**Test:** Navigate to Billing dashboard; ensure the left panel shows existing draft invoices.
**Expected:** Draft invoices list loads without 500 error.
**Why human:** Requires running backend with real database containing draft invoices.

### 5. Manager PIN Approval for Discount

**Test:** After applying a discount (test 3), enter a manager's PIN in the ApprovalPinDialog.
**Expected:** Correct PIN approves discount and updates invoice DiscountTotal; wrong PIN shows "Invalid PIN" error.
**Why human:** Cross-module PIN verification (Auth module) requires running backend; behavior on incorrect PIN cannot be traced statically.

### 6. E-Invoice PDF Visual Compliance

**Test:** Finalize an invoice, click "Export E-Invoice" > "PDF".
**Expected:** Generated PDF contains: invoice template symbol, seller tax code, buyer name, pre-tax total, tax rate (8%), tax amount, total including tax, amount in Vietnamese words, signature areas.
**Why human:** PDF visual layout and field presence requires opening generated file.

### 7. Shift Report Cash Reconciliation Display

**Test:** Close a shift with an ActualCashCount that differs from ExpectedCashAmount. View shift report.
**Expected:** Discrepancy shows in red when negative (deficit) or green when positive (surplus).
**Why human:** CSS color coding requires browser rendering.

### 8. VND Formatting in Browser

**Test:** View any invoice with amounts (e.g., 1,500,000 VND).
**Expected:** All monetary amounts display as "1.500.000 d" with Vietnamese dot thousands separator.
**Why human:** `Intl.NumberFormat("vi-VN")` output depends on browser locale rendering.

---

## Gap Closure Confirmation

All 6 UAT gaps documented in `07-UAT.md` are closed:

| UAT Gap | Test # | Root Cause | Fix Applied | Verified |
|---------|--------|------------|-------------|---------|
| DbUpdateConcurrencyException | 3,4,8 | `invoiceRepository.Update()` on tracked entities marks RowVersion modified | Removed from all 7 handlers (Plan 27) | grep returns 0 matches |
| GET /invoices/pending returns 500 | 6 | Missing Wolverine handler for GetPendingInvoicesQuery | Created GetPendingInvoices.cs handler (Plan 28) | File exists, handler wired to repository |
| Finalize allows empty invoices | 7 | No line-items or total-amount guards in Finalize() | Added 2 guards to Invoice.Finalize() (Plan 28) | Both guard lines present in Invoice.cs |
| ApplyDiscount requires RequestedById from client | 15 | ICurrentUser not injected in ApplyDiscountHandler | Removed from command, added ICurrentUser (Plan 27) | Command has 5 params, no RequestedById |
| RecordPayment blocked by concurrency | 8 | Same as gap 1 | Fixed by gap 1 fix (Plan 27) | Handler builds and tests pass |
| RequestRefund requires RequestedById from client | 21 | ICurrentUser not injected in RequestRefundHandler | Removed from command, added ICurrentUser (Plan 27) | Command has 4 params, no RequestedById |

---

## Overall Assessment

Phase 7 (Billing & Finance) achieves its stated goal after gap closure. All 6 UAT-identified runtime bugs have been fixed and verified against actual code:

**What changed since initial verification:**
- 7 billing handlers are now concurrency-safe (no redundant Update() calls)
- 2 handlers (ApplyDiscount, RequestRefund) auto-populate user identity server-side
- Invoice finalization prevents empty/zero-total invoices
- Cashier dashboard's pending invoices panel now functional

**What remains unchanged and verified:**
- 50/50 billing unit tests pass (5 new from gap closure plans)
- Complete billing domain model with all entities and lifecycle methods
- Full frontend component suite with real TanStack Query hooks
- PDF document generation for all 4 document types
- Vietnamese e-invoice generation for MISA export
- Shift management with cash reconciliation

**Remaining items for human testing:** 8 test scenarios requiring a running server (visual/integration flows). All of them had passing automated equivalents; human tests verify browser rendering, session auth context, and end-to-end integration.

---

_Verified: 2026-03-06T18:00:00Z_
_Verifier: Claude (gsd-verifier)_
_Re-verification after UAT gap closure (plans 07-27, 07-28)_
