---
phase: 03-clinical-workflow-examination
plan: 14
subsystem: ui
tags: [react, i18n, refraction, decimal-input, validation, localization]

# Dependency graph
requires:
  - phase: 03-clinical-workflow-examination
    provides: RefractionForm component, server-validation utility, clinical locale files
provides:
  - Decimal-safe number input via local string state with onBlur coercion
  - Localized refraction validation error messages (Vietnamese and English)
affects: [clinical-workflow-examination]

# Tech tracking
tech-stack:
  added: []
  patterns: [local-string-state-for-decimal-input, server-error-to-i18n-mapping]

key-files:
  created: []
  modified:
    - frontend/src/features/clinical/components/RefractionForm.tsx
    - frontend/public/locales/vi/clinical.json
    - frontend/public/locales/en/clinical.json

key-decisions:
  - "Extracted NumberInput as standalone React component to enable useState/useEffect hooks for local string state"
  - "Used type=text with inputMode=decimal instead of type=number to preserve raw string values during typing"
  - "Post-process server errors after handleServerValidationError to replace English messages with t() translations"

patterns-established:
  - "Decimal input pattern: local string state + onBlur coercion for number inputs that need decimal precision"
  - "Server error localization: SERVER_MSG_TO_I18N mapping from server English strings to i18n translation keys"

requirements-completed: [REF-01, REF-02]

# Metrics
duration: 5min
completed: 2026-03-09
---

# Phase 03 Plan 14: Refraction Form Decimal Input Fix and Validation Localization Summary

**Decimal-safe number input via extracted NumberInput component with local string state, plus i18n-mapped server validation errors in Vietnamese and English**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-09T08:56:58Z
- **Completed:** 2026-03-09T09:02:21Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Fixed decimal input destruction: typing "1.5" or "0.25" now preserves the decimal point during typing, coercing to Number only on blur
- Extracted NumberInput as a proper React component with useState/useEffect for per-field local string state management
- Added 9 validation message i18n keys to both Vietnamese and English clinical.json locale files
- Server validation errors now display in the user's selected language via SERVER_MSG_TO_I18N mapping

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix decimal input by using local string state with onBlur coercion** - `32c60e9` (fix)
2. **Task 2: Add validation message i18n keys and map server errors to localized text** - `5bbc8d3` (feat)

## Files Created/Modified
- `frontend/src/features/clinical/components/RefractionForm.tsx` - Extracted NumberInput component with local string state, added SERVER_MSG_TO_I18N mapping and error localization in onError handler
- `frontend/public/locales/vi/clinical.json` - Added refraction.validation namespace with 9 Vietnamese validation messages
- `frontend/public/locales/en/clinical.json` - Added refraction.validation namespace with 9 English validation messages

## Decisions Made
- Extracted NumberInput as a standalone React component (option a from the plan) since renderNumberInput was a render function that could not use hooks
- Used `type="text"` with `inputMode="decimal"` instead of `type="number"` to get full control over raw string values during typing while keeping numeric keyboard on mobile
- Added regex validation `^-?\d*\.?\d*$` on onChange to filter non-numeric input since type="text" accepts any character
- Post-processed errors after handleServerValidationError rather than intercepting before, as the simpler approach that keeps the server-validation utility unchanged

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Refraction form decimal input and validation localization are complete
- Ready for UAT retest of Test 3 (decimal input + localized validation errors)

---
*Phase: 03-clinical-workflow-examination*
*Completed: 2026-03-09*

## Self-Check: PASSED
- All 4 files verified present
- All 2 task commits verified (32c60e9, 5bbc8d3)
