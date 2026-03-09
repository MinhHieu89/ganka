# VA Value Decimal Part Lost on Page Refresh

**Found during:** 03-16 Task 2 human verification
**Date:** 2026-03-09
**Severity:** MEDIUM
**Phase:** 03-clinical-workflow-examination

## Problem

Entering VA value "5.0" works correctly during typing (the decimal is preserved). However, after page refresh, the value displays as "5" (the ".0" decimal part is lost).

## Context

Plan 03-14 fixed the decimal input component (NumberInput) to preserve decimal points during typing using local string state. The fix works for typing, but the issue is that when the value is loaded back from the server/database, the numeric value `5.0` is displayed as `5` because JavaScript `Number(5.0)` equals `5` and `.toString()` produces "5".

## Possible Causes

1. The NumberInput component initializes `localValue` from the form value using `String(value)` which drops trailing `.0`
2. The backend may store 5.0 as integer 5 in the database
3. The display formatting does not force decimal places for VA values

## Files

- `frontend/src/features/clinical/components/RefractionForm.tsx` (NumberInput component, lines 115-188)

## Needs

NumberInput should format loaded values with appropriate decimal places, or VA fields should use specific formatting (e.g., `toFixed(1)` for VA values).
