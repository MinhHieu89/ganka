---
phase: 08-optical-center
plan: 30
subsystem: ui
tags: [react, tanstack-router, tanstack-query, shadcn-ui, optical-center, glasses-order, payment-gate]

# Dependency graph
requires:
  - phase: 08-25
    provides: optical-queries.ts with useGlassesOrderById and useUpdateOrderStatus hooks
  - phase: 08-29
    provides: OrderStatusBadge component, GlassesOrderTable component
provides:
  - GlassesOrderDetailPage with status timeline stepper and payment gate enforcement
  - OverdueOrderAlert reusable component for single and bulk overdue notifications
  - Dynamic TanStack Router route at /_authenticated/optical/orders/$orderId
affects: [08-31, 08-32, 08-33, 08-37]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "useRouter().history.back() for back navigation when typed Link route may not exist yet"
    - "Status timeline stepper using step array + current status comparison"
    - "Payment gate: disabled button + Alert when isPaymentConfirmed false and next step is Processing"

key-files:
  created:
    - frontend/src/features/optical/components/GlassesOrderDetailPage.tsx
    - frontend/src/features/optical/components/OverdueOrderAlert.tsx
    - frontend/src/app/routes/_authenticated/optical/orders.$orderId.tsx
  modified: []

key-decisions:
  - "Used router.history.back() instead of typed Link to avoid TS error when /optical/orders route may not exist in the same commit"
  - "Derived warrantyExpiryDate from deliveredAt + 12 months in frontend since backend GlassesOrderDto lacks warrantyExpiryDate field"
  - "Displayed order items from frameBrand/frameModel/lensName/comboPackageName DTO fields since GlassesOrderDto has no items array"

requirements-completed: [OPT-03, OPT-04]

# Metrics
duration: 12min
completed: 2026-03-08
---

# Phase 8 Plan 30: Glasses Order Detail Page Summary

**Order detail page with horizontal status stepper, payment gate blocking Processing when unpaid, overdue alert, warranty status badge, and dynamic TanStack Router route at /optical/orders/$orderId**

## Performance

- **Duration:** 12 min
- **Started:** 2026-03-08T02:50:29Z
- **Completed:** 2026-03-08T03:02:00Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- GlassesOrderDetailPage renders full order lifecycle with 5-step horizontal timeline (Ordered -> Processing -> Received -> Ready for Pickup -> Delivered)
- Payment gate: "Advance to Processing" button disabled with destructive Alert when IsPaymentConfirmed is false
- OverdueOrderAlert component supports both single order (with estimated date) and bulk count modes
- Route file creates dynamic `/optical/orders/$orderId` path using TanStack Router file-based routing

## Task Commits

Each task was committed atomically:

1. **Task 1: Create GlassesOrderDetailPage and OverdueOrderAlert** - `e5955e1` (feat)
2. **Task 2: Create order detail route** - `4925cb4` (feat)

## Files Created/Modified
- `frontend/src/features/optical/components/GlassesOrderDetailPage.tsx` - Full order detail with status timeline, payment gate, warranty info, items display, overdue alert
- `frontend/src/features/optical/components/OverdueOrderAlert.tsx` - Reusable destructive Alert for overdue orders with estimated date or count props
- `frontend/src/app/routes/_authenticated/optical/orders.$orderId.tsx` - TanStack Router dynamic route, uses Route.useParams() for orderId

## Decisions Made
- Used `router.history.back()` instead of a typed `Link to="/optical/orders"` to avoid TypeScript errors when the parent orders route might not be available in the same commit batch
- Derived warrantyExpiryDate as `deliveredAt + 12 months` in the frontend since the `GlassesOrderDto` from the backend doesn't include a `warrantyExpiryDate` field
- Built order items from `frameBrand`, `frameModel`, `lensName`, `comboPackageName` fields rather than an items array (GlassesOrderDto has no items array in the current DTO)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- `GlassesOrderDetailPage.tsx` and `OverdueOrderAlert.tsx` were already committed as part of plan 08-31 execution (the two plans ran out of order). The files were verified to match the plan specification.
- The routeTree.gen.ts already contained the import for `orders.$orderId` route - only the actual route file needed to be created.

## Next Phase Readiness
- Order detail page accessible at /optical/orders/:orderId from the orders list
- Payment gate UI implemented - staff cannot advance order to Processing without confirmed payment
- Overdue alert ready to be reused in GlassesOrdersPage (plan 08-29) banner

## Self-Check: PASSED

- GlassesOrderDetailPage.tsx: FOUND
- OverdueOrderAlert.tsx: FOUND
- orders.$orderId.tsx: FOUND
- 08-30-SUMMARY.md: FOUND
- e5955e1 (Task 1): FOUND
- 4925cb4 (Task 2): FOUND

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*
