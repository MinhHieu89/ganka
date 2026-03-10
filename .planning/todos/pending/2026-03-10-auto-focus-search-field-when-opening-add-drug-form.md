---
created: 2026-03-10T09:12:00.000Z
title: Auto focus search field when opening Add Drug form
area: ui
files:
  - frontend/src/features/clinical/components/DrugPrescriptionSection.tsx
---

## Problem

When clicking the "Add Drug" input in the drug prescription form, the search field in the DrugCombobox does not auto-focus. User has to click again to start typing, which slows down the workflow.

## Solution

Auto-focus the search input field when the DrugCombobox dropdown opens, so the doctor can immediately start typing to search for a drug.
