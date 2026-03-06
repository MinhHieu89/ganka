---
phase: 07-billing-finance
verified: 2026-03-06T14:45:00Z
status: passed
score: 10/10 must-haves verified
re_verification: null
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

**Phase Goal:** Billing & Finance — Complete billing module with invoice management, payment processing (multi-method), discounts, refunds, cashier shift management, e-invoice export, and PDF document generation.
**Verified:** 2026-03-06T14:45:00Z
**Status:** PASSED
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | System generates consolidated invoice per visit with charges from all departments | VERIFIED | `Invoice.cs` AggregateRoot with `AddLineItem(Department department)`, `InvoiceLineItem.Department` field; `InvoiceLineItemsTable.tsx` groups by department |
| 2 | Internal revenue allocation tracked per department on each line item | VERIFIED | `InvoiceLineItem.cs` has `Department` (enum: Medical=0, Pharmacy=1, Optical=2, Treatment=3); `AddInvoiceLineItem.cs` handler maps department |
| 3 | Payment supports cash, bank transfer, QR (VNPay/MoMo/ZaloPay), card (Visa/MC) | VERIFIED | `PaymentMethod.cs` enum has all 7 values (0-6); `RecordPaymentCommandValidator` validates methods 0-6; `PaymentMethodSelector.tsx` renders all 7 options |
| 4 | E-invoice generated per Vietnamese tax law (Decree 123/2020) | VERIFIED | `EInvoiceExportDto.cs` contains all required fields (InvoiceTemplateSymbol, InvoiceSymbol, SellerTaxCode, TaxRate, TaxAmount, PreTaxTotal, Currency); `EInvoiceDocument.cs` composes ComposeSellerBuyerInfo, ComposeLineItemsTable, ComposeTaxSection, ComposeAmountInWords, ComposeSignatures |
| 5 | Treatment package 50/50 split payment tracked | VERIFIED | `Payment.cs` has `TreatmentPackageId`, `IsSplitPayment`, `SplitSequence`; `RecordPayment.cs` passes all 3 fields; `PaymentForm.tsx` has split payment conditional fields |
| 6 | Manual discounts require manager approval | VERIFIED | `Discount.cs` with `ApprovalStatus` starting Pending; `ApproveDiscount.cs` handler calls `VerifyManagerPinQuery` cross-module to Auth; `DiscountDialog.tsx` opens `ApprovalPinDialog` after discount applied |
| 7 | Refund processing requires manager/owner approval with full audit trail | VERIFIED | `Refund.cs` implements IAuditable with Requested→Approved→Processed lifecycle; `ApproveRefund.cs` and `ProcessRefund.cs` handlers; `RefundDialog.tsx` with `ApprovalPinDialog` |
| 8 | Price change audit log maintained | VERIFIED | All billing entities (`Invoice`, `Payment`, `Discount`, `Refund`, `CashierShift`) implement `IAuditable` — AuditInterceptor captures all changes automatically |
| 9 | Shift management: define shifts, assign staff, track revenue, cash reconciliation | VERIFIED | `CashierShift.cs` aggregate with Open→Locked→Closed lifecycle; `ExpectedCashAmount` computed property; `Discrepancy` on close; `GetShiftReport.cs` handler groups revenue by method; `ShiftCloseDialog.tsx` shows live discrepancy |
| 10 | PDF documents generated (invoice, receipt, e-invoice, shift report) | VERIFIED | `InvoiceDocument.cs`, `ReceiptDocument.cs`, `EInvoiceDocument.cs`, `ShiftReportDocument.cs` all implement `IDocument`; `BillingDocumentService.cs` implements all 4 `IBillingDocumentService` methods; print endpoints in `BillingApiEndpoints.cs` |

**Score:** 10/10 truths verified

---

## Required Artifacts

### Domain (Plan 01, 02, 03)

| Artifact | Status | Details |
|----------|--------|---------|
| `Billing.Domain/Entities/Invoice.cs` | VERIFIED | `AggregateRoot, IAuditable`; backing fields `_lineItems`, `_payments`, `_discounts`, `_refunds`; all 7 domain methods present (AddLineItem, RemoveLineItem, RecordPayment, ApplyDiscount, Finalize, Void, RecalculateAfterDiscountApproval) |
| `Billing.Domain/Entities/InvoiceLineItem.cs` | VERIFIED | Entity with Department field for revenue allocation |
| `Billing.Domain/Enums/InvoiceStatus.cs` | VERIFIED | Draft=0, Finalized=1, Voided=2 |
| `Billing.Domain/Enums/Department.cs` | VERIFIED | Medical=0, Pharmacy=1, Optical=2, Treatment=3 |
| `Billing.Domain/Events/InvoiceFinalizedEvent.cs` | VERIFIED | Sealed record domain event |
| `Billing.Domain/Entities/Payment.cs` | VERIFIED | `Entity, IAuditable`; all 7 payment methods; TreatmentPackageId, IsSplitPayment, SplitSequence |
| `Billing.Domain/Entities/Discount.cs` | VERIFIED | `Entity, IAuditable`; Pending→Approved/Rejected workflow; Approve/Reject methods |
| `Billing.Domain/Entities/Refund.cs` | VERIFIED | `Entity, IAuditable`; Requested→Approved→Processed lifecycle |
| `Billing.Domain/Enums/PaymentMethod.cs` | VERIFIED | 7 payment methods + DiscountType enum |
| `Billing.Domain/Enums/PaymentStatus.cs` | VERIFIED | PaymentStatus, ApprovalStatus, RefundStatus enums |
| `Billing.Domain/Entities/CashierShift.cs` | VERIFIED | `AggregateRoot, IAuditable`; Open→Locked→Closed; cash reconciliation |
| `Billing.Domain/Entities/ShiftTemplate.cs` | VERIFIED | Entity with name, times, IsActive |
| `Billing.Domain/Enums/ShiftStatus.cs` | VERIFIED | Open=0, Locked=1, Closed=2 |
| `Billing.Domain/Events/PaymentReceivedEvent.cs` | VERIFIED | Sealed record domain event |

### Infrastructure (Plans 04, 05)

| Artifact | Status | Details |
|----------|--------|---------|
| `Billing.Infrastructure/Configurations/InvoiceConfiguration.cs` | VERIFIED | `PropertyAccessMode.Field` for all 4 navigations; `HasPrecision(18, 0)` VND; unique index on InvoiceNumber; cascade/restrict deletes |
| `Billing.Infrastructure/Configurations/InvoiceLineItemConfiguration.cs` | VERIFIED | VND precision, Department stored as int |
| `Billing.Infrastructure/Configurations/PaymentConfiguration.cs` | VERIFIED | VND precision, indexes on InvoiceId/CashierShiftId |
| `Billing.Infrastructure/Configurations/DiscountConfiguration.cs` | VERIFIED | Percentage uses `(18, 2)`, calculated amount `(18, 0)` |
| `Billing.Infrastructure/Configurations/RefundConfiguration.cs` | VERIFIED | VND precision |
| `Billing.Infrastructure/Configurations/CashierShiftConfiguration.cs` | VERIFIED | Filtered unique index `(BranchId, Status) WHERE Status = 0` preventing duplicate open shifts |
| `Billing.Infrastructure/Configurations/ShiftTemplateConfiguration.cs` | VERIFIED | Tables, maxlengths |
| `Billing.Infrastructure/BillingDbContext.cs` | VERIFIED | All 7 DbSets; `ApplyConfigurationsFromAssembly`; `HasDefaultSchema("billing")` |
| `Billing.Infrastructure/Seeding/ShiftTemplateSeeder.cs` | VERIFIED | IHostedService seeder for Morning/Afternoon templates |

### Contracts (Plan 06)

| Artifact | Status | Details |
|----------|--------|---------|
| `Billing.Contracts/Dtos/InvoiceDto.cs` | VERIFIED | InvoiceDto, InvoiceLineItemDto, DiscountDto, RefundDto, InvoiceSummaryDto — all with int enums |
| `Billing.Contracts/Dtos/PaymentDto.cs` | VERIFIED | All payment fields including split payment |
| `Billing.Contracts/Dtos/CashierShiftDto.cs` | VERIFIED | CashierShiftDto, ShiftReportDto, ShiftTemplateDto |
| `Billing.Contracts/Dtos/EInvoiceExportDto.cs` | VERIFIED | All Decree 123/2020 required fields present |
| `Billing.Contracts/Queries/GetVisitInvoiceQuery.cs` | VERIFIED | Cross-module queries defined |

### Repositories (Plans 07, 08)

| Artifact | Status | Details |
|----------|--------|---------|
| `Billing.Application/Interfaces/IInvoiceRepository.cs` | VERIFIED | GetByVisitIdAsync, GetNextInvoiceNumberAsync (HD-YYYY-NNNNN), eager load methods |
| `Billing.Application/Interfaces/IPaymentRepository.cs` | VERIFIED | GetByShiftIdAsync for reconciliation |
| `Billing.Application/Interfaces/ICashierShiftRepository.cs` | VERIFIED | GetCurrentOpenAsync by branch |
| `Billing.Infrastructure/Repositories/InvoiceRepository.cs` | VERIFIED | Include/ThenInclude eager loading; HD-YYYY-NNNNN generation |
| `Billing.Infrastructure/Repositories/PaymentRepository.cs` | VERIFIED | Standard query methods |
| `Billing.Infrastructure/Repositories/CashierShiftRepository.cs` | VERIFIED | GetCurrentOpenAsync filters by BranchId AND Status=Open |
| `Billing.Infrastructure/UnitOfWork.cs` | VERIFIED | Wraps BillingDbContext SaveChanges |
| `Billing.Application/Interfaces/IBillingDocumentService.cs` | VERIFIED | 6 methods for PDF/JSON/XML generation |
| `Billing.Infrastructure/IoC.cs` | VERIFIED | All 3 repos, UnitOfWork, BillingDocumentService, ShiftTemplateSeeder registered |
| `Billing.Application/IoC.cs` | VERIFIED | FluentValidation registered from assembly |

### Application Handlers (Plans 09-12, 25-26)

| Artifact | Status | Details |
|----------|--------|---------|
| `Features/CreateInvoice.cs` | VERIFIED | Command, Validator, static Handler; GetNextInvoiceNumberAsync; Invoice.Create |
| `Features/AddInvoiceLineItem.cs` | VERIFIED | Maps department; calls AddLineItem domain method |
| `Features/FinalizeInvoice.cs` | VERIFIED | Validates payment; calls Finalize domain method |
| `Features/RemoveInvoiceLineItem.cs` | VERIFIED | Calls RemoveLineItem domain method |
| `Features/GetInvoiceById.cs` | VERIFIED | Full eager-loaded DTO mapping |
| `Features/GetInvoicesByVisit.cs` | VERIFIED | Summary list by VisitId |
| `Features/RecordPayment.cs` | VERIFIED | All 7 payment methods; IsSplitPayment/SplitSequence; shift integration |
| `Features/GetPaymentsByInvoice.cs` | VERIFIED | Query handler |
| `Features/ApplyDiscount.cs` | VERIFIED | Percentage/fixed; pending status; CalculatedAmount |
| `Features/ApproveDiscount.cs` | VERIFIED | VerifyManagerPinQuery cross-module; RecalculateAfterDiscountApproval |
| `Features/RejectDiscount.cs` | VERIFIED | Manager PIN; rejection reason |
| `Features/RequestRefund.cs` | VERIFIED | Finalized invoice required |
| `Features/ApproveRefund.cs` | VERIFIED | Manager PIN; validates Requested status |
| `Features/ProcessRefund.cs` | VERIFIED | shift.AddCashRefund for cash refunds |
| `Features/OpenShift.cs` | VERIFIED | Duplicate prevention via GetCurrentOpenAsync |
| `Features/CloseShift.cs` | VERIFIED | LockForClose then Close with cash count |
| `Features/GetCurrentShift.cs` | VERIFIED | Returns current open shift |
| `Features/GetShiftReport.cs` | VERIFIED | Revenue grouped by PaymentMethod |
| `Features/GetShiftTemplates.cs` | VERIFIED | Queries ICashierShiftRepository |

### TDD Tests (Plans 09-12)

| Test File | Status | Details |
|-----------|--------|---------|
| `Billing.Unit.Tests/Features/InvoiceCrudHandlerTests.cs` | VERIFIED | 6 tests; NSubstitute mocks |
| `Billing.Unit.Tests/Features/PaymentHandlerTests.cs` | VERIFIED | 7 tests; all payment method coverage |
| `Billing.Unit.Tests/Features/DiscountHandlerTests.cs` | VERIFIED | 6 tests including PIN approval |
| `Billing.Unit.Tests/Features/RefundHandlerTests.cs` | VERIFIED | 3 tests; lifecycle validation |
| `Billing.Unit.Tests/Features/ShiftHandlerTests.cs` | VERIFIED | 7 tests; duplicate shift prevention, reconciliation |
| **Total test results** | PASSED | 45/45 tests pass (verified by `dotnet test`) |

### Documents & Services (Plans 15, 16)

| Artifact | Status | Details |
|----------|--------|---------|
| `Billing.Infrastructure/Documents/InvoiceDocument.cs` | VERIFIED | `IDocument`; A4; department-grouped line items; ClinicHeaderComponent reuse |
| `Billing.Infrastructure/Documents/ReceiptDocument.cs` | VERIFIED | `IDocument`; compact format |
| `Billing.Infrastructure/Documents/EInvoiceDocument.cs` | VERIFIED | `IDocument`; ComposeSellerBuyerInfo, ComposeTaxSection, ComposeAmountInWords, ComposeSignatures |
| `Billing.Infrastructure/Documents/ShiftReportDocument.cs` | VERIFIED | `IDocument`; cash reconciliation section |
| `Billing.Infrastructure/Services/BillingDocumentService.cs` | VERIFIED | Implements IBillingDocumentService; all 4 PDF methods + ExportEInvoiceJsonAsync + ExportEInvoiceXmlAsync |
| `Billing.Infrastructure/Services/EInvoiceExportService.cs` | VERIFIED | Static service; `ExportToJson` (Vietnamese Unicode unescaped) and `ExportToXml` (XDocument pattern) |

### Presentation (Plans 13, 14, 16)

| Artifact | Status | Details |
|----------|--------|---------|
| `Billing.Presentation/BillingApiEndpoints.cs` | VERIFIED | MapBillingApiEndpoints; 7 groups: invoices, payments, discounts, refunds, shifts, print, export; bus.InvokeAsync pattern; RequireAuthorization |
| `Billing.Presentation/IoC.cs` | VERIFIED | AddBillingPresentation extension |
| `Bootstrapper/Program.cs` | VERIFIED | `AddBillingApplication`, `AddBillingInfrastructure`, `MapBillingApiEndpoints` all present (lines 88-89, 314) |
| `Billing.Infrastructure/Migrations/InitialBilling.cs` | VERIFIED | Migration created for all 7 billing tables |

### Frontend (Plans 17-22)

| Artifact | Status | Details |
|----------|--------|---------|
| `features/billing/api/billing-api.ts` | VERIFIED | billingKeys factory; all API functions; all TanStack Query hooks with invalidation; PAYMENT_METHOD_MAP, DEPARTMENT_MAP |
| `features/billing/api/shift-api.ts` | VERIFIED | shiftKeys; openShift, closeShift, getCurrentShift, getShiftReport, getShiftTemplates hooks |
| `shared/lib/format-vnd.ts` | VERIFIED | `formatVND` and `formatVNDCompact` using `Intl.NumberFormat("vi-VN", { currency: "VND" })` |
| `features/billing/components/InvoiceLineItemsTable.tsx` | VERIFIED | Groups by department; KHAM BENH/DUOC PHAM/KINH/DIEU TRI headers; shadcn/ui Table |
| `features/billing/components/InvoiceView.tsx` | VERIFIED | useInvoice hook; status badge; pay/print/finalize/discount/refund/e-invoice buttons; all dialog integrations |
| `features/billing/components/PaymentForm.tsx` | VERIFIED | useRecordPayment mutation; method-specific fields; split payment fields |
| `features/billing/components/PaymentMethodSelector.tsx` | VERIFIED | All 7 methods with icons |
| `features/billing/components/DiscountDialog.tsx` | VERIFIED | Percentage/fixed toggle; live preview; ApprovalPinDialog flow |
| `features/billing/components/ApprovalPinDialog.tsx` | VERIFIED | Reusable manager PIN dialog |
| `features/billing/components/RefundDialog.tsx` | VERIFIED | Full/partial refund; ApprovalPinDialog flow |
| `features/billing/components/EInvoiceExportButton.tsx` | VERIFIED | DropdownMenu with PDF/JSON/XML options |
| `features/billing/components/ShiftOpenDialog.tsx` | VERIFIED | Template selector; opening balance; useOpenShift mutation |
| `features/billing/components/ShiftCloseDialog.tsx` | VERIFIED | expectedCash display; live discrepancy calculation (green/red) |
| `features/billing/components/ShiftReportView.tsx` | VERIFIED | Revenue by method table; cash reconciliation card; print button |
| `routes/_authenticated/billing/index.tsx` | VERIFIED | `createFileRoute("/_authenticated/billing/")`; BillingDashboard; usePendingInvoices + useCurrentShift |
| `routes/_authenticated/billing/invoices.$invoiceId.tsx` | VERIFIED | Invoice detail route with InvoiceView |
| `routes/_authenticated/billing/shifts.tsx` | VERIFIED | ShiftsPage; history table; ShiftOpenDialog/ShiftCloseDialog |
| `shared/i18n/locales/en/billing.json` | VERIFIED | 50+ keys including all payment methods, departments, status labels |
| `shared/i18n/locales/vi/billing.json` | VERIFIED | Full Vietnamese translations with proper diacritics |
| `shared/i18n/i18n.ts` | VERIFIED | "billing" namespace registered in ns array |
| `shared/components/AppSidebar.tsx` | VERIFIED | Billing nav section with Billing Dashboard (/billing) and Shifts (/billing/shifts) |

---

## Key Link Verification

| From | To | Via | Status |
|------|----|-----|--------|
| `Invoice.cs` | `InvoiceLineItem.cs` | `private readonly List<InvoiceLineItem> _lineItems = []` | WIRED |
| `Invoice.cs` | `Payment.cs` | `private readonly List<Payment> _payments = []` | WIRED |
| `Invoice.cs` | `Discount.cs` | `private readonly List<Discount> _discounts = []` | WIRED |
| `InvoiceConfiguration.cs` | `Invoice.cs` | `PropertyAccessMode.Field` on all 4 navigations | WIRED |
| `CashierShiftConfiguration.cs` | filtered unique index | `HasIndex(BranchId, Status).HasFilter("Status = 0").IsUnique()` | WIRED |
| `InvoiceRepository.cs` | `BillingDbContext.cs` | constructor injection of BillingDbContext | WIRED |
| `CreateInvoice.cs` | `IInvoiceRepository` | `GetNextInvoiceNumberAsync` + `Add` + `SaveChangesAsync` | WIRED |
| `RecordPayment.cs` | `Invoice.cs` | `invoice.RecordPayment(payment)` domain method call | WIRED |
| `ApproveDiscount.cs` | Auth module | `messageBus.InvokeAsync<VerifyManagerPinResponse>(VerifyManagerPinQuery)` | WIRED |
| `ProcessRefund.cs` | `CashierShift.cs` | `shift.AddCashRefund(refund.Amount)` when RefundMethod=Cash | WIRED |
| `CloseShift.cs` | `CashierShift.cs` | `shift.LockForClose()` then `shift.Close(actualCashCount, managerNote)` | WIRED |
| `BillingDocumentService.cs` | `Documents/*.cs` | `new InvoiceDocument(data, header).GeneratePdf()` | WIRED |
| `EInvoiceExportService` | `BillingDocumentService` | `EInvoiceExportService.ExportToJson(eInvoiceData)` static call | WIRED |
| `Bootstrapper/Program.cs` | `Billing.Infrastructure/IoC.cs` | `services.AddBillingInfrastructure()` at line 89 | WIRED |
| `billing/index.tsx` | `billing-api.ts` | `usePendingInvoices()` + `useCurrentShift()` hooks | WIRED |
| `InvoiceView.tsx` | `billing-api.ts` | `useInvoice`, `useFinalizeInvoice`, `PAYMENT_METHOD_MAP` | WIRED |
| `AppSidebar.tsx` | `billing/index.tsx` | nav link `to: "/billing"` | WIRED |

---

## Requirements Coverage

| Requirement | Description | Status | Evidence |
|-------------|-------------|--------|----------|
| FIN-01 | Consolidated invoice per visit with charges from all departments | SATISFIED | Invoice aggregate, AddLineItem(department), department grouping in UI and PDF |
| FIN-02 | Revenue allocation per department per line item | SATISFIED | `InvoiceLineItem.Department` enum, `DEPARTMENT_MAP` in frontend, PDF department sections |
| FIN-03 | Payment methods: cash, bank transfer, QR, card | SATISFIED | PaymentMethod enum (7 values), RecordPayment validator, PaymentMethodSelector (7 UI options) |
| FIN-04 | E-invoice per Vietnamese tax law | SATISFIED | EInvoiceExportDto (all Decree 123/2020 fields), EInvoiceDocument.cs, JSON/XML export, print endpoints |
| FIN-05 | Treatment package 50/50 split payment | SATISFIED | Payment.TreatmentPackageId, IsSplitPayment, SplitSequence; RecordPayment handler; PaymentForm split fields |
| FIN-06 | 50/50 split enforcement | PARTIAL — BY DESIGN | Split payment DATA recorded in Phase 7; enforcement of "2nd payment before mid-course session" deferred to Phase 9 (Treatment module) per plan 10 design decision |
| FIN-07 | Manual discounts require manager approval | SATISFIED | Discount.ApprovalStatus (Pending default), ApproveDiscount with VerifyManagerPinQuery, DiscountDialog+ApprovalPinDialog UI |
| FIN-08 | Refund processing requires manager/owner approval with audit trail | SATISFIED | Refund Requested→Approved→Processed; ApproveRefund with PIN; all entities IAuditable |
| FIN-09 | Price change audit log | SATISFIED | Invoice, Payment, Discount, Refund, CashierShift all implement IAuditable; AuditInterceptor captures changes |
| FIN-10 | Shift management with cash reconciliation | SATISFIED | CashierShift aggregate; Open/Lock/Close lifecycle; CashierShiftConfiguration filtered unique index; ShiftReport; ShiftCloseDialog live discrepancy |

**Note on FIN-06:** The plans explicitly document this as a phased delivery. Phase 7 records all split payment data (IsSplitPayment, SplitSequence, TreatmentPackageId). The enforcement rule ("block mid-course session if 2nd payment not received") is deferred to Phase 9 (Treatment module) where session booking logic resides. This is a known architectural boundary, not a gap.

---

## Anti-Patterns Found

No blocker or warning anti-patterns found.

Items reviewed and determined safe:
- `return null` in `PaymentMethodSelector.tsx:137` — inside `getCardType()` utility function, valid null return for non-card methods
- `placeholder="Chon dich vu"` in DiscountDialog/RefundDialog — standard Select placeholder text, not a code stub
- Full solution build: 0 errors, 5 warnings (NU1608 package version warning pre-existing from other modules, 2 CS8602 in Patient module pre-existing — neither in billing module)

---

## Build & Test Results

- **Backend solution:** `dotnet build backend/Ganka28.slnx` — BUILD SUCCEEDED, 0 errors
- **Billing unit tests:** `dotnet test backend/tests/Billing.Unit.Tests` — 45/45 PASSED
- **Frontend TypeScript:** `npx tsc --noEmit` — 0 errors in any billing file (existing errors are in pre-phase-7 files: admin-api.ts, auth-api.ts, patient-api.ts)
- **Database migration:** `InitialBilling` migration created and present covering all 7 billing tables

---

## Human Verification Required

### 1. Payment Collection Flow

**Test:** Open an invoice, click "Collect Payment", select Cash, enter an amount, submit.
**Expected:** Dialog closes, invoice's Paid Amount updates, Balance Due decreases, payment appears in payments list.
**Why human:** Dialog state management, real-time balance update, optimistic query invalidation requires browser interaction.

### 2. Manager PIN Approval for Discount

**Test:** Apply a discount, enter a manager's PIN in the ApprovalPinDialog, confirm.
**Expected:** Correct PIN approves discount and updates invoice DiscountTotal; wrong PIN shows "Invalid PIN" error and allows retry.
**Why human:** Cross-module PIN verification (Auth module) requires running backend; behavior on incorrect PIN cannot be traced statically.

### 3. E-Invoice PDF Visual Compliance

**Test:** Finalize an invoice, click "Export E-Invoice" > "PDF".
**Expected:** Generated PDF contains: invoice template symbol, seller tax code, buyer name, pre-tax total, tax rate (8%), tax amount, total including tax, amount in Vietnamese words, signature areas for buyer and seller.
**Why human:** PDF visual layout and field presence requires opening generated file.

### 4. Shift Report Cash Reconciliation Display

**Test:** Close a shift with an ActualCashCount that differs from ExpectedCashAmount. View shift report.
**Expected:** Discrepancy shows in red when negative (deficit) or green when positive (surplus); manager note field visible when discrepancy is non-zero.
**Why human:** CSS color coding and conditional field visibility requires browser rendering.

### 5. VND Formatting in Browser

**Test:** View any invoice with amounts (e.g., 1,500,000 VND).
**Expected:** All monetary amounts display as "1.500.000 ₫" with Vietnamese dot thousands separator, no decimal places.
**Why human:** `Intl.NumberFormat("vi-VN")` output depends on browser locale rendering.

---

## Summary

Phase 7 (Billing & Finance) achieves its stated goal. All 10 FIN requirements have evidence of implementation:

**Backend:** Complete vertical-slice architecture from domain entities through infrastructure to API endpoints. Domain model is substantive (not stubs) — Invoice aggregate has 7 real domain methods, CashierShift has proper state machine lifecycle. EF Core configurations use PropertyAccessMode.Field and VND precision. 45 unit tests pass. Full solution builds without errors. Database migration created.

**Frontend:** Complete billing UI from cashier dashboard to invoice detail with payment collection, discount/refund dialogs, shift management, and e-invoice export. All components use real TanStack Query hooks (not stubs), proper VND formatting, and shadcn/ui components. i18n translations registered for both EN and VI.

**Integration wiring verified:** Bootstrapper registers all billing services; API endpoints dispatch via Wolverine bus; frontend API functions call actual backend routes; sidebar navigation links are enabled.

**FIN-06 partial delivery:** The 50/50 split payment enforcement rule is documented as deferred to Phase 9 by explicit design decision in Plan 10. The payment data infrastructure (IsSplitPayment, SplitSequence, TreatmentPackageId) is fully in place.

---

_Verified: 2026-03-06T14:45:00Z_
_Verifier: Claude (gsd-verifier)_
