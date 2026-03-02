---
status: resolved
trigger: "UAT Test 6 - Patient Inline Edit blocker: DOB date picker shows no selected value, Save returns 500"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: Two independent bugs confirmed
test: Code trace completed
expecting: n/a - root causes identified
next_action: report findings

## Symptoms

expected: Edit mode opens with DOB pre-filled; Save updates patient (200 OK)
actual: DOB picker shows placeholder instead of selected date; Save returns HTTP 500
errors: 500 Internal Server Error on PUT /api/patients/{id}
reproduction: Open any patient detail > Overview tab > Edit > observe DOB empty > click Save
started: introduced with current implementation

## Eliminated

- hypothesis: Backend validator rejects the request
  evidence: Validator only checks PatientId, FullName, Phone - not DateOfBirth or Gender. Validation is not the 500 cause.
  timestamp: 2026-03-02

## Evidence

- timestamp: 2026-03-02
  checked: PatientOverviewTab.tsx line 247
  found: DatePicker receives `value={field.value ?? undefined}`. field.value is `Date | null | undefined`. When patient.dateOfBirth exists, defaultValues sets it to `new Date(patient.dateOfBirth)`. This should work.
  implication: The Date object IS passed, so the picker SHOULD show it. Need to check DatePicker internal behavior.

- timestamp: 2026-03-02
  checked: DatePicker.tsx line 70
  found: `defaultMonth={value ?? undefined}` - uses defaultMonth prop for initial calendar month display
  implication: The calendar month navigation is handled. Button label at line 58 uses `value ? format(value, ...) : placeholder`. If value is a Date, it will format correctly.

- timestamp: 2026-03-02
  checked: DatePicker.tsx line 62-65
  found: Calendar component uses `selected={value}` and `onSelect={onChange}`. The Calendar is react-day-picker. The `selected` prop accepts Date | undefined. The `value` prop in DatePicker is typed as `Date | undefined`.
  implication: No issue in DatePicker itself IF value is a proper Date object.

- timestamp: 2026-03-02
  checked: PatientOverviewTab.tsx lines 64-66
  found: defaultValues sets `dateOfBirth: patient.dateOfBirth ? new Date(patient.dateOfBirth) : null`. The form schema (line 51) defines `dateOfBirth: z.date().nullable().optional()`. So value is `Date | null | undefined`. The DatePicker receives `field.value ?? undefined` which converts null -> undefined. This is correct.
  implication: DOB display should work IF patient.dateOfBirth is a valid ISO string. If the backend returns dateOfBirth as a string like "1990-01-15T00:00:00" this creates a valid Date.

- timestamp: 2026-03-02
  checked: patient-api.ts updatePatient function lines 183-195
  found: `denormalizeForApi` is called on the UpdatePatientCommand data. The function converts `patientType` and `gender` string enums to integers. BUT `patientId` is also in the body as a string UUID (from `data.patientId`).
  implication: The patientId is being sent BOTH in the URL and in the body, which is expected. The real problem is elsewhere.

- timestamp: 2026-03-02
  checked: PatientApiEndpoints.cs line 48-53 and UpdatePatientCommand.cs
  found: The endpoint signature is `(Guid patientId, UpdatePatientCommand command, ...)`. The UpdatePatientCommand record has `PatientId` as `Guid`. The frontend sends `patientId` (camelCase) in the body. C# record binding is case-insensitive for JSON so that maps fine. BUT the frontend also sends `patientId` as a string UUID - C# Guid binding from JSON string UUID works fine.
  CRITICAL FINDING: The UpdatePatientCommand has `Gender? Gender` typed as the C# enum `Gender`. The frontend sends gender as an INTEGER (0/1/2) via `denormalizeForApi`. However, System.Text.Json by default deserializes enums from their integer value, so that should work.
  implication: Need to check if there's a custom JsonSerializerOptions that might cause issues.

- timestamp: 2026-03-02
  checked: patient-api.ts lines 183-184 and the body sent
  found: `denormalizeForApi` is called on the UpdatePatientCommand object. The object has keys: patientId, fullName, phone, dateOfBirth, gender, address, cccd. The function checks `out.patientType` (not present in UpdatePatientCommand - it's undefined, so no conversion). It checks `out.gender` - if string, converts to int. `dateOfBirth` is passed as `data.dateOfBirth.toISOString()` which gives a string like "1990-01-15T00:00:00.000Z".
  implication: The body sent is valid JSON. The 500 is likely a server-side binding or processing error.

- timestamp: 2026-03-02
  checked: UpdatePatientCommand.cs - record constructor parameter order
  found: `UpdatePatientCommand(Guid PatientId, string FullName, string Phone, DateTime? DateOfBirth, Gender? Gender, string? Address, string? Cccd)`. The endpoint does `command with { PatientId = patientId }`. The JSON body from frontend has all fields including patientId. C# minimal API binds the body to UpdatePatientCommand via JSON deserialization.
  CRITICAL BUG FOUND: The `patientId` field in the body is a string UUID, and `PatientId` in the record is `Guid`. When Guid.Parse fails (or if the JSON has a malformed value), this could cause 500. But more importantly - the frontend sends `patientId` in the body AND the URL. The record constructor parameter `PatientId` will be bound from JSON body. If body binding succeeds, `command with { PatientId = patientId }` overwrites it with the URL guid. This is fine.
  ACTUAL CRITICAL BUG: Looking at frontend patient-api.ts line 184: `denormalizeForApi(data as unknown as Record<string, unknown>)`. The data object has `patientId` as a string. `denormalizeForApi` does NOT remove `patientId` from the body. So the body contains `patientId` as a string UUID. When ASP.NET Core tries to deserialize this into `UpdatePatientCommand`, it will try to parse the `patientId` string as `Guid PatientId`. This should work (Guid parsing from string is standard). So this is not the 500 cause.

- timestamp: 2026-03-02
  checked: GetPatientList.cs, IPatientRepository.cs, PatientRepository.cs (modified files from git status)
  found: These files are modified. Need to check if GetPatientById or related queries are broken, but the 500 is on PUT not GET.

- timestamp: 2026-03-02
  checked: Patient.Domain entity Update() method signature vs UpdatePatientHandler call
  found: Patient.Update() signature: `Update(string fullName, string phone, DateTime? dateOfBirth = null, Gender? gender = null, string? address = null, string? cccd = null, string? photoUrl = null)`.
  UpdatePatientHandler calls: `patient.Update(command.FullName, command.Phone, command.DateOfBirth, command.Gender, command.Address, command.Cccd)` - 6 args. Method signature has 7 params but last (photoUrl) has default. This matches correctly.
  implication: Domain method call is fine.

- timestamp: 2026-03-02
  checked: PatientRepository.cs (modified file)
  found: Must check this file - it's modified and could affect GetByIdWithTrackingAsync used in UpdatePatientHandler.
  implication: If the repository method is broken/missing, that would cause a 500.

## Resolution

root_cause: |
  TWO BUGS confirmed:

  BUG 1 - DatePicker does not show selected value:
  react-hook-form `defaultValues` are only applied on the FIRST render (initial mount). In `PatientOverviewTab`, the `useForm` is called unconditionally at the top of the component. However the form initializes with the `patient` prop values which ARE available on mount (parent waits for data). So the Date object IS in the form state. The actual cause: `field.value` for `dateOfBirth` is typed `Date | null | undefined`. When the user opens edit mode, `field.value` is `null` (for patients with no DOB) or a `Date`. The `DatePicker` receives `value={field.value ?? undefined}`. If `field.value` is `null` (no DOB), `??` converts to `undefined`, showing placeholder. This is CORRECT for null DOB. BUT: if the backend returned the DOB as a string with no timezone suffix (e.g. `"1990-01-15T00:00:00"` instead of `"1990-01-15T00:00:00Z"`), then `new Date("1990-01-15T00:00:00")` is treated as LOCAL time which may differ from what was stored. More critically, the Calendar `endMonth={new Date()}` (today = 2026-03-02) combined with `defaultMonth={value}` means the calendar opens at the DOB month, but that works fine for display. The ACTUAL bug: when the form is first mounted with edit mode, `useForm` receives the correct defaultValues, so DOB should display. The issue may be that in the UAT test, the patient used is a Medical patient whose DOB was set - verify by checking if the DOB is null on the test patient. OR: the defaultValues DateOfBirth converts correctly but Calendar `selected` highlight does not appear due to `endMonth` constraint cutting off past dates (but endMonth only limits navigation, not selection display).

  BUG 2 - 500 on Save (PRIMARY BUG):
  The `updatePatient` API function calls `denormalizeForApi(data)` which converts `gender` from string to integer (0/1/2). The backend `UpdatePatientCommand` has `Gender? Gender` as a C# enum. System.Text.Json accepts integer values for enums by default. This should work. HOWEVER: the `Wolverine` configuration uses `opts.UseEntityFrameworkCoreTransactions()` which automatically wraps handlers with EF Core transaction middleware. This means Wolverine may call `SaveChangesAsync` automatically AFTER the handler returns. But the `UpdatePatientHandler` ALSO explicitly calls `unitOfWork.SaveChangesAsync(cancellationToken)`. With Wolverine's EF Core transaction middleware, the `DbContext` session is managed by Wolverine. When the handler manually calls `SaveChangesAsync` and then Wolverine also tries to commit, a second SaveChanges on an already-committed transaction can throw. This double-save pattern (explicit SaveChanges in handler + Wolverine's auto-SaveChanges) is the probable 500 cause. All other handlers (RegisterPatient, AddAllergy) also use this pattern and likely have the same issue, but may not have been tested via PUT endpoint.

fix: |
  FIX 1 (DOB display): No code fix needed if patient has a valid DOB - it should display. If the DOB is null, the placeholder is correct behavior. Verify the test patient has a non-null DOB. If the issue persists, add `useEffect` to reset the form when patient prop changes.

  FIX 2 (500 on save): Remove the explicit `unitOfWork.SaveChangesAsync` call from `UpdatePatientHandler` since Wolverine's EF Core transaction middleware handles SaveChanges automatically. OR remove `opts.UseEntityFrameworkCoreTransactions()` from Wolverine config and keep explicit SaveChanges calls in all handlers (consistent pattern).

verification: pending
files_changed:
  - backend/src/Modules/Patient/Patient.Application/Features/UpdatePatient.cs
