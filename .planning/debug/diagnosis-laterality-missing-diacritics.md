# Diagnosis Laterality Labels Missing Vietnamese Diacritics

**Found during:** 03-16 Task 2 human verification
**Date:** 2026-03-09
**Severity:** MEDIUM
**Phase:** 03-clinical-workflow-examination

## Problem

Selected diagnoses show laterality labels as "(mat phai)" and "(mat trai)" instead of the correct Vietnamese with diacritics: "(mat phai)" and "(mat trai)".

## Context

When a diagnosis with laterality is selected, the laterality label displayed alongside the diagnosis code lacks Vietnamese diacritics. The labels should be localized.

## Files to Investigate

- `frontend/src/features/clinical/components/DiagnosisSection.tsx` — where selected diagnoses are rendered
- `frontend/public/locales/vi/clinical.json` — Vietnamese translation file for clinical module

## Needs

1. Identify where laterality labels "(mat phai)" / "(mat trai)" are rendered
2. Replace hardcoded ASCII strings with i18n-translated strings using proper Vietnamese diacritics
3. The correct labels should be "(mat phai)" and "(mat trai)" with full diacritical marks
