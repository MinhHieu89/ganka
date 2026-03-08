---
phase: 08-optical-center
plan: 34
subsystem: frontend-optical-stocktaking
tags: [frontend, react, stocktaking, barcode-scanner, discrepancy-report, tanstack-router]
dependency_graph:
  requires: [08-25, 08-26]
  provides: [StocktakingPage, StocktakingScanner, DiscrepancyReport, optical/stocktaking route]
  affects: [optical-api.ts, optical-queries.ts]
tech_stack:
  added: []
  patterns:
    - Tab-based scanner toggle (USB / Camera) using shadcn/ui Tabs
    - Optimistic scan list with color-coded discrepancy badges
    - Three-mode page pattern (list / active-session / report) with local state router
    - AlertDialog confirmation before completing stocktaking session
    - Backend-preferred summary counts with client-side fallback
key_files:
  created:
    - frontend/src/features/optical/components/StocktakingScanner.tsx
    - frontend/src/features/optical/components/DiscrepancyReport.tsx
    - frontend/src/features/optical/components/StocktakingPage.tsx
    - frontend/src/app/routes/_authenticated/optical/stocktaking.tsx
  modified:
    - frontend/src/features/optical/api/optical-api.ts
decisions:
  - "StocktakingPage uses local state for mode (list/active/report) rather than URL params to avoid route complexity"
  - "DiscrepancyReport prefers backend-calculated counts (totalScanned, overCount, underCount, missingFromSystem) with client-side fallback"
  - "StocktakingScanner shows optimistic scan list with physicalCount only (systemCount=0 until session refresh)"
  - "StocktakingSessionDto extended to include startedByName, discrepancyCount, notes matching backend contract"
  - "DiscrepancyReportDto extended with totalScanned, totalDiscrepancies, overCount, underCount, missingFromSystem"
metrics:
  duration: "~20 minutes"
  completed: "2026-03-08"
  tasks_completed: 2
  tasks_total: 2
  files_created: 4
  files_modified: 1
---

# Phase 08 Plan 34: Stocktaking Frontend Summary

**One-liner:** Barcode-based stocktaking frontend with USB/camera scanner toggle, optimistic scan list, discrepancy categorization, and three-mode page (list/scan/report) at `/optical/stocktaking`.

## What Was Built

### Task 1: StocktakingScanner and DiscrepancyReport

**StocktakingScanner** (`frontend/src/features/optical/components/StocktakingScanner.tsx`):
- Tab toggle between "USB Scanner" (BarcodeScannerInput) and "Camera Scanner" (CameraScanner)
- Physical count number input with auto-focus after barcode scan
- Record button calls `useRecordStocktakingItem` mutation
- Running list of last 10 scanned items: Barcode, Frame Name, Physical, System, Discrepancy
- Discrepancy color-coded: green (match), yellow (over), red (under)

**DiscrepancyReport** (`frontend/src/features/optical/components/DiscrepancyReport.tsx`):
- Uses `useDiscrepancyReport(sessionId)` hook
- 5 summary cards: Total Scanned, Matches (green), Over Count (yellow), Under Count (red), Missing (gray)
- Sortable detail table: Barcode, Frame Name, Physical, System, Discrepancy (+/-), Category badge
- Color-coded rows by category
- Print report button (window.print())

### Task 2: StocktakingPage and Route

**StocktakingPage** (`frontend/src/features/optical/components/StocktakingPage.tsx`):
- Three modes via local state: "list", "active", "report"
- Session List mode:
  - Table with columns: Name, Status, Started By, Date, Items, Actions
  - Start New Stocktaking button (disabled when InProgress session exists)
  - InProgress session banner with Resume button
  - Dialog to enter session name
- Active Session mode:
  - Session header with item scanned count and discrepancy count
  - StocktakingScanner component
  - Complete button with AlertDialog confirmation
  - After completing: auto-navigates to DiscrepancyReport view
- Report mode: DiscrepancyReport for selected session

**Route** (`frontend/src/app/routes/_authenticated/optical/stocktaking.tsx`):
- `createFileRoute('/_authenticated/optical/stocktaking')` with StocktakingPage component
- Route accessible at `/optical/stocktaking`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed @/lib/utils import path in StocktakingScanner**
- **Found during:** Task 1 TypeScript compilation check
- **Issue:** Import `from "@/lib/utils"` doesn't exist; correct path is `@/shared/lib/utils`
- **Fix:** Updated import to `@/shared/lib/utils`
- **Files modified:** `frontend/src/features/optical/components/StocktakingScanner.tsx`
- **Commit:** `6f8a759`

**2. [Rule 2 - Missing] Extended StocktakingSessionDto to match backend contract**
- **Found during:** Task 2 implementation review against `StocktakingReportDto.cs`
- **Issue:** Frontend DTO missing `startedByName`, `discrepancyCount`, `notes` that backend provides
- **Fix:** Added missing fields to `StocktakingSessionDto` in optical-api.ts
- **Files modified:** `frontend/src/features/optical/api/optical-api.ts`

**3. [Rule 2 - Missing] Extended DiscrepancyReportDto to include backend summary counts**
- **Found during:** Task 1 DiscrepancyReport implementation review
- **Issue:** Frontend DTO only had `totalDiscrepancy` but backend provides `totalScanned`, `overCount`, `underCount`, `missingFromSystem`
- **Fix:** Updated `DiscrepancyReportDto` to include all backend-provided summary fields
- **Files modified:** `frontend/src/features/optical/api/optical-api.ts`

**4. [Rule 1 - Bug] StocktakingPage.tsx and stocktaking.tsx already committed by parallel agent**
- **Found during:** Task 2 commit attempt
- **Issue:** Plan 08-32 agent had already created and committed these files as part of the optical module setup
- **Resolution:** Files matched our implementation; no duplicate commit needed. Existing commits satisfy the plan requirements.

## Commits

| Hash | Description |
|------|-------------|
| `6f8a759` | feat(08-34): create StocktakingScanner and DiscrepancyReport components |
| `0211016` | StocktakingPage.tsx + stocktaking.tsx already committed by plan 08-32 agent |

## Self-Check

### Files Exist
- `frontend/src/features/optical/components/StocktakingScanner.tsx`: FOUND
- `frontend/src/features/optical/components/DiscrepancyReport.tsx`: FOUND
- `frontend/src/features/optical/components/StocktakingPage.tsx`: FOUND
- `frontend/src/app/routes/_authenticated/optical/stocktaking.tsx`: FOUND

### Route Registration
- `/optical/stocktaking` registered in `routeTree.gen.ts`: FOUND

### TypeScript Compilation
- 0 errors in optical files
- 60 pre-existing errors in admin/auth/patient/shared modules (unrelated)
