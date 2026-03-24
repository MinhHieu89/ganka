---
phase: quick
plan: u4a
subsystem: frontend-ui
tags: [validation, ux, forms, field-components]
dependency-graph:
  requires: []
  provides: [validation-error-highlighting, required-field-indicators]
  affects: [all-forms]
tech-stack:
  added: []
  patterns: [aria-invalid-border-destructive, FieldLabel-required-prop]
key-files:
  created: []
  modified:
    - frontend/src/shared/components/ui/input.tsx
    - frontend/src/shared/components/ui/select.tsx
    - frontend/src/shared/components/ui/textarea.tsx
    - frontend/src/shared/components/NumberInput.tsx
    - frontend/src/shared/components/ui/field.tsx
    - frontend/src/features/admin/components/ClinicSettingsPage.tsx
    - 34 form files across optical, pharmacy, admin, billing, consumables, clinical, treatment, scheduling, patient, auth
decisions:
  - Used aria-[invalid=true] for Input/Textarea/NumberInput and both aria-[invalid=true] and data-[invalid=true] for SelectTrigger (Radix compatibility)
  - Skipped clinical grid forms (DryEyeForm, OpticalPrescriptionForm, RefractionForm) as they use plain Label for data headers, not form field labels
  - Skipped DrugPrescriptionForm and TreatmentSessionForm as they have no FieldLabel usage
  - Replaced manual asterisk text with required prop where found (CreateGlassesOrderForm, PaymentForm, ModifyPackageDialog)
metrics:
  duration: 16m
  completed: 2026-03-24
---

# Quick Task u4a: Standardize Validation Error Highlighting Summary

Standardized red border on invalid inputs via aria-[invalid=true]:border-destructive across all input components, added required prop to FieldLabel for red asterisk indicator, and applied required indicators to all form files.

## What Was Done

### Task 1: Error border styling and FieldLabel required prop (af87e26)

Added `aria-[invalid=true]:border-destructive` Tailwind class to:
- **Input** (input.tsx) - after existing `border-input`
- **SelectTrigger** (select.tsx) - both `aria-[invalid=true]` and `data-[invalid=true]` for Radix compatibility
- **Textarea** (textarea.tsx) - after existing `border-input`
- **NumberInput** (NumberInput.tsx) - after existing `border-input`

Added `required` prop to **FieldLabel** (field.tsx):
- Optional `required?: boolean` prop
- Renders `<span className="text-destructive">*</span>` after children when true

### Task 2: Required indicators across all forms (a646ff9)

Applied `required` prop to FieldLabel for required fields across **34 form files**:
- **Optical:** FrameFormDialog (11 fields), LensFormDialog (8 fields), ComboPackageForm (2), CreateGlassesOrderForm (7), WarrantyClaimForm (2)
- **Pharmacy:** DrugFormDialog (6), StockImportForm (2), SupplierForm (1), StockAdjustmentDialog (2), DrugInventoryTable (2)
- **Admin:** UserFormDialog (4), RoleManagementPage (2), ClinicSettingsPage (2 + migrated to Field system)
- **Billing:** DiscountDialog (4), RefundDialog (3), PaymentForm (5), ServiceCatalogFormDialog (4), ShiftCloseDialog (1), ShiftOpenDialog (1)
- **Consumables:** ConsumableItemForm (5), AddStockDialog (4), ConsumableAdjustDialog (2)
- **Clinical:** AmendmentDialog (1)
- **Treatment:** TreatmentPackageForm (1), ProtocolTemplateForm (9), CancellationRequestDialog (1), CancellationApprovalQueue (2), ModifyPackageDialog (1), SwitchTreatmentDialog (2)
- **Scheduling:** AppointmentBookingDialog (5)
- **Patient:** PatientRegistrationForm (2), PatientOverviewTab (2), AllergyForm (2)
- **Auth:** LoginForm (2)

**ClinicSettingsPage migration:** Converted from plain `<Label>` + `<div>` + `<p>` error display to `<Field>` + `<FieldLabel>` + `<FieldError>` system with proper `data-invalid` and `aria-invalid` attributes.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Enhancement] Replaced manual asterisk text with required prop**
- **Found during:** Task 2
- **Issue:** CreateGlassesOrderForm, PaymentForm, and ModifyPackageDialog used manual ` *` text in FieldLabel children instead of the new required prop
- **Fix:** Replaced `{t("label")} *` with `required` prop and removed inline asterisk
- **Files modified:** CreateGlassesOrderForm.tsx, PaymentForm.tsx, ModifyPackageDialog.tsx

## Decisions Made

1. Used `aria-[invalid=true]` for standard HTML elements (input, textarea) and added `data-[invalid=true]` for Radix SelectTrigger to handle both possible invalid-state propagation methods
2. Skipped clinical grid/table forms (DryEyeForm, OpticalPrescriptionForm, RefractionForm) since they use plain `<Label>` for column/row headers, not form field labels
3. Skipped OtcSaleForm since its only FieldLabel instances are for optional fields (customerName, notes)

## Known Stubs

None - all changes are complete and functional.

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | af87e26 | Add error border styling to input components and required indicator to FieldLabel |
| 2 | a646ff9 | Add required prop to FieldLabel across all form files |
