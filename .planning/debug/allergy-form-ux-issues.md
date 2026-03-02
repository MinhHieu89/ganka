---
status: diagnosed
trigger: "allergy select is too bad. Replace it with shadcn/ui component. Cannot enter free text. All options are in English although user set vn language."
created: 2026-03-02T00:00:00Z
updated: 2026-03-02T00:00:00Z
---

## Current Focus

hypothesis: Three distinct UX bugs in AllergyForm - bad cmdk filtering overrides free-text, category labels are hardcoded English, and the popover is unreliable.
test: Code inspection completed
expecting: Fix requires rewrite of AllergyForm using proper shadcn Combobox pattern + category i18n
next_action: DIAGNOSED - return structured result

## Symptoms

expected: Allergy dialog allows autocomplete from ophthalmology catalog, supports free-text entry for unlisted allergens, and shows translated labels in Vietnamese.
actual: (1) User cannot enter free-text not in catalog - cmdk's internal filtering overrides filtered list and shows CommandEmpty; (2) Category labels ("Ophthalmic Drug", "General Drug", "Material", "Environmental") are hardcoded English strings in ALLERGY_CATALOG_BILINGUAL; (3) The combobox pattern is awkward - Input inside PopoverTrigger loses focus on popover open.
errors: No runtime errors; UX failures only.
reproduction: Open patient, go to allergies, click Add Allergy, type something not in catalog, see it blocked; switch language to Vietnamese and see English category names.
started: Always - implementation was shipped with these defects.

## Eliminated

- hypothesis: Vietnamese translations missing from patient.json
  evidence: patient.json (vi) has all severity/label keys correctly translated (mild/moderate/severe, allergyName, etc.)
  timestamp: 2026-03-02

- hypothesis: i18n.language detection broken
  evidence: AllergyForm correctly reads i18n.language and maps vi label from ALLERGY_CATALOG_BILINGUAL; the issue is the CATEGORY field is not translated, not the allergy names.
  timestamp: 2026-03-02

## Evidence

- timestamp: 2026-03-02
  checked: AllergyForm.tsx lines 98-107
  found: catalogItems maps i18n.language === "vi" correctly for labels. BUT item.category is always the English constant string from ALLERGY_CATALOG_BILINGUAL (e.g. "Ophthalmic Drug").
  implication: Category chip shown in the dropdown list is always English regardless of language.

- timestamp: 2026-03-02
  checked: AllergyForm.tsx lines 104-107 (filtering) vs CommandItem onSelect (lines 156-159)
  found: inputValue state drives the pre-filtered `filtered` array passed to CommandGroup. BUT `cmdk` also applies its OWN internal filtering against CommandItem `value` prop. The two filtering layers conflict. When user types a free-text string not matching any catalog item, cmdk hides all items AND shows CommandEmpty, making it appear as though free-text is disallowed. The Input onChange does update field.value, so the form value IS set to free text - but the dropdown CommandEmpty message misleads the user and there is no affordance to "use this text" as the final value. The user must manually close the popover after typing to confirm free text.
  implication: Free-text IS technically possible (user types, popover says no results, user presses Enter/closes) but the UX is broken - no "Use X" option, no way to confirm free text intuitively.

- timestamp: 2026-03-02
  checked: ALLERGY_CATALOG_BILINGUAL (patient-api.ts lines 106-137)
  found: category field is an English-only string constant in every entry. No vi equivalent for category.
  implication: Category label in dropdown is always in English. Needs i18n keys in patient.json or a separate mapping.

- timestamp: 2026-03-02
  checked: AllergyForm.tsx PopoverTrigger pattern (lines 125-138)
  found: Input is wrapped in PopoverTrigger. This means clicking the Input toggles the popover via Radix's trigger mechanism rather than just opening it. This creates a janky experience - the popover may close unexpectedly when the input is re-focused.
  implication: The canonical shadcn Combobox pattern uses a Button as trigger + separate CommandInput inside the popover. For free-text + autocomplete, the correct pattern is: Input outside popover (always visible, always editable) + popover opens/closes based on input focus/blur, with a "Use this text" fallback item in the list.

## Resolution

root_cause: |
  Three compounding issues in AllergyForm:
  1. FREE-TEXT BLOCKED UX: cmdk's internal filtering + no "use typed text" option means free-text entry appears broken even though form state accepts it. The popover pattern is wrong for free-text + autocomplete.
  2. CATEGORY NOT TRANSLATED: ALLERGY_CATALOG_BILINGUAL.category is hardcoded English strings with no Vietnamese mapping. Category labels in the dropdown are always English.
  3. POPOVER TRIGGER BUG: Input wrapped in PopoverTrigger causes unreliable focus/toggle behavior.

fix: |
  1. Rewrite AllergyForm allergy name field as proper free-text + autocomplete:
     - Input is always editable and sits OUTSIDE PopoverTrigger
     - Popover opens on focus, closes on blur (with delay to allow item click)
     - CommandList shows filtered catalog items
     - Add a "use typed text" fallback CommandItem when inputValue doesn't match any item exactly
     - When user selects catalog item: store English value, show localized label
     - When user types free text and dismisses: store typed value as-is
  2. Add category i18n: add allergyCategory keys to patient.json (en + vi), map them in the component
  3. Remove Input-as-PopoverTrigger anti-pattern

files_changed: []
