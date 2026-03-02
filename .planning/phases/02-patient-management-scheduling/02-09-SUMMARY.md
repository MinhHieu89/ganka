---
phase: 02-patient-management-scheduling
plan: 09
subsystem: ui
tags: [shadcn-ui, combobox, i18n, vietnamese, allergy-autocomplete, free-text]

# Dependency graph
requires:
  - phase: 02-patient-management-scheduling
    provides: "AllergyForm component and ALLERGY_CATALOG_BILINGUAL catalog data"
provides:
  - "Rewritten allergy autocomplete with shouldFilter={false} and free-text support"
  - "Localized allergy category labels (en/vi) via i18n keys"
  - "Vietnamese diacritics on all ALLERGY_CATALOG_BILINGUAL entries"
affects: [patient-management, clinical]

# Tech tracking
tech-stack:
  added: []
  patterns: [shouldFilter-false-combobox, category-grouped-command-items, free-text-custom-item]

key-files:
  created: []
  modified:
    - frontend/src/features/patient/components/AllergyForm.tsx
    - frontend/src/features/patient/api/patient-api.ts
    - frontend/public/locales/en/patient.json
    - frontend/public/locales/vi/patient.json

key-decisions:
  - "Input wrapped in div inside PopoverTrigger to avoid click-to-toggle anti-pattern while keeping Radix positioning"
  - "shouldFilter={false} on Command to prevent cmdk internal filtering conflict with external filtering"
  - "categoryKeyMap maps English category strings to i18n keys for runtime translation"

patterns-established:
  - "shouldFilter={false} combobox: Disable cmdk filtering when doing custom external filtering on catalog items"
  - "Free-text CommandItem: Show dynamic 'Add custom: {value}' option when no exact match in catalog"
  - "Category-grouped CommandGroup: Group items by translated category heading using reduce + Object.entries"

requirements-completed: [PAT-03, PAT-04]

# Metrics
duration: 3min
completed: 2026-03-02
---

# Phase 02 Plan 09: Allergy Autocomplete Rewrite Summary

**Proper shadcn/ui combobox with shouldFilter={false}, free-text "Add custom" affordance, and Vietnamese category translations**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-02T12:06:28Z
- **Completed:** 2026-03-02T12:09:33Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Eliminated Input-as-PopoverTrigger anti-pattern by wrapping Input in a plain div
- Added free-text entry via dynamic "Add custom: {value}" CommandItem that appears when typed text has no exact catalog match
- Localized allergy category labels using i18n keys -- Vietnamese categories display with proper diacritics
- Fixed 6 Vietnamese allergy names in ALLERGY_CATALOG_BILINGUAL to use proper diacritics

## Task Commits

Each task was committed atomically:

1. **Task 1: Add allergyCategory i18n keys to both locale files** - `549d373` (feat)
2. **Task 2: Rewrite AllergyForm autocomplete with free-text and localized categories** - `e7f2709` (feat)

## Files Created/Modified
- `frontend/public/locales/en/patient.json` - Added allergyCategory section with 5 English category labels
- `frontend/public/locales/vi/patient.json` - Added allergyCategory section with 5 Vietnamese category labels (proper diacritics)
- `frontend/src/features/patient/components/AllergyForm.tsx` - Rewrote allergy name autocomplete with shouldFilter={false}, div wrapper, free-text support, category grouping
- `frontend/src/features/patient/api/patient-api.ts` - Fixed Vietnamese diacritics on 6 ALLERGY_CATALOG_BILINGUAL entries

## Decisions Made
- Input wrapped in div inside PopoverTrigger to avoid click-to-toggle anti-pattern while keeping Radix popover positioning intact
- shouldFilter={false} on Command to prevent cmdk internal filtering from conflicting with our external filter logic
- categoryKeyMap maps English category strings to i18n keys at component level for runtime translation lookup

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Allergy autocomplete is ready for UAT re-test (Test 7)
- Category translations extensible for future allergy categories

## Self-Check: PASSED

All 4 modified files exist on disk. Both task commits (549d373, e7f2709) verified in git log.

---
*Phase: 02-patient-management-scheduling*
*Completed: 2026-03-02*
