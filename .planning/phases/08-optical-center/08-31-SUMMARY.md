---
phase: 08-optical-center
plan: 31
subsystem: ui
tags: [react, tanstack-query, shadcn-ui, react-hook-form, zod, optical]

# Dependency graph
requires:
  - phase: 08-25
    provides: optical-api.ts and optical-queries.ts with useComboPackages, useCreateComboPackage, useUpdateComboPackage hooks
  - phase: 08-08
    provides: useFrames and useLensCatalog hooks for frame/lens selects in form

provides:
  - ComboPackagePage: grid card layout showing combo packages with savings visualization
  - ComboPackageForm: create/edit dialog with auto-calculated original price and savings preview
  - Route at /_authenticated/optical/combos
affects: [08-29, 08-30, 08-32, 08-33]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - Card grid layout (3 cols lg / 2 cols md / 1 col sm) for list views
    - Auto-calculated savings from frame+lens selects using sellingPrice fields

key-files:
  created:
    - frontend/src/features/optical/components/ComboPackagePage.tsx
    - frontend/src/features/optical/components/ComboPackageForm.tsx
    - frontend/src/app/routes/_authenticated/optical/combos.tsx
  modified:
    - frontend/src/features/optical/api/optical-api.ts

key-decisions:
  - "ComboPackageDto aligned to backend contract (comboPrice/originalTotalPrice/savings) — removed incorrect packagePrice field"
  - "Auto-populate originalTotalPrice from frame+lens sellingPrice when both are selected"
  - "Savings preview shown inline in form when comboPrice < originalTotalPrice"
  - "Show/hide inactive toggle in page header instead of tab/filter"

patterns-established:
  - "Card grid layout for catalog pages: 3/2/1 responsive columns"
  - "Savings badge shows percentage with line-through original price"

requirements-completed: [OPT-06]

# Metrics
duration: 15min
completed: 2026-03-08
---

# Phase 8 Plan 31: Combo Package Frontend Summary

**Combo package management page with card grid layout, savings visualization, and form with auto-calculated frame+lens pricing**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-08T02:51:31Z
- **Completed:** 2026-03-08T03:06:00Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- ComboPackagePage renders combo packages as responsive cards (3/2/1 col grid) with frame name, lens name, combo price, original price strikethrough, and savings badge
- ComboPackageForm dialog with frame and lens searchable selects, auto-calculates originalTotalPrice from frame+lens sellingPrice, shows savings preview
- Route accessible at /optical/combos via createFileRoute pattern
- Fixed ComboPackageDto mismatch between frontend (packagePrice) and backend (comboPrice/originalTotalPrice/savings)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create ComboPackagePage and ComboPackageForm** - `39d4513` (feat) — note: committed as part of 39d4513 batch
2. **Task 2: Create combo route file** - `1898867` (feat)

## Files Created/Modified
- `frontend/src/features/optical/components/ComboPackagePage.tsx` - Card grid page with savings visualization, show/hide inactive toggle
- `frontend/src/features/optical/components/ComboPackageForm.tsx` - Create/edit dialog with frame/lens selects, auto-calculated savings preview
- `frontend/src/app/routes/_authenticated/optical/combos.tsx` - TanStack Router route at /_authenticated/optical/combos
- `frontend/src/features/optical/api/optical-api.ts` - Fixed ComboPackageDto fields and input types to match backend contract

## Decisions Made
- Auto-populate originalTotalPrice from frame sellingPrice + lens sellingPrice when both selected in form; user can override
- Savings preview shown as green banner with formatted VND and percentage when combo < original
- "Any frame" / "Any lens" as first Select option (value = `__none__`) to allow null frameId/lensCatalogItemId
- Show/hide inactive as toggle button in page header; inactive cards styled with opacity-60 and dashed border

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed ComboPackageDto frontend/backend mismatch**
- **Found during:** Task 1 (ComboPackagePage implementation)
- **Issue:** Frontend ComboPackageDto used `packagePrice` but backend sends `comboPrice`, `originalTotalPrice`, and `savings`. Form inputs also used wrong field names.
- **Fix:** Updated ComboPackageDto to include `comboPrice`, `originalTotalPrice`, `savings`, `createdAt`. Updated CreateComboPackageInput and UpdateComboPackageInput to use `comboPrice` and `originalTotalPrice`.
- **Files modified:** `frontend/src/features/optical/api/optical-api.ts`
- **Verification:** TypeScript compilation passes with no errors in optical files
- **Committed in:** `39d4513` (Task 1 commit)

**2. [Rule 1 - Bug] Removed duplicate DeliveredOrderSummaryDto declaration**
- **Found during:** Task 1 (TypeScript compilation)
- **Issue:** optical-api.ts had two `DeliveredOrderSummaryDto` interface declarations with incompatible types (one with `string | null` from prior plan update, one with `string` from original version)
- **Fix:** Removed the duplicate older definition with `string` types
- **Files modified:** `frontend/src/features/optical/api/optical-api.ts`
- **Verification:** TypeScript TS2717 error resolved
- **Committed in:** `39d4513` (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (Rule 1 - Bug)
**Impact on plan:** Both fixes necessary for correctness. No scope creep.

## Issues Encountered
- Pre-existing TypeScript errors in unrelated files (patient-api.ts, admin-api.ts, auth-api.ts, api-client.ts) — out of scope, logged as deferred items

## Next Phase Readiness
- Combo package UI complete — staff can create/edit preset combos with savings display
- Route /optical/combos accessible for sidebar navigation wiring
- Form ready for integration with glasses order creation form (CreateGlassesOrderForm combo selection)

---
*Phase: 08-optical-center*
*Completed: 2026-03-08*
