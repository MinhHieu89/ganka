---
phase: 07-billing-finance
plan: 06
subsystem: api
tags: [dto, contracts, e-invoice, vietnamese-tax, billing, cross-module]

# Dependency graph
requires:
  - phase: 07-01
    provides: Billing.Contracts project scaffold
  - phase: 07-02
    provides: Domain entities defining enum types referenced as int in DTOs
  - phase: 07-03
    provides: Domain model structure informing DTO field shapes
provides:
  - InvoiceDto, InvoiceLineItemDto, DiscountDto, RefundDto, InvoiceSummaryDto
  - PaymentDto with split payment and card fields
  - CashierShiftDto, ShiftReportDto, ShiftTemplateDto
  - EInvoiceExportDto with all Vietnamese Decree 123/2020 required fields
  - Cross-module GetVisitInvoiceQuery, GetPendingInvoicesQuery
  - GetVisitChargesQuery and VisitChargeDto for charge collection from Clinical/Pharmacy/Optical
affects: [07-07, 07-08, 07-09, 07-10, 07-11, 07-12, 07-13, 07-14, 07-15]

# Tech tracking
tech-stack:
  added: []
  patterns: [sealed-record-dtos, int-enum-contracts, cross-module-query-records]

key-files:
  created:
    - backend/src/Modules/Billing/Billing.Contracts/Dtos/InvoiceDto.cs
    - backend/src/Modules/Billing/Billing.Contracts/Dtos/PaymentDto.cs
    - backend/src/Modules/Billing/Billing.Contracts/Dtos/CashierShiftDto.cs
    - backend/src/Modules/Billing/Billing.Contracts/Dtos/EInvoiceExportDto.cs
    - backend/src/Modules/Billing/Billing.Contracts/Queries/GetVisitInvoiceQuery.cs
  modified: []

key-decisions:
  - "All DTOs use int for enum values -- Contracts project has no Domain reference, matching Phase 5 pattern"
  - "EInvoiceExportDto includes all mandatory fields per Vietnamese Decree 123/2020 and Circular 32/2025"
  - "VisitChargeDto placed in Queries directory alongside GetVisitChargesQuery for cross-module charge collection"

patterns-established:
  - "Billing DTO pattern: sealed record with primary constructor, XML doc comments, int-based enums"
  - "Cross-module query pattern: sealed record queries in Billing.Contracts.Queries namespace"

requirements-completed: [FIN-01, FIN-02, FIN-03, FIN-04, FIN-10]

# Metrics
duration: 2min
completed: 2026-03-06
---

# Phase 07 Plan 06: Billing Contracts DTOs Summary

**Sealed record DTOs for invoice, payment, cashier shift, and Vietnamese e-invoice export with cross-module charge collection queries**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-06T13:57:23Z
- **Completed:** 2026-03-06T13:59:27Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Complete invoice data contracts with line items, discounts, refunds, and lightweight summary DTO
- Payment and cashier shift DTOs with shift report and template records
- Vietnamese e-invoice export DTO with all Decree 123/2020 mandatory fields (template, seller/buyer, tax, items)
- Cross-module query records for visit invoice lookup, pending invoices, and charge collection

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Invoice, Payment, and Shift DTOs** - `2f63567` (feat)
2. **Task 2: Create E-Invoice export DTO and cross-module queries** - `4df31e1` (feat)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Contracts/Dtos/InvoiceDto.cs` - InvoiceDto, InvoiceLineItemDto, DiscountDto, RefundDto, InvoiceSummaryDto sealed records
- `backend/src/Modules/Billing/Billing.Contracts/Dtos/PaymentDto.cs` - PaymentDto with split payment and card tracking fields
- `backend/src/Modules/Billing/Billing.Contracts/Dtos/CashierShiftDto.cs` - CashierShiftDto, ShiftReportDto, ShiftTemplateDto sealed records
- `backend/src/Modules/Billing/Billing.Contracts/Dtos/EInvoiceExportDto.cs` - EInvoiceExportDto and EInvoiceLineItemDto with Vietnamese tax law fields
- `backend/src/Modules/Billing/Billing.Contracts/Queries/GetVisitInvoiceQuery.cs` - GetVisitInvoiceQuery, GetPendingInvoicesQuery, GetVisitChargesQuery, VisitChargeDto

## Decisions Made
- All DTOs use `int` for enum values since Contracts project has no Domain reference, consistent with Phase 5 pattern across Clinical, Pharmacy, and Scheduling modules
- EInvoiceExportDto designed with all mandatory Vietnamese e-invoice fields per Decree 123/2020 (template name/symbol, seller/buyer info, tax calculations, payment method)
- VisitChargeDto co-located with GetVisitChargesQuery in Queries directory for cross-module charge collection clarity

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All Billing.Contracts DTOs ready for Application layer handlers (Plan 07-07+)
- Cross-module query records ready for Wolverine IMessageBus handler implementation
- E-invoice export DTO ready for integration with Vietnamese tax authority APIs

## Self-Check: PASSED

All 5 created files verified on disk. Both task commits (2f63567, 4df31e1) verified in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*
