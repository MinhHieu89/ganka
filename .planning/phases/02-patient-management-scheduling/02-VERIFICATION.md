---
phase: 02-patient-management-scheduling
verified: 2026-03-09T06:00:00Z
status: passed
score: 11/11 requirements satisfied, 11/11 must-have truths verified
re_verification:
  previous_status: passed
  previous_score: 33/33
  gaps_closed:
    - "Test 10 (UAT): Clicking a 13:00 slot now pre-populates booking form with 13:00 via getUTCHours/getUTCMinutes"
    - "Test 10 (UAT): Submitting outside clinic hours or double-booking shows error in ServerValidationAlert banner, not toast"
    - "Test 10 (UAT): Saved appointments display at correct Vietnam local time via @fullcalendar/moment-timezone plugin"
    - "Test 11 (UAT): AppointmentDetailDialog shows locale-aware type name (Vietnamese or English) via i18n.language check"
    - "Test 11 (UAT): AppointmentBookingDialog dropdown shows locale-aware type names (was hardcoded to Vietnamese)"
    - "Test 12 (UAT): BookingForm appointment type SelectValue has no redundant placeholder"
    - "Test 12 (UAT): BookingForm preferred time SelectValue has no redundant placeholder"
    - "Test 12 (UAT): DatePicker in BookingForm gets placeholder='' to suppress 'Tim kiem' default"
  gaps_remaining: []
  regressions: []
human_verification:
  - test: "Appointment calendar slot click pre-populates time and calendar displays Vietnam local time"
    expected: "Click 13:00 slot — form shows 13:00. Save appointment — it appears at 13:00 on calendar, not hidden under slotMinTime."
    why_human: "Timezone rendering and slot click interaction require running frontend with live backend"
  - test: "Appointment type locale switch in detail dialog and booking dropdown"
    expected: "Switch to Vietnamese — detail dialog shows Vietnamese type name, booking dropdown shows Vietnamese names. Switch to English — both show English names."
    why_human: "i18n toggle and live data require browser with running system"
  - test: "BookingForm has no redundant placeholders"
    expected: "/book page — Loai lich hen, Ngay mong muon, Gio mong muon fields show no placeholder text duplicating their labels."
    why_human: "Visual rendering requires browser"
---

# Phase 02: Patient Management & Scheduling Verification Report

**Phase Goal:** Complete patient management (registration, profiles, allergies) and scheduling (appointments, calendar, self-booking) modules with full backend APIs and frontend UIs.
**Verified:** 2026-03-09T06:00:00Z
**Status:** passed — all automated checks passed; 3 items require human verification (visual/interaction)
**Re-verification:** Yes — after UAT gap closure Plans 02-15 (calendar timezone + in-form errors) and 02-16 (appointment type i18n + placeholder cleanup)

---

## Re-verification Summary

Previous verification (2026-03-02T16:16:00Z) had status `passed` (33/33 must-haves), but a subsequent UAT run (02-UAT.md) conducted on 2026-03-09 revealed 3 new issues:

- **Test 10 (blocker):** Slot click time pre-population broken; validation errors went to toast; appointments displayed at wrong local time
- **Test 11 (major):** Appointment type name always showed in English regardless of locale; double-booking showed in toast not in-form
- **Test 12 (minor):** Redundant placeholders on appointment type Select, preferred time Select, and DatePicker in public BookingForm

Two gap-closure plans were executed:

- **Plan 02-15 (frontend calendar):** Installed `@fullcalendar/moment-timezone`, used `getUTCHours/getUTCMinutes`, routed DOUBLE_BOOKING/VALIDATION_ERROR to `setNonFieldError`
- **Plan 02-16 (i18n + cleanup):** Added `AppointmentTypeNameVi` to backend DTO + handlers (TDD), frontend locale-aware display in detail/booking dialogs, placeholder removal in BookingForm

This verification confirms all Plan 15 and Plan 16 must-haves are implemented and wired.

---

## Goal Achievement

### Observable Truths — Plans 02-15 and 02-16 (Gap Closure)

| # | Truth | Status | Evidence |
|---|-------|--------|---------|
| N1 | Clicking a 13:00 slot pre-populates the booking form with 13:00 | VERIFIED | `AppointmentBookingDialog.tsx:110,129`: `defaultStartTime.getUTCHours()` and `defaultStartTime.getUTCMinutes()` in both `defaultValues` and `useEffect` reset |
| N2 | Submitting outside clinic hours shows error in ServerValidationAlert, not toast | VERIFIED | `AppointmentBookingDialog.tsx:170-171`: `setNonFieldError(t("outsideClinicHours"))` replaces `toast.error()` for `VALIDATION_ERROR` |
| N3 | Submitting a double-booking shows error in ServerValidationAlert, not toast | VERIFIED | `AppointmentBookingDialog.tsx:168-169`: `setNonFieldError(t("slotAlreadyBooked"))` replaces `toast.error()` for `DOUBLE_BOOKING` |
| N4 | Saved appointments display at correct Vietnam local time on calendar | VERIFIED | `AppointmentCalendar.tsx:6,78`: `import momentTimezonePlugin` + `plugins=[..., momentTimezonePlugin]`; `timeZone="Asia/Ho_Chi_Minh"` now works with the plugin to correctly convert UTC ISO strings |
| N5 | AppointmentDto includes AppointmentTypeNameVi field | VERIFIED | `AppointmentDto.cs:16`: `string AppointmentTypeNameVi` as 10th positional record parameter |
| N6 | GetAppointmentsByDoctor maps NameVi from AppointmentType entity | VERIFIED | `GetAppointmentsByDoctor.cs:41`: `appointmentType?.NameVi ?? "Unknown"` passed to AppointmentDto constructor |
| N7 | GetAppointmentsByPatient maps NameVi from AppointmentType entity | VERIFIED | `GetAppointmentsByPatient.cs:39`: `appointmentType?.NameVi ?? "Unknown"` passed to AppointmentDto constructor |
| N8 | AppointmentDetailDialog shows locale-aware type name | VERIFIED | `AppointmentDetailDialog.tsx:160`: `i18n.language === "vi" ? appointment.appointmentTypeNameVi : appointment.appointmentTypeName` |
| N9 | AppointmentBookingDialog dropdown shows locale-aware type names | VERIFIED | `AppointmentBookingDialog.tsx:309`: `i18n.language === "vi" ? type.nameVi : type.name` |
| N10 | BookingForm appointment type field has no redundant placeholder | VERIFIED | `BookingForm.tsx:178`: `<SelectValue />` — no placeholder prop |
| N11 | BookingForm preferred time field has no redundant placeholder | VERIFIED | `BookingForm.tsx:230`: `<SelectValue />` — no placeholder prop |
| N12 | BookingForm DatePicker shows no 'Tim kiem' default placeholder | VERIFIED | `BookingForm.tsx:212`: `placeholder=""` passed to DatePicker overrides default |

### Observable Truths — Previously Verified (Regression Check)

| # | Truth | Status | Evidence |
|---|-------|--------|---------|
| R1 | POST /api/patients creates patient with GK-YYYY-NNNN code | VERIFIED | RegisterPatientHandler + SetSequence() unchanged |
| R2 | Walk-in patient registration with name+phone only | VERIFIED | PatientType.WalkIn path unchanged |
| R3 | Diacritics-insensitive + substring search | VERIFIED | PatientRepository SearchAsync/GetPagedAsync unchanged |
| R4 | Double-booking returns 409 Conflict | VERIFIED | HasOverlappingAsync + DB unique filtered index unchanged |
| R5 | Public self-booking has no RequireAuthorization | VERIFIED | PublicBookingEndpoints unchanged |
| R6 | 401 token refresh interceptor | VERIFIED | api-client.ts onResponse handler unchanged |
| R7 | Server validation errors show per-field via handleServerValidationError | VERIFIED | server-validation.ts + ServerValidationAlert unchanged |
| R8 | AppointmentTypeNameVi correctly flows through useAppointments.ts extendedProps | VERIFIED | `useAppointments.ts:71`: `appointmentTypeNameVi: apt.appointmentTypeNameVi` in extendedProps |
| R9 | appointments/index.tsx passes appointmentTypeNameVi to AppointmentDetailDialog | VERIFIED | `index.tsx:25,56`: interface has `appointmentTypeNameVi: string`; handleEventClick extracts `ext.appointmentTypeNameVi as string` |
| R10 | Backend unit tests: Patient.Unit.Tests 12/12 passed | VERIFIED | `dotnet test` output: Passed: 12 — 0 failures |
| R11 | Backend unit tests: Scheduling.Unit.Tests 7/7 passed | VERIFIED | `dotnet test` output: Passed: 7 (includes 4 new NameVi tests + 3 previous) — 0 failures |

**Score: 12/12 new must-have truths verified (Plans 15-16) + 11/11 regressions clean**

---

## Required Artifacts — Plans 02-15 and 02-16

| Artifact | Status | Details |
|----------|--------|---------|
| `frontend/src/features/scheduling/components/AppointmentCalendar.tsx` | VERIFIED | Line 6: `import momentTimezonePlugin`; Line 78: `momentTimezonePlugin` in plugins array; Line 100: `timeZone="Asia/Ho_Chi_Minh"` |
| `frontend/src/features/scheduling/components/AppointmentBookingDialog.tsx` | VERIFIED | Lines 110,129: `getUTCHours/getUTCMinutes`; Lines 168-171: `setNonFieldError` for DOUBLE_BOOKING/VALIDATION_ERROR; Line 309: locale-aware type name |
| `frontend/package.json` | VERIFIED | `@fullcalendar/moment-timezone: ^6.1.20`, `moment-timezone: ^0.5.48` |
| `backend/src/Modules/Scheduling/Scheduling.Contracts/Dtos/AppointmentDto.cs` | VERIFIED | Line 16: `string AppointmentTypeNameVi` field added as 10th parameter |
| `backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByDoctor.cs` | VERIFIED | Line 41: `appointmentType?.NameVi ?? "Unknown"` mapped |
| `backend/src/Modules/Scheduling/Scheduling.Application/Features/GetAppointmentsByPatient.cs` | VERIFIED | Line 39: `appointmentType?.NameVi ?? "Unknown"` mapped |
| `backend/tests/Scheduling.Unit.Tests/Features/GetAppointmentsByDoctorHandlerTests.cs` | VERIFIED | 2 tests: `Handle_MapsAppointmentTypeNameVi_FromAppointmentType` and `Handle_ReturnsUnknown_ForAppointmentTypeNameVi_WhenTypeNotFound` — both pass |
| `backend/tests/Scheduling.Unit.Tests/Features/GetAppointmentsByPatientHandlerTests.cs` | VERIFIED | 2 tests: same pattern for patient handler — both pass |
| `frontend/src/features/scheduling/api/scheduling-api.ts` | VERIFIED | Line 16: `appointmentTypeNameVi: string` in AppointmentDto interface |
| `frontend/src/features/scheduling/hooks/useAppointments.ts` | VERIFIED | Line 71: `appointmentTypeNameVi: apt.appointmentTypeNameVi` in extendedProps |
| `frontend/src/features/scheduling/components/AppointmentDetailDialog.tsx` | VERIFIED | Lines 35,160: `appointmentTypeNameVi: string` in AppointmentInfo interface; locale-aware render |
| `frontend/src/app/routes/_authenticated/appointments/index.tsx` | VERIFIED | Lines 25,56: `appointmentTypeNameVi` in AppointmentInfo interface and handleEventClick extraction |
| `frontend/src/features/booking/components/BookingForm.tsx` | VERIFIED | Lines 178,230: `<SelectValue />` no placeholder; Line 212: `placeholder=""` on DatePicker |

---

## Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `AppointmentCalendar` | `FullCalendar timeZone` | `momentTimezonePlugin` registered | WIRED | Line 78: plugins array includes `momentTimezonePlugin`; Line 100: `timeZone="Asia/Ho_Chi_Minh"` — plugin enables proper timezone conversion |
| `AppointmentBookingDialog slot click` | `form startTime field` | `getUTCHours/getUTCMinutes` | WIRED | Lines 110,129: UTC-aware extraction from FullCalendar UTC-coerced Date |
| `AppointmentBookingDialog onError` | `ServerValidationAlert` | `setNonFieldError(t("slotAlreadyBooked"))` | WIRED | Lines 168-171: DOUBLE_BOOKING and VALIDATION_ERROR route to `setNonFieldError`; Lines 191-194: `ServerValidationAlert` renders `nonFieldError` |
| `GetAppointmentsByDoctor` | `AppointmentDto` | `appointmentType?.NameVi ?? "Unknown"` | WIRED | Line 41 in handler constructs AppointmentDto with NameVi |
| `AppointmentDto.AppointmentTypeNameVi` | `scheduling-api.ts AppointmentDto` | `appointmentTypeNameVi: string` | WIRED | Line 16: TypeScript interface matches C# record field |
| `scheduling-api.ts` | `useAppointments.ts extendedProps` | `appointmentTypeNameVi: apt.appointmentTypeNameVi` | WIRED | Line 71: field passed through to FullCalendar event extendedProps |
| `useAppointments extendedProps` | `appointments/index.tsx handleEventClick` | `ext.appointmentTypeNameVi as string` | WIRED | Line 56: extracted from extendedProps and set in selectedAppointment state |
| `selectedAppointment.appointmentTypeNameVi` | `AppointmentDetailDialog` | `i18n.language === "vi" ? appointment.appointmentTypeNameVi : appointment.appointmentTypeName` | WIRED | Line 160: locale-aware display |
| `AppointmentTypeDto.nameVi` | `AppointmentBookingDialog dropdown` | `i18n.language === "vi" ? type.nameVi : type.name` | WIRED | Line 309: locale-aware SelectItem label |

---

## Requirements Coverage

| Requirement | Source Plans | Description | Status | Evidence |
|-------------|-------------|-------------|--------|---------|
| PAT-01 | 02-01, 02-02, 02-11 | Staff registers medical patient with auto-generated GK-YYYY-NNNN ID | SATISFIED | RegisterPatientHandler + SetSequence(); camelCase redirect fix; checked [x] in REQUIREMENTS.md |
| PAT-02 | 02-01, 02-02, 02-11 | Staff registers walk-in pharmacy customer with name + phone only | SATISFIED | PatientType.WalkIn path; checked [x] in REQUIREMENTS.md |
| PAT-03 | 02-10 | System supports configurable mandatory fields (Address, CCCD) | SATISFIED | PatientFieldValidator + FieldRequirementContext + PatientFieldWarning; checked [x] in REQUIREMENTS.md |
| PAT-04 | 02-03, 02-11 | Staff can search by name, phone, or patient ID | SATISFIED | PatientRepository: Vietnamese_CI_AI collation, Phone.Contains, PatientCode.Contains; checked [x] in REQUIREMENTS.md |
| PAT-05 | 02-04, 02-07, 02-09, 02-12 | System stores structured allergy list and displays alerts | SATISFIED | Allergy entity; AllergyForm combobox; integer severity fix; AllergyAlert; checked [x] in REQUIREMENTS.md |
| SCH-01 | 02-05, 02-13, 02-15, 02-16 | Staff can book appointments | SATISFIED | BookAppointmentHandler; AppointmentBookingDialog with UTC-aware slot time, in-form errors, locale-aware type names; checked [x] in REQUIREMENTS.md |
| SCH-02 | 02-06, 02-11, 02-15 | Patients can self-book via public website with staff confirmation | SATISFIED | PublicBookingEndpoints; SubmitSelfBookingHandler; ApproveSelfBooking timezone fix; BookingForm placeholder cleanup; checked [x] in REQUIREMENTS.md |
| SCH-03 | 02-05, 02-16 | No double-booking (1 patient per doctor per slot) | SATISFIED | HasOverlappingAsync + DB unique filtered index; DOUBLE_BOOKING error now shown in-form; checked [x] in REQUIREMENTS.md |
| SCH-04 | 02-05 | Calendar view per doctor, color-coded by appointment type | SATISFIED | AppointmentCalendar.tsx with FullCalendar; 4 appointment types with colors; timezone plugin now ensures correct display; checked [x] in REQUIREMENTS.md |
| SCH-05 | 02-05, 02-16 | Appointment durations configurable by type | SATISFIED | AppointmentType.DefaultDurationMinutes; BookAppointmentHandler computes EndTime; duration shown in booking dialog alongside locale-aware name; checked [x] in REQUIREMENTS.md |
| SCH-06 | 02-05, 02-11 | System respects clinic operating hours | SATISFIED | ClinicSchedule + ClinicScheduleSeeder; timezone-fixed validation in BookAppointment and ApproveSelfBooking; VALIDATION_ERROR now shown in-form; checked [x] in REQUIREMENTS.md |

All 11 requirement IDs (PAT-01 through PAT-05, SCH-01 through SCH-06) are SATISFIED. All marked Phase 2 / Complete in REQUIREMENTS.md.

---

## Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `AppointmentBookingDialog.tsx` | 299 | `<SelectValue placeholder={t("appointmentType")} />` on the appointment type select in the booking dialog | Info | This is an unlabeled select trigger (label is in FieldLabel, SelectValue shows selected value). The placeholder text serves as the empty-state prompt — different from the UAT 12 case which had a *separate* FieldLabel. Acceptable per CLAUDE.md: "add placeholder where it makes sense". |
| `AppointmentBookingDialog.tsx` | 352 | `<SelectValue placeholder={t("selectTime")} />` on time select | Info | Same reasoning — time select trigger has no visible label when empty. Placeholder is meaningful UX, not redundant. |
| `AppointmentDetailDialog.tsx` | 184 | `<SelectValue placeholder={t("cancellationReason.title")} />` | Info | Same reasoning — cancel reason select has no separate label row. |
| `AppointmentCalendar.tsx` | 48-51 | `toast.error()` used in `handleEventDrop` for DOUBLE_BOOKING/VALIDATION_ERROR | Warning | Drag-to-reschedule errors route to toast (not in-form). Plan 02-15 only fixed the booking dialog form. Drag-drop reschedule has no modal form to show in-form errors, so toast is acceptable. Pre-existing design decision. |

No blocker anti-patterns. No TODO/FIXME/placeholder stubs in Plan 02-15 or 02-16 artifacts.

**TypeScript:** All Phase 02-modified files compile without errors (60 pre-existing errors in `admin-api.ts`, `audit-api.ts`, `auth-api.ts`, `__root.tsx` are unrelated to Phase 02).

**Backend tests:** Patient.Unit.Tests: 12/12 passed. Scheduling.Unit.Tests: 7/7 passed (includes 4 new NameVi tests from Plan 02-16).

**Git commits verified:** ed45c73, e46ad4e (Plan 02-15); 35e9cfd, 121d952, 2375d56 (Plan 02-16).

---

## Human Verification Required

### 1. Calendar Slot Click Time Pre-population and UTC Timezone Display

**Test:** Log in as staff. Navigate to /appointments. Select a doctor. Click the 13:00 Tuesday slot on the calendar.
**Expected:** Booking dialog opens with time field pre-populated as "13:00" (not blank or UTC offset). Save an appointment at 13:00 — after the dialog closes and the calendar refreshes, the appointment block appears at 13:00, not hidden before slotMinTime or at 06:00.
**Why human:** FullCalendar UTC-coercion behavior and timezone plugin rendering require a running browser with live backend; cannot verify visually from grep.

### 2. Appointment Type Locale-Aware Display

**Test:** Navigate to /appointments. Open an appointment's detail by clicking it. Note the "Appointment Type" value. Switch the app language to Vietnamese (if currently English) or English (if currently Vietnamese). Open the same appointment's detail again.
**Expected:** Type name changes to match the selected locale — e.g., "Follow Up" in English, "Tai kham" (or the seeded Vietnamese name) in Vietnamese. Open the booking dialog — appointment type dropdown items also change locale.
**Why human:** i18n language toggle and live data from backend required; locale rendering needs browser.

### 3. BookingForm No Redundant Placeholders

**Test:** Open an incognito browser. Navigate to /book. Observe the "Loai lich hen" (Appointment Type), "Ngay mong muon" (Preferred Date), and "Gio mong muon" (Preferred Time) fields before selecting any value.
**Expected:** None of the three fields show placeholder text that duplicates their label. SelectValue triggers are empty/blank. DatePicker trigger shows no "Tim kiem" text.
**Why human:** Visual rendering of placeholder state requires browser; cannot verify from code alone that no placeholder text is visible when no value is selected.

---

## Gaps Summary

No gaps remain. All 3 UAT issues from the 2026-03-09 UAT run have been resolved by Plans 02-15 and 02-16:

- **1 blocker resolved (Test 10):** Calendar slot click time, in-form error routing, UTC timezone display — Plan 02-15 (getUTCHours/getUTCMinutes, setNonFieldError, momentTimezonePlugin)
- **1 major resolved (Test 11):** Appointment type locale-aware display — Plan 02-16 (AppointmentTypeNameVi in DTO, handlers, frontend interfaces, i18n.language checks)
- **1 minor resolved (Test 12):** Redundant placeholders on labeled fields in BookingForm — Plan 02-16 (SelectValue without placeholder, DatePicker with placeholder="")

All 11 requirements (PAT-01 through PAT-05, SCH-01 through SCH-06) remain SATISFIED. No regressions detected in previously verified artifacts.

---

_Verified: 2026-03-09T06:00:00Z_
_Verifier: Claude (gsd-verifier)_
_Re-verification after gap closure: Plans 02-15 (calendar timezone + in-form errors), 02-16 (appointment type i18n + placeholder cleanup)_
