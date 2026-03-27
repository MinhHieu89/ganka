---
phase: 14-implement-receptionist-role-flow
plan: 07
subsystem: frontend-receptionist
tags: [dialogs, check-in, action-menu, walk-in, reschedule, cancel, no-show]
dependency_graph:
  requires: [14-04, 14-05, 14-06]
  provides: [RowActionMenu, CheckInDialog, CheckInIncompleteDialog, WalkInVisitDialog, RescheduleDialog, CancelAppointmentDialog, NoShowDialog, CancelVisitDialog]
  affects: [PatientQueueTable, ReceptionistDashboard]
tech_stack:
  added: []
  patterns: [dialog-state-management, status-dependent-menus, rebook-navigation]
key_files:
  created:
    - frontend/src/features/receptionist/components/CheckInDialog.tsx
    - frontend/src/features/receptionist/components/CheckInIncompleteDialog.tsx
    - frontend/src/features/receptionist/components/WalkInVisitDialog.tsx
    - frontend/src/features/receptionist/components/RowActionMenu.tsx
    - frontend/src/features/receptionist/components/RescheduleDialog.tsx
    - frontend/src/features/receptionist/components/CancelAppointmentDialog.tsx
    - frontend/src/features/receptionist/components/NoShowDialog.tsx
    - frontend/src/features/receptionist/components/CancelVisitDialog.tsx
  modified:
    - frontend/src/features/receptionist/components/PatientQueueTable.tsx
    - frontend/src/features/receptionist/components/ReceptionistDashboard.tsx
decisions:
  - "Check-in dialog routing: incomplete patient detection uses isGuestBooking + missing birthYear/patientCode heuristic"
  - "RowActionMenu manages its own dialog state locally rather than lifting to dashboard"
  - "WalkInVisitDialog uses direct fetch for patient search since no shared search hook exists"
metrics:
  duration: 8min
  completed: "2026-03-28T19:30:00Z"
---

# Phase 14 Plan 07: Check-in/Walk-in Dialogs and Action Menu Summary

All check-in, walk-in, and action dialogs built with correct Vietnamese copy, color scheme per UI-SPEC, and API integration. Row action menu integrated into PatientQueueTable with status-dependent actions.

## What Was Built

### Task 1: Check-in Dialogs (1197955)

**CheckInDialog.tsx** - Complete patient check-in confirmation:
- Patient avatar with complete styling (purple bg #EEEDFE)
- Read-only info card showing name, code, birth year
- Blue info note with check-in instructions
- "Sua thong tin" button navigates to patient edit
- "Xac nhan check-in" calls useCheckInMutation with toast feedback
- Race condition handling with error toast

**CheckInIncompleteDialog.tsx** - Incomplete/guest patient redirect:
- Amber warning banner about incomplete profile
- Avatar with incomplete styling (amber bg #FAEEDA)
- "Check-in & bo sung ho so" navigates to /receptionist/intake with patientId or guest info
- Handles both existing incomplete patients and guest bookings

**WalkInVisitDialog.tsx** - Walk-in visit for existing patients:
- Patient search with inline results dropdown
- Selected patient info card with avatar
- Optional reason textarea
- Calls useCreateWalkInVisitMutation

### Task 2: Row Action Menu + Action Dialogs (ac5f0a5)

**RowActionMenu.tsx** - Status-dependent dropdown menu:
- not_arrived: Xem ho so, Sua thong tin, Doi lich hen (#534AB7), Danh dau khong den (#BA7517), Huy hen (#A32D2D)
- waiting: Xem ho so, Sua thong tin, Huy luot kham (#A32D2D)
- examining/completed: Xem ho so only
- Manages dialog state internally

**RescheduleDialog.tsx** - Reschedule appointment:
- Old schedule with strikethrough display
- Calendar date picker + TimeSlotGrid reuse from plan 14-06
- Uses useRescheduleAppointment from scheduling-api

**CancelAppointmentDialog.tsx** - Cancel appointment:
- Red warning banner about permanent deletion
- Required reason dropdown: BN yeu cau huy, BN doi phong kham, Le tan dat nham, Khac
- Optional note when "Khac" selected
- Red confirm button (#A32D2D)

**NoShowDialog.tsx** - Mark no-show:
- Amber styling throughout (#BA7517 confirm button)
- Optional note textarea with placeholder
- "Dat hen lai cho BN nay" checkbox
- Navigates to /appointments/new?patientId= when rebook checked (per D-15)

**CancelVisitDialog.tsx** - Cancel visit:
- 5 reason options including "BN khong muon cho, bo ve", "Le tan check-in nham nguoi"
- "Dat hen lai cho BN nay" checkbox with /appointments/new navigation (per D-15)
- Race condition handling for 409/conflict responses

**Integration:**
- PatientQueueTable updated: not_arrived rows show Check-in button + RowActionMenu, other rows show RowActionMenu only
- ReceptionistDashboard updated: routes check-in clicks to correct dialog (complete vs incomplete)

## Deviations from Plan

None -- plan executed exactly as written.

## Known Stubs

None -- all dialogs are fully wired to API mutations and navigation.

## Self-Check: PASSED

- All 8 component files verified present
- Commits 1197955 and ac5f0a5 verified in git log
