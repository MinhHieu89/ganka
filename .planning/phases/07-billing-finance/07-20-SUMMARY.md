---
phase: 07-billing-finance
plan: 20
subsystem: ui
tags: [react, shadcn-ui, billing, discount, refund, e-invoice, manager-pin, react-hook-form, zod]

# Dependency graph
requires:
  - phase: 07-18
    provides: InvoiceView, InvoiceLineItemsTable, billing API hooks
provides:
  - ApprovalPinDialog reusable manager PIN entry component
  - DiscountDialog with percentage/fixed toggle and live VND preview
  - RefundDialog with full/partial scope and approval workflow
  - EInvoiceExportButton with PDF, JSON, XML download options
affects: [07-billing-finance, billing-integration]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Manager PIN approval dialog pattern (reusable for discount and refund)"
    - "Button-based toggle group for discount type (percentage/fixed)"
    - "Blob URL file download pattern for e-invoice exports"
    - "Multi-step mutation flow (apply -> PIN approve -> process)"

key-files:
  created:
    - frontend/src/features/billing/components/ApprovalPinDialog.tsx
    - frontend/src/features/billing/components/DiscountDialog.tsx
    - frontend/src/features/billing/components/RefundDialog.tsx
    - frontend/src/features/billing/components/EInvoiceExportButton.tsx
  modified:
    - frontend/src/features/billing/api/billing-api.ts

key-decisions:
  - "Used button-based toggle instead of ToggleGroup component (not available in project shadcn/ui)"
  - "Discount preview calculated client-side for instant feedback"
  - "Refund auto-processes after PIN approval (no separate manual process step)"
  - "E-invoice PDF opens in new tab; JSON/XML trigger file download"

patterns-established:
  - "ApprovalPinDialog: reusable PIN entry pattern for any approval workflow"
  - "formatVnd helper: Intl.NumberFormat for Vietnamese currency display"
  - "triggerDownload: blob URL + temporary anchor element for file downloads"

requirements-completed: [FIN-04, FIN-07, FIN-08]

# Metrics
duration: 5min
completed: 2026-03-06
---

# Phase 07 Plan 20: Discount, Refund, PIN Approval, and E-Invoice Export UI Summary

**Discount dialog with percentage/fixed toggle and live VND preview, refund dialog with full/partial scope and approval workflow, reusable manager PIN dialog, and e-invoice export button with PDF/JSON/XML for MISA**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-06T13:59:52Z
- **Completed:** 2026-03-06T14:04:50Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- ApprovalPinDialog: reusable 4-6 digit PIN entry with auto-focus, numeric-only input, error display
- DiscountDialog: invoice/line-item scope selection, percentage/fixed type toggle, live calculated VND preview, submit-then-approve two-step flow
- RefundDialog: full/partial scope, line item selector, amount defaults to max, approval + auto-process chain
- EInvoiceExportButton: dropdown menu with PDF (new tab), JSON (file download), XML (file download) for MISA integration
- Added processRefund API function and useProcessRefund hook to billing-api

## Task Commits

Each task was committed atomically:

1. **Task 1: Create ApprovalPinDialog and DiscountDialog** - `b6a7aa7` (feat)
2. **Task 2: Create RefundDialog and EInvoiceExportButton** - `391efac` (feat)

## Files Created/Modified
- `frontend/src/features/billing/components/ApprovalPinDialog.tsx` - Reusable manager PIN entry dialog with validation
- `frontend/src/features/billing/components/DiscountDialog.tsx` - Discount form with type toggle, scope, preview, and PIN approval
- `frontend/src/features/billing/components/RefundDialog.tsx` - Refund request with scope, amount, and approval + process workflow
- `frontend/src/features/billing/components/EInvoiceExportButton.tsx` - E-invoice export dropdown (PDF, JSON, XML)
- `frontend/src/features/billing/api/billing-api.ts` - Added processRefund function and useProcessRefund hook

## Decisions Made
- Used custom button-based toggle for discount type (percentage/fixed) since shadcn/ui ToggleGroup component is not installed in the project
- Discount preview is calculated client-side using Intl.NumberFormat for instant VND feedback
- RefundDialog auto-processes after PIN approval rather than requiring a separate manual process step (simpler UX for small clinic)
- E-invoice PDF opens in a new tab (for viewing/printing); JSON and XML trigger file downloads (for MISA import)
- All Vietnamese text uses unaccented ASCII (matching existing codebase convention for UI labels)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added processRefund API function and hook**
- **Found during:** Task 1 (planning ahead for Task 2)
- **Issue:** billing-api.ts had no processRefund function needed by RefundDialog's approval-then-process flow
- **Fix:** Added processRefund async function and useProcessRefund mutation hook
- **Files modified:** frontend/src/features/billing/api/billing-api.ts
- **Verification:** TypeScript compilation passes
- **Committed in:** b6a7aa7 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** Essential for RefundDialog's approval workflow. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 4 dialog/button components ready for integration into InvoiceView
- ApprovalPinDialog is reusable for any future approval workflow (e.g., void invoice)
- E-invoice export ready for MISA manual import workflow

## Self-Check: PASSED

- All 4 component files exist on disk
- Both task commits (b6a7aa7, 391efac) verified in git log

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*
