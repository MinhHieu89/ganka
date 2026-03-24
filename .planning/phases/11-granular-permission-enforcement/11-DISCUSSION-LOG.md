# Phase 11: Granular Permission Enforcement - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-24
**Phase:** 11-granular-permission-enforcement
**Areas discussed:** Permission mapping strategy, Unauthorized user experience, Admin route hardening, Rollout & safety

---

## Permission Mapping Strategy

### Q1: How granular should endpoint permission mapping be?

| Option | Description | Selected |
|--------|-------------|----------|
| Module-level | Map by module + action: Patient.View for GET, Patient.Create for POST, etc. Matches Treatment module pattern. | ✓ |
| Fine-grained sub-resources | Some endpoints get elevated permissions (e.g., Patient.View for basic info but Clinical.View for medical records). More precise but complex. | |
| You decide | Claude picks based on codebase patterns | |

**User's choice:** Module-level (Recommended)
**Notes:** Consistent with existing Treatment module implementation. Simple, easy to reason about.

### Q2: Should Manage permission act as wildcard granting all actions?

| Option | Description | Selected |
|--------|-------------|----------|
| Yes, Manage = all | If user has Treatment.Manage, they can do everything in Treatment. Matches domain model. | |
| Manage = admin-only actions | Manage only covers special operations. View/Create/Update/Delete checked separately. | |

**User's choice:** Yes, Manage = all (conceptually), but with explicit matching in RequirePermissions()
**Notes:** The seeder already assigns all individual permissions per role, so explicit matching works without auto-expansion.

### Q3: Should Manage auto-expand in RequirePermissions()?

| Option | Description | Selected |
|--------|-------------|----------|
| Backend auto-expands Manage | Modify RequirePermissions() to accept Module.Manage when checking Module.View | |
| Keep explicit matching | Only exact permission matches work. Seeder handles assigning all needed permissions. | ✓ |
| You decide | Claude picks based on seeder and JWT claims | |

**User's choice:** Keep explicit matching
**Notes:** Seeder already maps all 8 roles to individual permissions correctly. No need to add auto-expansion logic.

---

## Unauthorized User Experience

### Q1: How should frontend handle nav items user doesn't have permission for?

| Option | Description | Selected |
|--------|-------------|----------|
| Hide them | Nav items don't appear if user lacks permission. Clean UI. | ✓ |
| Show but disable | Greyed out with lock icon/tooltip | |
| Show and redirect | All visible, click shows toast and redirects | |

**User's choice:** Hide them (Recommended)
**Notes:** Users only see what they can access.

### Q2: What happens on direct URL navigation to restricted page?

| Option | Description | Selected |
|--------|-------------|----------|
| Redirect to dashboard + toast | Redirect to /dashboard with toast error message | ✓ |
| Show 403 page | Dedicated Access Denied page | |
| You decide | Claude picks based on existing patterns | |

**User's choice:** Redirect to dashboard + toast (Recommended)
**Notes:** Matches existing audit-logs guard pattern.

---

## Admin Route Hardening

### Q1: What permission should admin routes require?

| Option | Description | Selected |
|--------|-------------|----------|
| Auth.View / Auth.Manage | Map to logical modules: users/roles → Auth, settings → Settings, audit → Audit | ✓ |
| Auth.Manage for all admin | All /admin/* require Auth.Manage | |
| You decide | Claude maps each endpoint appropriately | |

**User's choice:** Auth.View / Auth.Manage (Recommended)
**Notes:** Per-module mapping for admin routes.

### Q2: Should all module endpoints be hardened or only audit gaps?

| Option | Description | Selected |
|--------|-------------|----------|
| All modules | Systematic sweep across all modules | ✓ |
| Audit gaps only | Only fix what v1.0 audit flagged | |
| You decide | Claude audits and adds where missing | |

**User's choice:** All modules (Recommended)
**Notes:** One sweep, fully secured.

---

## Rollout & Safety

### Q1: Concern about locking out existing users?

| Option | Description | Selected |
|--------|-------------|----------|
| Trust the seeder | Seeder already maps all 8 roles correctly. Re-login gets updated tokens. | ✓ |
| Add bypass flag | Temporary config flag to skip permission checks | |
| You decide | Claude picks safest approach | |

**User's choice:** Trust the seeder (Recommended)
**Notes:** No bypass flag needed. Users re-login after deployment.

---

## Claude's Discretion

- Exact per-endpoint permission mapping within each module
- Implementation order across modules
- How to structure the beforeLoad guard helper

## Deferred Ideas

None — discussion stayed within phase scope.
