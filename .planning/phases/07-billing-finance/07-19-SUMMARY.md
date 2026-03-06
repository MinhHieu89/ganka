---
phase: 07-billing-finance
plan: 19
subsystem: ui
tags: [react, shadcn, payment-form, billing, vnd-formatting, tanstack-query]

# Dependency graph
requires:
  - phase: 07-billing-finance/07-17
    provides: "Billing API layer (billing-api.ts, shift-api.ts, format-vnd.ts)"
  - phase: 07-billing-finance/07-18
    provides: "InvoiceView and InvoiceLineItemsTable base components"
provides:
  - "PaymentMethodSelector component with 7 Vietnamese clinic payment methods"
  - "PaymentForm dialog with Zod validation, method-specific fields, split payment support"
  - "InvoiceView with payment collection, print, finalize, discount, and refund actions"
affects: [07-20, 07-21, 07-22, 07-24]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Payment method card selector with responsive grid layout"
    - "Method-specific conditional fields pattern (reference/card/split)"
    - "Blob URL print pattern for PDF (fetch blob, createObjectURL, window.open)"

key-files:
  created:
    - "frontend/src/features/billing/components/PaymentMethodSelector.tsx"
    - "frontend/src/features/billing/components/PaymentForm.tsx"
  modified:
    - "frontend/src/features/billing/components/InvoiceView.tsx"

key-decisions:
  - "Used custom card selector over shadcn ToggleGroup for better mobile UX with icons and labels"
  - "Payment method helper functions (methodRequiresReference, methodIsCard) centralized in PaymentMethodSelector"
  - "Split payment toggle with sequence selector (Lan 1 / Lan 2) buttons instead of dropdown"

patterns-established:
  - "Payment method selector: grid of clickable cards with primary highlight on selection"
  - "Dialog form with conditional fields based on enum selection"
  - "Invoice action toolbar: context-sensitive buttons based on status and payment state"

requirements-completed: [FIN-03, FIN-05]

# Metrics
duration: 9min
completed: 2026-03-06
---

# Phase 7 Plan 19: Payment Recording Form Summary

**Multi-method PaymentForm with 7 payment types, method-specific validation, treatment package split payments, and InvoiceView action toolbar integration**

## Performance

- **Duration:** 9 min
- **Started:** 2026-03-06T13:59:47Z
- **Completed:** 2026-03-06T14:08:33Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- PaymentMethodSelector with 7 payment methods (Cash, Bank Transfer, QR VNPay/MoMo/ZaloPay, Card Visa/MC) in responsive 4-column grid
- PaymentForm dialog with RHF + Zod validation, method-specific fields (reference number for QR/bank, card last 4 for cards), amount validation against balance due
- Treatment package split payment fields with 50/50 sequence selector
- InvoiceView now has full action toolbar: Collect Payment, Apply Discount, Request Refund, Print Invoice, Print Receipt, Finalize, E-Invoice Export

## Task Commits

Each task was committed atomically:

1. **Task 1: Create PaymentMethodSelector and PaymentForm** - `8916f80` (feat -- pre-existing from prior session)
2. **Task 2: Integrate PaymentForm into InvoiceView** - `95b960d` (feat)

## Files Created/Modified
- `frontend/src/features/billing/components/PaymentMethodSelector.tsx` - 7 payment methods with icons, responsive grid, helper functions
- `frontend/src/features/billing/components/PaymentForm.tsx` - Dialog form with Zod validation, conditional fields, split payment
- `frontend/src/features/billing/components/InvoiceView.tsx` - Added payment collection, print, finalize, discount, refund action buttons

## Decisions Made
- Used custom card selector pattern over shadcn RadioGroup/ToggleGroup for better visual distinction of payment methods with icons
- Helper functions (methodRequiresReference, methodIsCard, getCardType) exported from PaymentMethodSelector for reuse
- Treatment package fields are collapsible (hidden by default unless treatmentPackageId prop is provided)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] InvoiceView base component missing from 07-18**
- **Found during:** Task 2 (Integrate PaymentForm into InvoiceView)
- **Issue:** Plan 07-18 created InvoiceView.tsx but it was missing the action button integration area
- **Fix:** Updated existing InvoiceView.tsx with full action toolbar, state management for dialogs, and payment/print/finalize handlers
- **Files modified:** frontend/src/features/billing/components/InvoiceView.tsx
- **Verification:** TypeScript compiles with zero billing-related errors
- **Committed in:** 95b960d (Task 2 commit)

**2. [Rule 3 - Blocking] TypeScript strict mode issue with .includes() on const arrays**
- **Found during:** Task 1 (PaymentMethodSelector)
- **Issue:** TypeScript narrowed const array element types too strictly for .includes() parameter
- **Fix:** Declared helper arrays with `readonly number[]` type annotation instead of const assertion
- **Files modified:** frontend/src/features/billing/components/PaymentMethodSelector.tsx
- **Verification:** TypeScript compiles cleanly
- **Committed in:** Part of 8916f80 (pre-existing)

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both fixes necessary for compilation. No scope creep.

## Issues Encountered
- Task 1 files (PaymentMethodSelector.tsx, PaymentForm.tsx) were already committed from a prior execution session (commit 8916f80). Detected as no-diff and skipped re-commit.
- Linter auto-added imports for DiscountDialog, RefundDialog, and EInvoiceExportButton to InvoiceView.tsx during editing.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- PaymentForm ready for use from InvoiceView
- DiscountDialog and RefundDialog already exist and are integrated into InvoiceView
- Next plans (07-20 discount/refund, 07-21 shift management) can build on this foundation

## Self-Check: PASSED

All files verified present on disk. All commit hashes found in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*
