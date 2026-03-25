---
phase: 13-clinical-workflow-overhaul
plan: 16
subsystem: ui
tags: [react, clinical-workflow, pharmacy, optical-center, stage-views]

requires:
  - phase: 13-13
    provides: StageDetailShell and StageBottomBar components
  - phase: 13-14
    provides: Drug and optical prescription sections and APIs
provides:
  - Stage 7a Pharmacy dispensing view with medication checklist
  - Stage 7b Optical Center frame/lens selection view with price breakdown
  - Route mappings for stage 7 (Pharmacy) and stage 8 (OpticalCenter)
affects: [13-17, 13-18, cashier-integration]

tech-stack:
  added: []
  patterns: [stub-mutation-hooks-for-frontend-first, checklist-toggle-pattern]

key-files:
  created:
    - frontend/src/features/clinical/components/stage-views/Stage7aPharmacyView.tsx
    - frontend/src/features/clinical/components/stage-views/Stage7bOpticalCenterView.tsx
  modified:
    - frontend/src/app/routes/_authenticated/clinical/visit.$visitId.stage.tsx

key-decisions:
  - "Stub mutation hooks used (useDispensePharmacy, useConfirmOpticalOrder) since backend endpoints not yet built"
  - "No Stage8GlassesPaymentView created - single combined payment at Cashier per spec"
  - "Lens catalog hardcoded as LENS_OPTIONS constant; will be replaced with API-driven catalog"

patterns-established:
  - "Stub mutation hook pattern: useState + useCallback simulating async for frontend-first development"
  - "Checklist toggle pattern with Set<string> for tracked checked state"

requirements-completed: [CLN-03]

duration: 6min
completed: 2026-03-25
---

# Phase 13 Plan 16: Pharmacy & Optical Center Stage Views Summary

**Pharmacy dispensing checklist with per-row toggle validation and optical center frame/lens selection with Intl.NumberFormat price breakdown, routing to single Cashier payment**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-25T13:07:10Z
- **Completed:** 2026-03-25T13:13:23Z
- **Tasks:** 2/2
- **Files modified:** 3

## Accomplishments

### Task 1: Stage 7a Pharmacy Dispensing View
- Medication checklist with per-row checkboxes (IconSquare/IconSquareCheck toggle)
- Green highlight (bg-green-50) on checked rows
- All-checked validation: forward button disabled until every item checked
- Post-completion state: green banner with pharmacist name + timestamp, print label button
- Optional dispensing note textarea
- Stub useDispensePharmacy mutation hook

### Task 2: Stage 7b Optical Center View
- Read-only glasses prescription card showing SPH/CYL/AXIS/ADD per eye + PD
- Lens type dropdown with 6 catalog options (Essilor, Hoya, Zeiss variants)
- Frame code free-text input and frame cost entry
- Price breakdown: lens cost x2 + frame cost = total with Intl.NumberFormat('vi-VN') formatting
- Confirms and routes to Cashier for single combined payment (no separate CashierGlasses stage)
- Stub useConfirmOpticalOrder mutation hook

## Deviations from Plan

None - plan executed exactly as written.

## Known Stubs

| File | Location | Stub | Reason | Resolution Plan |
|------|----------|------|--------|-----------------|
| Stage7aPharmacyView.tsx | useDispensePharmacy function | Simulated async with setTimeout | Backend pharmacy dispensing endpoint not yet built | Plan 13-17 or backend implementation |
| Stage7bOpticalCenterView.tsx | useConfirmOpticalOrder function | Simulated async with setTimeout | Backend optical order confirmation endpoint not yet built | Plan 13-17 or backend implementation |
| Stage7bOpticalCenterView.tsx | LENS_OPTIONS constant | Hardcoded lens catalog | Lens catalog API not yet available | Optical center inventory module |

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | dce8331 | Stage 7a Pharmacy dispensing view with medication checklist |
| 2 | 206d3ed | Stage 7b Optical Center view with frame/lens selection |
