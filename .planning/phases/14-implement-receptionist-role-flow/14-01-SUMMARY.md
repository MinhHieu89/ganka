---
phase: 14-implement-receptionist-role-flow
plan: 01
subsystem: scheduling, clinical, patient, auth
tags: [domain-entities, enums, migrations, receptionist, role-seeding, frontend-auth]
dependency_graph:
  requires: []
  provides: [appointment-guest-booking, appointment-checkin, appointment-noshow, visit-source, visit-cancel-reason, patient-intake-fields, receptionist-role, frontend-roles]
  affects: [14-02, 14-03, 14-04, 14-05, 14-06, 14-07, 14-08, 14-09]
tech_stack:
  added: []
  patterns: [tdd-red-green-refactor, domain-events, nullable-aggregate-id, ef-migration]
key_files:
  created:
    - backend/src/Modules/Scheduling/Scheduling.Domain/Enums/AppointmentSource.cs
    - backend/src/Modules/Scheduling/Scheduling.Domain/Events/AppointmentCheckedInEvent.cs
    - backend/src/Modules/Scheduling/Scheduling.Domain/Events/AppointmentNoShowEvent.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/VisitSource.cs
    - backend/src/Modules/Patient/Patient.Domain/Enums/WorkEnvironment.cs
    - backend/src/Modules/Patient/Patient.Domain/Enums/ContactLensUsage.cs
    - backend/tests/Scheduling.Unit.Tests/Domain/AppointmentReceptionistTests.cs
    - backend/tests/Clinical.Unit.Tests/Domain/VisitReceptionistTests.cs
    - backend/tests/Patient.Unit.Tests/Domain/PatientIntakeTests.cs
  modified:
    - backend/src/Modules/Scheduling/Scheduling.Domain/Entities/Appointment.cs
    - backend/src/Modules/Scheduling/Scheduling.Domain/Enums/AppointmentStatus.cs
    - backend/src/Modules/Scheduling/Scheduling.Infrastructure/Configurations/AppointmentConfiguration.cs
    - backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/AppointmentDto.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs
    - backend/src/Modules/Patient/Patient.Domain/Entities/Patient.cs
    - backend/src/Modules/Patient/Patient.Infrastructure/Configurations/PatientConfiguration.cs
    - backend/src/Modules/Auth/Auth.Infrastructure/Seeding/AuthDataSeeder.cs
    - frontend/src/shared/stores/authStore.ts
    - frontend/src/features/auth/hooks/useAuth.ts
    - frontend/src/shared/lib/api-client.ts
    - frontend/src/app/routes/_authenticated.tsx
decisions:
  - "AppointmentDto.PatientId changed to Guid? (nullable) to support guest bookings - downstream callers already handle nullable"
  - "Receptionist role uses UpdatePermissions pattern consistent with other roles, 8 permissions total"
  - "Patient test namespace changed to Patient.Unit.Tests to avoid collision with Patient.Domain namespace"
metrics:
  duration: 16min
  completed: "2026-03-28T01:07:00Z"
  tasks_completed: 2
  tasks_total: 2
  files_changed: 31
---

# Phase 14 Plan 01: Domain Entities & Role Seeding Summary

Extended Appointment (guest bookings, check-in, no-show), Visit (source, cancel reason), Patient (9 intake fields) entities with TDD, seeded Receptionist role with 8 permissions, and added roles to frontend AuthUser.

## Tasks Completed

### Task 1: Extend Appointment, Visit, Patient entities + new enums + EF configs + migrations
- **Commit:** d0ca5e4
- **TDD:** RED phase verified (26 compilation errors), GREEN phase all 332 tests pass
- **Appointment:** Nullable PatientId, CreateGuest factory, CheckIn/MarkNoShow methods, AppointmentSource enum, NoShow status (=4), guest fields (GuestName/GuestPhone/GuestReason), CancelledBy audit
- **Visit:** VisitSource enum, Reason field, CancelWithReason method with audit trail, backward-compatible Cancel() preserved
- **Patient:** 9 intake fields (Email, Occupation, OcularHistory, SystemicHistory, CurrentMedications, ScreenTimeHours, WorkEnvironment, ContactLensUsage, LifestyleNotes), UpdateIntake method
- **New enums:** AppointmentSource (Staff/Phone/Web), VisitSource (Appointment/WalkIn), WorkEnvironment (Office/Outdoor/Factory/Other), ContactLensUsage (None/Daily/Occasional)
- **New events:** AppointmentCheckedInEvent, AppointmentNoShowEvent
- **EF configs:** All 3 modules updated with proper column types and max lengths
- **Migrations:** Created and applied for Patient, Scheduling, and Clinical modules
- **Tests:** 15 new unit tests across 3 test files

### Task 2: Seed Receptionist role + add roles to frontend AuthUser
- **Commit:** c2a9200
- **Backend:** Receptionist role seeded with 8 permissions (Patient.View/Create/Update, Scheduling.View/Create/Update, Clinical.View/Create)
- **Frontend:** AuthUser interface includes roles: string[], all 5 setAuth call sites updated, useHasRole() helper hook exported

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] AppointmentDto.PatientId nullable mismatch**
- **Found during:** Task 1
- **Issue:** Changing Appointment.PatientId to Guid? caused compilation errors in GetAppointmentsByDoctor and GetAppointmentsByPatient handlers that map to AppointmentDto with Guid PatientId
- **Fix:** Updated AppointmentDto.PatientId to Guid? in both property and constructor
- **Files modified:** backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/AppointmentDto.cs
- **Commit:** d0ca5e4

**2. [Rule 3 - Blocking] Patient test namespace collision**
- **Found during:** Task 1
- **Issue:** Patient.Unit.Tests.Domain namespace collided with Patient.Domain, causing CS0234 errors
- **Fix:** Changed test file namespace to Patient.Unit.Tests (matching project convention)
- **Files modified:** backend/tests/Patient.Unit.Tests/Domain/PatientIntakeTests.cs
- **Commit:** d0ca5e4

**3. [Rule 2 - Missing functionality] Roles not propagated in all setAuth callers**
- **Found during:** Task 2
- **Issue:** Plan only mentioned authStore.ts and auth-api.ts, but roles also needed in useAuth.ts (3 calls), api-client.ts (1 call), _authenticated.tsx (1 call)
- **Fix:** Updated all 5 setAuth call sites to include roles field
- **Files modified:** useAuth.ts, api-client.ts, _authenticated.tsx
- **Commit:** c2a9200

## Known Stubs

None - all data flows are wired end-to-end.

## Verification Results

- All 332 tests pass (23 Scheduling + 284 Clinical + 25 Patient)
- Backend builds with 0 errors
- Frontend TypeScript check passes (only pre-existing baseUrl deprecation warning)
- All 3 migrations created and applied successfully

## Self-Check: PASSED

- All 9 created files verified on disk
- Both commit hashes (d0ca5e4, c2a9200) found in git log
