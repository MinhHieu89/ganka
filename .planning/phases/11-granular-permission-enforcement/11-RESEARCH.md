# Phase 11: Granular Permission Enforcement - Research

**Researched:** 2026-03-24
**Domain:** ASP.NET Core Minimal API Authorization + TanStack Router permission guards
**Confidence:** HIGH

## Summary

This phase is a systematic enforcement sweep -- not new architecture. All infrastructure already exists: the `Permissions` static class (10 modules x 6 actions = 60 constants), the `RequirePermissions()` extension method, JWT claims with permissions, and the frontend auth store with `permissions[]`. Treatment module and a few Billing/Optical endpoints already serve as reference implementations.

The work is mechanical: add `.RequirePermissions(Permissions.Module.Action)` to every unguarded endpoint in 11 backend API files, add `beforeLoad` guards to ~30 frontend route files, and extend the sidebar's existing permission-based visibility filtering from admin-only to all module groups.

**Primary recommendation:** Follow the Treatment module pattern exactly -- per-endpoint `.RequirePermissions()` on backend, per-route `beforeLoad` guard on frontend, module-level sidebar filtering. No new libraries or architecture needed.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Use module-level mapping -- map each endpoint to its module + CRUD action (e.g., `Patient.View` for GET, `Patient.Create` for POST, `Patient.Update` for PUT, `Patient.Delete` for DELETE, `Patient.Export` for export endpoints)
- **D-02:** `Manage` permission means full access for that module (View+Create+Update+Delete+Export), but permission checking uses **explicit matching only** -- the `RequirePermissions()` extension does NOT auto-expand `.Manage` to cover `.View` etc. The seeder already assigns all individual permissions per role, so explicit matching works correctly.
- **D-03:** Apply permissions to ALL module endpoint groups in one sweep: Patient, Clinical, Scheduling, Pharmacy, Optical, Billing, Auth/Admin, Audit, Settings. Treatment already done as reference.
- **D-04:** Admin routes map to their logical modules:
  - `/api/admin/users` -> Auth.View / Auth.Create / Auth.Update
  - `/api/admin/roles` -> Auth.View / Auth.Create / Auth.Update
  - `/api/admin/clinic-settings` -> Settings.View / Settings.Update
  - `/api/admin/audit-logs` -> Audit.View
  - `/api/admin/access-logs` -> Audit.View
- **D-05:** Frontend nav items for restricted modules are **hidden** (not shown at all if user lacks the required `.View` permission for that module)
- **D-06:** Direct URL navigation to a restricted route -> **redirect to /dashboard + toast error** ("You do not have permission to access this page"). Matches existing audit-logs guard pattern.
- **D-07:** Trust the existing seeder -- all 8 predefined roles already have correct individual permissions assigned. No bypass flag or feature toggle needed.
- **D-08:** Users will need to re-login after deployment to get updated JWT claims with enforced permissions.

### Claude's Discretion
- Exact permission mapping for each endpoint within each module (following the module-level CRUD pattern decided above)
- Implementation order across modules
- How to structure the `beforeLoad` guard helper (reusable utility vs per-route inline check)

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| AUTH-04 | User session persists with token refresh, times out after inactivity, supports logout | Token refresh and logout already implemented. This phase closes the "granular permission" sub-gap: all endpoints enforce permission policies, not just JWT auth |
| AUTH-05 | System logs all login attempts, record access, and data views (access logging) | Access logging already implemented. This phase closes the "access log route guard" sub-gap: audit-logs frontend route gets `beforeLoad` guard (already done as reference), all other admin routes get matching guards |
</phase_requirements>

## Standard Stack

No new libraries needed. Everything uses existing project infrastructure.

### Core (Already in Project)
| Library | Purpose | Location |
|---------|---------|----------|
| ASP.NET Core Minimal API auth | `RequireAuthorization()` + custom policy assertions | Already on all endpoint groups |
| `RequirePermissions()` extension | Permission claim checking via JWT | `Shared.Presentation/EndpointAuthorizationExtensions.cs` |
| `Permissions` static class | 60 permission string constants | `Shared.Domain/Permissions.cs` |
| TanStack Router `beforeLoad` | Route-level permission guards | Already used in 5 route files |
| Zustand auth store | `user.permissions[]` array from JWT | `frontend/src/shared/stores/authStore.ts` |
| sonner `toast` | Permission denial toast messages | Already used in treatment/audit routes |

## Architecture Patterns

### Pattern 1: Backend Endpoint Permission Binding (Reference: Treatment module)

**What:** Chain `.RequirePermissions(Permissions.Module.Action)` after each endpoint registration.
**When to use:** Every endpoint that mutates or reads module-specific data.

```csharp
// Source: Treatment.Presentation/TreatmentApiEndpoints.cs (existing)
group.MapPost("/protocols", handler).RequirePermissions(Permissions.Treatment.Create);
group.MapGet("/protocols", handler);  // GET endpoints get .RequirePermissions(Permissions.Treatment.View)
group.MapPut("/protocols/{id:guid}", handler).RequirePermissions(Permissions.Treatment.Update);
group.MapDelete("/protocols/{id}", handler).RequirePermissions(Permissions.Treatment.Delete);
```

**Key detail:** The `RequirePermissions()` uses `RequireAssertion` with `permissions.Any()` -- meaning it checks if the user has AT LEAST ONE of the specified permissions. This is OR logic, not AND.

### Pattern 2: Frontend Route Guard (Reference: audit-logs, treatments)

**What:** `beforeLoad` hook checks `user.permissions` and redirects with toast on denial.

```typescript
// Source: frontend/src/app/routes/_authenticated/admin/audit-logs.tsx (existing)
export const Route = createFileRoute("/_authenticated/admin/audit-logs")({
  beforeLoad: () => {
    const { user } = useAuthStore.getState()
    const hasPermission =
      user?.permissions?.includes("Audit.View") ||
      user?.permissions?.includes("Admin")

    if (!hasPermission) {
      toast.error("You do not have permission to view audit logs")
      throw redirect({ to: "/dashboard" })
    }
  },
  component: AuditLogPage,
})
```

### Pattern 3: Sidebar Permission Filtering (Reference: admin group)

**What:** Conditionally render sidebar nav groups based on user permissions.
**Current state:** Only admin group is filtered (`hasAdminAccess` checks `Auth.View` or `Auth.Manage`). Clinic and Operations groups show to all authenticated users.
**Target state:** Each sidebar group/item checks the `.View` permission for its module.

```typescript
// Source: frontend/src/shared/components/AppSidebar.tsx (existing admin filter)
const hasAdminAccess = user?.permissions?.some(
  (p) => p.startsWith("Auth.Manage") || p.startsWith("Auth.View") || p === "Auth.Manage" || p === "Auth.View"
) ?? false

// Target: extend to all modules
const hasPatientAccess = user?.permissions?.includes("Patient.View") ?? false
const hasPharmacyAccess = user?.permissions?.includes("Pharmacy.View") ?? false
// etc.
```

### Recommended Helper: Reusable Permission Check Utility

Create a shared helper to reduce duplication across ~30 route files:

```typescript
// frontend/src/shared/utils/permission-guard.ts
import { redirect } from "@tanstack/react-router"
import { useAuthStore } from "@/shared/stores/authStore"
import { toast } from "sonner"

export function requirePermission(permission: string, errorMessage?: string) {
  const { user } = useAuthStore.getState()
  const hasPermission =
    user?.permissions?.includes(permission) ||
    user?.permissions?.includes("Admin")

  if (!hasPermission) {
    toast.error(errorMessage ?? "You do not have permission to access this page")
    throw redirect({ to: "/dashboard" })
  }
}

// Usage in route files:
// beforeLoad: () => requirePermission("Patient.View")
```

### Anti-Patterns to Avoid
- **Group-level RequirePermissions on mixed-CRUD groups:** Don't apply a single permission to the whole route group -- apply per-endpoint based on the HTTP method's CRUD action
- **Forgetting GET endpoints:** The Treatment reference leaves some GET endpoints unguarded (e.g., protocol list, package list). Per D-01, GET endpoints should get `.View` permission. Decide per-module whether read endpoints need guarding
- **Hardcoding "Admin" string:** The existing reference checks `user?.permissions?.includes("Admin")` -- this string is not in the `Permissions` class. Verify whether an "Admin" permission actually exists in the seeder, or if this is dead code

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Permission claim checking | Custom middleware or filters | `RequirePermissions()` extension | Already handles OR-logic, integrates with ASP.NET auth pipeline |
| Route protection | Custom React context/HOC | TanStack Router `beforeLoad` | Built into router, runs before component mount, supports redirect |
| Permission constants | String literals | `Permissions.Module.Action` constants | Type-safe, single source of truth, caught at compile time |

## Endpoint Inventory: Current State vs Target

### Backend Files Needing Permission Enforcement

| File | Module | Current Guards | Needs Work |
|------|--------|---------------|------------|
| `PatientApiEndpoints.cs` | Patient | None (only `RequireAuthorization`) | YES -- all CRUD endpoints |
| `ClinicalApiEndpoints.cs` | Clinical | None | YES -- all endpoints |
| `SchedulingApiEndpoints.cs` | Scheduling | None | YES -- all endpoints |
| `PharmacyApiEndpoints.cs` | Pharmacy | None | YES -- all endpoints |
| `DispensingApiEndpoints.cs` | Pharmacy | None | YES -- all endpoints |
| `ConsumablesApiEndpoints.cs` | Pharmacy | None | YES -- all endpoints (uses `/api/consumables` path but maps to Pharmacy permissions) |
| `OpticalApiEndpoints.cs` | Optical | None | YES -- all endpoints |
| `WarrantyApiEndpoints.cs` | Optical | 1 endpoint (approve) | YES -- remaining endpoints |
| `StocktakingApiEndpoints.cs` | Optical | 1 endpoint (complete) | YES -- remaining endpoints |
| `BillingApiEndpoints.cs` | Billing | 2 endpoints (service catalog) | YES -- remaining endpoints |
| `AuthApiEndpoints.cs` | Auth | None on admin group | YES -- admin user/role/permission endpoints |
| `AuditApiEndpoints.cs` | Audit | None | YES -- all endpoints |
| `SettingsApiEndpoints.cs` | Settings | None | YES -- all endpoints |
| `TreatmentApiEndpoints.cs` | Treatment | DONE (reference) | NO -- already complete |

**Note:** `PublicBookingEndpoints.cs` (Scheduling) is intentionally public and should NOT be modified.

### Endpoint-to-Permission Mapping (Recommended)

#### Patient Module (`PatientApiEndpoints.cs`)
| Endpoint | Permission |
|----------|-----------|
| `POST /api/patients` | `Patient.Create` |
| `GET /api/patients/{id}` | `Patient.View` |
| `PUT /api/patients/{id}` | `Patient.Update` |
| `POST /api/patients/{id}/deactivate` | `Patient.Update` |
| `POST /api/patients/{id}/reactivate` | `Patient.Update` |
| `GET /api/patients` | `Patient.View` |
| `GET /api/patients/recent` | `Patient.View` |
| `POST /api/patients/{id}/allergies` | `Patient.Update` |
| `DELETE /api/patients/{id}/allergies/{allergyId}` | `Patient.Update` |
| `GET /api/patients/search` | `Patient.View` |
| `POST /api/patients/{id}/photo` | `Patient.Update` |
| `GET /api/patients/{id}/field-validation` | `Patient.View` |
| `GET /api/dashboard/stats` | (leave unguarded -- dashboard accessible to all authenticated) |

#### Clinical Module (`ClinicalApiEndpoints.cs`)
| Endpoint | Permission |
|----------|-----------|
| `POST /api/clinical` (create visit) | `Clinical.Create` |
| `GET /api/clinical/{id}` | `Clinical.View` |
| `GET /api/clinical/active` | `Clinical.View` |
| `PUT /api/clinical/{id}/sign-off` | `Clinical.Update` |
| `POST /api/clinical/{id}/cancel` | `Clinical.Update` |
| `POST /api/clinical/{id}/amend` | `Clinical.Update` |
| `PUT /api/clinical/{id}/stage` | `Clinical.Update` |
| All PUT/POST data endpoints (notes, refraction, diagnoses, dry-eye, images, prescriptions) | `Clinical.Create` or `Clinical.Update` |
| All GET data endpoints (ICD-10, OSDI history, comparisons, images) | `Clinical.View` |
| All DELETE endpoints | `Clinical.Delete` |
| Print endpoints | `Clinical.View` |

#### Scheduling Module (`SchedulingApiEndpoints.cs`)
| Endpoint | Permission |
|----------|-----------|
| `POST /api/appointments` | `Scheduling.Create` |
| `PUT /api/appointments/{id}/cancel` | `Scheduling.Update` |
| `PUT /api/appointments/{id}/reschedule` | `Scheduling.Update` |
| All GET endpoints | `Scheduling.View` |
| Self-booking approve/reject | `Scheduling.Update` |

#### Pharmacy Module (`PharmacyApiEndpoints.cs`, `DispensingApiEndpoints.cs`, `ConsumablesApiEndpoints.cs`)
| Endpoint Pattern | Permission |
|----------|-----------|
| GET endpoints | `Pharmacy.View` |
| POST create endpoints (suppliers, stock-imports, drugs) | `Pharmacy.Create` |
| PUT update endpoints | `Pharmacy.Update` |
| POST dispense/OTC-sale | `Pharmacy.Create` |
| Drug catalog import (preview + confirm) | `Pharmacy.Create` |
| Consumables CRUD | `Pharmacy.Create` / `Pharmacy.Update` / `Pharmacy.View` |

#### Optical Module (`OpticalApiEndpoints.cs`, `WarrantyApiEndpoints.cs`, `StocktakingApiEndpoints.cs`)
| Endpoint Pattern | Permission |
|----------|-----------|
| GET endpoints | `Optical.View` |
| POST create endpoints (frames, lenses, orders, combos) | `Optical.Create` |
| PUT update endpoints | `Optical.Update` |
| Warranty approve (already done) | `Optical.Manage` |
| Stocktaking complete (already done) | `Optical.Manage` |
| Warranty create/list/upload | `Optical.Create` / `Optical.View` / `Optical.Update` |

#### Billing Module (`BillingApiEndpoints.cs`)
| Endpoint Pattern | Permission |
|----------|-----------|
| GET endpoints | `Billing.View` |
| POST invoice/payment/discount/refund | `Billing.Create` |
| Discount/refund approve | `Billing.Manage` |
| Shift open/close | `Billing.Create` |
| Service catalog (already done) | `Billing.Manage` |
| Print/export endpoints | `Billing.View` or `Billing.Export` |

#### Auth/Admin Module (`AuthApiEndpoints.cs`)
| Endpoint | Permission |
|----------|-----------|
| `GET /api/admin/users` | `Auth.View` |
| `POST /api/admin/users` | `Auth.Create` |
| `PUT /api/admin/users/{id}` | `Auth.Update` |
| `PUT /api/admin/users/{id}/roles` | `Auth.Update` |
| `GET /api/admin/roles` | `Auth.View` |
| `POST /api/admin/roles` | `Auth.Create` |
| `PUT /api/admin/roles/{id}/permissions` | `Auth.Update` |
| `GET /api/admin/permissions` | `Auth.View` |
| Auth flow endpoints (`/api/auth/*`) | NO CHANGE -- login/refresh are unauthenticated, logout/me/language use RequireAuthorization only |

#### Audit Module (`AuditApiEndpoints.cs`)
| Endpoint | Permission |
|----------|-----------|
| `GET /api/admin/audit-logs` | `Audit.View` |
| `GET /api/admin/audit-logs/export` | `Audit.Export` |
| `GET /api/admin/access-logs` | `Audit.View` |

#### Settings Module (`SettingsApiEndpoints.cs`)
| Endpoint | Permission |
|----------|-----------|
| `GET /api/settings/clinic` | `Settings.View` |
| `PUT /api/settings/clinic` | `Settings.Update` |
| `POST /api/settings/clinic/logo` | `Settings.Update` |

### Frontend Routes Needing `beforeLoad` Guards

| Route File | Required Permission | Has Guard? |
|------------|-------------------|------------|
| `dashboard.tsx` | None (accessible to all) | N/A |
| `patients/index.tsx` | `Patient.View` | NO |
| `patients/$patientId.tsx` | `Patient.View` | NO |
| `clinical/index.tsx` | `Clinical.View` | NO |
| `visits/$visitId.tsx` | `Clinical.View` | NO |
| `appointments/index.tsx` | `Scheduling.View` | NO |
| `pharmacy/index.tsx` | `Pharmacy.View` | NO |
| `pharmacy/drug-catalog.tsx` | `Pharmacy.View` | NO |
| `pharmacy/queue.tsx` | `Pharmacy.View` | NO |
| `pharmacy/dispensing-history.tsx` | `Pharmacy.View` | NO |
| `pharmacy/suppliers.tsx` | `Pharmacy.View` | NO |
| `pharmacy/stock-import.tsx` | `Pharmacy.Create` | NO |
| `pharmacy/otc-sales.tsx` | `Pharmacy.View` | NO |
| `consumables/index.tsx` | `Pharmacy.View` | NO |
| `billing/index.tsx` | `Billing.View` | NO |
| `billing/invoices.index.tsx` | `Billing.View` | NO |
| `billing/invoices.$invoiceId.tsx` | `Billing.View` | NO |
| `billing/shifts.tsx` | `Billing.View` | NO |
| `billing/service-catalog.tsx` | `Billing.Manage` | NO |
| `optical/frames.tsx` | `Optical.View` | NO |
| `optical/lenses.tsx` | `Optical.View` | NO |
| `optical/orders.tsx` | `Optical.View` | NO |
| `optical/orders.index.tsx` | `Optical.View` | NO |
| `optical/orders.$orderId.tsx` | `Optical.View` | NO |
| `optical/combos.tsx` | `Optical.View` | NO |
| `optical/warranty.tsx` | `Optical.View` | NO |
| `optical/stocktaking.tsx` | `Optical.View` | NO |
| `treatments/index.tsx` | `Treatment.View` | YES (reference) |
| `treatments/$packageId.tsx` | `Treatment.View` | YES |
| `treatments/templates.tsx` | `Treatment.Create` | YES |
| `treatments/approvals.tsx` | `Treatment.Manage` | YES |
| `admin/users.tsx` | `Auth.View` | NO |
| `admin/roles.tsx` | `Auth.View` | NO |
| `admin/audit-logs.tsx` | `Audit.View` | YES (reference) |
| `admin/clinic-settings.tsx` | `Settings.View` | NO |

**Total routes needing new guards: ~27** (excluding dashboard and already-guarded routes)

### Sidebar Navigation Filtering

**Current state:** Only admin group is conditionally rendered based on `hasAdminAccess`.
**Target state:** Each nav group and item filtered by module permission:

| Sidebar Group | Items | Filter Permission |
|---------------|-------|-------------------|
| Main | Dashboard | None (always visible) |
| Clinic | Patients | `Patient.View` |
| Clinic | Appointments | `Scheduling.View` |
| Clinic | Clinical | `Clinical.View` |
| Operations | Pharmacy (all sub-items) | `Pharmacy.View` |
| Operations | Consumables | `Pharmacy.View` |
| Operations | Billing (all sub-items) | `Billing.View` |
| Operations | Optical (all sub-items) | `Optical.View` |
| Operations | Treatments (all sub-items) | `Treatment.View` |
| Admin | Users | `Auth.View` |
| Admin | Roles | `Auth.View` |
| Admin | Audit Logs | `Audit.View` |
| Admin | Clinic Settings | `Settings.View` |

## Common Pitfalls

### Pitfall 1: Missing Import for Shared.Presentation
**What goes wrong:** Adding `.RequirePermissions()` calls without importing the extension's namespace.
**How to avoid:** Ensure `using Shared.Presentation;` and `using Shared.Domain;` are in each endpoint file. Most files already have these imports (check each file).

### Pitfall 2: Forgetting Public/Unauthenticated Endpoints
**What goes wrong:** Accidentally guarding public endpoints like OSDI self-fill tokens or public booking.
**How to avoid:** Do NOT modify: `PublicBookingEndpoints.cs`, OSDI public form endpoints, `/api/auth/login`, `/api/auth/refresh`. These are intentionally unauthenticated.

### Pitfall 3: Dashboard Stats Breaking
**What goes wrong:** Guarding `/api/dashboard/stats` with `Patient.View` would break the dashboard for non-patient-facing roles (Cashier, Pharmacist).
**How to avoid:** Leave the dashboard stats endpoint with just `RequireAuthorization()` (no specific permission) since it aggregates cross-module data.

### Pitfall 4: Consumables Route Mismatch
**What goes wrong:** Consumables endpoints use `/api/consumables` route prefix but belong to the Pharmacy module. Using `Permissions.Settings.View` or creating a non-existent `Consumables` permission module.
**How to avoid:** Use `Permissions.Pharmacy.*` for consumables endpoints -- they are logically part of the Pharmacy module.

### Pitfall 5: Sidebar Filtering vs Route Guard Mismatch
**What goes wrong:** Sidebar hides items but the route is still accessible via direct URL. Or sidebar shows items but route guard blocks access.
**How to avoid:** The sidebar `requiredPermission` on each nav item MUST exactly match the `beforeLoad` guard permission on the corresponding route.

### Pitfall 6: Admin Permission String "Admin"
**What goes wrong:** The existing reference code checks for `user?.permissions?.includes("Admin")` but "Admin" may not be a real permission constant in `Permissions.cs`.
**How to avoid:** Check if the seeder adds an "Admin" string permission or not. If not, this check is dead code and should be removed from new guards to avoid confusion. The admin role should have all individual permissions assigned via the seeder.

### Pitfall 7: Audit Log Export Gets Wrong Permission
**What goes wrong:** Using `Audit.View` for the CSV export endpoint when it should use `Audit.Export`.
**How to avoid:** The export endpoint (`/api/admin/audit-logs/export`) should use `Permissions.Audit.Export`, not `Permissions.Audit.View`.

## Open Questions

1. **"Admin" permission string legitimacy**
   - What we know: Reference code in treatments and audit-logs checks `includes("Admin")`. The `Permissions.cs` class does not define an "Admin" constant.
   - What's unclear: Whether the seeder assigns a raw "Admin" string permission or this is dead code.
   - Recommendation: Check the seeder. If "Admin" is not a real permission, omit it from new guards. If it is, add it to `Permissions.cs` as a constant.

2. **Dashboard stats endpoint permission**
   - What we know: Dashboard is accessible to all roles. Stats come from patient/visit data.
   - Recommendation: Leave unguarded beyond `RequireAuthorization()` -- it is a cross-cutting aggregate.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + FluentAssertions (unit), custom integration test base (integration) |
| Config file | Each `*.Tests.csproj` under `backend/tests/` |
| Quick run command | `dotnet test backend/tests/Auth.Unit.Tests --no-build -v q` |
| Full suite command | `dotnet test backend/tests/ --no-build -v q` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| AUTH-04 | Permission-denied returns 403 | integration | Integration test with unauthorized user | Wave 0 |
| AUTH-05 | Access log frontend guard | manual | Manual verification of redirect behavior | N/A |

### Sampling Rate
- **Per task commit:** `dotnet build backend/` (compile check for permission imports)
- **Per wave merge:** `dotnet test backend/tests/ --no-build -v q`
- **Phase gate:** Full suite green + manual 403 verification

### Wave 0 Gaps
- [ ] Architecture test to verify all endpoint groups have `RequirePermissions()` calls (could add to `Ganka28.ArchitectureTests`)
- [ ] Manual QA: login as a restricted role, verify 403 on forbidden endpoints and redirect on forbidden routes

## Sources

### Primary (HIGH confidence)
- `backend/src/Shared/Shared.Presentation/EndpointAuthorizationExtensions.cs` -- RequirePermissions implementation
- `backend/src/Shared/Shared.Domain/Permissions.cs` -- all 60 permission constants
- `backend/src/Modules/Treatment/Treatment.Presentation/TreatmentApiEndpoints.cs` -- reference backend implementation
- `frontend/src/app/routes/_authenticated/treatments/index.tsx` -- reference frontend guard
- `frontend/src/app/routes/_authenticated/admin/audit-logs.tsx` -- reference frontend guard
- `frontend/src/shared/components/AppSidebar.tsx` -- current sidebar with admin-only filtering
- All 13 `*ApiEndpoints.cs` files reviewed for current permission state

### Secondary (MEDIUM confidence)
- `frontend/src/shared/stores/authStore.ts` -- auth store with permissions array confirmed

## Project Constraints (from CLAUDE.md)

- Apply TDD strictly: write failing tests first, then implement (red-green-refactor)
- At least 80% code coverage
- Use shadcn/ui components where applicable
- Always use Context7 MCP for library/API documentation
- When making changes to models, create and run migrations (not applicable -- no model changes in this phase)
- Backend runs at port 5255, frontend at port 3000 for verification

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- no new libraries, all infrastructure exists and is verified
- Architecture: HIGH -- reference implementations already working in production code
- Pitfalls: HIGH -- identified through direct code inspection of all endpoint files
- Endpoint mapping: MEDIUM -- exact per-endpoint permission assignments are recommendations; implementer should verify CRUD semantics per endpoint

**Research date:** 2026-03-24
**Valid until:** 2026-04-24 (stable -- no external dependencies or version concerns)
