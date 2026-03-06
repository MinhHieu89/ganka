---
phase: 06-pharmacy-consumables
plan: 22
subsystem: pharmacy-frontend
tags: [dispensing, queue, fefo, patient-profile, react, tanstack-query]
dependency_graph:
  requires:
    - 06-19 (pharmacy-queries with usePendingPrescriptions, useDispenseDrugs hooks)
  provides:
    - PharmacyQueueTable (pending prescriptions table, patientId filter)
    - DispensingDialog (FEFO batch preview, 7-day expiry override)
    - Pharmacy queue page route (/pharmacy/queue)
    - PatientPrescriptionsTab (prescriptions + history per patient)
    - Updated PatientProfilePage (Prescriptions tab)
  affects:
    - Patient profile page (new tab added)
    - Pharmacy navigation (queue route available)
tech_stack:
  added: []
  patterns:
    - TanStack Table for queue display with row-click to dialog
    - FEFO batch suggestion via useDrugBatches per line item
    - Collapsible dispensing history for patient profile
    - 7-day expiry warning with required override reason textarea
key_files:
  created:
    - frontend/src/features/pharmacy/components/PharmacyQueueTable.tsx
    - frontend/src/features/pharmacy/components/DispensingDialog.tsx
    - frontend/src/app/routes/_authenticated/pharmacy/queue.tsx
    - frontend/src/features/pharmacy/components/PatientPrescriptionsTab.tsx
  modified:
    - frontend/src/features/patient/components/PatientProfilePage.tsx
    - frontend/src/features/pharmacy/api/pharmacy-api.ts
    - frontend/src/features/pharmacy/api/pharmacy-queries.ts
    - frontend/public/locales/en/pharmacy.json
    - frontend/public/locales/vi/pharmacy.json
decisions:
  - FEFO batch suggestion computed client-side from useDrugBatches (sorted by expiryDate ascending, allocate earliest first)
  - Off-catalog prescription items auto-skip (no stock deduction, no batch display)
  - Dispensing history in PatientPrescriptionsTab is collapsible to save vertical space
  - usePendingPrescriptions gets refetchInterval 30s (moved from usePendingCount to queue table hook)
  - patientId filtering done client-side in PharmacyQueueTable (API supports server-side but client filter avoids extra network calls)
metrics:
  duration: "~7 minutes"
  completed_date: "2026-03-06"
  tasks_completed: 2
  tasks_total: 2
  files_created: 4
  files_modified: 5
---

# Phase 06 Plan 22: Pharmacy Dispensing Queue & Patient Prescriptions Tab Summary

**One-liner:** Pharmacy dispensing queue with FEFO batch preview, 7-day expiry override dialog, and patient profile Prescriptions tab for cross-module dispensing access.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Pharmacy queue table and dispensing dialog | 5f7f500 | PharmacyQueueTable.tsx, DispensingDialog.tsx, pharmacy-api.ts, pharmacy-queries.ts, en/vi pharmacy.json |
| 2 | Queue route and patient profile prescriptions tab | d0f3035 | queue.tsx, PatientPrescriptionsTab.tsx, PatientProfilePage.tsx, routeTree.gen.ts |

## What Was Built

### PharmacyQueueTable.tsx
- DataTable showing pending prescriptions from `usePendingPrescriptions()` hook
- Accepts optional `patientId?: string` prop to filter for a specific patient
- Columns: Patient Name, Prescription Code, Prescribed At, Item Count, Days Remaining, Status
- Sorted by `prescribedAt` ascending (oldest first = most urgent)
- Expired rows highlighted with destructive badge and red background
- Days ≤ 2 rows highlighted with yellow warning badge
- Row click opens DispensingDialog
- Client-side filter when `patientId` prop provided

### DispensingDialog.tsx
- Sheet-style Dialog with prescription header (patient name, code, prescribed date)
- **7-day expiry warning**: destructive Alert banner with required Override Reason textarea when `isExpired=true`
- Dispense button disabled until override reason provided for expired prescriptions
- Per-line controls: Dispense / Skip toggle for each catalog drug
- Off-catalog items auto-marked as skipped (shows "Auto-skipped" badge, no toggle)
- FEFO batch suggestion per catalog line using `useDrugBatches` — displays earliest-expiry batch allocations as badges
- "All skipped" warning if user marks every item as skipped
- Confirm Dispense calls `useDispenseDrugs` mutation with proper `DispenseDrugsInput` shape
- Success: toast + close + cache invalidation via mutation

### queue.tsx (route)
- Page title: "Dispensing Queue" / "Hàng đợi pha chế"
- Count badge at top showing total pending prescriptions
- Auto-refresh note (30s via `usePendingPrescriptions` refetchInterval)
- Uses PharmacyQueueTable without patientId filter (all patients)
- TanStack Router `createFileRoute` for `/_authenticated/pharmacy/queue`

### PatientPrescriptionsTab.tsx
- Accepts `patientId: string` prop
- "Pending Prescriptions" section with PharmacyQueueTable filtered to patient
- "View full queue" link to `/pharmacy/queue`
- Collapsible "Dispensing History" section using `useDispensingHistory(1, patientId)`
- History table: dispensed date, dispensed by, line count

### PatientProfilePage.tsx (updated)
- Added "Prescriptions" tab trigger after "appointments" tab, before "dry-eye" tab
- Renders `PatientPrescriptionsTab` with `patientId={patient.id}`
- Tab label from pharmacy i18n: `tPharmacy("queue.prescriptions.tab")`

## API Updates

### pharmacy-api.ts
- `PendingPrescriptionItemDto`: added `dosage: string | null` and `isOffCatalog: boolean` fields
- `DispenseDrugsInput`: updated to match backend `DispenseDrugsCommand` shape (added `prescribedAt`, replaced `batchAllocations` with `skip`, `isOffCatalog`, `manualBatches`)
- `BatchOverride` type replacing old `BatchAllocationInput`
- `getDispensingHistory`: added `patientId` filter support, returns `DispensingHistoryResult` (paged)
- `getPendingPrescriptions`: added optional `patientId` parameter

### pharmacy-queries.ts
- `usePendingPrescriptions`: added `refetchInterval: 30_000` for auto-refresh
- `useDispensingHistory`: added `patientId?: string | null` parameter

## Deviations from Plan

### Auto-fixed Issues

None — plan executed as specified.

### Design Decisions

1. **FEFO batch suggestion computed client-side**: Used `useDrugBatches` per line item to show FEFO allocation preview. This fires individual requests per drug but avoids a backend endpoint change. Backend does automatic FEFO on actual dispense.

2. **patientId filtering client-side**: `PharmacyQueueTable` filters the already-fetched pending prescriptions client-side when `patientId` prop is provided, avoiding extra API calls for the patient profile view.

3. **30s refetchInterval moved to usePendingPrescriptions**: Queue page auto-refreshes every 30 seconds via the shared hook.

## Self-Check: PASSED

All created files verified present. Both task commits verified in git history.

| Item | Status |
|------|--------|
| PharmacyQueueTable.tsx | FOUND |
| DispensingDialog.tsx | FOUND |
| queue.tsx | FOUND |
| PatientPrescriptionsTab.tsx | FOUND |
| 06-22-SUMMARY.md | FOUND |
| Commit 5f7f500 (Task 1) | FOUND |
| Commit d0f3035 (Task 2) | FOUND |
| TypeScript compile (pharmacy files) | PASSED (0 errors in pharmacy files) |
