---
phase: 07-billing-finance
plan: 26
subsystem: billing
tags: [discount-rejection, refund-approval, refund-processing, manager-pin, cashier-shift, wolverine, tdd]

# Dependency graph
requires:
  - phase: 07-11
    provides: ApplyDiscount, ApproveDiscount, RequestRefund handlers, VerifyManagerPinQuery contract, Billing.Unit.Tests project
  - phase: 07-08
    provides: Discount, Refund, Payment domain entities with status lifecycle methods
  - phase: 07-12
    provides: CashierShift with AddCashRefund method
provides:
  - RejectDiscountCommand handler with manager PIN verification and rejection reason
  - ApproveRefundCommand handler with manager PIN verification and Requested status validation
  - ProcessRefundCommand handler with cash refund shift tracking and payment refund marking
  - 12 unit tests covering all handler paths (reject/approve/process)
affects: [07-13, 07-20]

# Tech tracking
tech-stack:
  added: []
  patterns: [discount-rejection-workflow, refund-approval-lifecycle, cash-refund-shift-tracking]

key-files:
  created:
    - backend/src/Modules/Billing/Billing.Application/Features/RejectDiscount.cs
    - backend/src/Modules/Billing/Billing.Application/Features/ApproveRefund.cs
    - backend/src/Modules/Billing/Billing.Application/Features/ProcessRefund.cs
  modified:
    - backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs
    - backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs

key-decisions:
  - "Added ManagerId to RejectDiscountCommand and ApproveRefundCommand for consistency with ApproveDiscountCommand PIN verification pattern"
  - "ProcessRefund marks all confirmed payments as Refunded when processing a refund (full invoice refund pattern)"
  - "Cash refunds update shift CashRefunds via AddCashRefund; non-cash refunds skip shift interaction entirely"

patterns-established:
  - "Discount rejection: same PIN verification as approval, recalculates invoice totals after rejection to exclude rejected discount"
  - "Refund lifecycle: Requested -> Approved (PIN) -> Processed (cash/non-cash), with shift cash refund tracking for cash method"
  - "ProcessRefund creates no negative payment entity; instead marks existing payments as Refunded status"

requirements-completed: [FIN-07, FIN-08]

# Metrics
duration: 5min
completed: 2026-03-06
---

# Phase 07 Plan 26: Supplementary Discount/Refund Handlers Summary

**RejectDiscount handler with PIN verification, ApproveRefund with Requested status validation, and ProcessRefund with cash shift refund tracking and payment marking**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-06T14:29:02Z
- **Completed:** 2026-03-06T14:34:04Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Discount rejection workflow with manager PIN verification and rejection reason, recalculating invoice totals to exclude rejected discounts
- Refund approval validates Requested status before allowing manager PIN-gated approval
- Refund processing differentiates cash vs non-cash: cash refunds update shift CashRefunds, non-cash skip shift
- All 12 new tests pass (4 RejectDiscount + 4 ApproveRefund + 4 ProcessRefund), 45 total billing tests green

## Task Commits

Each task was committed atomically:

1. **Task 1: Create RejectDiscount handler** - `b2e6f49` (feat)
2. **Task 2: Create ApproveRefund and ProcessRefund handlers** - `a5a2154` (feat)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Application/Features/RejectDiscount.cs` - RejectDiscountCommand, validator, handler with PIN verification and rejection reason
- `backend/src/Modules/Billing/Billing.Application/Features/ApproveRefund.cs` - ApproveRefundCommand, validator, handler with Requested status validation and PIN verification
- `backend/src/Modules/Billing/Billing.Application/Features/ProcessRefund.cs` - ProcessRefundCommand, validator, handler with cash shift tracking and payment refund marking
- `backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs` - Added 4 RejectDiscount tests (valid, invalid PIN, already processed, not found)
- `backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs` - Added 8 tests: 4 ApproveRefund + 4 ProcessRefund (cash/non-cash/not approved/no shift)

## Decisions Made
- Added ManagerId to RejectDiscountCommand and ApproveRefundCommand because the existing VerifyManagerPinQuery requires (ManagerId, Pin) and discount.Reject/refund.Approve domain methods require managerId -- consistent with ApproveDiscountCommand pattern from Plan 11
- ProcessRefund marks all confirmed payments as Refunded rather than creating a negative payment entity, keeping the payment model simple
- Non-cash refunds (BankTransfer, QR, Card) skip shift interaction entirely since they don't affect the cash drawer

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added ManagerId to RejectDiscountCommand**
- **Found during:** Task 1
- **Issue:** Plan specified RejectDiscountCommand without ManagerId, but VerifyManagerPinQuery requires (Guid ManagerId, string Pin) and discount.Reject() needs managerId
- **Fix:** Added ManagerId parameter to RejectDiscountCommand for consistency with ApproveDiscountCommand pattern
- **Files modified:** RejectDiscount.cs, DiscountHandlerTests.cs
- **Verification:** All RejectDiscount tests pass with ManagerId
- **Committed in:** b2e6f49 (Task 1 commit)

**2. [Rule 3 - Blocking] Added ManagerId to ApproveRefundCommand**
- **Found during:** Task 2
- **Issue:** Plan specified ApproveRefundCommand without ManagerId, but same VerifyManagerPinQuery dependency and refund.Approve(managerId) requirement
- **Fix:** Added ManagerId parameter to ApproveRefundCommand
- **Files modified:** ApproveRefund.cs, RefundHandlerTests.cs
- **Verification:** All ApproveRefund tests pass with ManagerId
- **Committed in:** a5a2154 (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both fixes necessary for cross-module PIN verification to work. No scope creep -- same pattern already established by ApproveDiscountCommand in Plan 11.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 3 supplementary handlers ready for Plan 13 API endpoints (RejectDiscount, ApproveRefund, ProcessRefund)
- Full discount lifecycle complete: Apply (Pending) -> Approve/Reject (with PIN)
- Full refund lifecycle complete: Request -> Approve (with PIN) -> Process (cash/non-cash)
- 45 billing unit tests provide regression safety for future plans

## Self-Check: PASSED

All 5 files verified on disk. Both task commits (b2e6f49, a5a2154) verified in git log.

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*
