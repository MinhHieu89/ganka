---
phase: quick
plan: 260324-te9
subsystem: frontend
tags: [i18n, validation, refactoring, zod]
dependency_graph:
  requires: []
  provides: ["shared-validation-utility", "localized-validation-messages"]
  affects: ["all-form-components"]
tech_stack:
  added: []
  patterns: ["schema-factory-with-t-function", "createValidationMessages-utility"]
key_files:
  created:
    - frontend/src/shared/lib/validation.ts
  modified:
    - frontend/public/locales/en/common.json
    - frontend/public/locales/vi/common.json
    - frontend/src/features/optical/components/FrameFormDialog.tsx
    - frontend/src/features/optical/components/LensFormDialog.tsx
    - frontend/src/features/optical/components/ComboPackageForm.tsx
    - frontend/src/features/optical/components/CreateGlassesOrderForm.tsx
    - frontend/src/features/optical/components/WarrantyClaimForm.tsx
    - frontend/src/features/pharmacy/components/DrugFormDialog.tsx
    - frontend/src/features/pharmacy/components/OtcSaleForm.tsx
    - frontend/src/features/pharmacy/components/StockImportForm.tsx
    - frontend/src/features/pharmacy/components/SupplierForm.tsx
    - frontend/src/features/pharmacy/components/DrugInventoryTable.tsx
    - frontend/src/features/pharmacy/components/StockAdjustmentDialog.tsx
    - frontend/src/features/admin/components/UserFormDialog.tsx
    - frontend/src/features/admin/components/RoleManagementPage.tsx
    - frontend/src/features/consumables/components/ConsumableItemForm.tsx
    - frontend/src/features/consumables/components/AddStockDialog.tsx
    - frontend/src/features/consumables/components/ConsumableAdjustDialog.tsx
    - frontend/src/features/clinical/components/DrugPrescriptionForm.tsx
    - frontend/src/features/treatment/components/TreatmentPackageForm.tsx
    - frontend/src/features/treatment/components/ProtocolTemplateForm.tsx
    - frontend/src/features/treatment/components/CancellationRequestDialog.tsx
    - frontend/src/features/treatment/components/CancellationApprovalQueue.tsx
decisions:
  - Use common.json namespace for generic validation keys (not billing-specific ones)
  - Keep billing forms untouched since they already use correct pattern with billing.json
  - Remove getErrorMessage workaround in favor of schema-provided localized messages
metrics:
  duration: "17m"
  completed: "2026-03-24"
  tasks_completed: 2
  tasks_total: 2
  files_modified: 24
---

# Quick Task 260324-te9: Localize Validation Messages Across All Forms Summary

Shared validation utility with schema factory pattern eliminating all hardcoded validation strings across 21+ form files

## What Was Done

### Task 1: Create Shared Validation Utility and Expand Translation Keys
- Created `frontend/src/shared/lib/validation.ts` with `createValidationMessages(t)` helper
- Exports 16 validation message helpers (required, mustBePositive, between, exactDigits, etc.)
- Added 12 new validation keys to both `en/common.json` and `vi/common.json`
- Proper Vietnamese diacritics used throughout

### Task 2: Migrate All Forms to Localized Validation
- Converted 21 form files from hardcoded strings to schema factory pattern
- Each form now creates its zod schema via `useMemo(() => createXxxSchema(tCommon), [tCommon])`
- Removed all `getErrorMessage` workaround functions (6 instances)
- Replaced hardcoded English strings like "Must be >= 0", "required", "Barcode must be exactly 13 digits"
- Replaced hardcoded Vietnamese strings like "Bat buoc", "Phai la so nguyen", "Vui long chon"

**Forms migrated by module:**
- Optical: FrameFormDialog, LensFormDialog, ComboPackageForm, CreateGlassesOrderForm, WarrantyClaimForm
- Pharmacy: DrugFormDialog, OtcSaleForm, StockImportForm, SupplierForm, DrugInventoryTable, StockAdjustmentDialog
- Admin: UserFormDialog, RoleManagementPage
- Consumables: ConsumableItemForm, AddStockDialog, ConsumableAdjustDialog
- Clinical: DrugPrescriptionForm
- Treatment: TreatmentPackageForm, ProtocolTemplateForm, CancellationRequestDialog, CancellationApprovalQueue

**Forms already correctly localized (untouched):**
- LoginForm, BookingForm, AllergyForm, PatientRegistrationForm
- DiscountDialog, RefundDialog (billing namespace)
- ServiceCatalogFormDialog, PaymentForm, ShiftCloseDialog, ShiftOpenDialog (billing factories)
- AppointmentBookingDialog (scheduling namespace)
- PatientOverviewTab (uses tCommon)
- ClinicSettingsPage (uses useClinicSettingsSchema hook)
- ModifyPackageDialog, SwitchTreatmentDialog (uses t() factory)

## Deviations from Plan

None - plan executed exactly as written.

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | 25d3779 | Create shared validation utility and expand translation keys |
| 2 | f52ef35 | Migrate all forms to use localized validation via schema factories |

## Known Stubs

None - all validation messages are fully wired to translation keys with both English and Vietnamese translations.
