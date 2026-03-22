---
status: resolved
trigger: "patient-registration-duplicate-phone-error: duplicate phone shows generic error instead of field-level"
created: 2026-03-22T00:00:00Z
updated: 2026-03-22T00:00:00Z
---

## Current Focus

hypothesis: CONFIRMED - Backend Conflict response uses { error: "..." } but frontend looks for { detail/title }, losing the descriptive message
test: Traced full flow from backend Error.Conflict through ResultExtensions to frontend catch
expecting: N/A - root cause confirmed
next_action: Fix the issue at backend level to return proper RFC 7807 with field info

## Symptoms

expected: Duplicate phone number error appears as field-level validation under phone field
actual: Generic "Failed to register patient" banner at top, no field-level error
errors: "Failed to register patient" generic banner
reproduction: Open patient registration -> enter existing phone (0934348344) -> submit -> generic error
started: Likely always been this way

## Eliminated

## Evidence

- timestamp: 2026-03-22T00:01:00Z
  checked: Backend RegisterPatientHandler returns Error.Conflict("A patient with this phone number already exists.")
  found: Error.Conflict maps to Results.Conflict(new { error = error.Description }) in ResultExtensions - HTTP 409 with body { error: "..." }
  implication: Response has "error" (singular) property, not "detail" or "title" or "errors" (plural)

- timestamp: 2026-03-22T00:02:00Z
  checked: Frontend registerPatient function in patient-api.ts
  found: Checks err.errors first (not present), then falls back to err.detail || err.title || "Failed to register patient" - none match the { error: "..." } shape
  implication: The descriptive message "A patient with this phone number already exists." is completely lost

- timestamp: 2026-03-22T00:03:00Z
  checked: Frontend handleSubmit catch block checks for "phone" and "already exists" in error message
  found: But error message is "Failed to register patient" (the fallback), not the original backend message
  implication: The phone duplicate detection code exists but can never trigger because the message is lost upstream

## Resolution

root_cause: Two-layer failure: (1) Backend Error.Conflict maps to Results.Conflict(new { error = ... }) which returns { error: "..." } - a non-standard shape that frontend cannot parse. (2) Frontend registerPatient() looks for .detail or .title on the response but not .error, so falls back to generic "Failed to register patient" message, which prevents the phone-specific catch from matching.
fix: (1) Backend RegisterPatient.cs: Changed Error.Conflict to Error.ValidationWithDetails with Phone field key, so duplicate phone returns as RFC 7807 structured validation error. (2) Backend ResultExtensions.cs: Changed Conflict mapping from custom { error } shape to standard RFC 7807 Problem format for consistency. (3) Frontend PatientRegistrationForm.tsx: Reordered error handling to let handleServerValidationError run first (maps Phone field), then override with localized message for phone duplicates.
verification: Backend builds successfully, all 22 patient unit tests pass, all 8 ResultExtensions tests pass. Human verified: duplicate phone error now appears under the phone field as expected.
files_changed: [backend/src/Modules/Patient/Patient.Application/Features/RegisterPatient.cs, backend/src/Shared/Shared.Presentation/ResultExtensions.cs, frontend/src/features/patient/components/PatientRegistrationForm.tsx]
