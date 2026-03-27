---
phase: 14-implement-receptionist-role-flow
plan: 03
subsystem: scheduling, clinical, patient
tags: [api-endpoints, minimal-api, authorization, receptionist]
dependency_graph:
  requires: [14-01, 14-02]
  provides: [receptionist-api-endpoints, scheduling-receptionist-routes, clinical-walkin-route, patient-intake-route]
  affects: [14-04, 14-05, 14-06, 14-07, 14-08, 14-09]
tech_stack:
  added: []
  patterns: [minimal-api-route-groups, permission-based-auth, httpcontext-userid-extraction]
key_files:
  created:
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/CheckInAppointment.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/BookGuestAppointment.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/MarkAppointmentNoShow.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAvailableSlots.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/GetReceptionistDashboard.cs
    - backend/src/Modules/Scheduling/Scheduling.Application/Features/GetReceptionistKpiStats.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/CreateWalkInVisit.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/CancelVisitWithReason.cs
    - backend/src/Modules/Patient/Patient.Application/Features/RegisterPatientFromIntake.cs
    - backend/src/Modules/Patient/Patient.Application/Features/UpdatePatientFromIntake.cs
    - backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/AvailableSlotDto.cs
    - backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/ReceptionistDashboardRowDto.cs
    - backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/ReceptionistKpiDto.cs
    - backend/src/Modules/Scheduling/Scheduling.Domain/Enums/AppointmentSource.cs
  modified:
    - backend/src/Modules/Scheduling/Scheduling.Presentation/SchedulingApiEndpoints.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
    - backend/src/Modules/Patient/Patient.Presentation/PatientApiEndpoints.cs
decisions:
  - "Created /api/scheduling route group for receptionist endpoints separate from /api/appointments to avoid route conflicts"
  - "Command/query record stubs created for parallel plan 14-02 types - will be overwritten with full handler implementations on merge"
  - "Request DTOs (CheckInAppointmentRequest, MarkNoShowRequest, CancelVisitWithReasonRequest) extract UserId from HttpContext, keeping commands clean"
metrics:
  duration: 5min
  completed: "2026-03-27T18:18:00Z"
  tasks_completed: 1
  tasks_total: 1
  files_changed: 17
---

# Phase 14 Plan 03: API Endpoints Summary

Wired all 10 receptionist HTTP endpoints across 3 modules with permission-based authorization, using separate /api/scheduling route group for new receptionist routes.

## Tasks Completed

### Task 1: Add all receptionist API endpoints to Presentation layers
- **Commit:** 4c7b221
- **Scheduling (6 endpoints):**
  - POST /api/scheduling/appointments/check-in (Scheduling.Update)
  - POST /api/scheduling/appointments/guest (Scheduling.Create)
  - POST /api/scheduling/appointments/{id}/no-show (Scheduling.Update)
  - GET /api/scheduling/slots (Scheduling.View)
  - GET /api/scheduling/receptionist/dashboard (Scheduling.View)
  - GET /api/scheduling/receptionist/kpi (Scheduling.View)
- **Clinical (2 endpoints):**
  - POST /api/clinical/visits/walk-in (Clinical.Create)
  - POST /api/clinical/visits/{id}/cancel-with-reason (Clinical.Update)
- **Patient (2 endpoints):**
  - POST /api/patients/intake (Patient.Create)
  - PUT /api/patients/{id}/intake (Patient.Update)
- All endpoints use RequirePermissions with correct permission constants
- UserId extracted from HttpContext.User via TryGetUserId() for audit trail

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Missing command/query types from parallel plan 14-02**
- **Found during:** Task 1
- **Issue:** Plan 14-02 (handlers) runs in parallel but creates the command/query record types that endpoints dispatch. Types did not exist, causing 13 build errors.
- **Fix:** Created stub command/query records in Application/Features and contract DTOs in Contracts/Dtos. These stubs will be overwritten by plan 14-02's full handler implementations on merge.
- **Files created:** 10 stub command/query files + 3 contract DTO files + 1 enum file
- **Commit:** 4c7b221

**2. [Rule 3 - Blocking] Missing AppointmentSource enum from plan 14-01**
- **Found during:** Task 1
- **Issue:** BookGuestAppointmentCommand references AppointmentSource enum which was created by plan 14-01 in a different worktree.
- **Fix:** Created AppointmentSource enum (Staff/Phone/Web) matching 14-01 spec.
- **Files created:** backend/src/Modules/Scheduling/Scheduling.Domain/Enums/AppointmentSource.cs
- **Commit:** 4c7b221

**3. [Rule 2 - Missing] Plan referenced incorrect file names**
- **Found during:** Task 1
- **Issue:** Plan referenced SchedulingEndpoints.cs, ClinicalEndpoints.cs, PatientEndpoints.cs but actual files are SchedulingApiEndpoints.cs, ClinicalApiEndpoints.cs, PatientApiEndpoints.cs.
- **Fix:** Used correct existing file names.
- **Commit:** 4c7b221

## Known Stubs

- Command/query record stubs in Application/Features files contain only the record declaration (no handler). Plan 14-02 will replace these with full handler implementations including FluentValidation and Wolverine handlers.
- This is intentional: stubs exist only to enable compilation in parallel execution. They do NOT prevent the plan's goal (endpoint wiring) from being achieved.

## Verification Results

- Build succeeds with 0 errors, 2 pre-existing warnings
- All 10 endpoints registered with correct routes and authorization policies
- Acceptance criteria verified: all endpoint strings present in respective files

## Self-Check: PASSED

- All 14 created files verified on disk
- Commit hash 4c7b221 found in git log
