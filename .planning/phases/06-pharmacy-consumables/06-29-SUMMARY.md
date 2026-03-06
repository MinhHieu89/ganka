---
phase: 06-pharmacy-consumables
plan: 29
subsystem: ui
tags: [react, i18n, dto-mapping, pharmacy, tanstack-table]

# Dependency graph
requires:
  - phase: 06-pharmacy-consumables
    provides: DrugInventoryDto, DrugBatchTable, pharmacy page layout
provides:
  - Fixed DrugInventoryDto.drugCatalogItemId field matching backend JSON
  - Fully bilingual pharmacy inventory page with no hardcoded strings
affects: []

# Tech tracking
tech-stack:
  added: []
  patterns: [dto-field-alignment-with-backend, i18n-translation-completeness]

key-files:
  created: []
  modified:
    - frontend/src/features/pharmacy/api/pharmacy-api.ts
    - frontend/src/features/pharmacy/components/DrugInventoryTable.tsx
    - frontend/src/app/routes/_authenticated/pharmacy/index.tsx
    - frontend/public/locales/en/pharmacy.json
    - frontend/public/locales/vi/pharmacy.json

key-decisions:
  - "Renamed DrugInventoryDto.id to drugCatalogItemId to match backend camelCase JSON serialization"

patterns-established:
  - "DTO field names must match backend JSON serialization exactly (camelCase)"

requirements-completed: [PHR-01, PHR-02]

# Metrics
duration: 2min
completed: 2026-03-06
---

# Phase 06 Plan 29: UAT Gap Closure Summary

**Fixed batch expand by aligning DrugInventoryDto.id with backend drugCatalogItemId, and replaced 3 hardcoded Vietnamese strings with i18n translation keys**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-06T13:27:35Z
- **Completed:** 2026-03-06T13:29:55Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Fixed drug batch expand showing "No batches" by renaming DrugInventoryDto.id to drugCatalogItemId to match backend JSON field name
- Updated DrugInventoryTable to pass drug.drugCatalogItemId to both DrugBatchTable and updateDrugPricing mutation
- Replaced 3 hardcoded Vietnamese strings in pharmacy/index.tsx with t() translation calls
- Added inventory.subtitle, supplier.manageLink, stockImport.importLink keys to both EN and VI locale files

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix DrugInventoryDto field mismatch and update all references** - `e1193c2` (fix)
2. **Task 2: Replace hardcoded Vietnamese strings with i18n translation keys** - `318d82b` (fix)

## Files Created/Modified
- `frontend/src/features/pharmacy/api/pharmacy-api.ts` - Renamed DrugInventoryDto.id to drugCatalogItemId
- `frontend/src/features/pharmacy/components/DrugInventoryTable.tsx` - Updated drug.id references to drug.drugCatalogItemId
- `frontend/src/app/routes/_authenticated/pharmacy/index.tsx` - Replaced hardcoded Vietnamese with t() calls
- `frontend/public/locales/en/pharmacy.json` - Added subtitle, manageLink, importLink keys
- `frontend/public/locales/vi/pharmacy.json` - Added matching Vietnamese translation keys

## Decisions Made
- Renamed DrugInventoryDto.id to drugCatalogItemId to match backend camelCase JSON serialization (root cause of batch expand failure)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- UAT tests 17 (batch expand) and 24 (EN translations) should now pass
- Phase 6 gap closure complete

## Self-Check: PASSED

All 5 modified files verified present. Both task commits (e1193c2, 318d82b) verified in git log.

---
*Phase: 06-pharmacy-consumables*
*Completed: 2026-03-06*
