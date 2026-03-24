---
phase: 11-granular-permission-enforcement
plan: 01
subsystem: backend-authorization
tags: [permissions, authorization, security, endpoints]
dependency_graph:
  requires: [11-00]
  provides: [granular-permission-enforcement]
  affects: [all-api-endpoints]
tech_stack:
  added: []
  patterns: [RequirePermissions-per-endpoint, Permissions-constants]
key_files:
  created:
    - backend/tests/Ganka28.ArchitectureTests/PermissionEnforcementTests.cs
  modified:
    - backend/src/Modules/Patient/Patient.Presentation/PatientApiEndpoints.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
    - backend/src/Modules/Scheduling/Scheduling.Presentation/SchedulingApiEndpoints.cs
    - backend/src/Modules/Auth/Auth.Presentation/AuthApiEndpoints.cs
    - backend/src/Modules/Audit/Audit.Presentation/AuditApiEndpoints.cs
    - backend/src/Shared/Shared.Presentation/SettingsApiEndpoints.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Presentation/PharmacyApiEndpoints.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Presentation/DispensingApiEndpoints.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Presentation/ConsumablesApiEndpoints.cs
    - backend/src/Modules/Optical/Optical.Presentation/OpticalApiEndpoints.cs
    - backend/src/Modules/Optical/Optical.Presentation/WarrantyApiEndpoints.cs
    - backend/src/Modules/Optical/Optical.Presentation/StocktakingApiEndpoints.cs
    - backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs
decisions:
  - "Dashboard stats endpoint left with RequireAuthorization only (accessible to all authenticated users)"
  - "Consumables endpoints use Pharmacy permissions (not a separate permission module)"
  - "Discount/refund approve endpoints use Billing.Manage (manager-only)"
  - "Warranty approve and stocktaking complete kept as Optical.Manage (pre-existing)"
  - "Export endpoints use module-specific Export permissions (Audit.Export, Billing.Export)"
metrics:
  duration: 25min
  completed: "2026-03-24T09:28:00Z"
  tasks_completed: 3
  tasks_total: 3
  files_modified: 14
---

# Phase 11 Plan 01: Add RequirePermissions to All Backend Endpoints Summary

Granular permission enforcement added to all 13 backend API endpoint files using RequirePermissions() extension method and Permissions.* constants.

## What Was Done

### Task 1: Patient, Clinical, Scheduling, Auth, Audit, Settings (commit 5790e88)
- **Patient**: 12 endpoints with Patient.View/Create/Update. Dashboard stats excluded (all authenticated).
- **Clinical**: 30+ endpoints covering visits, diagnoses, images, prescriptions, print. Uses Clinical.View/Create/Update/Delete.
- **Scheduling**: 10 endpoints with Scheduling.View/Create/Update. Public booking excluded.
- **Auth**: 8 admin endpoints with Auth.View/Create/Update. Auth flow (login/refresh/logout/me) excluded.
- **Audit**: 3 endpoints. View for logs, Export for CSV export.
- **Settings**: 3 endpoints. View for GET, Update for PUT/POST logo.

### Task 2: Pharmacy, Optical, Billing (commit 0d49576)
- **Pharmacy**: 20 endpoints with Pharmacy.View/Create/Update.
- **Dispensing**: 6 endpoints with Pharmacy.View/Create.
- **Consumables**: 7 endpoints with Pharmacy.View/Create/Update (uses Pharmacy permissions per design).
- **Optical**: 20 endpoints with Optical.View/Create/Update.
- **Warranty**: 4 endpoints. Create/View/Update + Manage for approve (pre-existing).
- **Stocktaking**: 6 endpoints. View/Create/Update + Manage for complete (pre-existing).
- **Billing**: 30+ endpoints. View/Create/Manage/Export mapped per action type.

### Task 3: Architecture Test Verification (commit 1320ce2)
- Created PermissionEnforcementTests.cs (from 11-00 spec, since parallel execution).
- Both tests pass GREEN: all endpoint files contain RequirePermissions(Permissions.*), no string literals used.
- Full backend bootstrapper builds successfully.

## Decisions Made

1. **Dashboard stats**: Left with RequireAuthorization only, accessible to all authenticated users.
2. **Consumables permissions**: Uses Pharmacy.* permissions (no separate Consumables permission module).
3. **Discount/refund approve**: Uses Billing.Manage for manager-only operations.
4. **Export actions**: Uses module-specific .Export permission (Audit.Export, Billing.Export).
5. **Pre-existing Manage guards**: Warranty approve and stocktaking complete already had Optical.Manage -- left as-is.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Created architecture test file from 11-00 spec**
- **Found during:** Task 3
- **Issue:** PermissionEnforcementTests.cs did not exist because 11-00 runs in a parallel worktree
- **Fix:** Created the test file following the 11-00 plan spec to enable GREEN verification
- **Files created:** backend/tests/Ganka28.ArchitectureTests/PermissionEnforcementTests.cs
- **Commit:** 1320ce2

## Known Stubs

None -- all endpoints are wired with actual Permissions.* constants from Shared.Domain.

## Verification Results

- All 13 endpoint files modified with RequirePermissions() calls
- Full bootstrapper build: PASSED
- Architecture tests (2/2): PASSED (GREEN)
- Public endpoints (PublicBookingEndpoints.cs, PublicOsdiEndpoints.cs): untouched
- Auth flow endpoints (login/refresh/logout/me/language): untouched
- Dashboard stats: RequireAuthorization only (no RequirePermissions)

## Self-Check: PASSED

All files verified present, all commits verified in git log.
