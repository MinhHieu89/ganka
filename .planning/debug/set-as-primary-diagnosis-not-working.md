# "Set as Primary" Diagnosis Button Not Working

**Found during:** 03-16 Task 2 human verification
**Date:** 2026-03-09
**Severity:** HIGH
**Phase:** 03-clinical-workflow-examination

## Problem

Clicking the "Set as Primary" button on a diagnosis does nothing. The button appears but has no effect when clicked.

## Context

This was noted in the VERIFICATION.md anti-patterns table as:
> `DiagnosisSection.tsx` ~80: `handleSetPrimary` no-op stub — "Set Primary" button does nothing. Not in Phase 3 scope. Deferred.

This is a known no-op stub that was deferred. However, it is now surfacing as a user-facing issue during UAT.

## Files

- `frontend/src/features/clinical/components/DiagnosisSection.tsx` (handleSetPrimary function, ~line 80)

## Needs

1. Implement the `handleSetPrimary` handler to actually set a diagnosis as primary
2. May need a backend endpoint to update the primary diagnosis on a visit
3. Should update the UI to reflect the primary diagnosis state
