# Phase 1: Foundation & Infrastructure - Context

**Gathered:** 2026-02-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Deliver a deployed modular monolith skeleton with authentication (login, session management, RBAC), audit logging with admin UI, bilingual Vietnamese/English UI, and all architectural foundations (schema-per-module DB, Azure Blob Storage, Wolverine FX messaging, BranchId multi-tenant scaffolding). Staff can log in, manage roles/permissions, switch languages, and view audit logs.

</domain>

<decisions>
## Implementation Decisions

### Login & session experience
- Split layout: branding/clinic imagery on left, login form on right
- No "Remember me" checkbox — all sessions use admin-configured timeout period
- Session timeout shows a warning countdown modal (e.g., "Session expires in 2 minutes") with option to extend — prevents losing unsaved clinical work
- Password reset: admin-only for Phase 1 (admin resets passwords manually) — no self-service email flow
- Session timeout duration is configurable via admin settings (stored in DB, not just appsettings.json)

### Role & permission admin
- Permissions grouped by module (Patient, Scheduling, Pharmacy, etc.) with checkboxes per action — maps to the modular monolith bounded contexts
- 7 predefined roles (Doctor, Technician, Nurse, Cashier, Optical Staff, Manager, Accountant) ship with preset permission templates — admin can customize from defaults
- Custom roles allowed — admin can create new roles beyond the 7 predefined ones
- Multiple roles per user — user gets union of all permissions from assigned roles (realistic for small clinic where staff wear multiple hats)

### Language switching
- Language preference stored per-user in DB — persists across devices and sessions
- Toggle placed in top navigation bar — always accessible, one click to switch
- Vietnamese as default language for new users
- Medical/clinical terms (ICD-10 codes, drug generic names, measurement labels like TBUT, OSDI, SPH, CYL) stay in English regardless of UI language — international medical standard

### Audit log visibility
- Admin UI for browsing audit logs included in Phase 1 (not backend-only)
- Access restricted to Manager and Owner/Admin roles only
- Filterable by: user (who), action type (login, view record, edit, etc.), and date range
- CSV/Excel export for compliance reporting and So Y te inspections

### Claude's Discretion
- Loading skeleton and spinner design for the login and admin pages
- Exact spacing, typography, and responsive breakpoints within shadcn/ui conventions
- Error state handling (failed login, network errors)
- Audit log pagination strategy (offset vs cursor)
- Exact audit log retention/archival implementation (append-only table, partitioning)
- i18n library choice on frontend (lightweight, mature, good DX)
- Session warning countdown duration (how many minutes before timeout to show warning)

</decisions>

<specifics>
## Specific Ideas

- Backend and frontend are separate projects in root-level folders (`backend/` and `frontend/`), deployed as two independent services
- TDD strictly: write failing tests first, then implement to pass (red-green-refactor)
- From assumptions discussion: corrections noted that session timeout must be configurable in admin settings (DB-stored), not just config files

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 01-foundation-infrastructure*
*Context gathered: 2026-02-28*
