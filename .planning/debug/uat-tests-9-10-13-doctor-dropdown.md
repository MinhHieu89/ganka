---
status: diagnosed
trigger: "UAT Tests 9, 10, 13 - Doctor dropdown empty across multiple views"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: DoctorSelector calls wrong API endpoint (/api/auth/users) instead of /api/admin/users; the response shape also wraps data in a {data, totalCount} envelope; textarea elements missing rounded-corner class; datetime-local input is a plain HTML input not a shadcn Popover+Calendar component; PendingBookingsPanel dialogs lack spacing and Field structure
test: read all relevant source files
expecting: confirmed all above
next_action: fix all issues

## Symptoms

expected: Doctor dropdowns show users with Doctor role; datetime picker styled correctly; textarea has rounded corners; approve/reject dialogs have good spacing
actual: Doctor dropdowns empty; datetime input is plain HTML; textareas have no border-radius; approve/reject dialogs feel cramped
errors: no JS errors; silent empty state
reproduction: open any appointment booking dialog or pending booking panel
started: since initial implementation

## Eliminated

- hypothesis: backend endpoint missing or not filtering roles
  evidence: /api/admin/users exists and returns Roles[] per user; filtering by role "Doctor" is handled in the frontend - backend returns all users with roles array
  timestamp: 2026-03-02

## Evidence

- timestamp: 2026-03-02
  checked: DoctorSelector.tsx line 23
  found: calls api.GET("/api/auth/users") - this endpoint does NOT exist. The correct endpoint is /api/admin/users
  implication: every call returns a 404/error, the catch converts empty result to [], dropdown is always empty

- timestamp: 2026-03-02
  checked: AuthApiEndpoints.cs MapAuthFlowEndpoints vs MapAdminUserEndpoints
  found: /api/auth/users does not exist; /api/admin/users returns { data: UserDto[], totalCount, page, pageSize }
  implication: even if the URL were fixed, DoctorSelector casts raw data as array but actual response is an envelope object with .data array

- timestamp: 2026-03-02
  checked: UserDto.cs
  found: roles field is List<string> Roles (PascalCase in C# -> camelCase "roles" in JSON serialization)
  implication: filter u.roles?.some(r => r === "Doctor") is correct once URL and envelope are fixed

- timestamp: 2026-03-02
  checked: AppointmentBookingDialog.tsx line 290-296
  found: startTime field uses <Input type="datetime-local"> (plain HTML native browser input)
  implication: inconsistent with shadcn/ui look-and-feel; should be a shadcn Popover+Calendar + time input combination

- timestamp: 2026-03-02
  checked: AppointmentBookingDialog.tsx line 302-307 and PendingBookingsPanel.tsx line 260-265, AppointmentDetailDialog.tsx line 199-204
  found: raw <textarea> elements with className that includes border styles but missing rounded-md class; no rounded corners applied
  implication: textareas look square while all other inputs (Input component) have rounded corners via Tailwind; needs rounded-md added

- timestamp: 2026-03-02
  checked: PendingBookingsPanel.tsx approve dialog (lines 207-223)
  found: uses raw <label> instead of FieldLabel component, no Field wrapper, no gap between form rows, no Field/FieldLabel usage - minimal spacing
  implication: form feels cramped vs the rest of the app

- timestamp: 2026-03-02
  checked: PendingBookingsPanel.tsx reject dialog (lines 257-266)
  found: same raw <label> and raw <textarea> pattern, no Field wrapper
  implication: styling inconsistency

## Resolution

root_cause: |
  DoctorSelector fetches /api/auth/users which does not exist (404). The correct
  endpoint is /api/admin/users which returns a paginated envelope {data: [], totalCount, page, pageSize}.
  Additionally: (1) response envelope not unwrapped - data needs to be accessed as response.data,
  (2) textareas lack rounded-md class globally causing square corners,
  (3) datetime-local uses native HTML input instead of shadcn Popover+Calendar,
  (4) approve/reject dialogs lack Field wrapper spacing.

fix: pending
verification: pending
files_changed: []
