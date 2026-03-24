---
phase: 11-granular-permission-enforcement
plan: 02
subsystem: frontend
tags: [rbac, permission-guard, route-protection, sidebar-filtering, tanstack-router]

# Dependency graph
requires:
  - phase: 11-00
    provides: "Research on permission constants and route structure"
provides:
  - "Reusable requirePermission() utility for TanStack Router beforeLoad guards"
  - "Permission guards on all 29 authenticated route files (except dashboard)"
  - "Permission-filtered sidebar navigation for all three nav groups (clinic, operations, admin)"
affects: [11-03]

# Tech tracking
tech-stack:
  added: []
  patterns: ["beforeLoad permission guard with redirect to /dashboard + toast error", "Centralized requirePermission utility reading from authStore", "Sidebar nav filtering by Module.View permission"]

key-files:
  created:
    - "frontend/src/shared/utils/permission-guard.ts"
  modified:
    - "frontend/src/shared/components/AppSidebar.tsx"
    - "frontend/src/app/routes/_authenticated/patients/index.tsx"
    - "frontend/src/app/routes/_authenticated/patients/$patientId.tsx"
    - "frontend/src/app/routes/_authenticated/clinical/index.tsx"
    - "frontend/src/app/routes/_authenticated/visits/$visitId.tsx"
    - "frontend/src/app/routes/_authenticated/appointments/index.tsx"
    - "frontend/src/app/routes/_authenticated/pharmacy/index.tsx"
    - "frontend/src/app/routes/_authenticated/pharmacy/drug-catalog.tsx"
    - "frontend/src/app/routes/_authenticated/pharmacy/queue.tsx"
    - "frontend/src/app/routes/_authenticated/pharmacy/dispensing-history.tsx"
    - "frontend/src/app/routes/_authenticated/pharmacy/suppliers.tsx"
    - "frontend/src/app/routes/_authenticated/pharmacy/stock-import.tsx"
    - "frontend/src/app/routes/_authenticated/pharmacy/otc-sales.tsx"
    - "frontend/src/app/routes/_authenticated/consumables/index.tsx"
    - "frontend/src/app/routes/_authenticated/billing/index.tsx"
    - "frontend/src/app/routes/_authenticated/billing/invoices.index.tsx"
    - "frontend/src/app/routes/_authenticated/billing/invoices.$invoiceId.tsx"
    - "frontend/src/app/routes/_authenticated/billing/shifts.tsx"
    - "frontend/src/app/routes/_authenticated/billing/service-catalog.tsx"
    - "frontend/src/app/routes/_authenticated/optical/frames.tsx"
    - "frontend/src/app/routes/_authenticated/optical/lenses.tsx"
    - "frontend/src/app/routes/_authenticated/optical/orders.tsx"
    - "frontend/src/app/routes/_authenticated/optical/orders.index.tsx"
    - "frontend/src/app/routes/_authenticated/optical/orders.$orderId.tsx"
    - "frontend/src/app/routes/_authenticated/optical/combos.tsx"
    - "frontend/src/app/routes/_authenticated/optical/warranty.tsx"
    - "frontend/src/app/routes/_authenticated/optical/stocktaking.tsx"
    - "frontend/src/app/routes/_authenticated/admin/users.tsx"
    - "frontend/src/app/routes/_authenticated/admin/roles.tsx"
    - "frontend/src/app/routes/_authenticated/admin/clinic-settings.tsx"
    - "frontend/src/app/routes/_authenticated/admin/audit-logs.tsx"

decisions:
  - "D-01: No 'Admin' permission check -- admin role has all individual permissions assigned by seeder"
  - "D-02: Simplified hasAdminAccess from .some() with 4 conditions to single Auth.View includes check"
  - "D-03: Consumables route uses Pharmacy.View permission since consumables is part of pharmacy module"
  - "D-04: stock-import uses Pharmacy.Create (write operation) while other pharmacy routes use Pharmacy.View"
  - "D-05: service-catalog uses Billing.Manage (admin operation) while other billing routes use Billing.View"

metrics:
  duration: "13min"
  completed: "2026-03-24"
  tasks: 4
  files: 32
---

# Phase 11 Plan 02: Frontend Route Permission Guards & Sidebar Filtering Summary

Reusable requirePermission() utility with beforeLoad guards on all 29 authenticated routes plus permission-filtered sidebar hiding restricted nav items

## What Was Done

### Task 1: Permission Guard Utility + Patient/Clinical/Scheduling Routes (7 files)
- Created `frontend/src/shared/utils/permission-guard.ts` with `requirePermission()` function
- Reads `user.permissions` from authStore via `getState()`, redirects to `/dashboard` with toast on denial
- Added beforeLoad guards to: patients/index (Patient.View), patients/$patientId (Patient.View), clinical/index (Clinical.View), visits/$visitId (Clinical.View), appointments/index (Scheduling.View)
- Refactored audit-logs.tsx to use shared utility, removing redundant "Admin" permission check
- Commit: `7c7c87d`

### Task 2: Pharmacy & Consumables Routes (8 files)
- Added beforeLoad guards to all 7 pharmacy routes (Pharmacy.View, stock-import uses Pharmacy.Create)
- Added beforeLoad guard to consumables/index (Pharmacy.View)
- Commit: `776e290`

### Task 3: Billing, Optical & Admin Routes (16 files)
- Added beforeLoad guards to 5 billing routes (Billing.View, service-catalog uses Billing.Manage)
- Added beforeLoad guards to 8 optical routes (Optical.View)
- Added beforeLoad guards to 3 admin routes (Auth.View for users/roles, Settings.View for clinic-settings)
- Commit: `a9ed643`

### Task 4: Sidebar Navigation Filtering
- Added 10 permission check variables for all module groups
- Simplified hasAdminAccess from complex `.some()` to single `Auth.View` includes
- Created filteredClinicItems, filteredOperationsItems, filteredAdminItems arrays
- Conditionally render entire sidebar groups only if filtered items exist
- Dashboard (mainItems) remains always visible to all authenticated users
- Commit: `7e6f47c`

## Decisions Made

1. **No "Admin" permission constant** -- Removed the `|| p === "Admin"` check from audit-logs. The admin role has all individual permissions assigned by the seeder, so explicit permission checks work correctly.
2. **Simplified hasAdminAccess** -- The original `.some()` with `startsWith("Auth.Manage") || startsWith("Auth.View") || === "Auth.Manage" || === "Auth.View"` was redundant. Simplified to `includes("Auth.View")`.
3. **Consumables uses Pharmacy.View** -- Consumables is part of the pharmacy module's bounded context.
4. **stock-import uses Pharmacy.Create** -- Import is a write operation requiring higher permission than viewing.
5. **service-catalog uses Billing.Manage** -- Service catalog management is an admin operation.

## Deviations from Plan

None -- plan executed exactly as written.

## Known Stubs

None -- all routes have real permission guards wired to the authStore.

## Verification

- TypeScript compiles with zero errors in modified files (61 pre-existing errors in unrelated files)
- All route files in `_authenticated/` except `dashboard.tsx` contain `requirePermission` or `beforeLoad`
- AppSidebar.tsx filters all three nav groups (clinic, operations, admin) by permission
- Permission strings in sidebar filters match permission strings in route beforeLoad guards
- Dashboard remains accessible to all authenticated users
