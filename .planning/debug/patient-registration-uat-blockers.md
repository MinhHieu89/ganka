---
status: diagnosed
trigger: "UAT Test 2 - Patient Registration: date picker year/month, tab switch, allergy autocomplete, dialog sticky header/footer, 404 on submit"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: All five issues confirmed through code reading — no single root cause, five discrete bugs
test: Static code analysis complete
expecting: N/A
next_action: Return diagnosis to caller

## Symptoms

expected: Patient registration form opens, allows quick date navigation, allergy autocomplete, sticky dialog chrome, and successful POST
actual: Date picker slow to navigate by year/month; tab switching uses custom button not standard tabs; allergy autocomplete missing in registration inline rows; dialog title/footer not visually sticky; POST returns 404 and navigate receives object not GUID
errors: 404 on POST /api/patients; navigate called with { Id: guid } object cast as string
reproduction: Open /patients -> click Register -> fill form -> submit
started: Current implementation

## Eliminated

- hypothesis: Backend route missing for POST /api/patients
  evidence: PatientApiEndpoints.cs line 36 correctly maps MapPost("/") on group "/api/patients"
  timestamp: 2026-03-02T00:00:00Z

- hypothesis: CORS or auth blocking the request
  evidence: Other patient endpoints work; CORS and auth configured; the 404 is a client-side routing/response parsing issue
  timestamp: 2026-03-02T00:00:00Z

## Evidence

- timestamp: 2026-03-02T00:00:00Z
  checked: DatePicker.tsx
  found: Uses react-day-picker with captionLayout="dropdown" — dropdown IS present, but the calendar renders inside a Popover that is not constrained in height. No issue with the component itself for quick year/month selection; the dropdown IS there.
  implication: The complaint is likely UX — dropdowns exist but are native <select> elements inside the Popover, which may be hard to interact with. captionLayout="dropdown" is correct.

- timestamp: 2026-03-02T00:00:00Z
  checked: PatientRegistrationForm.tsx lines 158-173
  found: Patient type switching (Medical/WalkIn) is done with <Tabs> component already — this IS tabs, not buttons.
  implication: The form already uses tabs for type switching. The complaint "use tabs instead of button" is already done. No fix needed here unless the visual is broken.

- timestamp: 2026-03-02T00:00:00Z
  checked: PatientRegistrationForm.tsx AllergyRow component lines 362-415
  found: AllergyRow in the registration form uses a Popover + Command (autocomplete pattern) but it uses `catalogItems` mapped to just a flat string array (en or vi name only), not the rich { label, value, category } structure used in AllergyForm.tsx. Also inputValue state is local but the filtered list uses item.toLowerCase() on the raw string. The display does NOT show category, and when selecting it sets the raw display string as the field value — not the English backend key.
  implication: The inline allergy autocomplete in the registration form is weaker than AllergyForm.tsx. The onSelect sets the display label as value, not the canonical English name. This means the wrong value may be sent to the backend.

- timestamp: 2026-03-02T00:00:00Z
  checked: PatientRegistrationForm.tsx lines 154, 325
  found: DialogContent has "max-h-[90vh] flex flex-col overflow-hidden". DialogHeader has "shrink-0". DialogFooter has "shrink-0 border-t pt-4 mt-2". The scrollable area is `<div className="flex-1 overflow-y-auto ...">`.
  implication: The title IS sticky (shrink-0 in header) and footer IS sticky (shrink-0 outside scroll area). However, the DialogFooter is INSIDE the <form> tag but the form is inside DialogContent. The layout structure: DialogContent > [DialogHeader (shrink-0), form > [scroll-div, DialogFooter (shrink-0)]]. This is CORRECT — the footer should be fixed. BUT the DialogContent uses `grid` layout by default (from dialog.tsx line 41: "grid ... gap-4") which conflicts with the flex column layout applied via className. The `grid` base class on DialogContent overrides the flex attempt.

- timestamp: 2026-03-02T00:00:00Z
  checked: ResultExtensions.cs ToCreatedHttpResult + patient-api.ts registerPatient
  found: Backend returns HTTP 201 with body `{ "Id": "guid-string" }` (line 44: `new { Id = result.Value }`). Frontend does `return res.data as string` (line 171). res.data is actually `{ Id: "guid-string" }` object, not a plain string. Then navigate is called with `params: { patientId }` where patientId is `{ Id: "guid-string" }` — an object, not a string. This causes the route to build a broken URL like `/patients/[object Object]` resulting in a 404 when the profile page tries to fetch by that invalid ID.
  implication: ROOT CAUSE of 404: registerPatient returns the whole `{ Id }` object instead of extracting the ID string.

## Resolution

root_cause: |
  Five discrete issues:
  1. DatePicker: captionLayout="dropdown" is present but the Popover height may clip the calendar on small screens. Minor UX issue.
  2. Patient type tabs: Already implemented correctly with <Tabs> component — no fix needed.
  3. Allergy autocomplete in registration form: AllergyRow maps catalog to flat string array and onSelect sets display label (not canonical English name) as form value. Should mirror AllergyForm.tsx's rich { label, value, category } pattern.
  4. Dialog sticky layout: DialogContent base class is `grid` (from dialog.tsx), which conflicts with the `flex flex-col` className override. The fix requires either overriding the grid via inline style or modifying the DialogContent className to use flex instead of grid.
  5. 404 on submit: registerPatient returns `res.data` which is `{ Id: guid }` object. Frontend casts it as string and passes to navigate(), building URL `/patients/[object Object]` — causing 404 on the patient profile GET. Fix: extract `(res.data as { Id: string }).Id`.

fix: Not applied (diagnose-only mode)
verification: ""
files_changed: []
