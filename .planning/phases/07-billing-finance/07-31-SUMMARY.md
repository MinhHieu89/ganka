---
phase: 07-billing-finance
plan: 31
subsystem: ui
tags: [react, shadcn, alert-dialog, i18n, invoice, line-items]

requires:
  - phase: 07-billing-finance
    provides: "useRemoveLineItem hook and DELETE endpoint for line items"
provides:
  - "Delete button UI on draft invoice line items with confirmation dialog"
affects: [billing-ui, invoice-management]

tech-stack:
  added: []
  patterns: [conditional-column-rendering, alert-dialog-confirmation]

key-files:
  created: []
  modified:
    - frontend/src/features/billing/components/InvoiceLineItemsTable.tsx
    - frontend/src/features/billing/components/InvoiceView.tsx
    - frontend/public/locales/en/billing.json
    - frontend/public/locales/vi/billing.json

key-decisions:
  - "Used AlertDialog from shadcn/ui for delete confirmation, consistent with existing finalize confirmation pattern"
  - "Skipped success toast in hook -- dialog closing plus item disappearing provides sufficient feedback"

patterns-established:
  - "Conditional table columns: render extra TableHead/TableCell only when isDraft, adjust colSpan accordingly"

requirements-completed: [FIN-01, FIN-04]

duration: 2min
completed: 2026-03-17
---

# Phase 07 Plan 31: Invoice Line Item Delete Button Summary

**Draft invoice line items now have a trash icon button with AlertDialog confirmation, wired to existing useRemoveLineItem mutation hook**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-17T10:09:27Z
- **Completed:** 2026-03-17T10:11:37Z
- **Tasks:** 1
- **Files modified:** 4

## Accomplishments
- Added delete (trash) icon button on each line item row, only visible on Draft invoices
- Clicking delete shows AlertDialog confirmation with the item name
- Confirming deletion calls useRemoveLineItem which triggers DELETE endpoint and auto-refreshes invoice data
- Finalized and Voided invoices show no delete button
- Added i18n keys in both English and Vietnamese

## Task Commits

Each task was committed atomically:

1. **Task 1: Add delete button to InvoiceLineItemsTable and wire to backend** - `bc170c9` (feat)

**Plan metadata:** [pending] (docs: complete plan)

## Files Created/Modified
- `frontend/src/features/billing/components/InvoiceLineItemsTable.tsx` - Added invoiceId/isDraft props, useRemoveLineItem hook, AlertDialog with trash icon, conditional column rendering
- `frontend/src/features/billing/components/InvoiceView.tsx` - Passes invoiceId and isDraft to InvoiceLineItemsTable
- `frontend/public/locales/en/billing.json` - Added remove, removeTitle, removeConfirm keys
- `frontend/public/locales/vi/billing.json` - Added Vietnamese translations for remove confirmation

## Decisions Made
- Used AlertDialog from shadcn/ui for delete confirmation, consistent with existing finalize confirmation pattern in InvoiceView
- Skipped success toast in the hook -- the dialog closing plus the item disappearing from table is sufficient UX feedback
- Passed removeLineItem mutation instance from parent to DepartmentSection to avoid multiple hook instantiations

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- UAT Gap 3 (Test 10) is now closed
- Line item deletion fully functional on draft invoices

---
*Phase: 07-billing-finance*
*Completed: 2026-03-17*
