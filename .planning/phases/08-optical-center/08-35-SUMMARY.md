---
phase: 08-optical-center
plan: 35
subsystem: frontend-i18n
tags: [i18n, translations, sidebar, navigation, optical]
dependency_graph:
  requires: [08-27, 08-28, 08-29, 08-30, 08-31, 08-32, 08-33, 08-34]
  provides: [optical-translations, optical-sidebar-navigation]
  affects: [app-sidebar, i18n-config]
tech_stack:
  added: []
  patterns: [i18next-http-backend namespace pattern, sidebar collapsible nav group pattern]
key_files:
  created:
    - frontend/public/locales/en/optical.json
    - frontend/public/locales/vi/optical.json
  modified:
    - frontend/src/shared/components/AppSidebar.tsx
    - frontend/src/shared/i18n/i18n.ts
    - frontend/public/locales/en/common.json
    - frontend/public/locales/vi/common.json
decisions:
  - Used common.json for sidebar sub-item translation keys (matching established pharmacy pattern)
  - Added optical namespace to i18n ns array following existing pattern
  - Replaced disabled optical sidebar entry with collapsible group containing 6 sub-items
  - Children nav items use titleKey pattern from common namespace (no icons needed per existing NavSubItem interface)
metrics:
  duration_seconds: 305
  completed_date: "2026-03-08"
  tasks_completed: 2
  tasks_total: 2
  files_created: 2
  files_modified: 4
---

# Phase 8 Plan 35: i18n Translations & Sidebar Navigation Summary

**One-liner:** EN/VI optical translation files (10 sections, 150+ keys) plus sidebar Optical Center group with 6 active sub-navigation items replacing disabled placeholder.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Create i18n translation files | 77f985f | en/optical.json, vi/optical.json |
| 2 | Update sidebar navigation | 0d0169c | AppSidebar.tsx, i18n.ts, en/common.json, vi/common.json |

## What Was Built

### Task 1: i18n Translation Files

Created `frontend/public/locales/en/optical.json` and `frontend/public/locales/vi/optical.json` with 10 matching top-level sections:

- **nav**: Navigation labels for Optical Center and all 6 sub-pages
- **frames**: Frame catalog labels (brand, model, color, size, material, barcode, etc.)
- **lenses**: Lens catalog labels (type, material, coatings, SPH/CYL/ADD power fields, stock entries)
- **orders**: Glasses orders labels (status, processing type, payment status, estimated delivery, overdue)
- **combos**: Combo packages labels (name, combo price, savings, original price)
- **warranty**: Warranty claims labels (resolution types, approval status, assessment notes, claim date, documents)
- **prescriptions**: Prescription history labels (visit date, comparison, change direction, year-over-year)
- **stocktaking**: Stocktaking labels (session name, physical count, system count, discrepancy, over/under/missing)
- **common**: Shared UI labels (active, inactive, search, filter, actions, CRUD operations)
- **enums**: All enum value names (FrameMaterial, FrameType, FrameGender, LensMaterial, LensCoating, LensType, GlassesOrderStatus, ProcessingType, WarrantyResolution, WarrantyApprovalStatus, StocktakingStatus)

Vietnamese translations use proper diacritics throughout (e.g., "Danh mục gọng kính", "Đơn đặt kính", "Yêu cầu bảo hành", "Kiểm kê", "Chênh lệch", "Số lượng thực tế", "Số lượng hệ thống").

### Task 2: Sidebar Navigation

Updated `AppSidebar.tsx`:
- Replaced disabled `optical` entry with a collapsible nav group with 6 sub-items
- Sub-items: Frame Catalog (`/optical/frames`), Lens Catalog (`/optical/lenses`), Glasses Orders (`/optical/orders`), Combo Packages (`/optical/combos`), Warranty Claims (`/optical/warranty`), Stocktaking (`/optical/stocktaking`)
- Uses `IconEyeglass` for parent group (already imported)
- Follows exact pharmacy collapsible pattern

Updated `i18n.ts`: Added `optical` namespace to ns array so optical translation files are loaded.

Updated `en/common.json` and `vi/common.json`: Added 7 new sidebar keys:
- `sidebar.optical` updated to "Optical Center" / "Trung tâm kính mắt"
- `sidebar.opticalFrames`, `opticalLenses`, `opticalOrders`, `opticalCombos`, `opticalWarranty`, `opticalStocktaking`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing] Added sidebar sub-item keys to common.json**
- **Found during:** Task 2
- **Issue:** Sidebar children use `t(child.titleKey)` which resolves via the `common` namespace, but common.json had no keys for optical sub-items
- **Fix:** Added 6 optical sub-item keys to both EN and VI common.json
- **Files modified:** frontend/public/locales/en/common.json, frontend/public/locales/vi/common.json
- **Commit:** 0d0169c

**2. [Rule 1 - Cleanup] Removed unused icon imports**
- **Found during:** Task 2
- **Issue:** Added icon imports (IconFrame, IconEyeglass2, IconPackage, IconShieldCheck, IconClipboardList) that are not used since NavSubItem interface has no icon field
- **Fix:** Removed the unused icon imports after confirming children nav items don't use icons
- **Commit:** 0d0169c

## Self-Check: PASSED

Files verified:
- FOUND: frontend/public/locales/en/optical.json
- FOUND: frontend/public/locales/vi/optical.json
- FOUND: frontend/src/shared/components/AppSidebar.tsx (modified)
- FOUND: frontend/src/shared/i18n/i18n.ts (modified)

Commits verified:
- FOUND: 77f985f - feat(08-35): add EN/VI i18n translation files for optical center
- FOUND: 0d0169c - feat(08-35): update sidebar navigation with Optical Center sub-items

TypeScript: No errors in modified files (63 pre-existing errors in unrelated files unchanged).
