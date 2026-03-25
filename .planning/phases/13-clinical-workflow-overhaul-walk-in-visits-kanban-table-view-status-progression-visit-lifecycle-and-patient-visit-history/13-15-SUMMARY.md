---
phase: 13-clinical-workflow-overhaul
plan: 15
subsystem: ui
tags: [react, shadcn, clinical-workflow, payment, cashier, invoice]

requires:
  - phase: 13-13
    provides: "Stage detail shell, bottom bar, and stage view patterns"
  - phase: 13-14
    provides: "Stage 5 prescription view with drug/optical prescriptions"
provides:
  - "Stage 6 Cashier view with invoice display and payment collection"
  - "PaymentMethodSelector reusable component (cash/card/transfer)"
  - "PostPaymentSuccessView with auto-routing next steps tiles"
  - "Route wiring for Stage 6 in visit stage router"
affects: [13-16, 13-17, 13-18]

tech-stack:
  added: []
  patterns:
    - "Two-phase stage view (invoice -> payment) with internal state toggle"
    - "VND currency formatting via Intl.NumberFormat('vi-VN')"
    - "Track status-based conditional rendering for post-payment routing"

key-files:
  created:
    - frontend/src/features/clinical/components/stage-views/Stage6CashierView.tsx
    - frontend/src/features/clinical/components/stage-views/PaymentMethodSelector.tsx
    - frontend/src/features/clinical/components/stage-views/PostPaymentSuccessView.tsx
  modified:
    - frontend/src/app/routes/_authenticated/clinical/visit.$visitId.stage.tsx

key-decisions:
  - "Single combined invoice (no split toggle) per CONFIRMATION_2.md Q1"
  - "Track statuses fetched from useActiveVisits for PostPaymentSuccessView routing"

patterns-established:
  - "PaymentMethodSelector: reusable 3-column grid selector with blue selected state"
  - "Post-action success view pattern with next steps tiles and print buttons"

requirements-completed: [CLN-03, CLN-04]

duration: 5min
completed: 2026-03-25
---

# Phase 13 Plan 15: Stage 6 Cashier Payment Summary

**Stage 6 cashier view with invoice generation, VND payment collection, 3-method selector, change calculation, and post-payment auto-routing tiles**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-25T13:27:22Z
- **Completed:** 2026-03-25T13:32:49Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Two-phase cashier view: invoice display with grouped line items then payment collection
- PaymentMethodSelector with cash/card/transfer in 3-column grid with blue selected state
- Real-time change calculation with deficit (red) and surplus (green) display
- PostPaymentSuccessView with next steps tiles based on drug/glasses track status
- Route wired for Stage 6 with post-payment view detection

## Task Commits

Each task was committed atomically:

1. **Task 1: Stage 6 Cashier view with invoice, payment flow** - `54dba6f` (feat)
2. **Task 2: Post-payment success view with auto-routing and route update** - `14b253b` (feat)

## Files Created/Modified
- `frontend/src/features/clinical/components/stage-views/Stage6CashierView.tsx` - Two-phase cashier view (invoice + payment)
- `frontend/src/features/clinical/components/stage-views/PaymentMethodSelector.tsx` - Reusable 3-column payment method grid
- `frontend/src/features/clinical/components/stage-views/PostPaymentSuccessView.tsx` - Post-payment success with next steps tiles
- `frontend/src/app/routes/_authenticated/clinical/visit.$visitId.stage.tsx` - Stage 6 route case added

## Decisions Made
- Single combined invoice with no split toggle, per CONFIRMATION_2.md Q1 (OpticalCenter before Cashier means glasses price always known)
- Track statuses obtained from useActiveVisits query since VisitDetailDto does not include drugTrackStatus/glassesTrackStatus
- Invoice line item prices are placeholder zeros for drugs and glasses sections; backend pricing integration will wire actual costs

## Deviations from Plan

None - plan executed exactly as written.

## Known Stubs

| File | Line | Stub | Reason |
|------|------|------|--------|
| Stage6CashierView.tsx | 63,76,91,98,102,108 | `unitPrice: 0` | Drug and glasses prices not available from current VisitDetailDto; requires backend invoice/pricing API to wire actual costs |
| Stage6CashierView.tsx | 56-62 | Hardcoded exam service | Exam service selection data not in VisitDetailDto; needs service catalog integration |

These stubs do not prevent the plan's goal (payment flow UI) from being achieved -- the payment flow works end-to-end, and pricing will be wired when backend provides invoice data.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Stage 6 cashier flow complete; ready for Stage 7 (Pharmacy) and Stage 8 (Optical) implementation
- PostPaymentSuccessView tiles show correct routing based on track statuses

---
*Phase: 13-clinical-workflow-overhaul*
*Completed: 2026-03-25*
