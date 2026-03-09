---
status: resolved
trigger: "UAT test 11: (1) Appointment type not translated to Vietnamese in detail dialog, (2) Double-booking 409 error shows toast instead of in-form ServerValidationAlert"
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:00:00Z
---

## Current Focus

hypothesis: Both root causes confirmed with evidence
test: n/a
expecting: n/a
next_action: Return diagnosis

## Symptoms

expected: (1) Appointment type shown in Vietnamese when VI locale selected. (2) Double-booking 409 error shown as in-form alert (ServerValidationAlert pattern).
actual: (1) Appointment type shown in English regardless of locale. (2) 409 error shown as toast notification.
errors: 409 Conflict on overlapping appointment booking
reproduction: (1) Switch to VI locale, open appointment detail. (2) Book overlapping appointment for same doctor.
started: Unknown

## Eliminated

## Evidence

- timestamp: 2026-03-09
  checked: Backend GetAppointmentsByDoctorHandler (line 40) and GetAppointmentsByPatientHandler (line 38)
  found: Both handlers set appointmentTypeName to `appointmentType?.Name ?? "Unknown"` - only English Name, never NameVi
  implication: Backend only returns English appointment type name in AppointmentDto

- timestamp: 2026-03-09
  checked: AppointmentDto backend DTO
  found: Single field `AppointmentTypeName` (no NameVi field). AppointmentTypeDto has both Name and NameVi, but AppointmentDto only has one name field populated with English name.
  implication: The AppointmentDto needs both Name and NameVi, OR the frontend needs to look up names from cached appointment types

- timestamp: 2026-03-09
  checked: AppointmentDetailDialog.tsx line 159
  found: Renders `{appointment.appointmentTypeName}` raw, no translation lookup
  implication: Even if backend sent NameVi, there's no locale-aware selection logic

- timestamp: 2026-03-09
  checked: AppointmentBookingDialog.tsx line 309
  found: Booking dropdown hardcodes `type.nameVi` (ignores EN locale). Related sub-bug.
  implication: The booking dropdown also has a locale bug (always Vietnamese)

- timestamp: 2026-03-09
  checked: i18n scheduling.json files
  found: Both EN and VI have `types.newPatient`, `types.followUp`, `types.treatment`, `types.orthoK` keys
  implication: Translation keys exist but are NOT used for display -- the data-driven approach (Name/NameVi from DB) is used instead

- timestamp: 2026-03-09
  checked: AppointmentBookingDialog.tsx lines 167-177 (onError handler)
  found: DOUBLE_BOOKING caught on line 168 -> toast.error() on line 169. Falls through to handleServerValidationError only for non-DOUBLE_BOOKING, non-VALIDATION_ERROR errors.
  implication: DOUBLE_BOOKING and VALIDATION_ERROR errors bypass ServerValidationAlert, shown as toast instead

- timestamp: 2026-03-09
  checked: AppointmentBookingDialog.tsx lines 81, 191-194
  found: nonFieldError state and ServerValidationAlert component already wired up in the form. The infrastructure is there but DOUBLE_BOOKING errors bypass it.
  implication: Fix is straightforward: route DOUBLE_BOOKING to setNonFieldError instead of toast.error

## Resolution

root_cause: See per-issue root causes below
fix:
verification:
files_changed: []
