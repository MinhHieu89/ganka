---
phase: 07-billing-finance
plan: 17
subsystem: api
tags: [tanstack-query, openapi-fetch, billing, shifts, vnd-formatter, react]

# Dependency graph
requires:
  - phase: 07-13
    provides: "Billing module backend contracts and domain model"
provides:
  - "Billing API functions (invoice, payment, discount, refund CRUD)"
  - "Shift API functions (open, close, current, report, templates)"
  - "TanStack Query hooks with cache invalidation for all billing operations"
  - "PDF/print functions for invoices, receipts, e-invoices, shift reports"
  - "E-invoice export (JSON/XML) for MISA import"
  - "VND currency formatter (formatVND, formatVNDCompact)"
  - "Enum display maps (payment methods, departments, statuses)"
affects: [07-18, 07-19, 07-20, 07-21, 07-22, 07-23]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Billing query key factory (billingKeys, shiftKeys) following pharmacyKeys pattern"
    - "Native fetch blob for PDF endpoints following document-api.ts pattern"
    - "Separate API modules for billing and shift concerns"

key-files:
  created:
    - "frontend/src/features/billing/api/billing-api.ts"
    - "frontend/src/features/billing/api/shift-api.ts"
    - "frontend/src/shared/lib/format-vnd.ts"
  modified: []

key-decisions:
  - "Matched TypeScript DTOs to backend Billing.Contracts.Dtos record shapes exactly"
  - "Added null/undefined guard to formatVND for safer usage in optional numeric fields"
  - "Split billing-api.ts and shift-api.ts into separate modules for cohesion"
  - "Added processRefund API function for full refund workflow (Requested -> Approved -> Processed)"
  - "Used native fetch for PDF/export endpoints following Phase 5 document-api.ts pattern"

patterns-established:
  - "billingKeys: query key factory with all/invoices/pendingInvoices/invoice/visitInvoice/pendingApprovals/paymentsByInvoice"
  - "shiftKeys: query key factory with all/current/report/templates"
  - "Cross-module invalidation: useRecordPayment and useFinalizeInvoice invalidate both billingKeys and shiftKeys"

requirements-completed: [FIN-01, FIN-03, FIN-10]

# Metrics
duration: 5min
completed: 2026-03-06
---

# Phase 07 Plan 17: Billing Frontend API Layer Summary

**Billing and shift TanStack Query API layer with 12+ hooks, PDF/export functions, and VND currency formatter**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-06T13:58:45Z
- **Completed:** 2026-03-06T14:03:48Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Complete billing API with invoice, payment, discount, refund CRUD functions and TanStack Query hooks
- Shift management API with open/close/report/template functions and hooks
- PDF print functions for invoices, receipts, e-invoices, shift reports using native fetch blob pattern
- E-invoice export (JSON/XML) for MISA import
- VND currency formatter with null-safe formatVND and formatVNDCompact utilities
- Enum display maps for all billing domain enums (7 maps covering payment methods, departments, statuses)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create billing API functions and hooks** - `0f4d934` / `84f2311` (feat)
   - billing-api.ts: billingKeys factory, 12 API functions, 13 TanStack Query hooks, 7 enum display maps
   - shift-api.ts: shiftKeys factory, 5 API functions, 5 TanStack Query hooks, 6 PDF/export functions
2. **Task 2: Create VND currency formatter** - `b6a7aa7` (feat)
   - format-vnd.ts: formatVND and formatVNDCompact with null/undefined guards

## Files Created/Modified
- `frontend/src/features/billing/api/billing-api.ts` - Invoice, payment, discount, refund API functions with TanStack Query hooks and enum maps
- `frontend/src/features/billing/api/shift-api.ts` - Shift management API functions, PDF print and e-invoice export functions
- `frontend/src/shared/lib/format-vnd.ts` - VND currency formatter using Intl.NumberFormat vi-VN locale

## Decisions Made
- TypeScript DTO interfaces match backend Billing.Contracts.Dtos record shapes exactly (InvoiceDto, PaymentDto, DiscountDto, RefundDto, CashierShiftDto, ShiftReportDto, ShiftTemplateDto)
- Added null/undefined guard to formatVND/formatVNDCompact for safer usage with optional numeric fields
- PaymentMethod enum values match backend exactly: Cash(0), BankTransfer(1), QrVnPay(2), QrMomo(3), QrZaloPay(4), CardVisa(5), CardMastercard(6)
- Department enum values match backend: Medical(0)="Kham benh", Pharmacy(1)="Duoc pham", Optical(2)="Kinh", Treatment(3)="Dieu tri"
- Cross-module cache invalidation: useRecordPayment and useFinalizeInvoice invalidate both billingKeys and shiftKeys
- Used native fetch for PDF endpoints to handle blob responses, following document-api.ts pattern from Phase 5

## Deviations from Plan

None - plan executed exactly as written. Files were already created by prior phase 07 plan executions.

## Issues Encountered
- Files were already committed by prior phase 07 plan executions (07-02, 07-05, 07-20) due to parallel plan execution. All content verified to match plan requirements exactly.
- Pre-existing TypeScript errors in 9 other files (admin-api, auth-api, patient-api, etc.) -- not related to billing changes.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Billing API layer complete, ready for UI component development (plans 07-18 through 07-23)
- All hooks follow established patterns (onError toast, queryClient.invalidateQueries on success)
- Enum maps available for display in billing UI components

## Self-Check: PASSED

All files exist, all commits found, all key content verified:
- 3/3 files present (billing-api.ts, shift-api.ts, format-vnd.ts)
- 3/3 commits found (0f4d934, 84f2311, b6a7aa7)
- All key exports verified: billingKeys, shiftKeys, useVisitInvoice, useRecordPayment, useCurrentShift, formatVND, formatVNDCompact, PAYMENT_METHOD_MAP, DEPARTMENT_MAP, onError toast pattern

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*
