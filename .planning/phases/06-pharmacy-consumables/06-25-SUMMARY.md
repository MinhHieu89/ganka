---
phase: 06-pharmacy-consumables
plan: 25
subsystem: frontend-navigation-i18n
tags: [sidebar, navigation, i18n, pharmacy, consumables, translations]
dependency_graph:
  requires: [06-20, 06-21, 06-22, 06-23, 06-24]
  provides: [sidebar-pharmacy-nav, sidebar-consumables-nav, consumables-translations]
  affects: [AppSidebar, i18n-config, locales]
tech_stack:
  added: []
  patterns: [collapsible-sub-nav, pending-count-badge, i18n-namespace-registration]
key_files:
  created:
    - frontend/public/locales/en/consumables.json
    - frontend/public/locales/vi/consumables.json
  modified:
    - frontend/src/shared/components/AppSidebar.tsx
    - frontend/src/shared/i18n/i18n.ts
    - frontend/public/locales/en/common.json
    - frontend/public/locales/vi/common.json
decisions:
  - Used Collapsible component for pharmacy sub-nav to match shadcn/ui sidebar pattern
  - Badge displays only when pendingCount > 0 to avoid cluttering nav when empty
  - Pharmacy sub-nav: Inventory as root /pharmacy path, Queue/Suppliers/StockImport/OtcSales as sub-paths
  - consumables namespace registered in i18n.ts alongside existing namespaces
metrics:
  duration: ~15 minutes
  completed: "2026-03-06"
  tasks_completed: 2
  files_modified: 6
---

# Phase 06 Plan 25: Sidebar Navigation and i18n Translations Summary

**One-liner:** Pharmacy sub-nav with pending-count badge and collapsible children, plus EN/VI consumables translation files.

## What Was Built

### Task 1: Updated Sidebar with Pharmacy Sub-Nav and Badge (c75e3f3)

**AppSidebar.tsx** was updated to:
- Add collapsible pharmacy sub-nav with 5 child items: Inventory, Dispensing Queue, Suppliers, Stock Import, OTC Sales
- Import and use `usePendingCount` hook to display badge on Pharmacy nav item when pending prescriptions exist
- Use `SidebarMenuBadge` component with `SidebarMenuSub`/`SidebarMenuSubButton`/`SidebarMenuSubItem` for sub-navigation
- Import `Collapsible`, `CollapsibleTrigger`, `CollapsibleContent` for expandable nav
- Import `IconChevronDown` for collapse chevron indicator
- Extend `NavItem` interface with optional `children?: NavSubItem[]`

**i18n.ts** updated to include `consumables` in the `ns` array.

**common.json (EN/VI)** updated with pharmacy sub-nav sidebar keys:
- EN: `pharmacyInventory`, `pharmacyQueue`, `pharmacySuppliers`, `pharmacyStockImport`, `pharmacyOtcSales`
- VI: Kho thuốc, Hàng đợi pha chế, Nhà cung cấp, Nhập kho, Bán lẻ không đơn

### Task 2: Create Consumables Translation Files (0253e1b)

**en/consumables.json** created with sections:
- `warehouse`: title, subtitle, empty state, search
- `item`: name, trackingMode, currentStock, minStockLevel, status labels
- `actions`: addConsumable, editConsumable, addStock, adjustStock
- `stock`: add/adjust stock form fields, reasons, success messages
- `alerts`: lowStockTitle, description with count interpolation, none state

**vi/consumables.json** created with proper Vietnamese diacritics:
- Kho vật tư tiêu hao, Thêm vật tư, Thêm hàng, Điều chỉnh
- Phương thức theo dõi, Theo dõi hạn sử dụng, Theo dõi số lượng
- Tồn kho hiện tại, Tồn kho tối thiểu, Cảnh báo tồn kho thấp

## Verification

- TypeScript compiles with no errors in modified files (pre-existing errors in other files unrelated)
- Pharmacy nav shows collapsible sub-items with chevron indicator
- Badge visible on Pharmacy when pending prescriptions exist (pendingCount > 0)
- Consumables namespace registered in i18n configuration
- All Vietnamese text uses proper diacritics

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Enhancement] Added pharmacy sub-nav sub-item keys to common.json**
- **Found during:** Task 1
- **Issue:** Plan referenced `sidebar.pharmacyInventory` etc. in sub-nav but these keys didn't exist in common.json
- **Fix:** Added `pharmacyInventory`, `pharmacyQueue`, `pharmacySuppliers`, `pharmacyStockImport`, `pharmacyOtcSales` to both EN and VI common.json
- **Files modified:** `frontend/public/locales/en/common.json`, `frontend/public/locales/vi/common.json`
- **Commit:** c75e3f3

**2. [Rule 2 - Enhancement] Pharmacy already active (no disabled flag) in prior plan**
- **Found during:** Task 1 analysis
- **Issue:** Plan said "Change pharmacy nav item from disabled:true to active" but it was already active from a previous plan
- **Fix:** Kept active state, focused on adding badge and sub-nav (the actual new work needed)
- **Impact:** None - no behavioral regression

## Commits

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Update sidebar with pharmacy sub-nav and badge | c75e3f3 | AppSidebar.tsx, i18n.ts, common.json (EN/VI) |
| 2 | Create consumables i18n translation files | 0253e1b | en/consumables.json, vi/consumables.json |

## Self-Check: PASSED
