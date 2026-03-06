---
phase: 06-pharmacy-consumables
plan: 21
subsystem: ui
tags: [react, tanstack-query, react-hook-form, zod, shadcn, pharmacy, supplier, stock-import]

# Dependency graph
requires:
  - phase: 06-19
    provides: pharmacy API hooks (useSuppliers, useCreateStockImport, useDrugCatalogList)

provides:
  - SupplierForm dialog for create/edit supplier with Zod validation
  - SuppliersPage route with DataTable, activate/deactivate toggle
  - StockImportForm with dynamic line items, drug combobox, date pickers
  - ExcelImportDialog with template download, file upload, row-level error preview
  - StockImportPage route with invoice/history tabs and Excel dialog trigger
affects:
  - 06-pharmacy-consumables (all remaining plans use these pages)
  - 07-billing (links to pharmacy for stock import workflows)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - DrugCombobox uses Popover+Command+shouldFilter=false with full catalog list
    - useFieldArray for dynamic line items in StockImportForm
    - ExcelImportDialog two-step flow: upload for preview, then confirm for commit
    - generateExcelTemplate as TSV blob with BOM for Excel UTF-8 support

key-files:
  created:
    - frontend/src/features/pharmacy/components/SupplierForm.tsx
    - frontend/src/features/pharmacy/components/StockImportForm.tsx
    - frontend/src/features/pharmacy/components/ExcelImportDialog.tsx
    - frontend/src/app/routes/_authenticated/pharmacy/suppliers.tsx
    - frontend/src/app/routes/_authenticated/pharmacy/stock-import.tsx
  modified:
    - frontend/src/app/routes/_authenticated/pharmacy/index.tsx
    - frontend/public/locales/en/pharmacy.json
    - frontend/public/locales/vi/pharmacy.json

key-decisions:
  - "SupplierForm uses single form instance with isEdit flag (not dual-form pattern) following DrugFormDialog precedent from 05-13"
  - "ExcelImportDialog generates TSV template with BOM for Excel Unicode support rather than calling backend endpoint"
  - "StockImportPage uses Tabs (invoice/history) with Excel dialog triggered from header button for clean separation"
  - "Drug combobox in StockImportForm loads full catalog list (not search API) for instant filtering without debounce"
  - "Pharmacy index page updated with active Link navigation to suppliers/stock-import (removing disabled placeholders from plan 06-20)"

patterns-established:
  - "DrugCombobox: Popover+Command pattern with shouldFilter=false, full catalog loaded once, display nameVi || name"
  - "Excel template: client-side TSV blob generation with UTF-8 BOM, download via anchor click with 30s revokeObjectURL"
  - "Two-step Excel import: upload triggers preview API call, confirmation triggers createStockImport mutation"

requirements-completed:
  - PHR-01
  - PHR-02

# Metrics
duration: 5min
completed: 2026-03-06
---

# Phase 06 Plan 21: Supplier Management and Stock Import Pages Summary

**Supplier CRUD management page and dual-method stock import (supplier invoice form + Excel bulk upload with row-level error preview)**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-06T08:56:25Z
- **Completed:** 2026-03-06T09:01:00Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments

- Supplier management page with DataTable showing name, contact info, active/inactive status, edit dialog, and activate/deactivate toggle button per row
- Stock import page with Tabs layout: "Nhap theo hoa don" tab shows StockImportForm with supplier selector, invoice number, dynamic drug line items (drug combobox, batch number, expiry date picker, quantity, purchase price), "Lich su nhap kho" tab shows import history DataTable
- Excel import dialog with supplier selector, template download (TSV with BOM), file upload, two-step preview (valid rows in green table + error list), and "Confirm Import" button to commit valid rows via createStockImport mutation
- Updated pharmacy index page to enable navigation buttons linking to /pharmacy/suppliers and /pharmacy/stock-import routes
- Added 60+ translation keys (EN/VI) for supplier and stockImport namespaces

## Task Commits

1. **Task 1: Create supplier management components and route** - `fe56a7f` (feat)
2. **Task 2: Create stock import components and route** - `02a9ea9` (feat)

## Files Created/Modified

- `frontend/src/features/pharmacy/components/SupplierForm.tsx` - Dialog form for create/edit supplier
- `frontend/src/features/pharmacy/components/StockImportForm.tsx` - Full supplier invoice import form with dynamic line items
- `frontend/src/features/pharmacy/components/ExcelImportDialog.tsx` - Excel upload dialog with template, preview, row-level errors
- `frontend/src/app/routes/_authenticated/pharmacy/suppliers.tsx` - Suppliers page route
- `frontend/src/app/routes/_authenticated/pharmacy/stock-import.tsx` - Stock import page route with tabs
- `frontend/src/app/routes/_authenticated/pharmacy/index.tsx` - Updated with active navigation links
- `frontend/public/locales/en/pharmacy.json` - Added supplier/stockImport translation keys
- `frontend/public/locales/vi/pharmacy.json` - Added supplier/stockImport translation keys (Vietnamese)

## Decisions Made

- **SupplierForm single-form pattern:** Used single form instance with `isEdit = !!supplier` flag instead of dual-form pattern (following DrugFormDialog precedent from plan 05-13)
- **Excel template as client-side TSV:** Generated template as client-side TSV blob with BOM rather than calling a backend endpoint, avoiding need for an extra API route and keeping template schema documentation local to the dialog
- **Drug combobox loads full catalog:** DrugCombobox loads full `useDrugCatalogList()` on mount for instant filtering rather than debounced search API -- acceptable since catalog is bounded in size for a clinic
- **StockImportPage layout:** Excel button in header triggers dialog; Tabs separate invoice form from history table -- keeps history accessible without leaving import form context

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed leftover `tCommon` reference in suppliers.tsx**
- **Found during:** Task 1 (TypeScript verification)
- **Issue:** Removed `useTranslation("common")` import but `tCommon` was still referenced in the useMemo dependency array
- **Fix:** Removed `tCommon` from dependency array
- **Files modified:** `frontend/src/app/routes/_authenticated/pharmacy/suppliers.tsx`
- **Verification:** TypeScript no longer reports TS2304 error for suppliers.tsx
- **Committed in:** fe56a7f (Task 1 commit)

**2. [Rule 1 - Bug] Fixed Link `to` prop type error in pharmacy index**
- **Found during:** Task 1 (TypeScript verification)
- **Issue:** `to="/_authenticated/pharmacy/suppliers"` causes TS2322 -- TanStack Router only accepts known route strings
- **Fix:** Cast with `to={"/pharmacy/suppliers" as string}` following established pattern from PatientProfilePage
- **Files modified:** `frontend/src/app/routes/_authenticated/pharmacy/index.tsx`
- **Verification:** TypeScript no longer reports TS2322 error for pharmacy/index.tsx
- **Committed in:** fe56a7f (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (2 x Rule 1 bugs)
**Impact on plan:** Both fixes were minor TypeScript compilation issues discovered during verification. No scope creep.

## Issues Encountered

None beyond the two auto-fixed TypeScript issues above.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Supplier management (PHR-01) complete with full CRUD and active/inactive toggle
- Stock import (PHR-02) complete with both import methods (supplier invoice + Excel)
- Routes `/pharmacy/suppliers` and `/pharmacy/stock-import` registered and navigable
- Ready for Phase 06-22+ plans building dispensing queue and OTC sales pages

---
*Phase: 06-pharmacy-consumables*
*Completed: 2026-03-06*
