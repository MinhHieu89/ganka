# Phase 11: Granular Permission Enforcement - Context

**Gathered:** 2026-03-24
**Status:** Ready for planning

<domain>
## Phase Boundary

Enforce granular permission-based authorization on ALL API endpoints (not just JWT authentication) and add `beforeLoad` permission guards to ALL frontend authenticated routes. Treatment module and audit-logs serve as the reference implementation.

</domain>

<decisions>
## Implementation Decisions

### Permission Mapping Strategy
- **D-01:** Use module-level mapping — map each endpoint to its module + CRUD action (e.g., `Patient.View` for GET, `Patient.Create` for POST, `Patient.Update` for PUT, `Patient.Delete` for DELETE, `Patient.Export` for export endpoints)
- **D-02:** `Manage` permission means full access for that module (View+Create+Update+Delete+Export), but permission checking uses **explicit matching only** — the `RequirePermissions()` extension does NOT auto-expand `.Manage` to cover `.View` etc. The seeder already assigns all individual permissions per role, so explicit matching works correctly.
- **D-03:** Apply permissions to ALL module endpoint groups in one sweep: Patient, Clinical, Scheduling, Pharmacy, Optical, Billing, Auth/Admin, Audit, Settings. Treatment already done as reference.

### Admin Route Mapping
- **D-04:** Admin routes map to their logical modules:
  - `/api/admin/users` → Auth.View / Auth.Create / Auth.Update
  - `/api/admin/roles` → Auth.View / Auth.Create / Auth.Update
  - `/api/admin/clinic-settings` → Settings.View / Settings.Update
  - `/api/admin/audit-logs` → Audit.View
  - `/api/admin/access-logs` → Audit.View

### Unauthorized User Experience
- **D-05:** Frontend nav items for restricted modules are **hidden** (not shown at all if user lacks the required `.View` permission for that module)
- **D-06:** Direct URL navigation to a restricted route → **redirect to /dashboard + toast error** ("You do not have permission to access this page"). Matches existing audit-logs guard pattern.

### Rollout & Safety
- **D-07:** Trust the existing seeder — all 8 predefined roles already have correct individual permissions assigned. No bypass flag or feature toggle needed.
- **D-08:** Users will need to re-login after deployment to get updated JWT claims with enforced permissions.

### Claude's Discretion
- Exact permission mapping for each endpoint within each module (following the module-level CRUD pattern decided above)
- Implementation order across modules
- How to structure the `beforeLoad` guard helper (reusable utility vs per-route inline check)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Authorization Infrastructure
- `backend/src/Shared/Shared.Domain/Permissions.cs` — All permission constants (Module.Action format)
- `backend/src/Shared/Shared.Presentation/EndpointAuthorizationExtensions.cs` — `RequirePermissions()` extension method
- `backend/src/Modules/Auth/Auth.Domain/Enums/PermissionModule.cs` — Permission module enum
- `backend/src/Modules/Auth/Auth.Domain/Enums/PermissionAction.cs` — Permission action enum

### Reference Implementations (Already Guarded)
- `backend/src/Modules/Treatment/Treatment.Presentation/TreatmentApiEndpoints.cs` — Backend reference with `RequirePermissions()` usage
- `frontend/src/app/routes/_authenticated/treatments/index.tsx` — Frontend reference with `beforeLoad` permission guard
- `frontend/src/app/routes/_authenticated/admin/audit-logs.tsx` — Frontend reference for audit-logs guard

### Auth Infrastructure
- `backend/src/Modules/Auth/Auth.Infrastructure/Seeding/AuthDataSeeder.cs` — Role-permission seeding (all 8 roles)
- `backend/src/Modules/Auth/Auth.Infrastructure/Services/JwtService.cs` — JWT claim generation (permissions in token)
- `frontend/src/shared/stores/authStore.ts` — Frontend auth store with `permissions[]` array

### Audit Report
- `.planning/v1.0-MILESTONE-AUDIT.md` — v1.0 audit identifying AUTH-04/AUTH-05 gaps and admin route integration gap

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `RequirePermissions()` extension — ready to use on any endpoint group, accepts `params string[] permissions`
- `Permissions` static class — all 60 permission constants already defined (10 modules x 6 actions)
- Auth store `user.permissions[]` — frontend already receives permissions array from JWT
- Existing `beforeLoad` guard pattern in audit-logs and treatment routes — copy and adapt

### Established Patterns
- Treatment module: `group.MapPost("/protocols", handler).RequirePermissions(Permissions.Treatment.Create)` — per-endpoint permission binding
- Frontend: `beforeLoad` checks `user?.permissions?.includes("Module.Action")` with toast error and redirect
- All API endpoints grouped under `/api/{module}` or `/api/admin/{resource}` route groups

### Integration Points
- Each module's `*ApiEndpoints.cs` file — add `.RequirePermissions()` calls
- Each frontend route file under `_authenticated/` — add `beforeLoad` guards
- Frontend sidebar/navigation component — add permission-based visibility filtering

</code_context>

<specifics>
## Specific Ideas

No specific requirements — standard systematic enforcement following the Treatment module pattern across all modules.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

### Reviewed Todos (not folded)
The following todos were matched by keyword but are unrelated to permission enforcement:
- Chart view for dry eye metrics — UI feature, not auth
- Clinic logo upload endpoint — API feature, not auth
- Patient name link to detail page — UI navigation, not auth
- Print all pharmacy labels — UI feature, not auth
- Realtime OSDI score update — UI feature, not auth
- View OSDI question answers — UI feature, not auth
- Auto expand optical prescription — UI behavior, not auth
- Auto focus search field — UI behavior, not auth
- Import drugs from Excel — data feature, not auth
- Server side pagination for pharmacy — performance, not auth
- Textarea auto-expand — UI behavior, not auth
- Stock import drug search — bug fix, not auth
- OTC sale stock validation — UX fix, not auth

</deferred>

---

*Phase: 11-granular-permission-enforcement*
*Context gathered: 2026-03-24*
