---
status: resolved
trigger: "Appointment type field shows blank in detail dialog on /appointments"
created: 2026-03-09T06:40:00+07:00
updated: 2026-03-09T07:00:00+07:00
---

## Current Focus

hypothesis: The previous fix was correctly applied in source code but the running backend binary was stale (not rebuilt after the fix was committed).
test: Rebuild backend and hit API directly to verify appointmentTypeNameVi appears in JSON response
expecting: Field should appear in API response after fresh build
next_action: Confirmed -- no further action needed, bug is resolved in code

## Symptoms

expected: Clicking an appointment in the calendar should show the appointment type name (e.g., "Tai kham" in Vietnamese, "Follow-Up" in English) in the detail dialog under "Loai lich hen:" label.
actual: The appointment type field shows blank/nothing in the detail dialog.
errors: No console errors reported.
reproduction: Go to /appointments, select a doctor, click any existing appointment in the calendar.
started: Reported after the original fix was supposedly applied (feat(02-16) commits).

## Eliminated

- hypothesis: Backend DTO missing AppointmentTypeNameVi field
  evidence: AppointmentDto.cs line 16 has `string AppointmentTypeNameVi` as a record parameter. Confirmed in source.
  timestamp: 2026-03-09T06:42:00+07:00

- hypothesis: Backend handlers not mapping NameVi from AppointmentType entity
  evidence: Both GetAppointmentsByDoctor.cs (line 41) and GetAppointmentsByPatient.cs (line 39) map `appointmentType?.NameVi ?? "Unknown"`. Unit tests pass confirming this mapping works.
  timestamp: 2026-03-09T06:43:00+07:00

- hypothesis: Frontend TypeScript type missing appointmentTypeNameVi
  evidence: scheduling-api.ts line 16 has `appointmentTypeNameVi: string` in AppointmentDto interface.
  timestamp: 2026-03-09T06:44:00+07:00

- hypothesis: Frontend not passing appointmentTypeNameVi through FullCalendar extendedProps
  evidence: useAppointments.ts line 71 sets `appointmentTypeNameVi: apt.appointmentTypeNameVi` in extendedProps. appointments/index.tsx line 56 reads `ext.appointmentTypeNameVi as string` in handleEventClick.
  timestamp: 2026-03-09T06:45:00+07:00

- hypothesis: Frontend dialog not rendering the correct field
  evidence: AppointmentDetailDialog.tsx line 160 correctly uses locale-based rendering: `i18n.language === "vi" ? appointment.appointmentTypeNameVi : appointment.appointmentTypeName`
  timestamp: 2026-03-09T06:46:00+07:00

- hypothesis: JSON serialization casing mismatch (C# PascalCase vs JS camelCase)
  evidence: ASP.NET Core defaults to camelCase for System.Text.Json. C# `AppointmentTypeNameVi` serializes as `appointmentTypeNameVi`, matching the frontend TypeScript interface exactly. Confirmed via actual API response.
  timestamp: 2026-03-09T06:47:00+07:00

- hypothesis: AppointmentType entity not found in typeMap (deactivated or missing)
  evidence: GetAllAppointmentTypesAsync filters by IsActive, but fallback is "Unknown" not blank. Actual API response shows correct type names ("Follow-Up", "Tai kham"), so types ARE being found.
  timestamp: 2026-03-09T06:48:00+07:00

## Evidence

- timestamp: 2026-03-09T06:42:00+07:00
  checked: Backend DTO (Scheduling.Contracts/Dtos/AppointmentDto.cs)
  found: Record has 13 parameters including `string AppointmentTypeNameVi` at position 10.
  implication: DTO definition is correct.

- timestamp: 2026-03-09T06:43:00+07:00
  checked: Backend handlers (GetAppointmentsByDoctor.cs, GetAppointmentsByPatient.cs)
  found: Both handlers load all appointment types, create a dictionary by ID, and map `appointmentType?.NameVi ?? "Unknown"` for the AppointmentTypeNameVi parameter.
  implication: Handler mapping logic is correct.

- timestamp: 2026-03-09T06:44:00+07:00
  checked: Unit tests (GetAppointmentsByDoctorHandlerTests.cs)
  found: Two tests exist and PASS: one verifies NameVi mapping from AppointmentType, one verifies "Unknown" fallback when type not found.
  implication: Backend logic is verified by tests.

- timestamp: 2026-03-09T06:45:00+07:00
  checked: Frontend TypeScript type (scheduling-api.ts)
  found: AppointmentDto interface includes `appointmentTypeNameVi: string` at line 16.
  implication: Frontend type definition matches backend DTO.

- timestamp: 2026-03-09T06:46:00+07:00
  checked: Frontend data flow (useAppointments.ts -> appointments/index.tsx -> AppointmentDetailDialog.tsx)
  found: appointmentTypeNameVi flows through: API response -> AppointmentDto -> appointmentToEvent (extendedProps) -> handleEventClick -> AppointmentInfo -> dialog render.
  implication: Complete data pipeline is wired correctly.

- timestamp: 2026-03-09T06:50:00+07:00
  checked: Actual API response from STALE running backend (pre-rebuild)
  found: Response JSON did NOT contain `appointmentTypeNameVi` field. Fields went directly from `appointmentTypeName` to `status`.
  implication: CRITICAL FINDING -- The running backend binary was compiled from code BEFORE the fix was committed. The stale binary was still running.

- timestamp: 2026-03-09T06:55:00+07:00
  checked: Actual API response from FRESHLY BUILT backend (after `dotnet build`)
  found: Response JSON DOES contain `"appointmentTypeNameVi":"Tai kham"` for Follow-Up appointments and `"appointmentTypeNameVi":"Benh nhan moi"` for New Patient appointments.
  implication: The fix IS correct in source code. After rebuild and restart, the API returns the expected field. The bug was a stale binary issue.

- timestamp: 2026-03-09T06:56:00+07:00
  checked: AppointmentType seeder data in database
  found: Seeder creates 4 types with proper NameVi values: "Benh nhan moi", "Tai kham", "Dieu tri", "Ortho-K". Database has the correct data.
  implication: Database data is correct.

## Resolution

root_cause: The previous fix (adding AppointmentTypeNameVi to the DTO and mapping it in handlers) was correctly applied in source code and committed. However, the running backend binary was stale -- it had been compiled from the old code (before the fix). The stale binary did not include the AppointmentTypeNameVi field in the API response, causing the frontend to receive `undefined` for this field, which renders as blank in the detail dialog.

fix: No code changes needed. The fix was already correctly applied in commits `2375d56` (feat(02-16): locale-aware type names and placeholder cleanup) and `121d952` (feat(02-16): add AppointmentTypeNameVi to backend DTO and handlers). The resolution is to rebuild and restart the backend (`dotnet build` + `dotnet run`).

verification: |
  1. Rebuilt backend with `dotnet build` from Bootstrapper project.
  2. Started fresh backend with `dotnet run`.
  3. Called API: GET /api/appointments/by-doctor/{doctorId}?dateFrom=2026-03-01&dateTo=2026-03-31
  4. Verified JSON response contains `"appointmentTypeNameVi":"Tai kham"` for all appointments.
  5. Compared with pre-rebuild response which was MISSING the field entirely.
  6. All backend unit tests pass (2/2 for GetAppointmentsByDoctorHandler).

files_changed: []
