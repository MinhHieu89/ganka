---
created: 2026-03-10T09:20:00.000Z
title: Print all pharmacy labels at once
area: ui
files:
  - frontend/src/features/clinical/components/DrugPrescriptionSection.tsx
  - frontend/src/features/clinical/api/document-api.ts
---

## Problem

Currently, users can only print pharmacy labels for each drug individually by clicking the label icon next to each prescription item. When a prescription has multiple drugs, this is tedious — the doctor has to click print for each one separately.

## Solution

Add a "Print All Labels" button at the prescription level (next to the existing "Print Rx" button) that generates all pharmacy labels for all drugs in the prescription in a single PDF/print action.
