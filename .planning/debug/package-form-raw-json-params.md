---
status: diagnosed
trigger: "Treatment package creation form shows raw JSON textarea for treatment parameters instead of structured input fields"
created: 2026-03-19T00:00:00Z
updated: 2026-03-19T00:00:00Z
---

## Current Focus

hypothesis: TreatmentPackageForm stores parametersJson as a raw string and renders it in an AutoResizeTextarea, whereas ProtocolTemplateForm decomposes the JSON into individual typed fields (iplEnergy, llltWavelength, etc.) using buildParametersJson/parseParametersJson helpers
test: Compare rendering of parametersJson in both components
expecting: TreatmentPackageForm uses raw textarea; ProtocolTemplateForm uses structured fields
next_action: Report root cause

## Symptoms

expected: When creating a treatment package, treatment parameters should be displayed as structured, labeled input fields (e.g., Energy, Pulse Count, Spot Size, Treatment Zones for IPL) matching what the protocol template form shows
actual: Treatment parameters are displayed as a single raw JSON textarea with label "Tham so dieu tri (JSON)" showing raw JSON like {"wavelength":11,"power":11,"duration":11,"treatmentArea":"11"}
errors: None (functional issue, not a crash)
reproduction: Navigate to /treatments, click Create Package, select a template - observe the parameters section
started: Since TreatmentPackageForm was first implemented

## Eliminated

(none needed - root cause was clear from code review)

## Evidence

- timestamp: 2026-03-19
  checked: TreatmentPackageForm.tsx lines 501-522
  found: parametersJson field is rendered as a single AutoResizeTextarea with className="font-mono text-xs". The form schema defines parametersJson as z.string().nullable().optional() - a plain string field.
  implication: The component treats parameters as an opaque JSON string, not structured data.

- timestamp: 2026-03-19
  checked: TreatmentPackageForm.tsx lines 144-165 (handleTemplateSelect)
  found: When a template is selected, template.defaultParametersJson is copied directly into parametersJson as a raw string via form.setValue("parametersJson", template.defaultParametersJson || null)
  implication: No parsing/decomposition of the JSON into individual fields occurs.

- timestamp: 2026-03-19
  checked: ProtocolTemplateForm.tsx lines 99-197 (buildParametersJson / parseParametersJson helpers)
  found: ProtocolTemplateForm has helper functions that convert between structured form fields (iplEnergy, iplPulseCount, iplSpotSize, iplTreatmentZones, llltWavelength, etc.) and a JSON string. It parses JSON into individual fields on load, and builds JSON from individual fields on submit.
  implication: The structured-field approach already exists and works correctly in ProtocolTemplateForm.

- timestamp: 2026-03-19
  checked: ProtocolTemplateForm.tsx lines 581-861 (treatment-type-specific parameter UI)
  found: ProtocolTemplateForm renders treatment-type-specific structured fields conditionally based on treatmentType: IPL shows Energy/PulseCount/SpotSize/TreatmentZones, LLLT shows Wavelength/Power/Duration/TreatmentArea, LidCare shows Duration/ProcedureSteps/Products. Each uses proper Input components with labels, types, and validation.
  implication: The UI pattern for structured parameter editing is fully implemented but only used in ProtocolTemplateForm.

- timestamp: 2026-03-19
  checked: TreatmentPackageForm.tsx line 82-84 (selectedTemplate state)
  found: TreatmentPackageForm already tracks the selected template via useState, which includes template.treatmentType. This means the treatment type IS available to conditionally render type-specific fields.
  implication: The data needed to determine which structured fields to show is already present.

## Resolution

root_cause: TreatmentPackageForm was implemented with a shortcut - it stores and renders parametersJson as a raw JSON string in a single AutoResizeTextarea (lines 501-522), instead of decomposing it into individual typed fields the way ProtocolTemplateForm does. The ProtocolTemplateForm already has buildParametersJson() and parseParametersJson() helper functions (lines 121-197) that convert between structured form fields and JSON, plus full conditional UI for IPL/LLLT/LidCare parameter types (lines 586-861). None of this logic was reused or replicated in TreatmentPackageForm.

fix: (not applied - diagnosis only)
verification: (not applied - diagnosis only)
files_changed: []
