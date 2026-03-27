---
phase: 14-implement-receptionist-role-flow
plan: "06"
subsystem: frontend-receptionist
tags: [booking, appointment, receptionist, frontend]
dependency_graph:
  requires: [14-03, 14-04]
  provides: [booking-page, time-slot-grid, confirmation-bar]
  affects: [receptionist-dashboard]
tech_stack:
  added: []
  patterns: [command-autocomplete, calendar-date-picker, time-slot-grid]
key_files:
  created:
    - frontend/src/features/receptionist/components/booking/TimeSlotGrid.tsx
    - frontend/src/features/receptionist/components/booking/ConfirmationBar.tsx
    - frontend/src/features/receptionist/components/booking/NewAppointmentPage.tsx
    - frontend/src/app/routes/_authenticated/appointments/new.tsx
  modified: []
decisions:
  - Used Command+Popover pattern for patient autocomplete search
  - Used NO_DOCTOR_VALUE sentinel for "BS nao trong" select option
  - Guest booking uses useBookGuestMutation, existing patient uses useBookAppointment
metrics:
  duration: 5min
  completed: "2026-03-27T19:20:00Z"
---

# Phase 14 Plan 06: New Appointment Booking Page Summary

Complete booking page at /appointments/new with 2-column layout, patient search/guest entry, calendar, time slot grid, and confirmation bar for receptionist workflow.

## What Was Built

### TimeSlotGrid Component
- Splits slots into "Sang" (morning) and "Chieu" (afternoon) groups
- Three visual states: available (white/border), selected (purple #EEEDFE/#534AB7), full (gray/line-through)
- Shows availability count per group and total
- Legend at bottom showing slot states
- Skeleton loading state

### ConfirmationBar Component
- Sticky bottom card with purple (#EEEDFE) background
- Shows patient name, formatted date/time, reason, doctor
- Cancel (outline) and confirm (filled purple) buttons
- Loading spinner on confirm when submitting

### NewAppointmentPage
- 2-column layout: left 40% (patient info), right 60% (calendar + slots)
- Patient search via Command autocomplete (usePatientSearch hook)
- Found patient: green info bar with name + code
- Not found: yellow warning bar + guest input fields (name, phone)
- Blue info note for new patients about minimal data needed
- Doctor selector with "BS nao trong" (any available) option
- shadcn Calendar with past date disabling, 3-month max
- TimeSlotGrid loads via useAvailableSlots(date, doctorId)
- Confirmation bar appears when date + slot selected
- Submit: existing patient via useBookAppointment, guest via useBookGuestMutation
- Success toast and navigate to /dashboard
- Pre-fill support via ?patientId search param for rebook flow

### Route File
- `/appointments/new` with `Scheduling.Create` permission guard
- Accepts optional `patientId` search param

## Deviations from Plan

None - plan executed exactly as written.

## Known Stubs

None - all data sources are wired to real API hooks (useAvailableSlots, useBookGuestMutation, useBookAppointment, usePatientSearch, useDoctors).

## Self-Check: PASSED

All 4 files verified present. Both commit hashes (38e6134, a17d3ea) verified in git log.
