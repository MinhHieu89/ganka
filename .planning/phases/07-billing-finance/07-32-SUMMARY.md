---
phase: 07-billing-finance
plan: 32
subsystem: ui
tags: [react, tanstack-router, signalr, real-time, navigation]

requires:
  - phase: 07-billing-finance
    provides: InvoiceHistoryPage, InvoiceView, use-billing-hub hook, billing API
provides:
  - Clickable invoice history rows navigating to detail page
  - SignalR-driven auto-refresh on invoice detail page
affects: []

tech-stack:
  added: []
  patterns:
    - "Row-level onClick navigation pattern for table rows"
    - "Reusing useBillingHub hook across multiple pages for real-time updates"

key-files:
  created: []
  modified:
    - frontend/src/features/billing/components/InvoiceHistoryPage.tsx
    - frontend/src/features/billing/components/InvoiceView.tsx

key-decisions:
  - "Used onClick on TableRow instead of wrapping in Link for row navigation"
  - "Reused existing useBillingHub hook rather than adding polling or new SignalR logic"

patterns-established:
  - "TableRow onClick navigation: add useNavigate + onClick handler for clickable rows"

requirements-completed: [FIN-01, FIN-02]

duration: 2min
completed: 2026-03-17
---

# Phase 07 Plan 32: Gap Closure Summary

**Clickable invoice history rows with useNavigate and SignalR auto-refresh on invoice detail via useBillingHub hook**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-17T15:10:53Z
- **Completed:** 2026-03-17T15:12:49Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- Invoice history table rows are now fully clickable, navigating to /billing/invoices/{id}
- Invoice detail page auto-refreshes via SignalR when prescription line items are added

## Task Commits

Each task was committed atomically:

1. **Task 1: Make invoice history rows clickable for navigation** - `97bbc65` (feat)
2. **Task 2: Enable SignalR auto-refresh on invoice detail page** - `f0a528f` (feat)

## Files Created/Modified
- `frontend/src/features/billing/components/InvoiceHistoryPage.tsx` - Added useNavigate import, navigate() call, and onClick handler on TableRow
- `frontend/src/features/billing/components/InvoiceView.tsx` - Added useBillingHub import and hook call for real-time updates

## Decisions Made
- Used onClick on TableRow instead of wrapping rows in Link -- simpler approach that coexists with the existing Link on the invoice number cell
- Reused the existing useBillingHub hook in InvoiceView rather than adding polling or duplicating SignalR logic -- the hook already handles LineItemAdded events with query invalidation

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Both UAT retest gaps (row navigation and real-time updates) are closed
- Ready for re-verification

---
*Phase: 07-billing-finance*
*Completed: 2026-03-17*
