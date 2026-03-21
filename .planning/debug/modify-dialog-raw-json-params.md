---
status: diagnosed
trigger: "ModifyPackageDialog shows raw JSON textarea for treatment parameters instead of structured input fields"
created: 2026-03-19T00:00:00Z
updated: 2026-03-19T00:00:00Z
---

## Current Focus

hypothesis: ModifyPackageDialog renders parametersJson as a raw AutoResizeTextarea (lines 205-223) instead of using the shared TreatmentParameterFields component that TreatmentPackageForm already uses
test: Compare parameter rendering in ModifyPackageDialog vs TreatmentPackageForm
expecting: ModifyPackageDialog uses raw textarea; TreatmentPackageForm uses TreatmentParameterFields
next_action: Report root cause

## Symptoms

expected: When modifying a treatment package, treatment parameters should be displayed as structured, labeled input fields (e.g., Energy/PulseCount/SpotSize for IPL, Wavelength/Power/Duration for LLLT) matching the TreatmentPackageForm create dialog
actual: ModifyPackageDialog renders a single raw JSON textarea labeled "Tham so dieu tri (JSON)" with monospace font, showing raw JSON like {"wavelength":11,"power":11,"duration":11,"treatmentArea":"11"}
errors: None (UX issue, not a crash)
reproduction: Navigate to treatment package detail, click Modify, observe the parameters field
started: Since ModifyPackageDialog was first implemented

## Eliminated

(none needed - root cause clear from code review)

## Evidence

- timestamp: 2026-03-19
  checked: ModifyPackageDialog.tsx lines 205-223
  found: parametersJson is rendered as a single AutoResizeTextarea with className="font-mono text-xs". The form schema (line 40) defines parametersJson as z.string().nullable().optional() - a plain string field. No import of TreatmentParameterFields exists.
  implication: The component treats parameters as an opaque JSON string, not structured data.

- timestamp: 2026-03-19
  checked: ModifyPackageDialog.tsx lines 69-74 (defaultValues) and lines 78-87 (useEffect reset)
  found: parametersJson is initialized from package_.parametersJson as a raw string. No parsing into structured fields occurs on dialog open.
  implication: The raw JSON string flows directly from DTO to textarea without decomposition.

- timestamp: 2026-03-19
  checked: TreatmentPackageForm.tsx lines 38-42 (imports) and lines 529-533 (render)
  found: TreatmentPackageForm imports and uses `TreatmentParameterFields`, `buildParametersJson`, and `parseParametersJson` from "./TreatmentParameterFields". It maintains a `paramFields` state (line 92), calls `parseParametersJson` on template select (line 173), renders `<TreatmentParameterFields>` (line 529), and calls `buildParametersJson` on submit (line 218).
  implication: The fix pattern is already established and working in the sibling component.

- timestamp: 2026-03-19
  checked: TreatmentParameterFields.tsx (shared component)
  found: A fully reusable shared component already exists at `./TreatmentParameterFields`. It accepts `treatmentType`, `values` (Record<string, unknown>), and `onChange` callback. It renders structured fields for IPL (energy, pulseCount, spotSize, treatmentZones), LLLT (wavelength, power, duration, treatmentArea), and LidCare (duration, procedureSteps, products). Helper functions `buildParametersJson()` and `parseParametersJson()` handle JSON <-> structured field conversion.
  implication: No new component is needed. The shared component just needs to be wired into ModifyPackageDialog.

- timestamp: 2026-03-19
  checked: treatment-types.ts lines 36-58 (TreatmentPackageDto)
  found: TreatmentPackageDto has a `treatmentType: string` field. The ModifyPackageDialog receives `package_: TreatmentPackageDto`, so `package_.treatmentType` is available to pass to TreatmentParameterFields.
  implication: All data needed to render structured fields is already available in the dialog props.

## Resolution

root_cause: ModifyPackageDialog was implemented with a raw JSON textarea for the parametersJson field (lines 205-223), while TreatmentPackageForm was later upgraded to use the shared `TreatmentParameterFields` component. The ModifyPackageDialog was never updated to use the same shared component. The shared component (`TreatmentParameterFields`) and its helpers (`buildParametersJson`, `parseParametersJson`) already exist and are fully reusable. The `package_.treatmentType` needed to drive the structured fields is already available in the dialog props.

fix: Replace the raw AutoResizeTextarea in ModifyPackageDialog with the shared TreatmentParameterFields component. Specifically:
1. Import `TreatmentParameterFields`, `buildParametersJson`, `parseParametersJson` from "./TreatmentParameterFields"
2. Add `paramFields` state (Record<string, unknown>) initialized by calling `parseParametersJson(package_.treatmentType, package_.parametersJson)` on dialog open
3. Add `handleParamFieldChange` callback to update paramFields state
4. Replace the AutoResizeTextarea block (lines 205-223) with `<TreatmentParameterFields treatmentType={package_.treatmentType} values={paramFields} onChange={handleParamFieldChange} />`
5. In handleSubmit, call `buildParametersJson(package_.treatmentType, paramFields)` instead of passing `data.parametersJson` directly
6. The `parametersJson` field can be removed from the zod schema since it will be built from structured state on submit

verification: (not applied - diagnosis only)
files_changed:
- frontend/src/features/treatment/components/ModifyPackageDialog.tsx
