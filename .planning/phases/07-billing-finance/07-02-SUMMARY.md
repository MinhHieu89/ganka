---
phase: 07-billing-finance
plan: 02
subsystem: domain
tags: [billing, payment, discount, refund, approval-workflow, ddd, entity]

# Dependency graph
requires:
  - phase: 07-01
    provides: "Invoice and InvoiceLineItem entities (InvoiceId foreign key target)"
provides:
  - "Payment entity with multi-method support (cash, bank, QR, card) and split payment tracking"
  - "Discount entity with percentage/fixed-amount types and manager approval workflow"
  - "Refund entity with multi-step approval (Requested -> Approved -> Processed/Rejected)"
  - "5 billing enums: PaymentMethod, DiscountType, PaymentStatus, ApprovalStatus, RefundStatus"
affects: [07-billing-finance, cashier-shift, e-invoice, billing-api]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Multi-enum file pattern: related small enums grouped in single file"
    - "Status-based approval workflow with state-validated transitions"
    - "Split payment tracking via IsSplitPayment + SplitSequence fields"

key-files:
  created:
    - "backend/src/Modules/Billing/Billing.Domain/Enums/PaymentMethod.cs"
    - "backend/src/Modules/Billing/Billing.Domain/Enums/PaymentStatus.cs"
    - "backend/src/Modules/Billing/Billing.Domain/Entities/Payment.cs"
    - "backend/src/Modules/Billing/Billing.Domain/Entities/Discount.cs"
    - "backend/src/Modules/Billing/Billing.Domain/Entities/Refund.cs"
  modified: []

key-decisions:
  - "Multiple related enums per file to stay within 5-file plan constraint"
  - "Refund rejection allowed from both Requested and Approved states for flexibility"
  - "CalculateAmount rounds to 0 decimal places (VND has no fractional units)"
  - "Split payment validates sequence must be 1 or 2"

patterns-established:
  - "Approval workflow: Pending -> Approved/Rejected with managerId + timestamp"
  - "State transition validation: InvalidOperationException on invalid status"
  - "Domain factory methods with argument validation (ArgumentException)"

requirements-completed: [FIN-03, FIN-05, FIN-07, FIN-08]

# Metrics
duration: 2min
completed: 2026-03-06
---

# Phase 07 Plan 02: Payment, Discount, and Refund Entities Summary

**Payment, Discount, and Refund domain entities with 5 billing enums, multi-method payment support, manager approval workflows, and state-validated transitions**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-06T13:57:36Z
- **Completed:** 2026-03-06T13:59:27Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Payment entity supporting 7 payment methods (cash, bank transfer, 3 QR wallets, 2 card types) with split payment tracking for treatment packages
- Discount entity with percentage and fixed-amount types, manager approval workflow, and CalculateAmount computation
- Refund entity with multi-step approval (Requested -> Approved -> Processed) and rejection at multiple stages
- All 5 billing enums (PaymentMethod, DiscountType, PaymentStatus, ApprovalStatus, RefundStatus) defined in 2 organized files

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Payment enums** - `3be0709` (feat)
2. **Task 2: Create Payment, Discount, and Refund entities** - `962e3ae` (feat)

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Domain/Enums/PaymentMethod.cs` - PaymentMethod and DiscountType enums
- `backend/src/Modules/Billing/Billing.Domain/Enums/PaymentStatus.cs` - PaymentStatus, ApprovalStatus, and RefundStatus enums
- `backend/src/Modules/Billing/Billing.Domain/Entities/Payment.cs` - Payment entity with multi-method, split payment, Confirm/MarkRefunded methods
- `backend/src/Modules/Billing/Billing.Domain/Entities/Discount.cs` - Discount entity with approval workflow, Approve/Reject/CalculateAmount methods
- `backend/src/Modules/Billing/Billing.Domain/Entities/Refund.cs` - Refund entity with multi-step approval, Approve/Reject/Process methods

## Decisions Made
- Multiple related enums grouped per file (PaymentMethod+DiscountType, PaymentStatus+ApprovalStatus+RefundStatus) to stay within 5-file constraint
- Refund rejection allowed from both Requested and Approved states for workflow flexibility (manager can reject even after initial approval)
- CalculateAmount rounds to 0 decimal places since VND has no fractional currency units
- Split payment SplitSequence validated to be exactly 1 or 2 (first half or second half)
- Card type stored as string rather than enum for simpler display and extensibility

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Payment, Discount, and Refund entities ready for EF Core configuration and DbContext registration
- Enums ready for use in application layer commands/queries
- Approval workflows ready for integration with permission system (manager/owner roles)
- Split payment fields ready for treatment package integration

## Self-Check: PASSED

- All 5 created files verified present on disk
- All 3 commits verified in git log (3be0709, 962e3ae, cc314cd)
- Build succeeds with 0 warnings, 0 errors

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*
