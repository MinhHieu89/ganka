---
status: resolved
trigger: "Investigate two issues in the refraction form: (1) validation error messages not localized, (2) number input doesn't accept decimals"
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:00:00Z
---

## Current Focus

hypothesis: Both root causes confirmed through direct code reading
test: Static analysis of code execution path
expecting: N/A - resolved
next_action: N/A - root cause analysis complete, do not fix

## Symptoms

expected:
  - Validation error messages displayed in Vietnamese (e.g., "UCVA phải nằm trong khoảng 0.01 đến 2.0")
  - Number inputs accept decimal values (e.g., typing "1.5" keeps "1.5")

actual:
  - Validation error messages appear in English (e.g., "VA must be between 0.01 and 2.0")
  - Number inputs coerce typed decimal input to integer (e.g., typing "5.0" becomes "5")

errors:
  - "VA must be between 0.01 and 2.0" (server validation error shown in English on Vietnamese UI)
  - Decimal input lost on blur (user types "1.5", field displays "1")

reproduction:
  - Enter a UCVA value outside range (e.g., "5") and blur → English error appears
  - Enter "1.5" in any refraction number field → value collapses to "1"

started: Unknown - both appear to be design gaps, not regressions

## Eliminated

- hypothesis: The Input component strips decimals at the HTML level
  evidence: frontend/src/shared/components/ui/input.tsx is a plain passthrough wrapper
    over a native <input>. It spreads all props including `step`, `min`, `max` directly to
    the DOM element. No value coercion occurs inside the component itself.
  timestamp: 2026-03-09

- hypothesis: FluentValidation messages are localised via ValidatorOptions.Global.LanguageManager
  evidence: Grep for "ValidatorOptions", "LanguageManager", "ValidatorLanguageManager" across
    the entire backend/src tree returns zero matches. No global locale override is configured.
  timestamp: 2026-03-09

- hypothesis: FluentValidation uses .resx resource files for Vietnamese translations
  evidence: Glob for **/*.resx across backend/src returns zero files. No resource files exist.
  timestamp: 2026-03-09

- hypothesis: The backend sends Accept-Language or locale context to select error message language
  evidence: Program.cs does not configure request localization middleware
    (no app.UseRequestLocalization, no AddLocalization, no IStringLocalizer). The language
    preference stored in localStorage ("ganka28-language") is never forwarded to the API.
  timestamp: 2026-03-09

## Evidence

- timestamp: 2026-03-09
  checked: backend/src/Modules/Clinical/Clinical.Application/Features/UpdateVisitRefraction.cs
  found: >
    All 18 WithMessage() calls in UpdateRefractionCommandValidator use English hardcoded string
    literals. Example:
      .WithMessage("VA must be between 0.01 and 2.0.")
      .WithMessage("SPH must be between -30 and +30.")
    No WithMessage overload using an IStringLocalizer<T> or resource key is used.
    FluentValidation's localisation API (WithLocalizedMessage / WithMessage(() => ...)) is
    not used anywhere in this validator.
  implication: >
    Every validation error that reaches the frontend is English only. There is no runtime
    path for these messages to become Vietnamese.

- timestamp: 2026-03-09
  checked: backend/src/Bootstrapper/Program.cs
  found: >
    No request localization middleware registered. The pipeline is:
      UseCors -> UseAuthentication -> UseAuthorization -> UseRateLimiter ->
      UseMiddleware<AccessLoggingMiddleware> -> MapWolverineEndpoints (with FluentValidation)
    There is no UseRequestLocalization(), no AddLocalization(), no IStringLocalizer injection.
    The backend has no mechanism to detect or respond to user language preference.
  implication: >
    Even if FluentValidation resource-based messages were added, there is no infrastructure
    to select the Vietnamese variant based on the browser's Accept-Language header.

- timestamp: 2026-03-09
  checked: frontend/src/features/clinical/components/RefractionForm.tsx lines 252-255
  found: >
    The onChange handler in renderNumberInput is:
      onChange={(e) => {
        const val = e.target.value          // string from input
        form.setValue(name, val === "" ? null : Number(val))
      }}
    Number("5.0") === 5 in JavaScript. When the user types "5." or "5.0" and the
    onChange fires, the raw string "5." or "5.0" is immediately converted with Number(),
    which produces the integer 5. This is then set as the form value.
    The controlled value fed back to the input via:
      value={toFormValue(value as number | null | undefined)}
    where toFormValue calls String(v), so String(5) === "5".
    The input therefore displays "5" and the decimal portion is lost.
  implication: >
    The decimal is stripped on every keystroke, not just on blur. Typing "1", "1.", "1.5"
    fires onChange three times. After "1." the value is set to Number("1.") = 1, and the
    controlled input re-renders with "1", erasing the decimal point before the user can
    type the digit after it.

- timestamp: 2026-03-09
  checked: frontend/src/features/clinical/components/RefractionForm.tsx line 87-90
  found: >
    toFormValue(v) does String(v). When v is an integer number like 1, String(1) === "1".
    There is no toFixed() or explicit decimal formatting. Even if Number() did not strip
    the decimal, the round-trip through String() on an integer JS number discards ".0".
  implication: Confirms the value round-trip never preserves a trailing decimal point.

- timestamp: 2026-03-09
  checked: frontend/src/shared/components/ui/input.tsx
  found: >
    Standard shadcn/ui Input component. It is a React.forwardRef wrapper that passes
    type, className, and all ...props directly to a native <input> element. No value
    transformation, no filtering of key events.
  implication: The decimal problem is not in the Input component; it is entirely in the
    onChange handler in RefractionForm.

- timestamp: 2026-03-09
  checked: frontend/src/shared/lib/server-validation.ts lines 60-63
  found: >
    handleServerValidationError sets field errors using:
      setError(formField, { type: "server", message: messages[0] })
    The message is taken verbatim from the server's JSON error response. No translation
    lookup, no i18n key mapping, no t() call is applied to server error messages.
  implication: >
    Server validation messages pass straight through to the UI. If the server sends
    English text, the UI displays English text. There is no client-side interception
    to translate server errors.

- timestamp: 2026-03-09
  checked: frontend/src/shared/i18n/i18n.ts
  found: >
    i18next is configured with fallbackLng: 'vi' and language detection order
    ['localStorage', 'navigator']. The detected language is stored in localStorage key
    "ganka28-language". This language code is only used for loading /locales/{lng}/{ns}.json
    files. It is never forwarded to the backend API as an Accept-Language or X-Language
    header.
  implication: >
    The frontend knows the user's preferred language but never communicates it to the
    backend. All API responses, including validation error messages, are language-agnostic
    from the backend's perspective.

- timestamp: 2026-03-09
  checked: frontend/public/locales/vi/clinical.json - refraction section
  found: >
    The translation file contains UI labels (sph, cyl, axis, ucva, etc.) but has no
    keys for validation error messages. There is no "validation" namespace or key block
    such as "vaOutOfRange", "sphOutOfRange" etc. that could be used to produce
    Vietnamese validation messages on the client side.
  implication: >
    Even a client-side approach (mapping server error keys to i18n keys) would require
    adding translation keys. Currently the translation file only covers field labels,
    not validation feedback strings.

## Resolution

root_cause:

  ISSUE 1 - Validation errors not localised:
    The root cause is two-fold and both layers must be addressed:

    A. BACKEND: UpdateRefractionCommandValidator (and all other validators in the codebase)
       use hardcoded English string literals in WithMessage() calls. There is no use of
       FluentValidation's localisation API (WithLocalizedMessage, IStringLocalizer, or
       resource files). There are no .resx files in the project.

    B. BACKEND INFRASTRUCTURE: Program.cs has no request localisation middleware
       (UseRequestLocalization / AddLocalization). The backend cannot detect the user's
       preferred language from Accept-Language headers and cannot select a localised
       message variant even if one existed.

    C. FRONTEND PASSTHROUGH: handleServerValidationError in
       frontend/src/shared/lib/server-validation.ts sets RHF field errors using the raw
       server message string. No translation or key-to-i18n mapping is attempted.

    D. MISSING TRANSLATION KEYS: The vi/clinical.json locale file has no validation message
       keys. Even a pure client-side solution (intercept server errors and map them to
       i18n keys) would need new keys added.

    The simplest fix path: Add Vietnamese messages directly in WithMessage() on the backend,
    passing the language context via Accept-Language header, OR move validation entirely to
    the client-side (Zod schema with i18n error maps) for these range checks and remove the
    redundant server messages from the UI surface.

  ISSUE 2 - Decimal input lost:
    The root cause is in RefractionForm.tsx renderNumberInput onChange handler (line 253-255).
    The handler converts the raw input string to a JavaScript Number immediately on every
    keystroke:
      form.setValue(name, val === "" ? null : Number(val))
    Number("1.") === 1 and Number("1.0") === 1. The numeric state is then rendered back to
    the controlled input via String(1) === "1", which erases the decimal point while the user
    is still typing.

    The field needs to store its in-progress string value locally (React state or uncontrolled
    input with ref) and only convert to Number on blur, OR the onChange must preserve the raw
    string in the form state (the Zod schema already handles string->number via the
    optionalNumber transformer, so the form can hold strings safely).

fix: NOT APPLIED - diagnosis only
verification: NOT APPLIED - diagnosis only
files_changed: []
