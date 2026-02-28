# Phase 1: Foundation & Infrastructure - Context

**Gathered:** 2026-02-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Deliver a deployed modular monolith skeleton with authentication (login, session management, RBAC), audit logging with admin UI, bilingual Vietnamese/English UI, and all architectural foundations (schema-per-module DB, Azure Blob Storage, Wolverine FX messaging, BranchId multi-tenant scaffolding). Staff can log in, manage roles/permissions, switch languages, and view audit logs.

</domain>

<decisions>
## Implementation Decisions

### Backend scaffolding — Modular Monolith
- 4 layers per module, each a separate .csproj class library: Domain → Contracts → Application → Infrastructure
- **Domain**: Entities, value objects, domain events. References only Shared.Domain. Pure C#, private setters, DDD patterns
- **Contracts**: Public DTOs, integration events, shared interfaces. This is the module's public face — the ONLY thing other modules reference
- **Application**: Use cases, command/query handlers, AND Wolverine.HTTP endpoints. References Domain + Contracts
- **Infrastructure**: EF DbContext (schema-per-module), concrete repos, external services. References Application + Domain
- Cross-module communication only through Contracts — never reference another module's Domain/Application/Infrastructure
- Shared kernel: Shared.Domain (AggregateRoot, Entity, BranchId, IDomainEvent), Shared.Contracts, Shared.Application (ICurrentUser, CQRS abstractions), Shared.Infrastructure (EF interceptors, Wolverine config, Azure Blob)
- Bootstrapper host project wires all modules, middleware, and config

### Wolverine.HTTP endpoints
- Wolverine.HTTP for all API endpoints — no separate MediatR, Wolverine IS the mediator
- Endpoints live in Application layer (not Infrastructure or Presentation) — handler methods with `[WolverineGet]`/`[WolverinePost]` attributes alongside command/query handlers
- Bootstrapper auto-discovers endpoints via `MapWolverineEndpoints`
- Built-in multi-tenancy detection for BranchId
- FluentValidation middleware via `UseFluentValidationProblemDetailMiddleware()`

### Database
- **SQL Server** — single database, schema-per-module isolation (e.g., `auth.Users`, `audit.AuditLogs`, `patient.Patients`)
- Per-module EF Core migrations — each module has its own Migrations folder and migration history table (e.g., `auth.__EFMigrationsHistory`)
- Independent schema evolution per module

### Authentication
- JWT + refresh token — stateless JWT access token (short-lived) + refresh token (longer-lived)
- "Remember me" checkbox controls refresh token duration — both durations admin-configurable in DB SystemSettings table

### Error handling
- Result pattern — commands/queries return `Result<T>` with typed errors, no throwing for expected failures
- Wolverine maps Result.Failure to ProblemDetails automatically

### Testing strategy
- Bogus for test data generation — builder pattern per entity (UserFaker, RoleFaker), reproducible with seed values
- Testcontainers for integration tests — real SQL Server in Docker per test run, tests real schema isolation and constraints
- NetArchTest for architecture tests — automated enforcement of module boundary rules (Domain has no infra refs, no cross-module references)
- TDD strictly: write failing tests first, then implement (red-green-refactor)

### Data seeding
- Code-based seeding via IHostedService — creates 7 default roles, permission matrix, and root admin user on startup
- Idempotent and version-controlled — only creates if not exists

### Login & session experience
- Split layout: branding/clinic imagery on left, login form on right
- Session timeout shows a warning countdown modal (e.g., "Session expires in 2 minutes") with option to extend — prevents losing unsaved clinical work
- Password reset: admin-only for Phase 1 (admin resets passwords manually) — no self-service email flow
- Session timeout duration is configurable via admin settings (stored in DB, not just appsettings.json)

### Role & permission admin
- Permissions grouped by module (Patient, Scheduling, Pharmacy, etc.) with checkboxes per action — maps to the modular monolith bounded contexts
- 7 predefined roles (Doctor, Technician, Nurse, Cashier, Optical Staff, Manager, Accountant) ship with preset permission templates — admin can customize from defaults
- Custom roles allowed — admin can create new roles beyond the 7 predefined ones
- Multiple roles per user — user gets union of all permissions from assigned roles (realistic for small clinic where staff wear multiple hats)

### Language switching
- i18next for internationalization
- Language preference stored per-user in DB — persists across devices and sessions
- Toggle placed in top navigation bar — always accessible, one click to switch
- Vietnamese as default language for new users
- Medical/clinical terms (ICD-10 codes, drug generic names, measurement labels like TBUT, OSDI, SPH, CYL) stay in English regardless of UI language — international medical standard

### Audit log visibility
- Admin UI for browsing audit logs included in Phase 1 (not backend-only)
- Access restricted to Manager and Owner/Admin roles only
- Filterable by: user (who), action type (login, view record, edit, etc.), and date range
- CSV/Excel export for compliance reporting and So Y te inspections
- TanStack Table for audit log data table

### Frontend architecture
- TanStack Start — SPA mode for clinic management, SSR for patient/customer portal
- Feature-based folder structure mirroring backend modules: `features/auth/`, `features/audit/`, etc.
- Each feature has: components/, hooks/, api/ (generated from openapi-typescript), routes/
- Shared UI in `shared/` (shadcn/ui components, common hooks, i18n, Zustand stores)
- App shell in `app/` (TanStack Router config, layouts)
- Zustand for client-side state (sidebar toggle, UI state, active filters)
- TanStack Query for server state
- TanStack Table for data tables
- React Hook Form + Zod for form handling and validation
- openapi-typescript to generate TypeScript types from backend OpenAPI spec

### App shell layout
- Collapsible sidebar + top bar
- Left sidebar with module navigation (collapsible to icons)
- Top bar for user info, language toggle, notifications

### Claude's Discretion
- Loading skeleton and spinner design for the login and admin pages
- Exact spacing, typography, and responsive breakpoints within shadcn/ui conventions
- Error state handling (failed login, network errors)
- Audit log pagination strategy (offset vs cursor)
- Exact audit log retention/archival implementation (append-only table, partitioning)
- Session warning countdown duration (how many minutes before timeout to show warning)
- API versioning strategy

</decisions>

<specifics>
## Specific Ideas

- Backend and frontend are separate projects in root-level folders (`backend/` and `frontend/`), deployed as two independent services
- Use .NET 10 (not .NET 9) — user explicitly requested the latest .NET version
- From assumptions discussion: ICD-10 ophthalmology subset only, audit logging configurable per entity (IAuditable), scaffold empty DbContexts for all future modules, DDD private set properties, GRASP patterns

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 01-foundation-infrastructure*
*Context gathered: 2026-02-28*
