---
phase: 07-billing-finance
verified: 2026-03-17T16:30:00Z
status: passed
score: 14/14 must-haves verified
re_verification:
  previous_status: passed
  previous_score: 10/10
  previous_note: "Second re-verification after UAT retest (0/4 tests passed). Plans 07-32 and 07-33 addressed all 4 UAT retest gaps."
  gaps_closed:
    - "Invoice history rows are now clickable and navigate to /billing/invoices/{id}"
    - "Invoice detail page auto-refreshes via SignalR (useBillingHub) when prescription items are added"
    - "Prescription-linked line items cannot be manually removed by cashier (domain guard + frontend lock icon)"
    - "When doctor removes medicines from a visit prescription, billing line items are removed automatically via DrugPrescriptionRemoved event chain"
  gaps_remaining: []
  regressions:
    - "3 TypeScript errors in billing files (billing-api.ts line 429, RefundDialog.tsx lines 177/183, use-billing-hub.ts line 14) — verified pre-existing from commit e162107 before plan 32/33 changes, not regressions"
gaps: []
human_verification:
  - test: "Invoice history row click navigation"
    expected: "Clicking any part of an invoice row in /billing/invoices navigates to /billing/invoices/{id}"
    why_human: "Row onClick with useNavigate requires browser interaction to verify routing"
  - test: "Invoice detail page auto-refresh when prescription is added"
    expected: "Doctor adds prescription; cashier's open InvoiceView auto-refreshes to show new line items without manual browser refresh"
    why_human: "SignalR real-time behavior requires running backend + frontend with two concurrent browser sessions"
  - test: "Prescription line item lock icon on draft invoice"
    expected: "Prescription-sourced items show lock icon instead of delete button; manually-added items show delete button"
    why_human: "UI rendering of conditional icon requires browser visual verification"
  - test: "Doctor removes prescription; cashier invoice auto-updates"
    expected: "Doctor removes prescription in clinical module; cashier's invoice detail page auto-removes the line items in real-time via SignalR LineItemRemoved event"
    why_human: "Cross-module event chain with real-time SignalR notification requires two browser sessions and running backend"
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
**Verified:** 2026-03-17T16:30:00Z
**Status:** PASSED
**Re-verification:** Yes — third verification after UAT retest gap closure (plans 07-32, 07-33)

---

## Summary of Re-verification

This is the third verification of Phase 7. The second verification (2026-03-06) passed after fixing 6 runtime UAT bugs. A subsequent UAT retest (2026-03-17) found 4 more issues (0/4 tests passed):

1. Invoice history row click did not navigate to detail page
2. Prescription line items required manual page refresh
3. Cashier could remove prescription-linked line items (revenue protection gap)
4. Removing a doctor's prescription did not remove the corresponding billing line items

Gap closure plans 07-32 and 07-33 addressed all 4 gaps. This verification confirms all gaps are genuinely closed in the codebase.

---

## UAT Retest Gap Closure Verification

### Gap 1: Invoice history rows not clickable (Plan 07-32, Task 1)

**UAT finding (test 1):** Clicking a row in the Invoice History page did not navigate to the invoice detail page.

**Verification:**
- `InvoiceHistoryPage.tsx` line 2: `import { Link, useNavigate } from "@tanstack/react-router"`
- `InvoiceHistoryPage.tsx` line 40: `const navigate = useNavigate()`
- `InvoiceHistoryPage.tsx` lines 119-123: `<TableRow className="cursor-pointer hover:bg-muted/50" onClick={() => navigate({ to: "/billing/invoices/$invoiceId", params: { invoiceId: invoice.id } })}>`
- Row still contains the Link on the invoice number cell for visual affordance — both row click and link click navigate to the same route.

**Status: CLOSED**

### Gap 2: Prescription line items required manual refresh (Plan 07-32, Task 2)

**UAT finding (test 2):** When a doctor added a prescription, the cashier had to manually refresh the browser to see the new line items on the invoice detail page.

**Verification:**
- `InvoiceView.tsx` line 34: `import { useBillingHub } from "@/features/billing/hooks/use-billing-hub"`
- `InvoiceView.tsx` line 77: `useBillingHub() // Real-time updates when prescription items are added`
- `use-billing-hub.ts` lines 52-55: `connection.on("LineItemAdded", (notification: { invoiceId: string }) => { queryClientRef.current.invalidateQueries({ queryKey: billingKeys.invoice(notification.invoiceId) }); queryClientRef.current.invalidateQueries({ queryKey: billingKeys.pendingInvoices() }) })`
- Full chain: doctor prescribes -> domain event -> `HandleDrugPrescriptionAdded` creates line items + sends SignalR "LineItemAdded" -> `useBillingHub` invalidates `billingKeys.invoice(id)` -> `useInvoice` refetches -> `InvoiceView` re-renders.

**Status: CLOSED**

### Gap 3: Cashier could remove prescription-linked line items (Plan 07-33, Task 1 + Task 2)

**UAT finding (test 3):** Prescription-linked line items (created when doctor prescribed medicine) had a delete button, allowing cashier to silently remove them and create revenue loss.

**Verification — Backend domain guard:**
- `Invoice.cs` lines 133-147: `RemoveLineItem()` now checks `if (lineItem.SourceType == "Prescription") throw new InvalidOperationException("Cannot manually remove prescription-linked line items...")`
- `RemoveInvoiceLineItem.cs` lines 52-60: Handler catches `InvalidOperationException` and returns `Error.Custom("Error.InvalidOperation", ex.Message)` — API returns 4xx, not 500.

**Verification — Frontend UI guard:**
- `InvoiceLineItemsTable.tsx` lines 161-196: When `isDraft`, each item renders either a lock icon (`item.sourceType === "Prescription"`) or the delete button (all other sourceTypes).
- `InvoiceLineItemsTable.tsx` line 164: `<IconLock className="h-4 w-4 text-muted-foreground" title={t("lineItems.prescriptionLocked")} />`
- i18n: `frontend/public/locales/en/billing.json:101` → `"prescriptionLocked": "Managed by doctor's prescription"`
- i18n: `frontend/public/locales/vi/billing.json:101` → `"prescriptionLocked": "Được quản lý bởi đơn thuốc của bác sĩ"`

**Status: CLOSED**

### Gap 4: Removing doctor's prescription did not remove billing line items (Plan 07-33, Task 1)

**UAT finding (test 4):** When a doctor removed medicines from a visit prescription, the corresponding billing line items remained on the invoice.

**Verification — Event chain:**
- `Visit.cs` lines 228-239: `RemoveDrugPrescription()` now raises `AddDomainEvent(new DrugPrescriptionRemovedEvent(Id, BranchId.Value, drugNames))` after removal
- `DrugPrescriptionRemovedEvent.cs`: Domain event record with `VisitId`, `BranchId`, `DrugNames`
- `PublishDrugPrescriptionRemovedIntegrationEvent.cs`: Cascading Wolverine handler converts domain event to `DrugPrescriptionRemovedIntegrationEvent`
- `DrugPrescriptionRemovedIntegrationEvent.cs`: Cross-module integration event record
- `HandleDrugPrescriptionRemoved.cs`: Wolverine handler loads invoice by VisitId, calls `invoice.RemoveLineItemsBySource(@event.VisitId, "Prescription")`, saves changes if items removed, sends SignalR `NotifyLineItemRemovedAsync`
- `Invoice.cs` lines 154-165: `RemoveLineItemsBySource()` removes all matching items by SourceId+SourceType, recalculates totals
- `IBillingNotificationService.cs` line 26: `Task NotifyLineItemRemovedAsync(Guid invoiceId, string invoiceNumber, int removedCount, CancellationToken ct)`
- `BillingNotificationService.cs` lines 71-89: Sends "LineItemRemoved" SignalR event to cashier-dashboard group
- `use-billing-hub.ts` lines 57-60: `connection.on("LineItemRemoved", ...)` invalidates `billingKeys.invoice(id)` and `billingKeys.pendingInvoices()`

**Status: CLOSED**

---

## Observable Truths (All 14 Must-Haves)

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | System generates consolidated invoice per visit with charges from all departments | VERIFIED | Invoice aggregate; AddLineItem(Department); department grouping in UI, PDF; GetPendingInvoices handler |
| 2 | Cashier can collect payment via cash, bank transfer, QR, or card | VERIFIED | PaymentMethod enum (7 values); RecordPayment handler concurrency-safe; PaymentMethodSelector (7 options) |
| 3 | System generates e-invoice compliant with Vietnamese tax law | VERIFIED | EInvoiceExportDto (all Decree 123/2020 fields); EInvoiceDocument.cs; JSON/XML export |
| 4 | Treatment package payments (full/50-50) tracked | VERIFIED | Payment.TreatmentPackageId, IsSplitPayment, SplitSequence; PaymentForm split fields |
| 5 | Discounts require manager approval; refunds require manager/owner approval with audit trail | VERIFIED | Discount.ApprovalStatus Pending default; ApproveDiscount with VerifyManagerPinQuery; ICurrentUser auto-populates identity |
| 6 | Finalizing empty invoice is rejected | VERIFIED | Invoice.Finalize() guards: 0 line items throws; TotalAmount <= 0 throws |
| 7 | Pending invoices dashboard panel loads without 500 error | VERIFIED | GetPendingInvoicesHandler exists; IInvoiceRepository.GetPendingAsync implemented |
| 8 | Shift management: open/close shifts, cash reconciliation | VERIFIED | CashierShift aggregate Open/Lock/Close lifecycle; ShiftCloseDialog live discrepancy |
| 9 | All price changes audit logged | VERIFIED | Invoice, Payment, Discount, Refund, CashierShift all implement IAuditable |
| 10 | PDF documents generated for invoice, receipt, e-invoice, shift report | VERIFIED | All 4 Document classes; BillingDocumentService; print endpoints |
| 11 | Invoice history rows are clickable and navigate to invoice detail page | VERIFIED | InvoiceHistoryPage.tsx TableRow has onClick+useNavigate; cursor-pointer class |
| 12 | Invoice detail page auto-refreshes when prescription items are added | VERIFIED | InvoiceView.tsx calls useBillingHub(); LineItemAdded handler invalidates query |
| 13 | Prescription-linked line items cannot be removed by cashier | VERIFIED | Invoice.RemoveLineItem() guard throws for SourceType=="Prescription"; InvoiceLineItemsTable shows lock icon |
| 14 | Removing doctor's prescription automatically removes billing line items | VERIFIED | Full event chain: Visit.RemoveDrugPrescription -> DrugPrescriptionRemovedEvent -> integration event -> HandleDrugPrescriptionRemoved -> RemoveLineItemsBySource + SignalR |

**Score:** 14/14 truths verified

---

## Required Artifacts — Plans 07-32 and 07-33

### Plan 07-32 Artifacts (Navigation + Real-time Refresh)

| Artifact | Status | Evidence |
|----------|--------|----------|
| `frontend/src/features/billing/components/InvoiceHistoryPage.tsx` | VERIFIED | `useNavigate` imported and called; `onClick` on `TableRow` with correct route params |
| `frontend/src/features/billing/components/InvoiceView.tsx` | VERIFIED | `useBillingHub` imported at line 34 and called at line 77 |
| `frontend/src/features/billing/hooks/use-billing-hub.ts` | VERIFIED | `LineItemAdded` handler invalidates both `billingKeys.invoice(id)` and `billingKeys.pendingInvoices()` |

### Plan 07-33 Artifacts (Prescription Guard + Removal Event Chain)

| Artifact | Status | Evidence |
|----------|--------|----------|
| `backend/src/Modules/Billing/Billing.Domain/Entities/Invoice.cs` | VERIFIED | `RemoveLineItem()` has Prescription guard (line 140-142); `RemoveLineItemsBySource()` added (lines 154-165) |
| `backend/src/Modules/Billing/Billing.Application/Features/RemoveInvoiceLineItem.cs` | VERIFIED | Handler catches `InvalidOperationException` and returns `Error.Custom` (lines 52-60) |
| `backend/src/Modules/Clinical/Clinical.Domain/Events/DrugPrescriptionRemovedEvent.cs` | VERIFIED | File exists; sealed record with VisitId, BranchId, DrugNames |
| `backend/src/Modules/Clinical/Clinical.Contracts/IntegrationEvents/DrugPrescriptionRemovedIntegrationEvent.cs` | VERIFIED | File exists; sealed record with same fields |
| `backend/src/Modules/Clinical/Clinical.Application/Features/PublishDrugPrescriptionRemovedIntegrationEvent.cs` | VERIFIED | Wolverine cascading handler; converts domain event to integration event |
| `backend/src/Modules/Billing/Billing.Application/Features/HandleDrugPrescriptionRemoved.cs` | VERIFIED | Loads invoice by VisitId; calls RemoveLineItemsBySource; saves if items removed; sends SignalR |
| `backend/src/Modules/Billing/Billing.Application/Interfaces/IBillingNotificationService.cs` | VERIFIED | `NotifyLineItemRemovedAsync` signature at line 26 |
| `backend/src/Modules/Billing/Billing.Infrastructure/Services/BillingNotificationService.cs` | VERIFIED | `NotifyLineItemRemovedAsync` implemented; sends "LineItemRemoved" to cashier-dashboard group |
| `frontend/src/features/billing/components/InvoiceLineItemsTable.tsx` | VERIFIED | `IconLock` imported; conditional render: Prescription -> lock icon, other -> delete button |
| `frontend/src/features/billing/hooks/use-billing-hub.ts` | VERIFIED | `LineItemRemoved` handler at lines 57-60; invalidates invoice + pending invoices queries |
| `frontend/public/locales/en/billing.json` | VERIFIED | `"prescriptionLocked": "Managed by doctor's prescription"` at line 101 |
| `frontend/public/locales/vi/billing.json` | VERIFIED | `"prescriptionLocked": "Được quản lý bởi đơn thuốc của bác sĩ"` at line 101 |

---

## Key Link Verification — Plans 07-32 and 07-33

| From | To | Via | Status |
|------|----|-----|--------|
| `InvoiceHistoryPage.tsx TableRow` | `/billing/invoices/$invoiceId` | `onClick` calls `navigate({ to: "/billing/invoices/$invoiceId", params: { invoiceId: invoice.id } })` | WIRED |
| `InvoiceView.tsx` | `use-billing-hub.ts LineItemAdded handler` | `useBillingHub()` hook call; invalidates `billingKeys.invoice(invoiceId)` | WIRED |
| `Visit.RemoveDrugPrescription` | `DrugPrescriptionRemovedEvent` | `AddDomainEvent(new DrugPrescriptionRemovedEvent(Id, BranchId.Value, drugNames))` at Visit.cs:238 | WIRED |
| `DrugPrescriptionRemovedEvent` | `DrugPrescriptionRemovedIntegrationEvent` | `PublishDrugPrescriptionRemovedIntegrationEventHandler.Handle()` converts domain->integration event | WIRED |
| `DrugPrescriptionRemovedIntegrationEvent` | `HandleDrugPrescriptionRemovedHandler` | Wolverine handler; calls `invoiceRepository.GetByVisitIdAsync` then `invoice.RemoveLineItemsBySource` | WIRED |
| `HandleDrugPrescriptionRemovedHandler` | `IBillingNotificationService.NotifyLineItemRemovedAsync` | Called after `SaveChangesAsync` when items were removed | WIRED |
| `BillingNotificationService.NotifyLineItemRemovedAsync` | `use-billing-hub.ts LineItemRemoved handler` | Sends "LineItemRemoved" SignalR event; hook invalidates `billingKeys.invoice(id)` | WIRED |
| `Invoice.RemoveLineItem()` (SourceType=="Prescription") | `RemoveInvoiceLineItemHandler` error response | Handler catches `InvalidOperationException`, returns `Error.Custom` | WIRED |
| `InvoiceLineItemsTable` item render | lock icon (Prescription) / delete button (other) | `item.sourceType === "Prescription"` conditional in DepartmentSection | WIRED |

---

## Requirements Coverage

| Requirement | Description | Status | Evidence |
|-------------|-------------|--------|----------|
| FIN-01 | Consolidated invoice per visit with charges from all departments | SATISFIED | Invoice aggregate; AddLineItem(department); department grouping in UI, PDF, GetPendingAsync. Row navigation (plan 32) and real-time refresh (plan 32) complete the cashier workflow. |
| FIN-02 | Revenue allocation per department per line item | SATISFIED | InvoiceLineItem.Department enum; DEPARTMENT_MAP in frontend; PDF department sections. Prescription guard (plan 33) prevents fraudulent removal of department-billed items. |
| FIN-03 | Payment methods: cash, bank transfer, QR, card | SATISFIED | PaymentMethod enum (7 values); RecordPayment handler concurrency-safe; PaymentMethodSelector. Prescription guard preserves invoice integrity before payment collection. |
| FIN-04 | E-invoice per Vietnamese tax law | SATISFIED | EInvoiceExportDto (all Decree 123/2020 fields); EInvoiceDocument.cs; JSON/XML export; print endpoints. Prescription removal event chain (plan 33) ensures invoice totals are accurate when e-invoice is generated. |
| FIN-05 | Treatment package 50/50 split payment | SATISFIED | Payment.TreatmentPackageId, IsSplitPayment, SplitSequence; RecordPayment handler; PaymentForm split fields |
| FIN-06 | 50/50 split enforcement | PARTIAL — BY DESIGN | Data recorded in Phase 7; enforcement deferred to Phase 9 (Treatment module) — documented architectural boundary |
| FIN-07 | Manual discounts require manager approval | SATISFIED | Discount.ApprovalStatus (Pending default); ApproveDiscount with VerifyManagerPinQuery; ICurrentUser auto-populates requestedById |
| FIN-08 | Refund processing requires manager/owner approval with audit trail | SATISFIED | Refund Requested-Approved-Processed lifecycle; ApproveRefund with PIN; all entities IAuditable |
| FIN-09 | Price change audit log | SATISFIED | Invoice, Payment, Discount, Refund, CashierShift all implement IAuditable; AuditInterceptor captures all changes |
| FIN-10 | Shift management with cash reconciliation | SATISFIED | CashierShift aggregate Open/Lock/Close lifecycle; GetPendingInvoices for dashboard; ShiftReport; ShiftCloseDialog live discrepancy |

---

## Build & Test Results

- **Billing unit tests:** `dotnet test tests/Billing.Unit.Tests/` — **103/103 PASSED** (8 new tests for prescription guard, 10 total new from plans 32+33)
- **Clinical unit tests:** `dotnet test tests/Clinical.Unit.Tests/` — **177/177 PASSED** (2 new tests for DrugPrescriptionRemovedEvent)
- **Targeted filter tests (plan 33):** `dotnet test --filter "RemoveLineItem|DrugPrescriptionRemoved|RemoveLineItemsBySource"` — **8/8 PASSED** (Billing)
- **Targeted filter tests (plan 33 Clinical):** `dotnet test --filter "DrugPrescriptionRemoved"` — **2/2 PASSED** (Clinical)
- **Frontend TypeScript (billing files):** 3 pre-existing errors in billing files (billing-api.ts:429, RefundDialog.tsx:177/183, use-billing-hub.ts:14); verified pre-existing from commit `e162107` before plan 32/33 changes — NOT regressions from gap closure work. The InvoiceHistoryPage.tsx, InvoiceView.tsx, and InvoiceLineItemsTable.tsx changes introduced zero new TypeScript errors.

---

## Anti-Patterns Checked

No new anti-patterns introduced in plans 07-32 or 07-33.

Items reviewed:
- `InvoiceHistoryPage.tsx`: No stub patterns; full data-driven table with tabs, search, pagination, and row navigation
- `InvoiceView.tsx`: `useBillingHub()` call is substantive (not a comment or TODO); hook has full SignalR connection logic
- `HandleDrugPrescriptionRemoved.cs`: Handles no-invoice and no-matching-items cases gracefully (idempotent) — not a stub, this is the correct design
- `RemoveLineItemsBySource()`: Only calls `RecalculateTotals()` and `SetUpdatedAt()` when matching items were actually found — correct optimization
- Pre-existing TypeScript `ImportMeta.env` errors exist across multiple modules (admin, audit, billing) — project-wide pre-existing issue unrelated to Phase 7 gap closure

---

## Human Verification Required

### 1. Invoice History Row Click Navigation (Previously UAT Retest Test 1)

**Test:** Navigate to /billing/invoices page. Click anywhere on an invoice row (not just the invoice number link).
**Expected:** Browser navigates to /billing/invoices/{id} showing the full invoice detail view.
**Why human:** Row onClick with useNavigate requires browser routing interaction to verify.

### 2. Invoice Detail Page Auto-Refresh on Prescription (Previously UAT Retest Test 2)

**Test:** Open a draft invoice detail page. In a separate session, doctor adds a drug prescription to the same visit.
**Expected:** New pharmacy line item appears on the cashier's invoice detail page within seconds — no manual refresh.
**Why human:** SignalR real-time behavior requires two concurrent browser sessions with running backend.

### 3. Prescription Lock Icon and No Delete Button

**Test:** Open a draft invoice that has prescription-linked line items (pharmacy department). Check the action column.
**Expected:** Prescription items show a lock icon (no delete button). Manually-added items still show the delete button with confirmation dialog.
**Why human:** UI conditional rendering requires visual browser inspection.

### 4. Doctor Removes Prescription; Invoice Auto-Updates (Previously UAT Retest Test 4)

**Test:** Open a draft invoice with prescription line items as cashier. In another session, doctor removes a drug prescription from the visit.
**Expected:** The pharmacy line items disappear from the cashier's invoice in real-time (via SignalR LineItemRemoved event). Invoice totals recalculate.
**Why human:** Cross-module event chain with SignalR notification requires two concurrent sessions.

### 5. Add Line Item to Invoice (Regression Check)

**Test:** On a draft invoice, add a manually-entered line item.
**Expected:** Line item appears, totals update. Delete button appears for this item (not a lock icon).
**Why human:** Confirms prescription guard did not break manual line item workflow.

### 6. Payment Collection Flow

**Test:** Open invoice with balance due, click "Collect Payment", select Cash, enter balance amount, submit.
**Expected:** Dialog closes, invoice Paid Amount updates, Balance Due becomes 0.
**Why human:** Dialog state management and real-time balance update requires browser interaction.

### 7. Manager PIN Approval for Discount

**Test:** Apply discount to draft invoice (no RequestedById needed). Enter manager PIN in approval dialog.
**Expected:** Correct PIN approves discount; wrong PIN shows error.
**Why human:** Cross-module PIN verification requires running backend with authenticated session.

### 8. E-Invoice PDF Visual Compliance

**Test:** Finalize an invoice, click "Export E-Invoice" > "PDF".
**Expected:** PDF contains all Decree 123/2020 fields: invoice template symbol, seller tax code, buyer name, pre-tax total, tax rate (8%), tax amount, total including tax, amount in Vietnamese words, signature areas.
**Why human:** PDF visual layout requires opening the generated file.

### 9. Shift Report Cash Reconciliation Display

**Test:** Close a shift with ActualCashCount differing from ExpectedCashAmount. View shift report.
**Expected:** Discrepancy shows in red (deficit) or green (surplus).
**Why human:** CSS color coding requires browser rendering.

---

## Gap Closure Confirmation

All 4 UAT retest gaps from `07-UAT-retest.md` are closed:

| UAT Retest Gap | Test # | Root Cause | Fix Applied | Verified |
|----------------|--------|------------|-------------|---------|
| Invoice history row not clickable | 1 | Missing onClick + useNavigate on TableRow | Added in InvoiceHistoryPage.tsx (Plan 32) | onClick present at line 122; useNavigate at line 40 |
| No real-time refresh on prescription added | 2 | InvoiceView not calling useBillingHub | Added useBillingHub() call in InvoiceView.tsx (Plan 32) | Hook called at line 77; LineItemAdded handler invalidates query |
| Cashier could delete prescription line items | 3 | No domain guard; no frontend UI guard | Backend guard in Invoice.RemoveLineItem() + frontend lock icon (Plan 33) | Guard at Invoice.cs:140-142; lock icon in InvoiceLineItemsTable.tsx:163-164 |
| Removing prescription left billing items stale | 4 | Visit.RemoveDrugPrescription raised no domain event | Full DrugPrescriptionRemoved event chain created (Plan 33) | Visit.cs:238; 4 new files + 3 modified files; 10 new passing tests |

---

## Overall Assessment

Phase 7 (Billing & Finance) fully achieves its stated goal after three rounds of gap closure. The phase now has:

**Newly verified since second verification:**
- Invoice history is fully navigable (row click routing)
- Invoice detail page has real-time SignalR updates via useBillingHub
- Prescription integrity is protected end-to-end: domain guard rejects cashier removal; frontend hides delete button; doctor-initiated removal propagates automatically via event chain
- 10 new unit tests pass (103 total billing, 177 total clinical)

**Cumulative state:**
- 103/103 billing unit tests pass
- 177/177 clinical unit tests pass
- Complete billing domain model with full lifecycle methods and guards
- Real-time billing hub with InvoiceCreated, LineItemAdded, LineItemRemoved, InvoiceVoided events
- Full event chain integration between Clinical and Billing modules (add and remove)
- All FIN-01 through FIN-10 requirements satisfied (FIN-06 partial-by-design deferred to Phase 9)

**Remaining human verification items:** 9 test scenarios requiring a running server (visual/integration flows covering routing, SignalR real-time behavior, PDF rendering, and UI conditional display).

---

_Verified: 2026-03-17T16:30:00Z_
_Verifier: Claude (gsd-verifier)_
_Re-verification after UAT retest gap closure (plans 07-32, 07-33)_
