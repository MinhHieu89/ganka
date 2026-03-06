---
phase: 07-billing-finance
plan: 18
subsystem: ui
tags: [react, tanstack-router, shadcn-ui, billing, invoice, i18n, vnd-formatting]

# Dependency graph
requires:
  - phase: 07-billing-finance
    provides: "Billing API hooks, query keys, VND formatter (07-17)"
provides:
  - "Cashier billing dashboard with pending invoices list and shift status"
  - "Invoice detail page with line items grouped by department"
  - "InvoiceView and InvoiceLineItemsTable reusable components"
affects: [07-19, 07-20, 07-21, 07-22]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Department-grouped line items table with section subtotals"
    - "Two-column billing dashboard layout (invoices + shift status)"

key-files:
  created:
    - "frontend/src/features/billing/components/InvoiceLineItemsTable.tsx"
    - "frontend/src/features/billing/components/InvoiceView.tsx"
    - "frontend/src/app/routes/_authenticated/billing/index.tsx"
    - "frontend/src/app/routes/_authenticated/billing/invoices.$invoiceId.tsx"
  modified:
    - "frontend/src/features/billing/api/billing-api.ts"
    - "frontend/public/locales/vi/billing.json"
    - "frontend/public/locales/en/billing.json"

key-decisions:
  - "Added usePendingInvoices hook with 30s refetch for dashboard real-time updates"
  - "Used department enum number as grouping key with Vietnamese section headers from i18n"

patterns-established:
  - "InvoiceLineItemsTable: department grouping pattern with DEPARTMENT_ORDER and section subtotals"
  - "InvoiceView: status badge variant mapping pattern (Draft=secondary, Finalized=default, Voided=destructive)"

requirements-completed: [FIN-01, FIN-02]

# Metrics
duration: 7min
completed: 2026-03-06
---

# Phase 07 Plan 18: Cashier Dashboard and Invoice Detail Summary

**Billing dashboard with pending invoices list, shift status panel, and department-grouped invoice detail view with VND formatting**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-06T13:59:47Z
- **Completed:** 2026-03-06T14:06:47Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments
- InvoiceLineItemsTable groups line items by department (Medical, Pharmacy, Optical, Treatment) with section subtotals and grand total
- InvoiceView displays full invoice detail: header with status badge, patient info, grouped line items, financial summary, payments list, discounts list
- Cashier dashboard with two-column layout: pending Draft invoices on left, current shift status on right
- Invoice detail route page with breadcrumb navigation back to billing dashboard

## Task Commits

Each task was committed atomically:

1. **Task 1: Create InvoiceView and InvoiceLineItemsTable components** - `6da1eb4` (feat)
2. **Task 2: Create billing route pages** - `3e4879a` (feat)

## Files Created/Modified
- `frontend/src/features/billing/components/InvoiceLineItemsTable.tsx` - Department-grouped line items table with section subtotals and VND formatting
- `frontend/src/features/billing/components/InvoiceView.tsx` - Invoice detail view with status badge, payments, discounts, balance due
- `frontend/src/app/routes/_authenticated/billing/index.tsx` - Cashier dashboard with pending invoices + shift status
- `frontend/src/app/routes/_authenticated/billing/invoices.$invoiceId.tsx` - Invoice detail route with breadcrumb navigation
- `frontend/src/features/billing/api/billing-api.ts` - Added pendingInvoices query key, getPendingInvoices function, usePendingInvoices hook
- `frontend/public/locales/vi/billing.json` - Added lineItems section i18n keys
- `frontend/public/locales/en/billing.json` - Added lineItems section i18n keys

## Decisions Made
- Added usePendingInvoices hook to billing-api.ts with 30-second refetch interval for near-real-time dashboard updates
- Used Vietnamese department names from i18n (departments.medical/pharmacy/optical/treatment) as section headers in line items table
- Status badge variant mapping: Draft=secondary (muted), Finalized=default (green), Voided=destructive (red)
- Balance Due highlighted in destructive color when > 0, green when fully paid

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added getPendingInvoices API function and usePendingInvoices hook**
- **Found during:** Task 1 (InvoiceView and InvoiceLineItemsTable components)
- **Issue:** billing-api.ts lacked a pending invoices query needed by the billing dashboard
- **Fix:** Added pendingInvoices query key, getPendingInvoices function (filters by status=0/Draft), and usePendingInvoices hook with 30s refetch
- **Files modified:** frontend/src/features/billing/api/billing-api.ts
- **Verification:** TypeScript compilation passes, no billing-related errors
- **Committed in:** 6da1eb4 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Auto-fix necessary for dashboard functionality. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- InvoiceView component ready for payment form integration (07-19)
- Action buttons area prepared as placeholder for payment, discount, finalize buttons (07-20, 07-21)
- Dashboard ready for shift management features (07-22)

## Self-Check: PASSED

All files verified present on disk. All commit hashes verified in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*
