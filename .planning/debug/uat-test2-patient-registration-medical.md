---
status: diagnosed
trigger: "UAT Test 2 Patient Registration (Medical) - 4 sub-issues: DatePicker misalignment, redirect to /patients/undefined, allergy autocomplete broken, server validation errors not shown"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: All four sub-issues confirmed through code reading - four discrete bugs with clear root causes
test: Static code analysis complete
expecting: N/A
next_action: Return diagnosis to caller

## Symptoms

expected: DatePicker dropdowns/chevrons align correctly, form submits and navigates to /patients/{id}, allergy autocomplete works with Vietnamese categories and free-text, server validation errors appear per-field
actual: DatePicker month/year dropdowns and chevrons misaligned, redirect goes to /patients/undefined, allergy autocomplete lacks categories and free-text in registration form, server validation errors only show as generic toast
errors: Navigation to /patients/undefined after successful registration
reproduction: Open /patients -> Register -> fill Medical form -> submit
started: Current implementation

## Eliminated

- hypothesis: Backend route missing for POST /api/patients
  evidence: PatientApiEndpoints.cs line 38 correctly maps MapPost("/") on group "/api/patients"; handler returns Result<Guid>
  timestamp: 2026-03-02T00:00:00Z

## Evidence

- timestamp: 2026-03-02T00:00:00Z
  checked: calendar.tsx nav classNames vs react-day-picker v9 default CSS (style.css lines 250-259)
  found: The calendar.tsx applies Tailwind classes AND appends defaultClassNames (rdp-*) via cn(). The default react-day-picker CSS for .rdp-nav uses "position: absolute; inset-block-start: 0; inset-inline-end: 0;" which conflicts with the Tailwind "flex w-full items-center justify-between" on the nav element. Additionally, when captionLayout="dropdown", the month_caption and nav are siblings in the DOM - month_caption contains the dropdowns while nav contains only prev/next buttons. Both have "w-full" creating layout conflicts. The shadcn calendar was likely customized from an older version or has drift from the official latest.
  implication: The calendar component does not match the official shadcn/ui Calendar. Must be replaced with the exact latest version from shadcn/ui.

- timestamp: 2026-03-02T00:00:00Z
  checked: ResultExtensions.cs line 44 ToCreatedHttpResult + patient-api.ts line 183
  found: Backend returns HTTP 201 with body "new { Id = result.Value }" where result.Value is a Guid. ASP.NET Core uses camelCase by default, so the actual JSON response body is { "id": "guid-string" }. But patient-api.ts line 183 does "return (res.data as { Id: string }).Id" - using PascalCase "Id" to extract from a camelCase "id" response. This returns undefined. Then PatientRegistrationForm.tsx line 133 navigates to "/patients/$patientId" with patientId=undefined.
  implication: ROOT CAUSE of /patients/undefined: case mismatch between backend JSON response ("id") and frontend extraction ("Id"). Fix: change line 183 to use lowercase "id".

- timestamp: 2026-03-02T00:00:00Z
  checked: PatientRegistrationForm.tsx AllergyRow (lines 342-458) vs AllergyForm.tsx (lines 57-268)
  found: AllergyRow in registration form has multiple problems - (1) maps catalog to flat { label, value } without category, so no category grouping is shown; (2) uses Popover+Command pattern but typing in the Input directly updates the form field value to the display text, not the canonical English name for backend; (3) no "add custom" / free-text entry option exists - unlike AllergyForm.tsx which has a "custom: text" CommandItem for non-catalog entries; (4) filtering works but no visual category headers. AllergyForm.tsx is a much better implementation with categories grouped by CommandGroup headings, shouldFilter={false}, and proper custom entry support.
  implication: The allergy autocomplete in PatientRegistrationForm is a weak implementation that should be replaced. User says "delete current autocomplete component and install a new one" - suggesting replace with a proper combobox/autocomplete component or at minimum port AllergyForm.tsx's pattern.

- timestamp: 2026-03-02T00:00:00Z
  checked: PatientRegistrationForm.tsx handleSubmit (lines 109-141) and all other form components
  found: Server validation errors are caught in try/catch and displayed only as toast.error with the raw error.message string. The backend returns validation errors as a semicolon-separated string in the Problem Details "detail" field (RegisterPatient.cs line 61: string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))). There is NO per-field error mapping - all validation errors are concatenated into one string. The frontend has no mechanism to parse these back to field-level errors and call form.setError(). This pattern is repeated across all forms (AllergyForm, PatientOverviewTab, PatientProfileHeader).
  implication: Two changes needed: (1) Backend must return structured validation errors with field names (e.g., RFC 7807 errors array with property names), not a flat string. (2) Frontend needs a shared utility that parses structured server errors and maps them to react-hook-form setError() calls. For user-friendly exceptions: in dialogs show above the first field (below tabs if tabs exist); in non-dialog forms show in Alert dialog.

## Resolution

root_cause: |
  Four discrete issues:

  1. **DatePicker misalignment** (calendar.tsx):
     - File: frontend/src/shared/components/ui/calendar.tsx
     - The calendar component has drifted from the official shadcn/ui Calendar. The classNames combine custom Tailwind with defaultClassNames (rdp-* classes) via cn(). The default react-day-picker CSS for .rdp-nav uses "position: absolute" (style.css line 252) which conflicts with the Tailwind "flex w-full justify-between" override. When captionLayout="dropdown", the month_caption and nav are siblings where month_caption holds dropdowns and nav holds chevrons - both with "w-full" creating overlap/misalignment.
     - Fix: Replace calendar.tsx entirely with the exact latest version from shadcn/ui (npx shadcn@latest add calendar). This ensures 100% match with official component styling including dropdown mode.

  2. **Redirect to /patients/undefined** (patient-api.ts line 183):
     - File: frontend/src/features/patient/api/patient-api.ts, line 183
     - Backend returns { "id": "guid" } (camelCase, ASP.NET Core default). Frontend extracts with PascalCase: (res.data as { Id: string }).Id -- returns undefined because "Id" !== "id".
     - File: backend/src/Shared/Shared.Presentation/ResultExtensions.cs, line 44
     - Also consider: the anonymous object "new { Id = result.Value }" gets serialized to "id" in camelCase.
     - Fix: Change patient-api.ts line 183 from ".Id" to ".id" -- i.e., return (res.data as { id: string }).id

  3. **Allergy autocomplete broken** (PatientRegistrationForm.tsx AllergyRow):
     - File: frontend/src/features/patient/components/PatientRegistrationForm.tsx, lines 342-458
     - AllergyRow has: no category grouping (flat list), no free-text entry option, typing updates form value to display label (not canonical English name for backend), no shouldFilter={false} on Command. Compare with AllergyForm.tsx which has all of these features.
     - Fix: User explicitly says to "delete current autocomplete component and install a new one". Replace the Popover+Command in AllergyRow with a proper autocomplete/combobox component that supports: Vietnamese categories, free-text entry, proper value mapping (display label -> backend canonical name).

  4. **Server validation errors not shown per-field**:
     - Backend: RegisterPatient.cs line 61 joins all errors into one string: string.Join("; ", errors)
     - Backend: ResultExtensions.cs line 59-62 returns this as Problem Details with "detail" field
     - Frontend: PatientRegistrationForm.tsx line 138 shows toast.error with the flat string
     - No per-field error mapping exists anywhere in the codebase.
     - Fix requires two parts:
       (a) Backend: Return structured validation errors with property names (e.g., use FluentValidation's error dictionary with PropertyName -> ErrorMessage mapping in the Problem Details "errors" field, matching RFC 7807 standard).
       (b) Frontend: Create a shared utility (e.g., useServerValidation hook or handleServerErrors function) that parses the structured error response and calls form.setError() for each field. For user-friendly exceptions in dialogs: show at top of form (below tabs if form has tabs). For non-dialog forms: show in Alert dialog. Make this a reusable pattern across the app.

fix: Not applied (diagnose-only mode)
verification: ""
files_changed: []
