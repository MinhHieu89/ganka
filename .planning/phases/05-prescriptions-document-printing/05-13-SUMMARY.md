---
phase: 05-prescriptions-document-printing
plan: 13
subsystem: ui
tags: [react, tanstack-query, react-hook-form, zod, shadcn-ui, drug-catalog, pharmacy, i18n]

# Dependency graph
requires:
  - phase: 05-10
    provides: Pharmacy API endpoints (GET/POST/PUT /api/pharmacy/drugs, GET /api/pharmacy/drugs/search)
provides:
  - DrugCatalogPage admin page for managing pharmacy drug catalog
  - TanStack Query hooks for drug catalog CRUD and search (reusable by DrugCombobox)
  - DRUG_FORM_MAP and DRUG_ROUTE_MAP enum translations exported for prescription forms
  - Pharmacy route at /pharmacy path
  - Vietnamese pharmacy translations with proper medical diacritics
affects: [05-14, 05-15, 05-16, 05-18, frontend-prescription-forms]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Pharmacy API hooks following clinical-api.ts TanStack Query pattern with pharmacyKeys factory"
    - "DrugCatalogTable with global filter using getFilteredRowModel and custom globalFilterFn"
    - "DrugFormDialog with single form instance + mode prop (create/edit) instead of separate form instances"

key-files:
  created:
    - frontend/src/features/pharmacy/api/pharmacy-api.ts
    - frontend/src/features/pharmacy/components/DrugCatalogTable.tsx
    - frontend/src/features/pharmacy/components/DrugFormDialog.tsx
    - frontend/src/features/pharmacy/components/DrugCatalogPage.tsx
    - frontend/src/app/routes/_authenticated/pharmacy/index.tsx
    - frontend/public/locales/vi/pharmacy.json
  modified:
    - frontend/public/locales/en/pharmacy.json
    - frontend/src/shared/i18n/i18n.ts
    - frontend/src/app/routeTree.gen.ts

key-decisions:
  - "Single form instance with mode prop for DrugFormDialog instead of separate create/edit forms (simpler than UserFormDialog pattern)"
  - "Added pharmacy namespace to i18n config and created Vietnamese translations with proper medical diacritics"
  - "Drug form/route enum maps exported as constants for reuse across prescription forms and catalog table"

patterns-established:
  - "pharmacyKeys query key factory: { all: ['pharmacy'], drugs: ['pharmacy', 'drugs'], drugSearch: (term) => [..., 'search', term] }"
  - "Global filter on DataTable using getFilteredRowModel with multi-field custom globalFilterFn"

requirements-completed: [RX-01]

# Metrics
duration: 4min
completed: 2026-03-06
---

# Phase 05 Plan 13: Drug Catalog Admin Page Summary

**Admin drug catalog management page with DataTable, create/edit dialog, TanStack Query hooks, and pharmacy route -- drug search hook reusable by prescription DrugCombobox**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-05T17:07:21Z
- **Completed:** 2026-03-05T17:11:21Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- Created pharmacy-api.ts with TanStack Query hooks for drug catalog list, search, create, and update with cache invalidation and error toasts
- Created DrugCatalogTable with DataTable, sortable columns, global text filter, and enum translations for drug form/route
- Created DrugFormDialog with React Hook Form + Zod validation, Select dropdowns for enums, create/edit modes
- Created DrugCatalogPage with loading skeleton, table, and dialog state management following UserManagementPage pattern
- Created pharmacy route file and added pharmacy namespace to i18n with Vietnamese translations

## Task Commits

Each task was committed atomically:

1. **Task 1: Create pharmacy API hooks and drug catalog table** - `6330573` (feat)
2. **Task 2: Create DrugFormDialog, DrugCatalogPage, and route file** - `f9ceeec` (docs -- files swept into concurrent 05-11 commit)

## Files Created/Modified
- `frontend/src/features/pharmacy/api/pharmacy-api.ts` - TanStack Query hooks for drug catalog CRUD and search with pharmacyKeys factory and enum maps
- `frontend/src/features/pharmacy/components/DrugCatalogTable.tsx` - DataTable with sorting, global filter, enum translations, and edit actions
- `frontend/src/features/pharmacy/components/DrugFormDialog.tsx` - Dialog for create/edit drug with RHF + Zod, Select for form/route enums, Textarea for dosage template
- `frontend/src/features/pharmacy/components/DrugCatalogPage.tsx` - Admin page with header, add button, table, and dialog state management
- `frontend/src/app/routes/_authenticated/pharmacy/index.tsx` - Route file for /pharmacy path
- `frontend/public/locales/en/pharmacy.json` - English translations including form/route enum labels
- `frontend/public/locales/vi/pharmacy.json` - Vietnamese translations with proper medical diacritics
- `frontend/src/shared/i18n/i18n.ts` - Added pharmacy namespace to i18n config
- `frontend/src/app/routeTree.gen.ts` - Auto-generated route tree with pharmacy route

## Decisions Made
- Used single form instance with mode prop for DrugFormDialog instead of the dual-form pattern used in UserFormDialog -- simpler since both create/edit share the same fields
- Exported DRUG_FORM_MAP and DRUG_ROUTE_MAP from pharmacy-api.ts for reuse in prescription forms (DrugCombobox, prescription table)
- Added Vietnamese pharmacy translations with proper medical diacritics (e.g., "Dạng bào chế" for Form, "Đường dùng" for Route)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Created Vietnamese pharmacy translation file and added i18n namespace**
- **Found during:** Task 1 (creating DrugCatalogTable with translations)
- **Issue:** English pharmacy.json existed but Vietnamese pharmacy.json was missing. The pharmacy namespace was not registered in i18n.ts config.
- **Fix:** Created frontend/public/locales/vi/pharmacy.json with all translations. Added 'pharmacy' to ns array in i18n.ts.
- **Files modified:** frontend/public/locales/vi/pharmacy.json (new), frontend/src/shared/i18n/i18n.ts
- **Verification:** TypeScript compilation passes with no pharmacy-related errors
- **Committed in:** 6330573 (Task 1 commit)

**2. [Rule 2 - Missing Critical] Added form/route enum translations to pharmacy.json**
- **Found during:** Task 1 (DrugCatalogTable needs to display enum labels)
- **Issue:** Existing pharmacy.json only had catalog.* keys but no form.* or route.* translation keys needed for enum display
- **Fix:** Added form.* (eyeDrops, tablet, capsule, etc.) and route.* (topical, oral, etc.) translation sections to both en and vi pharmacy.json
- **Files modified:** frontend/public/locales/en/pharmacy.json, frontend/public/locales/vi/pharmacy.json
- **Verification:** DrugCatalogTable and DrugFormDialog reference t("form.xxx") and t("route.xxx") -- TypeScript passes
- **Committed in:** 6330573 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (1 blocking, 1 missing critical)
**Impact on plan:** Both auto-fixes required for correct i18n support. No scope creep.

## Issues Encountered
- Task 2 files were committed by a concurrent agent running 05-11 (the docs commit swept up untracked files). Content is correct and verified; commit attribution is split across 6330573 and f9ceeec.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Drug catalog admin page accessible at /pharmacy URL
- Drug search hook (useDrugCatalogSearch) ready for reuse in DrugCombobox prescription form
- DRUG_FORM_MAP and DRUG_ROUTE_MAP exported for prescription item display
- Sidebar navigation link to /pharmacy not yet added (will be handled when sidebar is updated alongside other navigation changes)

## Self-Check: PASSED

- [x] pharmacy-api.ts exists with useDrugCatalogSearch and useCreateDrugCatalogItem
- [x] DrugCatalogTable.tsx exists with DrugCatalogTable export
- [x] DrugFormDialog.tsx exists with DrugFormDialog export
- [x] DrugCatalogPage.tsx exists with DrugCatalogPage export
- [x] pharmacy/index.tsx route file exists with DrugCatalogPage import
- [x] Vietnamese pharmacy.json exists with proper diacritics
- [x] English pharmacy.json updated with form/route translations
- [x] Commit 6330573 found in git log
- [x] Commit f9ceeec found in git log
- [x] TypeScript compilation passes with no pharmacy-related errors

---
*Phase: 05-prescriptions-document-printing*
*Completed: 2026-03-06*
