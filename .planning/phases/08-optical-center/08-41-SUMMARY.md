---
phase: 08-optical-center
plan: 41
subsystem: ui
tags: [react, numberinput, optical, bugfix, onChange]

requires:
  - phase: 08-optical-center
    provides: "SharedNumberInput component with (value: number) => void onChange API"
provides:
  - "Fixed NumberInput onChange handlers in ComboPackageForm, WarrantyClaimForm, StocktakingScanner"
affects: [08-optical-center]

tech-stack:
  added: []
  patterns: ["NumberInput onChange receives number directly, not event object"]

key-files:
  created: []
  modified:
    - frontend/src/features/optical/components/ComboPackageForm.tsx
    - frontend/src/features/optical/components/WarrantyClaimForm.tsx
    - frontend/src/features/optical/components/StocktakingScanner.tsx

key-decisions:
  - "Removed unnecessary undefined/null fallbacks since NumberInput only fires onChange with valid numbers"

patterns-established:
  - "NumberInput onChange pattern: (value) => handler(value) -- never use e.target.value"

requirements-completed: [OPT-06, OPT-07, OPT-09]

duration: 2min
completed: 2026-03-18
---

# Phase 08 Plan 41: NumberInput onChange Fix Summary

**Fixed three NumberInput onChange API mismatches in optical components that caused TypeError crashes on numeric input fields**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-18T16:01:07Z
- **Completed:** 2026-03-18T16:02:31Z
- **Tasks:** 1
- **Files modified:** 3

## Accomplishments
- Fixed ComboPackageForm originalTotalPrice field crash by replacing e.target.value with direct value callback
- Fixed WarrantyClaimForm discountAmount field crash by replacing e.target.value with direct value callback
- Fixed StocktakingScanner physicalCount field crash by replacing e.target.value/parseInt with direct value callback

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix NumberInput onChange in ComboPackageForm, WarrantyClaimForm, and StocktakingScanner** - `9369922` (fix)

## Files Created/Modified
- `frontend/src/features/optical/components/ComboPackageForm.tsx` - Fixed originalTotalPrice onChange handler
- `frontend/src/features/optical/components/WarrantyClaimForm.tsx` - Fixed discountAmount onChange handler
- `frontend/src/features/optical/components/StocktakingScanner.tsx` - Fixed physicalCount onChange handler

## Decisions Made
- Removed unnecessary undefined/null fallback logic since NumberInput only calls onChange with valid parsed numbers
- Removed parseInt() in StocktakingScanner since NumberInput already provides a number type

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All three optical form inputs now correctly handle numeric input without crashes
- No blockers for subsequent plans

---
*Phase: 08-optical-center*
*Completed: 2026-03-18*
