---
status: diagnosed
trigger: "Phase 02 UAT Test 7 - 500 Internal Server Error when submitting add allergy form"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: Frontend sends AllergySeverity as a string ("Mild", "Moderate", "Severe") but backend expects an integer (0, 1, 2), causing JSON deserialization failure and 500 error
test: Traced full data flow from AllergyForm submit through API call to backend endpoint
expecting: Mismatch between frontend string enum and backend integer enum
next_action: Report diagnosis

## Symptoms

expected: Adding an allergy to a patient should succeed and show the new allergy in the list
actual: 500 Internal Server Error on form submission
errors: 500 Internal Server Error
reproduction: Open patient detail -> Allergy tab -> click Add Allergy -> fill name + severity -> click Save
started: Likely since initial implementation of standalone addAllergy endpoint

## Eliminated

(none needed -- root cause found on first hypothesis)

## Evidence

- timestamp: 2026-03-02T00:00:00Z
  checked: AllergyForm.tsx handleSubmit (line 79-95)
  found: Calls addAllergyMutation.mutateAsync with { patientId, name, severity } where severity is a string like "Mild"
  implication: Frontend sends string severity values

- timestamp: 2026-03-02T00:00:00Z
  checked: patient-api.ts addAllergy function (line 259-271)
  found: Sends body { name, severity } directly WITHOUT calling denormalizeForApi(). Severity remains a string.
  implication: The body sent to the backend contains severity as a string, not an integer

- timestamp: 2026-03-02T00:00:00Z
  checked: patient-api.ts registerPatient function (line 172-184)
  found: DOES call denormalizeForApi() which converts severity strings to integers (lines 161-167)
  implication: Patient registration with allergies works because it converts enums; addAllergy does not

- timestamp: 2026-03-02T00:00:00Z
  checked: patient-api.ts denormalizeForApi function (line 157-168)
  found: Converts severity string to int via severityToInt map, but only processes top-level `allergies` array
  implication: Even if addAllergy called denormalizeForApi, severity is not at top level -- it's a flat field, not inside an allergies array

- timestamp: 2026-03-02T00:00:00Z
  checked: Backend AddAllergyCommand (AddAllergy.cs line 9)
  found: record AddAllergyCommand(Guid PatientId, string Name, AllergySeverity Severity) -- Severity is C# enum AllergySeverity
  implication: Backend expects Severity as integer (0, 1, 2) in JSON since no JsonStringEnumConverter is configured

- timestamp: 2026-03-02T00:00:00Z
  checked: Backend AllergySeverity enum (Patient.Domain/Enums/AllergySeverity.cs)
  found: Mild = 0, Moderate = 1, Severe = 2
  implication: Confirms integer values expected

- timestamp: 2026-03-02T00:00:00Z
  checked: Backend Program.cs and global JSON config
  found: No JsonStringEnumConverter registered anywhere in backend src
  implication: ASP.NET uses default System.Text.Json behavior which serializes/deserializes enums as integers, not strings

- timestamp: 2026-03-02T00:00:00Z
  checked: Backend PatientApiEndpoints.cs MapAllergyEndpoints (line 88-93)
  found: Endpoint binds AddAllergyCommand from request body, then enriches PatientId from route
  implication: The deserialization of the request body must parse Severity from JSON -- string "Mild" cannot deserialize to integer enum

## Resolution

root_cause: The frontend `addAllergy` function in `patient-api.ts` (line 259-271) sends `severity` as a string (e.g., "Mild") but the backend `AddAllergyCommand` expects `AllergySeverity` as an integer (0, 1, 2). Unlike `registerPatient` which calls `denormalizeForApi()` to convert enum strings to integers, `addAllergy` sends the raw string directly. The backend has no `JsonStringEnumConverter` configured, so ASP.NET's default JSON deserialization fails when it encounters a string where it expects an integer, causing a 500 Internal Server Error.
fix: (not applied -- diagnosis only)
verification: (not applied -- diagnosis only)
files_changed: []
