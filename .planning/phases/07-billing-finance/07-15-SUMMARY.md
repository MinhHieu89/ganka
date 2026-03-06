---
phase: 07-billing-finance
plan: 15
subsystem: documents
tags: [questpdf, pdf, invoice, e-invoice, shift-report, vietnamese, vnd]

# Dependency graph
requires:
  - phase: 05-clinical
    provides: "QuestPDF pattern with ClinicHeaderComponent and Noto Sans font"
  - phase: 07-billing-finance (plan 14)
    provides: "Billing domain enums (Department, PaymentMethod, InvoiceStatus)"
provides:
  - "InvoiceDocument: A4 QuestPDF invoice with department grouping and VND formatting"
  - "ReceiptDocument: A5 compact payment receipt"
  - "EInvoiceDocument: Vietnamese e-invoice per Decree 123/2020 with tax breakdown"
  - "ShiftReportDocument: Shift report with revenue by method and cash reconciliation"
  - "VndFormatter: VND formatting utilities and number-to-words converter"
  - "BillingDocumentDataRecords: data transfer records for all billing documents"
affects: [07-billing-finance]

# Tech tracking
tech-stack:
  added: [QuestPDF (added to Billing.Infrastructure)]
  patterns: [QuestPDF IDocument pattern for billing, VND amount-to-words conversion, department-grouped invoice layout]

key-files:
  created:
    - backend/src/Modules/Billing/Billing.Infrastructure/Documents/InvoiceDocument.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Documents/ReceiptDocument.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Documents/EInvoiceDocument.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Documents/ShiftReportDocument.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Documents/Shared/ClinicHeaderComponent.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Documents/Shared/DocumentFontManager.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Documents/Shared/BillingDocumentDataRecords.cs
    - backend/src/Modules/Billing/Billing.Infrastructure/Documents/Shared/VndFormatter.cs
  modified:
    - backend/src/Modules/Billing/Billing.Infrastructure/Billing.Infrastructure.csproj

key-decisions:
  - "Duplicated ClinicHeaderComponent in Billing.Infrastructure instead of cross-module reference to maintain modular monolith boundary"
  - "Added TaxCode field to billing ClinicHeaderData for e-invoice MST display"
  - "VND-to-words converter handles up to billions (ty) with proper Vietnamese numeral rules"

patterns-established:
  - "Billing QuestPDF documents follow same IDocument pattern as Clinical module"
  - "VndFormatter centralized utility for VND formatting across all billing documents"
  - "Department grouping in invoice using GroupBy on Department enum"

requirements-completed: [FIN-01, FIN-04]

# Metrics
duration: 6min
completed: 2026-03-06
---

# Phase 07 Plan 15: Billing QuestPDF Documents Summary

**4 QuestPDF documents for invoice, receipt, e-invoice (Decree 123/2020 compliant), and shift report with department grouping, VND formatting, and Vietnamese number-to-words**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-06T13:58:35Z
- **Completed:** 2026-03-06T14:04:27Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- InvoiceDocument with department-grouped line items (Kham benh, Duoc pham, Kinh, Dieu tri), VND dot-thousands formatting, payment summary, and cashier signature
- EInvoiceDocument with all Decree 123/2020 mandatory fields: seller/buyer tax codes, invoice template/symbol, tax breakdown (pre-tax, GTGT rate, tax amount), amount in Vietnamese words
- ShiftReportDocument with revenue-by-payment-method table and cash reconciliation with discrepancy highlighting
- ReceiptDocument as compact A5 payment confirmation with payment method listing
- VndFormatter utility with dot-separator formatting and full VND-to-words converter (handles up to billions)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Invoice and Receipt QuestPDF documents** - `6d346a6` (feat)
2. **Task 2: Create E-Invoice and Shift Report QuestPDF documents** - `4eac6b8` (feat)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Infrastructure/Billing.Infrastructure.csproj` - Added QuestPDF package reference and embedded font resources
- `backend/src/Modules/Billing/Billing.Infrastructure/Documents/InvoiceDocument.cs` - A4 invoice with department-grouped line items and VND formatting
- `backend/src/Modules/Billing/Billing.Infrastructure/Documents/ReceiptDocument.cs` - A5 compact payment receipt
- `backend/src/Modules/Billing/Billing.Infrastructure/Documents/EInvoiceDocument.cs` - Vietnamese e-invoice per Decree 123/2020 with all mandatory fields
- `backend/src/Modules/Billing/Billing.Infrastructure/Documents/ShiftReportDocument.cs` - Shift report with revenue breakdown and cash reconciliation
- `backend/src/Modules/Billing/Billing.Infrastructure/Documents/Shared/ClinicHeaderComponent.cs` - Reusable clinic header for billing documents
- `backend/src/Modules/Billing/Billing.Infrastructure/Documents/Shared/DocumentFontManager.cs` - Noto Sans Vietnamese font registration
- `backend/src/Modules/Billing/Billing.Infrastructure/Documents/Shared/BillingDocumentDataRecords.cs` - Data transfer records for all 4 document types
- `backend/src/Modules/Billing/Billing.Infrastructure/Documents/Shared/VndFormatter.cs` - VND formatting and number-to-words conversion

## Decisions Made
- **Duplicated shared components in Billing.Infrastructure** instead of creating cross-module reference to Clinical.Infrastructure. The plan referenced ClinicHeaderComponent from Shared.Infrastructure, but it actually lives in Clinical.Infrastructure. Creating a Billing-specific copy maintains modular monolith boundaries and avoids circular/inappropriate dependencies between infrastructure layers.
- **Added TaxCode field** to billing ClinicHeaderData (beyond the Clinical module version) for MST (Ma so thue) display on e-invoices.
- **VND-to-words converter** handles Vietnamese numeral rules including "le" for units after hundreds, "mot/lam" alternation in tens position, and ranges up to billions (ty).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] ClinicHeaderComponent not in Shared.Infrastructure**
- **Found during:** Task 1 (Create Invoice and Receipt documents)
- **Issue:** Plan instructed "Add project reference to Shared.Infrastructure for ClinicHeaderComponent and FontManager access" but these classes are in Clinical.Infrastructure, not Shared.Infrastructure
- **Fix:** Created Billing-specific copies of ClinicHeaderComponent, ClinicHeaderData, and DocumentFontManager in Billing.Infrastructure/Documents/Shared/ to maintain modular monolith boundaries
- **Files modified:** Created new files under Documents/Shared/
- **Verification:** Build succeeds with 0 warnings, 0 errors
- **Committed in:** 6d346a6 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Necessary to avoid cross-module infrastructure dependency. No scope creep.

## Issues Encountered
None - both tasks compiled cleanly on first build.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 4 billing document types ready for integration with invoice/payment/shift endpoints
- VndFormatter utility available for any future VND display needs
- Font files embedded and DocumentFontManager ready for registration at startup

## Self-Check: PASSED

All 9 files verified present. Both task commits (6d346a6, 4eac6b8) verified in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*
