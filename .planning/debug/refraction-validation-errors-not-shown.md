---
status: diagnosed
trigger: "Refraction validation errors not shown under fields"
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:00:00Z
---

## Current Focus

hypothesis: RefractionForm onError handler discards server validation details -- only shows toast
test: Traced full error flow from backend validator to frontend UI
expecting: n/a -- root cause confirmed
next_action: report diagnosis

## Symptoms

expected: When refraction auto-save fails with validation errors (e.g., UcvaOd out of range), the validation message should display under the specific field
actual: A 400 Bad Request error is returned with validation details but error is not shown under fields -- only a generic toast error appears
errors: 400 with `{ "UcvaOd.Value": ["VA must be between 0.01 and 2.0."] }` returned but not mapped to form fields
reproduction: Enter an out-of-range value (e.g. UCVA OD = 5.0), blur the field, observe only toast error
started: Since implementation -- never worked

## Eliminated

(none -- root cause found on first hypothesis)

## Evidence

- timestamp: 2026-03-09T00:00:00Z
  checked: RefractionForm.tsx onError handler (line 177)
  found: onError callback is `() => { toast.error(t("refraction.saveFailed")) }` -- the `error` parameter is not even accepted, so the server validation response is completely discarded
  implication: No mechanism exists to route server errors to form fields

- timestamp: 2026-03-09T00:00:00Z
  checked: renderNumberInput function (lines 205-233)
  found: No error display whatsoever -- no `form.formState.errors[name]` check, no error message element rendered below inputs
  implication: Even if setError were called, errors would be invisible because the UI doesn't render them

- timestamp: 2026-03-09T00:00:00Z
  checked: shared/lib/server-validation.ts (handleServerValidationError utility)
  found: A fully implemented utility exists that parses RFC 7807 validation responses, maps PascalCase backend field names to camelCase, and calls form.setError() per field. Used by 8+ other forms in the codebase.
  implication: The solution pattern already exists -- RefractionForm simply doesn't use it

- timestamp: 2026-03-09T00:00:00Z
  checked: updateRefraction in clinical-api.ts (lines 385-421)
  found: When errors exist (`err?.errors`), the function throws `new Error(JSON.stringify(err))` -- this preserves the structured JSON body in the error message, which is exactly what handleServerValidationError expects to parse
  implication: The API layer already passes structured errors correctly -- the form just doesn't consume them

- timestamp: 2026-03-09T00:00:00Z
  checked: Backend validator (UpdateVisitRefraction.cs lines 22-53)
  found: FluentValidation uses `.Value` accessor on nullable types, e.g. `RuleFor(x => x.UcvaOd!.Value)`. This produces property names like `UcvaOd.Value` in the errors dictionary, NOT `UcvaOd`
  implication: server-validation.ts camelCase conversion would produce `ucvaOd.Value` which does NOT match the form field name `ucvaOd`. A fieldMap is needed to strip `.Value` suffix, OR the server-validation utility needs to handle dot-notation

- timestamp: 2026-03-09T00:00:00Z
  checked: refractionSchema (lines 29-49)
  found: The Zod schema only transforms types (string/number/null) but has NO range validation rules. Frontend schema allows any numeric value through without checking min/max ranges.
  implication: Client-side validation doesn't catch out-of-range values, so they always reach the server

## Resolution

root_cause: |
  THREE gaps prevent server validation errors from appearing under refraction form fields:

  1. **onError discards the error** (RefractionForm.tsx line 177): The mutation's `onError` callback is
     `() => { toast.error(...) }` -- it doesn't accept the `error` argument at all, so the structured
     validation response from the server is completely ignored.

  2. **No error rendering in UI** (RefractionForm.tsx lines 205-233): The `renderNumberInput` function
     never reads `form.formState.errors` and never renders error messages below inputs. Even if
     `form.setError()` were called, errors would remain invisible.

  3. **Backend field name mismatch** (UpdateVisitRefraction.cs): FluentValidation's `.Value` accessor
     on nullable types produces error keys like `UcvaOd.Value` instead of `UcvaOd`. The
     `handleServerValidationError` utility's simple PascalCase-to-camelCase conversion would produce
     `ucvaOd.Value`, which doesn't match the form field name `ucvaOd`. A `fieldMap` parameter or
     `.Value` suffix stripping is needed.

fix: (not applied -- diagnosis only)
verification: (not applicable)
files_changed: []
