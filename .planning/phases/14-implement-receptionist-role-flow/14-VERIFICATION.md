---
phase: 14-implement-receptionist-role-flow
verified: 2026-03-28T00:00:00Z
status: gaps_found
score: 5/7 must-haves verified
gaps:
  - truth: "All flows navigate correctly between dashboard, intake form, and booking page"
    status: failed
    reason: "ReceptionistDashboard 'Tiep nhan BN moi' button navigates to /receptionist/intake which has no registered route. Actual route is /patients/intake. Same broken path used in CheckInIncompleteDialog for guest and existing incomplete-profile patients."
    artifacts:
      - path: "frontend/src/features/receptionist/components/ReceptionistDashboard.tsx"
        issue: "Line 110: navigates to /receptionist/intake — route does not exist"
      - path: "frontend/src/features/receptionist/components/CheckInIncompleteDialog.tsx"
        issue: "Lines 43, 47: navigates to /receptionist/intake — route does not exist"
    missing:
      - "Change navigation target from /receptionist/intake to /patients/intake in ReceptionistDashboard.tsx and CheckInIncompleteDialog.tsx"
  - truth: "RCP-01 through RCP-07 requirements are tracked in central REQUIREMENTS.md"
    status: failed
    reason: "RCP-01 through RCP-07 are referenced in all 9 plan files but do not appear in .planning/REQUIREMENTS.md. They are phase-local identifiers with no central definition or traceability entry."
    artifacts: []
    missing:
      - "Add RCP-01 through RCP-07 definitions and Phase 14 traceability rows to .planning/REQUIREMENTS.md"
human_verification:
  - test: "Log in as a user with Receptionist role, then click 'Tiep nhan BN moi' button on the dashboard"
    expected: "Should navigate to patient intake form at /patients/intake"
    why_human: "Navigation is blocked by broken route — needs visual confirmation after fix"
  - test: "On the dashboard, click a row action for a patient with appointment status 'not_arrived' and observe check-in incomplete dialog, then click 'Check-in & bo sung ho so'"
    expected: "Should navigate to intake form pre-filled with patient data"
    why_human: "Requires a seeded Receptionist-role user and live appointments in the system"
  - test: "KPI cards show correct counts after booking, checking-in, and completing a patient flow"
    expected: "Appointments, waiting, examining, completed counts update in real-time every 30s"
    why_human: "End-to-end polling behavior requires live interaction"
---

# Phase 14: Implement Receptionist Role Flow — Verification Report

**Phase Goal:** Receptionist can manage front-desk workflow: role-based dashboard with KPI cards and patient queue, patient intake form, appointment booking for existing and guest patients, check-in flows, and context-menu actions (reschedule, cancel, no-show)
**Verified:** 2026-03-28
**Status:** gaps_found
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Receptionist sees a dedicated dashboard at /dashboard with KPI cards and patient queue | ✓ VERIFIED | dashboard.tsx checks `user?.roles?.includes("Receptionist")` and renders `<ReceptionistDashboard />` |
| 2 | Receptionist can register new walk-in patients with 4-section intake form and auto-advance | ✓ VERIFIED | PatientIntakeForm.tsx with 4 sections exists; calls useRegisterFromIntakeMutation + useAdvanceStageMutation; route at /patients/intake registered in routeTree.gen.ts |
| 3 | Receptionist can book appointments for existing patients or phone-in guests | ✓ VERIFIED | NewAppointmentPage.tsx uses useAvailableSlots + useBookGuestMutation; route /appointments/new registered |
| 4 | Receptionist can check-in patients and create walk-in visits | ✓ VERIFIED | CheckInDialog + WalkInVisitDialog both wired in ReceptionistDashboard.tsx; CheckInAppointment handler creates Visit in DB |
| 5 | Receptionist can reschedule, cancel, mark no-show via context menu | ✓ VERIFIED | RowActionMenu.tsx imports all 4 action dialogs; RescheduleDialog uses TimeSlotGrid; all dialogs call correct API mutations |
| 6 | All navigation flows work between dashboard, intake form, and booking page | ✗ FAILED | "Tiep nhan BN moi" navigates to `/receptionist/intake` (no route); CheckInIncompleteDialog also targets `/receptionist/intake`; actual route is `/patients/intake` |
| 7 | Vietnamese user stories cover all RCP requirements with traceability | ✓ VERIFIED | docs/user-stories/receptionist-workflow.md exists (545 lines), contains US-RCP-001 through US-RCP-016, references RCP-01 through RCP-07 |

**Score:** 6/7 truths verified (1 failed)

---

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `backend/src/Modules/Scheduling/Scheduling.Domain/Entities/Appointment.cs` | Nullable PatientId, guest fields, CheckIn(), MarkNoShow(), CreateGuest() | ✓ VERIFIED | All fields and methods present. PatientId is `Guid?`, GuestName/Phone/Reason, CheckedInAt, Source, NoShowAt/By/Notes, CancelledBy, CheckIn() at line 149, MarkNoShow() at line 174, CreateGuest() at line 95 |
| `backend/src/Modules/Scheduling/Scheduling.Domain/Enums/AppointmentSource.cs` | AppointmentSource enum (Staff, Phone, Web) | ✓ VERIFIED | File exists with Staff=0, Phone=1, Web=2 |
| `backend/src/Modules/Scheduling/Scheduling.Domain/Enums/AppointmentStatus.cs` | NoShow = 4 | ✓ VERIFIED | NoShow = 4 present |
| `backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs` | CancelWithReason(), VisitSource, CancelledReason | ✓ VERIFIED | CancelWithReason() at line 233, VisitSource at line 22, CancelledReason at line 27 |
| `backend/src/Modules/Clinical/Clinical.Domain/Enums/VisitSource.cs` | VisitSource enum | ✓ VERIFIED | File exists with Appointment=0, WalkIn=1 |
| `backend/src/Modules/Patient/Patient.Domain/Entities/Patient.cs` | 9 intake form fields + UpdateIntake() | ✓ VERIFIED | Email, Occupation, OcularHistory, SystemicHistory, CurrentMedications, ScreenTimeHours, WorkEnvironment, ContactLensUsage, LifestyleNotes all present; UpdateIntake() at line 187 |
| `backend/src/Modules/Patient/Patient.Domain/Enums/WorkEnvironment.cs` | WorkEnvironment enum | ✓ VERIFIED | File exists |
| `backend/src/Modules/Auth/Auth.Infrastructure/Seeding/AuthDataSeeder.cs` | Receptionist role with 8 permissions | ✓ VERIFIED | Role "Receptionist" seeded at line 171; permissions added via UpdatePermissions() covering Patient (View/Create/Update), Scheduling (View/Create/Update), Clinical (View/Create) — 8 total |
| `frontend/src/shared/stores/authStore.ts` | roles: string[] in AuthUser + useHasRole() | ✓ VERIFIED | roles: string[] at line 8; roles propagated in initialize() at line 48; useHasRole() exported at line 68 |
| `backend/src/Modules/Scheduling/Scheduling.Application/Features/CheckInAppointment.cs` | Check-in handler creating Visit | ✓ VERIFIED | Visit.Create() called at line 77 |
| `backend/src/Modules/Scheduling/Scheduling.Application/Features/GetReceptionistDashboard.cs` | Dashboard query with WorkflowStage mapping | ✓ VERIFIED | Queries real DB via appointmentRepository.GetTodayAppointmentsAsync() + visitRepository.GetTodayVisitsAsync(); maps WorkflowStage to 4 statuses |
| `backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/ReceptionistDashboardDto.cs` | Dashboard row DTO | ✓ VERIFIED | File exists |
| `backend/src/Modules/Scheduling/Scheduling.Application/Features/BookGuestAppointment.cs` | Guest booking handler | ✓ VERIFIED | File exists |
| `backend/src/Modules/Scheduling/Scheduling.Application/Features/MarkAppointmentNoShow.cs` | No-show handler | ✓ VERIFIED | File exists |
| `backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAvailableSlots.cs` | Available slots query | ✓ VERIFIED | File exists; endpoint at /api/scheduling/slots |
| `backend/src/Modules/Scheduling/Scheduling.Application/Features/GetReceptionistKpiStats.cs` | KPI stats handler | ✓ VERIFIED | File exists; real DB queries |
| `backend/src/Modules/Clinical/Clinical.Application/Features/CreateWalkInVisit.cs` | Walk-in visit handler | ✓ VERIFIED | File exists |
| `backend/src/Modules/Clinical/Clinical.Application/Features/CancelVisitWithReason.cs` | Cancel visit with reason handler | ✓ VERIFIED | File exists |
| `backend/src/Modules/Patient/Patient.Application/Features/RegisterPatientFromIntake.cs` | Register from intake handler | ✓ VERIFIED | File exists |
| `backend/src/Modules/Patient/Patient.Application/Features/UpdatePatientFromIntake.cs` | Update from intake handler | ✓ VERIFIED | File exists |
| `backend/src/Modules/Scheduling/Scheduling.Presentation/SchedulingApiEndpoints.cs` | 6 scheduling endpoints for receptionist | ✓ VERIFIED | /appointments/check-in, /appointments/guest, /appointments/{id}/no-show, /receptionist/dashboard, /receptionist/kpi, /slots all present |
| `backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs` | 2 clinical endpoints | ✓ VERIFIED | /visits/walk-in and /visits/{id}/cancel-with-reason present |
| `backend/src/Modules/Patient/Patient.Presentation/PatientApiEndpoints.cs` | 2 patient intake endpoints | ✓ VERIFIED | POST /intake and PUT /{id}/intake present |
| `frontend/src/features/receptionist/api/receptionist-api.ts` | TanStack Query hooks for all endpoints | ✓ VERIFIED | Fetches /api/scheduling/receptionist/dashboard with refetchInterval: 15000; /api/scheduling/receptionist/kpi; /api/scheduling/slots |
| `frontend/src/features/receptionist/components/ReceptionistDashboard.tsx` | Main dashboard with KPI + table | ✓ VERIFIED | Present; uses KpiCards, PatientQueueTable, StatusFilterPills; imports CheckInDialog, CheckInIncompleteDialog, WalkInVisitDialog |
| `frontend/src/features/receptionist/types/receptionist.types.ts` | TypeScript types | ✓ VERIFIED | ReceptionistDashboardRow, ReceptionistKpi, AvailableSlot all defined |
| `frontend/src/app/routes/_authenticated/dashboard.tsx` | Role-based rendering | ✓ VERIFIED | isReceptionist check at line 35 renders ReceptionistDashboard |
| `frontend/src/features/receptionist/components/intake/PatientIntakeForm.tsx` | 4-section intake form | ✓ VERIFIED | 4 section sub-components imported; useRegisterFromIntakeMutation + useAdvanceStageMutation wired |
| `frontend/src/app/routes/_authenticated/patients/intake.tsx` | Route /patients/intake | ✓ VERIFIED | Route registered; renders PatientIntakeForm |
| `frontend/src/features/receptionist/components/booking/NewAppointmentPage.tsx` | 2-column booking page | ✓ VERIFIED | useAvailableSlots + useBookGuestMutation wired at lines 30-31, 79, 85 |
| `frontend/src/features/receptionist/components/booking/TimeSlotGrid.tsx` | Slot grid with morning/afternoon groups | ✓ VERIFIED | File exists (substantive); reused in RescheduleDialog at line 119 |
| `frontend/src/app/routes/_authenticated/appointments/new.tsx` | Route /appointments/new | ✓ VERIFIED | Registered; renders NewAppointmentPage |
| `frontend/src/features/receptionist/components/RowActionMenu.tsx` | Context menu with status-dependent actions | ✓ VERIFIED | 149 lines; imports and renders all 4 action dialogs |
| `frontend/src/features/receptionist/components/CheckInDialog.tsx` | Check-in popup | ✓ VERIFIED | useCheckInMutation wired at line 30 |
| `frontend/src/features/receptionist/components/CheckInIncompleteDialog.tsx` | Incomplete check-in popup | ✓ VERIFIED | Navigates to intake form (BROKEN: /receptionist/intake instead of /patients/intake) |
| `frontend/src/features/receptionist/components/WalkInVisitDialog.tsx` | Walk-in popup | ✓ VERIFIED | File present and wired in ReceptionistDashboard |
| `frontend/src/features/receptionist/components/RescheduleDialog.tsx` | Reschedule popup with TimeSlotGrid | ✓ VERIFIED | TimeSlotGrid imported at line 16, used at line 119; useRescheduleAppointment() at line 45 |
| `frontend/src/features/receptionist/components/CancelAppointmentDialog.tsx` | Cancel appointment popup | ✓ VERIFIED | File exists |
| `frontend/src/features/receptionist/components/NoShowDialog.tsx` | No-show popup | ✓ VERIFIED | File exists |
| `frontend/src/features/receptionist/components/CancelVisitDialog.tsx` | Cancel visit popup | ✓ VERIFIED | Navigates to /appointments/new (correct path) at line 82 |
| `docs/user-stories/receptionist-workflow.md` | Vietnamese user stories | ✓ VERIFIED | 545 lines; US-RCP-001 through US-RCP-016 present; RCP-01 through RCP-07 referenced |

---

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `dashboard.tsx` | `ReceptionistDashboard.tsx` | `user?.roles?.includes("Receptionist")` | ✓ WIRED | Line 35-38: isReceptionist guard + conditional render |
| `receptionist-api.ts` | `/api/scheduling/receptionist/dashboard` | TanStack Query refetchInterval: 15000 | ✓ WIRED | Line 45: refetchInterval: 15_000 confirmed |
| `CheckInAppointment.cs` | `Visit.Create()` | Creates visit during check-in | ✓ WIRED | Line 77: Visit.Create() called in handler |
| `GetReceptionistDashboard.cs` | WorkflowStage enum | Maps 11 stages to 4 receptionist statuses | ✓ WIRED | Lines 125-131: WorkflowStage comparisons present |
| `SchedulingApiEndpoints.cs` | `CheckInAppointmentCommand` | Wolverine bus dispatch | ✓ WIRED | Line 97-102: MapPost + CheckInAppointmentCommand dispatched |
| `PatientIntakeForm.tsx` | `receptionist-api.ts` | useRegisterFromIntakeMutation + useAdvanceStageMutation | ✓ WIRED | Lines 32, 35, 57, 60: both hooks imported and called |
| `intake.tsx` | `PatientIntakeForm.tsx` | Route component | ✓ WIRED | Line 4: import; line 25: rendered |
| `NewAppointmentPage.tsx` | `receptionist-api.ts` | useAvailableSlots + useBookGuestMutation | ✓ WIRED | Lines 30-31, 79, 85: hooks wired |
| `new.tsx` (appointments) | `NewAppointmentPage.tsx` | Route component | ✓ WIRED | Line 4: import; line 18: rendered |
| `RowActionMenu.tsx` | Dialog components | Dialog state management via setActiveDialog | ✓ WIRED | Lines 120-143: all 4 dialogs rendered conditionally |
| `CheckInDialog.tsx` | `receptionist-api.ts` | useCheckInMutation | ✓ WIRED | Line 14: import; line 30: called |
| `RescheduleDialog.tsx` | `TimeSlotGrid.tsx` | Reuses TimeSlotGrid | ✓ WIRED | Line 16: import; line 119: rendered |
| `ReceptionistDashboard.tsx` | `/patients/intake` | "Tiep nhan BN moi" button | ✗ BROKEN | Line 110: navigates to `/receptionist/intake` — NO SUCH ROUTE. routeTree.gen.ts has no /receptionist/intake entry |
| `CheckInIncompleteDialog.tsx` | `/patients/intake` | "Check-in & bo sung ho so" button | ✗ BROKEN | Lines 43, 47: navigate to `/receptionist/intake` — NO SUCH ROUTE |
| `CancelVisitDialog.tsx` | `/appointments/new` | "Dat lich hen lai" after cancel | ✓ WIRED | Line 82: navigates to /appointments/new with patientId param |

---

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|---------------|--------|-------------------|--------|
| `ReceptionistDashboard.tsx` | dashboardQuery.data | `GET /api/scheduling/receptionist/dashboard` | appointmentRepository.GetTodayAppointmentsAsync() + visitRepository.GetTodayVisitsAsync() queries DB | ✓ FLOWING |
| `KpiCards.tsx` | kpi | `GET /api/scheduling/receptionist/kpi` | GetReceptionistKpiStats handler queries real DB | ✓ FLOWING |
| `NewAppointmentPage.tsx` | slots | `GET /api/scheduling/slots` | GetAvailableSlots handler queries ClinicSchedule + doctor bookings | ✓ FLOWING |

---

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|----------|---------|--------|--------|
| Backend builds cleanly | `dotnet build backend/src/Bootstrapper/ --no-restore -v q` | Build succeeded, 0 errors, 2 unrelated warnings | ✓ PASS |
| Scheduling unit tests pass (incl. receptionist tests) | `dotnet test backend/tests/Scheduling.Unit.Tests/ -v q` | Passed: 37, Failed: 0 | ✓ PASS |
| Patient unit tests pass (incl. intake tests) | `dotnet test backend/tests/Patient.Unit.Tests/ -v q` | Passed: 31, Failed: 0 | ✓ PASS |
| Clinical unit tests pass (incl. walk-in + cancel-with-reason tests) | `dotnet test backend/tests/Clinical.Unit.Tests/ -v q` | Passed: 289, Failed: 0 | ✓ PASS |
| No TypeScript errors in receptionist feature files | `npx tsc --noEmit` (check receptionist files) | 0 errors in receptionist files (pre-existing errors in patient-api.ts unrelated to Phase 14) | ✓ PASS |

---

### Requirements Coverage

| Requirement | Source Plan(s) | Description | Status | Evidence |
|-------------|---------------|-------------|--------|----------|
| RCP-01 | 14-01, 14-02, 14-03, 14-04 | Role-based dashboard with KPI cards and patient queue | ✓ SATISFIED | dashboard.tsx role-checks + ReceptionistDashboard.tsx with KpiCards + PatientQueueTable; 15s polling |
| RCP-02 | 14-01, 14-02, 14-03, 14-05 | Patient intake form (new walk-in registration) | ✓ SATISFIED | PatientIntakeForm.tsx with 4 sections; RegisterPatientFromIntake handler; /patients/intake route |
| RCP-03 | 14-02, 14-03, 14-06 | Appointment booking for existing and guest patients | ✓ SATISFIED | BookGuestAppointment handler + NewAppointmentPage.tsx; TimeSlotGrid; /appointments/new route |
| RCP-04 | 14-01, 14-02, 14-03, 14-06 | Appointment booking time slot grid | ✓ SATISFIED | TimeSlotGrid.tsx with morning/afternoon groups; GetAvailableSlots handler |
| RCP-05 | 14-02, 14-03, 14-07 | Check-in flow (complete + incomplete patient profiles) | ✓ SATISFIED | CheckInDialog + CheckInIncompleteDialog wired in dashboard; CheckInAppointment handler creates Visit |
| RCP-06 | 14-02, 14-03, 14-07 | Context-menu actions (reschedule, cancel, no-show, walk-in, cancel visit) | ✓ SATISFIED | RowActionMenu.tsx renders all action dialogs; all dialogs wired to API mutations |
| RCP-07 | 14-08 | Vietnamese user stories documentation | ✓ SATISFIED | docs/user-stories/receptionist-workflow.md exists (545 lines) with US-RCP-001 through US-RCP-016 |

**IMPORTANT NOTE — Requirements Not in Central REQUIREMENTS.md:**
RCP-01 through RCP-07 are defined only within Phase 14 plan files. They have no entries in `.planning/REQUIREMENTS.md` and no traceability rows in the requirements table. This is an orphaned requirement set. The central REQUIREMENTS.md must be updated to add these definitions and map them to Phase 14.

---

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `frontend/src/features/receptionist/components/ReceptionistDashboard.tsx` | 110 | `/receptionist/intake` — non-existent route | Blocker | "Tiep nhan BN moi" primary CTA is dead — clicks lead to 404/no-match |
| `frontend/src/features/receptionist/components/CheckInIncompleteDialog.tsx` | 43, 47 | `/receptionist/intake` — non-existent route | Blocker | Incomplete-profile check-in flow is entirely broken — cannot reach intake form for incomplete patients |

---

### Human Verification Required

#### 1. Broken Navigation — New Patient Intake Flow

**Test:** Log in with a Receptionist-role account. On the /dashboard page, click "Tiep nhan BN moi" button.
**Expected:** Should navigate to /patients/intake — a full patient registration form with 4 sections.
**Why human:** Route mismatch; confirm after fix that form loads correctly and "Save" advances to Pre-Exam stage.

#### 2. Incomplete-Profile Check-In Flow

**Test:** With an appointment in the queue that has an incomplete profile (e.g., a phone-in guest booking), click the check-in action, observe the "incomplete" dialog, then click "Check-in & bo sung ho so".
**Expected:** Should navigate to /patients/intake pre-filled with guest name and appointmentId.
**Why human:** Requires seeded test data with a confirmed appointment in "not_arrived" status for a guest booking.

#### 3. KPI Card Polling (30s)

**Test:** Open /dashboard as Receptionist. Observe KPI cards, then book a new appointment or check in a patient in another tab/window.
**Expected:** KPI counts update within 30 seconds automatically.
**Why human:** Polling behavior requires live time-based observation.

#### 4. Status Filter Pills

**Test:** On /dashboard as Receptionist, click each filter pill (All, Not Arrived, Waiting, Examining, Completed).
**Expected:** Patient queue table filters to show only rows matching the selected status.
**Why human:** UI filtering behavior requires visual confirmation against live data.

---

### Gaps Summary

**1 blocker gap — broken navigation to patient intake form.**

The "Tiep nhan BN moi" button in `ReceptionistDashboard.tsx` and the "Check-in & bo sung ho so" path in `CheckInIncompleteDialog.tsx` both navigate to `/receptionist/intake`. This route does not exist anywhere in the frontend route tree (routeTree.gen.ts). The actual route for the patient intake form is registered at `/patients/intake`.

This is a one-line fix in each file: change `/receptionist/intake` to `/patients/intake`. Both affected files are isolated with no cascading changes needed.

**Secondary gap — RCP requirements not in central requirements registry.**

RCP-01 through RCP-07 exist only as labels in phase plan files. The central `.planning/REQUIREMENTS.md` has no knowledge of these requirements and no traceability row maps them to Phase 14. This is a documentation completeness gap, not a runtime blocker.

**What is working correctly:**
- All 9 backend handlers (CheckIn, BookGuest, MarkNoShow, GetSlots, Dashboard, KPI, WalkInVisit, CancelVisitWithReason, RegisterFromIntake) are implemented, tested, and wired to HTTP endpoints
- All 8 frontend dialog components are implemented and wired
- Role-based dashboard rendering works correctly
- Patient intake form, booking page, KPI cards, patient queue table all render real data
- 357 unit tests pass across Scheduling, Patient, and Clinical test projects
- Receptionist role seeded with 8 permissions (via UpdatePermissions pattern, not AddPermission)
- Frontend AuthUser includes roles array with useHasRole helper exported

---

_Verified: 2026-03-28_
_Verifier: Claude (gsd-verifier)_
