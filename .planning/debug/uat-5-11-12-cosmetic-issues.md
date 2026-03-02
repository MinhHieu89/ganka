---
status: diagnosed
trigger: "UAT Tests 5, 11, 12 — profile header design, datepicker misalignment, missing rounded corners"
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: Three independent cosmetic/design issues confirmed via code reading
test: Static code analysis complete
expecting: N/A
next_action: Return diagnosis to caller

## Symptoms

expected: Test 5 — polished, elegant profile header; Test 11 — datepicker calendar dropdowns/chevrons aligned; Test 12 — booking status check card has rounded corners
actual: Test 5 — header is functional but visually flat/boring; Test 11 — same calendar misalignment as Test 2; Test 12 — status result container and all colored status boxes have sharp corners
errors: None (all cosmetic/UX)
reproduction: Test 5 — open any patient profile; Test 11 — open public self-booking, click date picker; Test 12 — check booking status with a valid reference
started: Current implementation

## Eliminated

(none — all hypotheses confirmed on first pass)

## Evidence

- timestamp: 2026-03-02T00:00:00Z
  checked: PatientProfileHeader.tsx (full file)
  found: |
    The header is a plain div with space-y-4. No Card, no background, no border, no shadow,
    no gradient, no visual separation from surrounding content. The avatar is basic (h-20 w-20)
    with a simple bg-primary/10 fallback. Patient info is plain text with no visual hierarchy
    beyond font sizes. Action buttons are small outline variants. There is no visual container,
    no accent color treatment, and no design finesse — it reads as a data dump, not a polished
    profile header. The AllergyAlert at the bottom also lacks rounded corners (bare border, no
    rounded-* class).
  implication: The header needs a design refresh — wrapping in a Card with subtle background,
    better avatar treatment (gradient ring, larger size), improved typography hierarchy,
    and visual grouping of metadata.

- timestamp: 2026-03-02T00:00:00Z
  checked: BookingForm.tsx line 194, DatePicker.tsx, calendar.tsx
  found: |
    BookingForm.tsx uses <DatePicker> at line 194. DatePicker.tsx imports Calendar from
    @/shared/components/ui/calendar (line 8) and passes captionLayout="dropdown" (line 67).
    This is the exact same Calendar component and DatePicker wrapper used in the patient
    registration form (Test 2). The calendar.tsx uses react-day-picker v9.14+ with
    getDefaultClassNames() which merges its own default CSS classes (rdp-* prefixed) with
    the custom Tailwind classes. The nav, dropdowns, and chevron layout conflict comes from
    the combination of default rdp classes and the custom classNames — specifically:
    - nav class (line 50-53) uses "flex w-full items-center justify-between gap-1" combined
      with defaultClassNames.nav
    - dropdowns class (line 68-71) uses "flex h-[--cell-size] w-full items-center justify-center
      gap-1.5" combined with defaultClassNames.dropdowns
    The default rdp classes add their own flex/positioning that conflicts with these overrides,
    causing misalignment of month/year dropdown selects relative to the chevron buttons.
  implication: Same root cause as Test 2 — the shared calendar.tsx component needs its
    nav/dropdown/caption class overrides adjusted to fully suppress the conflicting default
    rdp styles.

- timestamp: 2026-03-02T00:00:00Z
  checked: BookingStatusCheck.tsx (full file, grep for "rounded")
  found: |
    Zero occurrences of "rounded" anywhere in the component. Specific elements missing rounded:
    - Line 86: `<div className="border p-6 space-y-4">` — the main status result container
    - Line 97: `<div className="... bg-yellow-50 border border-yellow-200">` — pending status box
    - Line 111: `<div className="... bg-green-50 border border-green-200">` — approved status box
    - Line 130: `<div className="... bg-red-50 border border-red-200">` — rejected status box
    - Line 154: `<div className="... bg-destructive/10 border border-destructive/20">` — error box
    The Input component (ui/input.tsx) already has rounded-md built in, and the Button
    component inherits rounded from buttonVariants, so those are fine. But every manually-styled
    container div in the component has `border` without any `rounded-*` class.
  implication: Need to add rounded-lg (or rounded-md) to each of the 5 container divs.

- timestamp: 2026-03-02T00:00:00Z
  checked: AllergyAlert.tsx line 53
  found: |
    AllergyAlert also uses `border border-orange-200` without any rounded class on its
    container div. This is rendered inside PatientProfileHeader via line 234. Same pattern
    as BookingStatusCheck — missing rounded corners on manually-styled alert containers.
  implication: AllergyAlert.tsx needs rounded-lg too (related to Test 5 polish).

## Resolution

root_cause: |
  Three independent issues:

  1. TEST 5 (PatientProfileHeader — boring design):
     The header at PatientProfileHeader.tsx is a bare `div` with `space-y-4` (line 112).
     No Card wrapper, no background/shadow, no border, no visual container. The avatar is
     undersized (h-20 w-20) with plain bg-primary/10 fallback. Patient metadata is a flat
     list with minimal hierarchy. Action buttons are small outline variants with no emphasis.
     AllergyAlert also lacks rounded corners on its container. The overall effect is "data
     dump" rather than a polished profile card.

  2. TEST 11 (DatePicker calendar misalignment):
     Same root cause as Test 2. BookingForm.tsx line 194 uses DatePicker which uses the
     shared Calendar component (calendar.tsx). The Calendar merges custom Tailwind classNames
     with react-day-picker v9's getDefaultClassNames(), and the default rdp CSS classes for
     nav, dropdowns, and caption conflict with the Tailwind overrides, causing month/year
     dropdown selects and chevron buttons to be misaligned.
     Shared component: frontend/src/shared/components/ui/calendar.tsx

  3. TEST 12 (BookingStatusCheck — no rounded corners):
     All 5 container divs in BookingStatusCheck.tsx use `border` class without any `rounded-*`
     class. Lines 86, 97, 111, 130, 154. The result is sharp-cornered boxes that look
     inconsistent with the rest of the app (which uses rounded-md/rounded-lg via shadcn
     components with --radius: 0.625rem).

fix: Not applied (diagnose-only mode)
verification: ""
files_changed: []
