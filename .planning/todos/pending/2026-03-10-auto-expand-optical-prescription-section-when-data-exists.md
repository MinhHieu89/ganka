---
created: 2026-03-10T09:38:00.000Z
title: Auto expand Optical Prescription section when data exists
area: ui
files:
  - frontend/src/features/clinical/components/OpticalPrescriptionSection.tsx
---

## Problem

The Optical Prescription section in visit detail page is always collapsed by default (`defaultOpen={false}`). When a visit already has an optical prescription saved, the user has to manually expand the section to see it, which is inconvenient.

## Solution

Set `defaultOpen` based on whether `prescriptions.length > 0`. If the visit has existing optical prescriptions, the section should be expanded by default. Only collapse by default when there are no prescriptions.
