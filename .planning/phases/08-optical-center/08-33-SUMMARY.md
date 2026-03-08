---
phase: 08-optical-center
plan: 33
subsystem: frontend-optical
tags: [prescription-history, comparison, patient-profile, optical, react, tanstack-query]
dependency_graph:
  requires: [08-25]
  provides: [PrescriptionHistoryTab, PrescriptionComparisonView, PatientProfilePage-optical-tab]
  affects: [patient-profile-page]
tech_stack:
  added: []
  patterns:
    - TanStack Query hooks via usePatientPrescriptionHistory / usePrescriptionComparison
    - shadcn/ui Card, Badge, Checkbox, Button, Table components
    - "@tabler/icons-react for change direction icons (IconArrowUp, IconArrowDown, IconEqual)"
    - Standard optical notation formatting (SPH / CYL x AXIS)
    - Change direction logic (SPH/CYL closer-to-zero = improved)
key_files:
  created:
    - frontend/src/features/optical/components/PrescriptionHistoryTab.tsx
    - frontend/src/features/optical/components/PrescriptionComparisonView.tsx
  modified:
    - frontend/src/features/patient/components/PatientProfilePage.tsx
    - frontend/public/locales/en/optical.json
    - frontend/public/locales/vi/optical.json
decisions:
  - "Used optical-queries.ts hooks (usePatientPrescriptionHistory, usePrescriptionComparison) which already existed"
  - "Added prescriptionHistory.tab i18n key to both en and vi optical locales"
  - "AXIS and ADD fields use neutral change direction (not improved/worsened) since they are not clinically directional"
  - "Tab value set to optical-history to avoid conflicts with prescriptions tab"
metrics:
  duration_minutes: 15
  completed_date: "2026-03-08"
  tasks_completed: 3
  files_created: 2
  files_modified: 3
---

# Phase 8 Plan 33: Prescription History Tab & Comparison View Summary

**One-liner:** Lens prescription history timeline with year-over-year comparison using closer-to-zero SPH/CYL improvement logic wired as a new tab in patient profile.

## Tasks Completed

| # | Task | Commit | Files |
|---|------|--------|-------|
| 1 | Create PrescriptionHistoryTab | 7fe117b | PrescriptionHistoryTab.tsx, en/vi optical.json |
| 2 | Create PrescriptionComparisonView | 39d4513 | PrescriptionComparisonView.tsx |
| 3 | Wire tab into PatientProfilePage | 001146b | PatientProfilePage.tsx |

## What Was Built

### PrescriptionHistoryTab.tsx
- Props: `{ patientId: string }`
- Uses `usePatientPrescriptionHistory(patientId)` hook from `optical-queries.ts`
- Timeline view showing prescriptions in reverse chronological order (newest first)
- Each Card shows: OD values (SPH, CYL, AXIS, ADD), OS values, PD, Notes
- Values formatted in standard optical notation (e.g., "-2.00 / -0.75 x 180°")
- Checkboxes to select two prescriptions for comparison (max 2 selected)
- When two selected, `PrescriptionComparisonView` appears below the timeline
- Empty state with `IconEye` when no history exists
- "Compare" hint bar with clear button

### PrescriptionComparisonView.tsx
- Props: `{ patientId: string, prescriptionId1: string, prescriptionId2: string }`
- Uses `usePrescriptionComparison({ patientId, id1, id2 })` hook
- Side-by-side table with older prescription on left, newer on right
- Rows: SPH OD, CYL OD, AXIS OD, ADD OD, SPH OS, CYL OS, AXIS OS, ADD OS, PD
- Change indicators per row:
  - `IconArrowUp` (green): improvement (SPH/CYL closer to 0)
  - `IconArrowDown` (red): worsened (SPH/CYL further from 0)
  - `IconEqual` (gray): unchanged
- Summary badges at the top highlighting changed fields
- Loading skeleton and empty state handled

### PatientProfilePage.tsx (updated)
- Added import for `PrescriptionHistoryTab`
- Added `tOptical` translation hook for `optical` namespace
- New `TabsTrigger value="optical-history"` with label from `tOptical("prescriptionHistory.tab")`
- New `TabsContent value="optical-history"` rendering `<PrescriptionHistoryTab patientId={patient.id} />`
- Tab appears after the existing "dry-eye" tab

### i18n Updates
- Added `prescriptionHistory.tab` key in both `en/optical.json` ("Optical Rx History") and `vi/optical.json` ("Lịch sử toa kính")

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check: PASSED

- [x] `frontend/src/features/optical/components/PrescriptionHistoryTab.tsx` - FOUND
- [x] `frontend/src/features/optical/components/PrescriptionComparisonView.tsx` - FOUND
- [x] `frontend/src/features/patient/components/PatientProfilePage.tsx` - FOUND (with optical tab)
- [x] TypeScript compiler: no errors in created/modified files
- [x] Commits 7fe117b, 39d4513, 001146b exist
