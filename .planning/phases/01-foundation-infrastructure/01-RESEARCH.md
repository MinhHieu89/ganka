# Phase 1: Foundation & Infrastructure - Research

**Researched:** 2026-02-28
**Domain:** Modular monolith (.NET 10 + Wolverine), SPA frontend (TanStack Start), authentication, audit logging, internationalization
**Confidence:** HIGH

## Summary

Phase 1 establishes the entire technical foundation for the Ganka28 clinic management system: a .NET 10 modular monolith backend with Wolverine as the mediator/message bus, EF Core 10 with schema-per-module SQL Server isolation, JWT authentication with refresh tokens, immutable audit logging, and a TanStack Start SPA frontend with shadcn/ui, i18next for Vietnamese/English bilingual support, and TanStack ecosystem libraries for routing, data fetching, and tables.

The stack is well-supported and production-ready. .NET 10 is GA (released November 2025, LTS until 2028). Wolverine 5.x is stable with native EF Core integration and HTTP endpoint support. TanStack Start is in RC stage (v1.154.0) but production-usable in SPA mode. EF Core 10 introduces named query filters, which directly enable the multi-tenant BranchId + soft-delete pattern without workarounds. All libraries have strong Context7/official documentation coverage.

**Primary recommendation:** Use the exact stack versions documented below. Scaffold all module DbContexts (even empty ones for future phases) in Phase 1 to validate schema isolation and migration independence early. Build authentication as a self-contained Auth module following the same 4-layer pattern as all other modules.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- 4 layers per module, each a separate .csproj class library: Domain, Contracts, Application, Infrastructure
- Domain: Entities, value objects, domain events. References only Shared.Domain. Pure C#, private setters, DDD patterns
- Contracts: Public DTOs, integration events, shared interfaces. This is the module's public face -- the ONLY thing other modules reference
- Application: Use cases, command/query handlers, AND Wolverine.HTTP endpoints. References Domain + Contracts
- Infrastructure: EF DbContext (schema-per-module), concrete repos, external services. References Application + Domain
- Cross-module communication only through Contracts -- never reference another module's Domain/Application/Infrastructure
- Shared kernel: Shared.Domain (AggregateRoot, Entity, BranchId, IDomainEvent), Shared.Contracts, Shared.Application (ICurrentUser, CQRS abstractions), Shared.Infrastructure (EF interceptors, Wolverine config, Azure Blob)
- Bootstrapper host project wires all modules, middleware, and config
- Wolverine.HTTP for all API endpoints -- no separate MediatR, Wolverine IS the mediator
- Endpoints live in Application layer with `[WolverineGet]`/`[WolverinePost]` attributes alongside command/query handlers
- Bootstrapper auto-discovers endpoints via `MapWolverineEndpoints`
- Built-in multi-tenancy detection for BranchId
- FluentValidation middleware via `UseFluentValidationProblemDetailMiddleware()`
- SQL Server -- single database, schema-per-module isolation (e.g., `auth.Users`, `audit.AuditLogs`)
- Per-module EF Core migrations -- each module has its own Migrations folder and migration history table
- JWT + refresh token -- stateless JWT access token (short-lived) + refresh token (longer-lived)
- "Remember me" checkbox controls refresh token duration -- both durations admin-configurable in DB SystemSettings table
- Result pattern -- commands/queries return `Result<T>` with typed errors, no throwing for expected failures
- Wolverine maps Result.Failure to ProblemDetails automatically
- Bogus for test data generation -- builder pattern per entity, reproducible with seed values
- Testcontainers for integration tests -- real SQL Server in Docker per test run
- NetArchTest for architecture tests -- automated enforcement of module boundary rules
- TDD strictly: write failing tests first, then implement (red-green-refactor)
- Code-based seeding via IHostedService -- creates 7 default roles, permission matrix, and root admin user on startup
- Idempotent and version-controlled -- only creates if not exists
- Split layout: branding/clinic imagery on left, login form on right
- Session timeout shows a warning countdown modal with option to extend
- Password reset: admin-only for Phase 1 (no self-service email flow)
- Session timeout duration is configurable via admin settings (stored in DB)
- Permissions grouped by module with checkboxes per action -- maps to modular monolith bounded contexts
- 7 predefined roles ship with preset permission templates -- admin can customize from defaults
- Custom roles allowed -- admin can create new roles beyond the 7 predefined ones
- Multiple roles per user -- user gets union of all permissions from assigned roles
- i18next for internationalization
- Language preference stored per-user in DB -- persists across devices and sessions
- Toggle placed in top navigation bar -- always accessible, one click to switch
- Vietnamese as default language for new users
- Medical/clinical terms (ICD-10 codes, drug names, measurement labels) stay in English regardless of UI language
- Admin UI for browsing audit logs included in Phase 1
- Access restricted to Manager and Owner/Admin roles only
- Filterable by: user, action type, and date range
- CSV/Excel export for compliance reporting
- TanStack Table for audit log data table
- TanStack Start -- SPA mode for clinic management, SSR for patient/customer portal
- Feature-based folder structure mirroring backend modules: `features/auth/`, `features/audit/`, etc.
- Each feature has: components/, hooks/, api/ (generated from openapi-typescript), routes/
- Shared UI in `shared/` (shadcn/ui components, common hooks, i18n, Zustand stores)
- App shell in `app/` (TanStack Router config, layouts)
- Zustand for client-side state (sidebar toggle, UI state, active filters)
- TanStack Query for server state
- TanStack Table for data tables
- React Hook Form + Zod for form handling and validation
- openapi-typescript to generate TypeScript types from backend OpenAPI spec
- Collapsible sidebar + top bar layout
- Backend and frontend are separate projects in root-level folders (`backend/` and `frontend/`), deployed as two independent services
- Use .NET 10 (not .NET 9)
- ICD-10 ophthalmology subset only, audit logging configurable per entity (IAuditable), scaffold empty DbContexts for all future modules, DDD private set properties, GRASP patterns

### Claude's Discretion
- Loading skeleton and spinner design for the login and admin pages
- Exact spacing, typography, and responsive breakpoints within shadcn/ui conventions
- Error state handling (failed login, network errors)
- Audit log pagination strategy (offset vs cursor)
- Exact audit log retention/archival implementation (append-only table, partitioning)
- Session warning countdown duration (how many minutes before timeout to show warning)
- API versioning strategy

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| AUTH-01 | Staff can log in with credentials and receive JWT token with role-based claims | JWT bearer auth in .NET 10, Wolverine.HTTP endpoint, BCrypt/Argon2 password hashing, short-lived access tokens |
| AUTH-02 | System supports roles: Doctor, Technician, Nurse, Cashier, Optical Staff, Manager, Accountant | EF Core entity design with Role/Permission tables, IHostedService seeding, union-of-permissions model |
| AUTH-03 | Admin can configure granular permissions per role (CRUD per entity/action) | Permission entity with Module+Action+IsAllowed, checkbox matrix UI with shadcn/ui, TanStack Table |
| AUTH-04 | User session persists with token refresh, times out after inactivity, supports logout | Refresh token rotation, httpOnly cookie storage, configurable timeout from SystemSettings table, session warning modal |
| AUTH-05 | System logs all login attempts, record access, and data views | Audit logging via EF Core SaveChanges interceptor + dedicated access logging middleware |
| AUD-01 | Field-level audit trail for all medical record changes | EF Core SaveChanges interceptor captures old/new values per property, writes to append-only audit.AuditLogs |
| AUD-02 | Access log for all user logins, logouts, and medical record views | Custom middleware for access logging, Wolverine.HTTP endpoint filter for record-view tracking |
| AUD-03 | Audit logs are immutable and retained for minimum 10 years | Append-only table (no UPDATE/DELETE permissions), SQL Server table partitioning by year for long-term retention |
| AUD-04 | ICD-10 coding from Day 1 for So Y te data readiness | Seed ICD-10 ophthalmology subset codes into reference table in auth schema or shared schema |
| UI-01 | All UI text in Vietnamese and English | i18next with react-i18next, namespace-per-feature JSON files, Vietnamese as fallback language |
| UI-02 | Staff can switch language preference per user session | Language toggle in top nav, persisted to user profile via API, i18n.changeLanguage() on toggle |
| ARC-01 | ACL adapter pattern for external system integrations | Domain ports (interfaces) in Domain layer, infrastructure adapters in Infrastructure layer |
| ARC-02 | BranchId on all aggregate roots with EF Core global query filters | Shared.Domain AggregateRoot base class with BranchId, EF Core 10 named query filters for tenant isolation |
| ARC-03 | Template engine supports adding new disease templates without code changes | Config/plugin-driven template registration -- scaffolded interface in Phase 1, implemented in Phase 4 |
| ARC-04 | Azure SQL automatic daily backup with point-in-time recovery (35 days) | Azure SQL Database built-in automated backups, configure retention via Azure Portal/CLI |
| ARC-05 | Azure Blob Storage with soft delete and versioning for medical images | Azure.Storage.Blobs SDK, enable blob versioning + soft delete, container-per-module structure |
| ARC-06 | Full data export capability ensuring data ownership | Schema-per-module allows per-module export, design export interfaces in Shared.Contracts |
</phase_requirements>

## Standard Stack

### Core Backend

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET 10 | 10.0 (GA Nov 2025, LTS) | Runtime and SDK | Latest LTS, supported until Nov 2028, EF Core 10 named query filters |
| WolverineFx | 5.x | Message bus + mediator | Replaces MediatR, built-in command/query handling, domain event publishing |
| WolverineFx.Http | 5.2.0 | HTTP endpoints | `[WolverineGet]`/`[WolverinePost]` attributes, auto-discovery via `MapWolverineEndpoints` |
| WolverineFx.Http.FluentValidation | 5.x | Request validation middleware | `UseFluentValidationProblemDetailMiddleware()` for automatic ProblemDetails on validation failure |
| WolverineFx.SqlServer | 5.x | Message persistence | Transactional inbox/outbox with SQL Server, EF Core transaction middleware |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.x | ORM + migrations | Schema-per-module, named query filters (new in EF 10), per-context migrations |
| FluentValidation | 12.1.1 | Request validation | Strongly-typed validation rules, DI integration, pairs with Wolverine middleware |
| Ardalis.Result | 10.1.0 | Result pattern | `Result<T>` with typed errors, Railway Oriented Programming (Map/Bind), maps to ProblemDetails |
| Ardalis.Result.AspNetCore | 10.1.0 | Result-to-HTTP mapping | Maps Result status to HTTP status codes (200, 400, 404, etc.) |
| Azure.Storage.Blobs | latest | Blob storage SDK | Medical image storage, soft delete + versioning support |

### Core Frontend

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| TanStack Start | RC (v1.154.0+) | SPA framework | File-based routing, SPA mode via `spa: { enabled: true }`, built on TanStack Router |
| @tanstack/react-router | v1.x | Type-safe routing | `beforeLoad` auth guards, file-based route generation, search params API |
| @tanstack/react-query | v5.x | Server state management | Cache management, background refetching, mutation support, pairs with openapi-fetch |
| @tanstack/react-table | v8.x | Headless data tables | Sorting, filtering, pagination for audit log viewer and permission matrix |
| shadcn/ui | latest (v3.x CLI) | UI component library | Copy-paste components, Tailwind CSS + Radix UI, sidebar/form/table primitives |
| Zustand | v5.x | Client-side state | Sidebar toggle, UI state, active filters -- minimal boilerplate |
| React Hook Form | latest | Form state management | Uncontrolled form performance, pairs with Zod resolver |
| Zod | latest | Schema validation | TypeScript-first validation, `zodResolver` for React Hook Form |
| i18next + react-i18next | latest | Internationalization | Language detection, namespace-per-feature, `useTranslation` hook |
| openapi-typescript | latest | Type generation from OpenAPI | Generates TypeScript types from .NET OpenAPI spec |
| openapi-fetch | 0.17.0 | Type-safe fetch client | 6kb runtime, uses types from openapi-typescript, pairs with TanStack Query |
| Tailwind CSS | v4.x | Utility-first CSS | `@tailwindcss/vite` plugin, CSS variables for theming |

### Testing

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| xUnit | latest | Unit + integration test framework | All .NET tests |
| Testcontainers.MsSql | 4.6.0 | SQL Server in Docker | Integration tests with real schema isolation |
| Bogus | 35.6.5 | Fake data generation | Test data builders (UserFaker, RoleFaker) |
| NetArchTest.eNhancedEdition | 1.4.5 | Architecture rule enforcement | Module boundary tests, dependency direction validation |
| FluentAssertions | latest | Readable test assertions | All .NET test assertions |
| Vitest | latest | Frontend unit tests | Component and hook testing |
| Playwright | latest | E2E browser tests | Login flow, language switching, audit log UI |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Wolverine | MediatR + ASP.NET Minimal APIs | MediatR is simpler but requires separate HTTP layer; Wolverine unifies mediator + messaging + HTTP |
| Ardalis.Result | FluentResults | FluentResults is more flexible but less opinionated; Ardalis.Result maps directly to HTTP status codes |
| TanStack Start | Next.js or Remix | Next.js/Remix are more mature but require Node SSR; TanStack Start SPA mode is pure client-side, simpler deployment |
| BCrypt | Argon2id | Argon2id is the 2025 gold standard (memory-hard), but BCrypt is simpler. Recommend Argon2id for new project |
| NetArchTest.Rules | NetArchTest.eNhancedEdition | eNhancedEdition has better diagnostics (explains WHY a type fails) and more rules. Use eNhancedEdition |

**Backend Installation:**
```bash
# From backend/ directory
dotnet new sln -n Ganka28

# Shared kernel projects
dotnet new classlib -n Shared.Domain -f net10.0
dotnet new classlib -n Shared.Contracts -f net10.0
dotnet new classlib -n Shared.Application -f net10.0
dotnet new classlib -n Shared.Infrastructure -f net10.0

# Auth module (example -- repeat pattern for Audit, Patient, etc.)
dotnet new classlib -n Auth.Domain -f net10.0
dotnet new classlib -n Auth.Contracts -f net10.0
dotnet new classlib -n Auth.Application -f net10.0
dotnet new classlib -n Auth.Infrastructure -f net10.0

# Bootstrapper host
dotnet new web -n Bootstrapper -f net10.0

# Key NuGet packages for Bootstrapper
dotnet add Bootstrapper package WolverineFx --version 5.*
dotnet add Bootstrapper package WolverineFx.Http --version 5.*
dotnet add Bootstrapper package WolverineFx.Http.FluentValidation --version 5.*
dotnet add Bootstrapper package WolverineFx.SqlServer --version 5.*
dotnet add Bootstrapper package Microsoft.EntityFrameworkCore.SqlServer
dotnet add Bootstrapper package Ardalis.Result
dotnet add Bootstrapper package Ardalis.Result.AspNetCore
dotnet add Bootstrapper package FluentValidation
dotnet add Bootstrapper package Azure.Storage.Blobs

# Test project
dotnet new xunit -n Ganka28.Tests -f net10.0
dotnet add Ganka28.Tests package Testcontainers.MsSql
dotnet add Ganka28.Tests package Bogus
dotnet add Ganka28.Tests package NetArchTest.eNhancedEdition
dotnet add Ganka28.Tests package FluentAssertions
```

**Frontend Installation:**
```bash
# From root directory
npm create @tanstack/start@latest frontend

# From frontend/ directory
cd frontend
npm install @tanstack/react-query @tanstack/react-table zustand
npm install react-hook-form @hookform/resolvers zod
npm install i18next react-i18next i18next-http-backend i18next-browser-languagedetector
npm install openapi-fetch
npm install -D openapi-typescript
npx shadcn@latest init
```

## Architecture Patterns

### Recommended Project Structure

```
ganka28/
├── backend/
│   ├── Ganka28.sln
│   ├── src/
│   │   ├── Bootstrapper/                    # ASP.NET Core host
│   │   │   ├── Program.cs                   # Wire all modules, middleware, Wolverine
│   │   │   ├── appsettings.json
│   │   │   └── Properties/
│   │   ├── Shared/
│   │   │   ├── Shared.Domain/               # AggregateRoot, Entity, BranchId, IDomainEvent
│   │   │   ├── Shared.Contracts/            # Cross-module DTOs, integration event interfaces
│   │   │   ├── Shared.Application/          # ICurrentUser, CQRS abstractions
│   │   │   └── Shared.Infrastructure/       # EF interceptors, Wolverine config, Azure Blob helpers
│   │   ├── Modules/
│   │   │   ├── Auth/
│   │   │   │   ├── Auth.Domain/             # User, Role, Permission entities
│   │   │   │   ├── Auth.Contracts/          # LoginRequest, UserDto, RoleDto
│   │   │   │   ├── Auth.Application/        # LoginHandler, endpoints, validators
│   │   │   │   └── Auth.Infrastructure/     # AuthDbContext (schema: auth), UserRepository
│   │   │   ├── Audit/
│   │   │   │   ├── Audit.Domain/            # AuditLog, AccessLog entities
│   │   │   │   ├── Audit.Contracts/         # AuditLogDto, AuditLogQuery
│   │   │   │   ├── Audit.Application/       # QueryAuditLogs handler, endpoints
│   │   │   │   └── Audit.Infrastructure/    # AuditDbContext (schema: audit)
│   │   │   ├── Patient/                     # Empty scaffold for Phase 2
│   │   │   │   ├── Patient.Domain/
│   │   │   │   ├── Patient.Contracts/
│   │   │   │   ├── Patient.Application/
│   │   │   │   └── Patient.Infrastructure/  # PatientDbContext (schema: patient)
│   │   │   ├── Clinical/                    # Empty scaffold for Phase 3
│   │   │   ├── Scheduling/                  # Empty scaffold for Phase 2
│   │   │   ├── Pharmacy/                    # Empty scaffold for Phase 6
│   │   │   ├── Optical/                     # Empty scaffold for Phase 8
│   │   │   ├── Billing/                     # Empty scaffold for Phase 7
│   │   │   └── Treatment/                   # Empty scaffold for Phase 9
│   │   └── ...
│   └── tests/
│       ├── Ganka28.UnitTests/
│       ├── Ganka28.IntegrationTests/
│       └── Ganka28.ArchitectureTests/
├── frontend/
│   ├── package.json
│   ├── vite.config.ts
│   ├── app.config.ts
│   ├── tsconfig.json
│   ├── components.json                      # shadcn/ui config
│   ├── public/
│   │   └── locales/
│   │       ├── vi/                          # Vietnamese translations
│   │       │   ├── common.json
│   │       │   ├── auth.json
│   │       │   └── audit.json
│   │       └── en/                          # English translations
│   │           ├── common.json
│   │           ├── auth.json
│   │           └── audit.json
│   └── src/
│       ├── app/
│       │   ├── routes/
│       │   │   ├── __root.tsx               # Root layout
│       │   │   ├── _authenticated.tsx       # Auth guard layout route
│       │   │   ├── _authenticated/
│       │   │   │   ├── dashboard.tsx
│       │   │   │   ├── admin/
│       │   │   │   │   ├── users.tsx
│       │   │   │   │   ├── roles.tsx
│       │   │   │   │   └── audit-logs.tsx
│       │   │   │   └── settings.tsx
│       │   │   └── login.tsx                # Public login route
│       │   └── router.tsx                   # Router configuration
│       ├── features/
│       │   ├── auth/
│       │   │   ├── components/              # LoginForm, SessionWarningModal
│       │   │   ├── hooks/                   # useAuth, useSession
│       │   │   └── api/                     # Generated types + fetch client
│       │   └── audit/
│       │       ├── components/              # AuditLogTable, AuditLogFilters
│       │       ├── hooks/                   # useAuditLogs
│       │       └── api/
│       ├── shared/
│       │   ├── components/ui/              # shadcn/ui components
│       │   ├── hooks/                      # useLanguage, useMediaQuery
│       │   ├── lib/                        # utils.ts, api-client.ts
│       │   ├── i18n/                       # i18n config
│       │   └── stores/                     # Zustand stores (sidebar, UI state)
│       └── generated/
│           └── api-types.ts                # openapi-typescript output
└── .planning/
```

### Pattern 1: Wolverine.HTTP Endpoint with FluentValidation

**What:** Define HTTP endpoints as static methods with Wolverine attributes, co-located with command/query records and their validators.
**When to use:** Every API endpoint in the application.

```csharp
// Source: Context7 /jasperfx/wolverine - HTTP endpoints + FluentValidation
// File: Auth.Application/Login/LoginEndpoint.cs

public record LoginCommand(string Email, string Password, bool RememberMe);

public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public static class LoginEndpoint
{
    [WolverinePost("/api/auth/login")]
    public static async Task<Result<LoginResponse>> Post(
        LoginCommand command,
        IAuthService authService,
        CancellationToken ct)
    {
        return await authService.LoginAsync(command, ct);
    }
}
```

### Pattern 2: EF Core Schema-Per-Module with Named Query Filters

**What:** Each module has its own DbContext targeting a specific SQL Server schema, with BranchId and soft-delete named query filters.
**When to use:** Every module's Infrastructure layer.

```csharp
// Source: Context7 /dotnet/entityframework.docs - named query filters, multi-context migrations
// File: Auth.Infrastructure/AuthDbContext.cs

public class AuthDbContext : DbContext
{
    private readonly IBranchContext _branchContext;

    public AuthDbContext(DbContextOptions<AuthDbContext> options, IBranchContext branchContext)
        : base(options)
    {
        _branchContext = branchContext;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        // EF Core 10 named query filters
        modelBuilder.Entity<User>()
            .HasQueryFilter("Tenant", u => u.BranchId == _branchContext.CurrentBranchId)
            .HasQueryFilter("SoftDelete", u => !u.IsDeleted);

        modelBuilder.Entity<Role>()
            .HasQueryFilter("Tenant", r => r.BranchId == _branchContext.CurrentBranchId);
    }
}
```

Migration command per module:
```bash
dotnet ef migrations add InitialCreate --context AuthDbContext --output-dir Migrations --project src/Modules/Auth/Auth.Infrastructure --startup-project src/Bootstrapper -- --schema auth
```

### Pattern 3: TanStack Router Authentication Guard

**What:** Layout route with `beforeLoad` that checks auth state and redirects unauthenticated users.
**When to use:** All routes that require authentication.

```tsx
// Source: Context7 /tanstack/router - authenticated routes, beforeLoad redirect
// File: frontend/src/app/routes/_authenticated.tsx

import { createFileRoute, redirect, Outlet } from '@tanstack/react-router'

export const Route = createFileRoute('/_authenticated')({
  beforeLoad: ({ context, location }) => {
    if (!context.auth.isAuthenticated) {
      throw redirect({
        to: '/login',
        search: {
          redirect: location.href,
        },
      })
    }
  },
  component: () => <Outlet />,
})
```

### Pattern 4: i18next Setup with Feature-Based Namespaces

**What:** Initialize i18next with namespace-per-feature, lazy loading of translation files, language detection.
**When to use:** Application initialization.

```tsx
// Source: Context7 /i18next/react-i18next - initialization, useTranslation hook
// File: frontend/src/shared/i18n/i18n.ts

import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import Backend from 'i18next-http-backend'
import LanguageDetector from 'i18next-browser-languagedetector'

i18n
  .use(Backend)
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: 'vi',         // Vietnamese as default
    defaultNS: 'common',
    ns: ['common', 'auth', 'audit'],
    interpolation: {
      escapeValue: false,
    },
    backend: {
      loadPath: '/locales/{{lng}}/{{ns}}.json',
    },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
      lookupLocalStorage: 'ganka28-language',
    },
    react: {
      useSuspense: true,
    },
  })

export default i18n
```

### Pattern 5: EF Core SaveChanges Interceptor for Audit Logging

**What:** Intercept all SaveChanges calls to capture field-level changes before they are committed.
**When to use:** All entities implementing IAuditable interface.

```csharp
// Source: Context7 /dotnet/entityframework.docs - SaveChanges interceptor
// File: Shared.Infrastructure/Interceptors/AuditInterceptor.cs

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private List<AuditEntry> _auditEntries = new();

    public AuditInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        var context = eventData.Context!;
        context.ChangeTracker.DetectChanges();

        _auditEntries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => CreateAuditEntry(e))
            .ToList();

        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default)
    {
        if (_auditEntries.Any())
        {
            // Write audit entries to audit schema via AuditDbContext
            // This happens AFTER the main transaction succeeds
        }
        return await base.SavedChangesAsync(eventData, result, ct);
    }

    private AuditEntry CreateAuditEntry(EntityEntry entry)
    {
        // Capture entity name, state, old/new property values, user, timestamp
        // ...
    }
}
```

### Pattern 6: Result Pattern with Wolverine HTTP

**What:** All command/query handlers return `Result<T>`, Wolverine maps to appropriate HTTP response.
**When to use:** Every handler.

```csharp
// Source: Ardalis.Result docs + Wolverine HTTP patterns
// File: Auth.Application/Users/CreateUserHandler.cs

public record CreateUserCommand(string Email, string FullName, Guid RoleId);

public static class CreateUserEndpoint
{
    [WolverinePost("/api/admin/users")]
    public static async Task<Result<UserDto>> Post(
        CreateUserCommand command,
        IUserService userService,
        CancellationToken ct)
    {
        // Returns Result.Success(userDto) or Result.Invalid(validationErrors) or Result.Error("message")
        return await userService.CreateUserAsync(command, ct);
    }
}
```

### Anti-Patterns to Avoid

- **Cross-module direct references:** Never reference Auth.Domain from Audit.Application. Use Contracts only. NetArchTest enforces this.
- **Fat controllers/endpoints:** Wolverine endpoints should be thin -- delegate to domain services or command handlers.
- **Throwing exceptions for expected failures:** Use `Result<T>` pattern. Exceptions are for unexpected/infrastructure failures only.
- **Single DbContext for all modules:** Each module MUST have its own DbContext with its own schema. Shared entities go in Shared.Domain but are owned by a specific module's context.
- **Storing JWTs in localStorage:** Use httpOnly cookies for refresh tokens. Access tokens can be in-memory only (Zustand store).
- **Mixing translation keys with component code:** Keep translation keys in separate JSON files, use `useTranslation()` hook.
- **Mutable audit logs:** The audit table must have no UPDATE or DELETE operations. Use database-level permissions to enforce.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| HTTP endpoints + routing | Custom middleware pipeline | Wolverine.HTTP `[WolverinePost]` etc. | Auto-discovery, middleware chain, OpenAPI generation |
| Request validation | Manual if/else checks | FluentValidation + Wolverine middleware | Automatic ProblemDetails, DI-aware, composable rules |
| Password hashing | Custom hash + salt | BCrypt.Net-Next or Konscious.Security.Cryptography (Argon2) | Timing attacks, salt generation, work factor tuning -- too easy to get wrong |
| JWT token generation | Manual string building | System.IdentityModel.Tokens.Jwt | Standard claims, signing, validation, key rotation |
| Internationalization | Custom JSON loader + context | i18next + react-i18next | Namespace lazy loading, pluralization, interpolation, language detection |
| Data tables with sort/filter/page | Custom table component | TanStack Table | Headless, type-safe, handles complex column definitions, virtualization |
| Form state + validation | Manual state + error tracking | React Hook Form + Zod | Uncontrolled form perf, schema-based validation, error mapping |
| UI component primitives | Custom button/input/dialog | shadcn/ui (Radix + Tailwind) | Accessibility (ARIA), keyboard navigation, focus management |
| API type safety | Manual type definitions | openapi-typescript + openapi-fetch | Auto-generated from backend spec, catches breaking changes at compile time |
| Result/error handling | Custom result class | Ardalis.Result | Railway-oriented programming (Map/Bind), HTTP status mapping, community standard |
| Architecture rule tests | Manual code review | NetArchTest.eNhancedEdition | Automated CI enforcement, explains violations, catches regressions |

**Key insight:** This phase is almost entirely infrastructure wiring. Every problem here has a battle-tested library solution. Hand-rolling any of these will create maintenance debt from Day 1 and miss edge cases the libraries have already solved.

## Common Pitfalls

### Pitfall 1: EF Core Migration Conflicts with Multiple DbContexts
**What goes wrong:** Running `dotnet ef migrations add` without specifying `--context` applies to the wrong DbContext or fails.
**Why it happens:** EF Core CLI doesn't know which DbContext to use when multiple are registered.
**How to avoid:** Always specify `--context AuthDbContext` and `--output-dir Migrations/Auth`. Store migration history in schema-specific table: `modelBuilder.HasDefaultSchema("auth")` automatically puts `__EFMigrationsHistory` in the auth schema.
**Warning signs:** "More than one DbContext was found" error, migrations appearing in wrong project.

### Pitfall 2: Wolverine Endpoint Discovery Fails Silently
**What goes wrong:** Endpoints are defined but never appear in the OpenAPI spec or are not reachable.
**Why it happens:** Wolverine only discovers endpoints in assemblies that are referenced by the host project. If the Application layer assembly isn't referenced, endpoints are invisible.
**How to avoid:** Ensure the Bootstrapper project has a direct or transitive project reference to every module's Application layer. Use `MapWolverineEndpoints()` in `app.UseEndpoints()` pipeline. Verify with Swagger/OpenAPI.
**Warning signs:** Empty Swagger page, 404 on known endpoints, no log output for endpoint registration.

### Pitfall 3: JWT Refresh Token Reuse Attack
**What goes wrong:** A stolen refresh token can be used indefinitely to generate new access tokens.
**Why it happens:** Refresh tokens are not rotated on use, or old tokens are not invalidated.
**How to avoid:** Implement refresh token rotation: each use generates a new refresh token and invalidates the old one. Store a token family identifier -- if a revoked token is used, revoke the entire family (indicates theft).
**Warning signs:** Multiple active refresh tokens for the same session, refresh tokens with no expiry.

### Pitfall 4: i18next Key Mismatches Between Languages
**What goes wrong:** Vietnamese translation shows key path (e.g., `auth.login.title`) instead of translated text.
**Why it happens:** Key exists in English but not in Vietnamese translation file, or namespace not loaded.
**How to avoid:** Use `fallbackLng: 'vi'` (Vietnamese is primary), always add Vietnamese keys first. CI/build script to compare key coverage between language files. Type-safe keys with `i18next-resources-to-backend` or compile-time checks.
**Warning signs:** Raw key strings appearing in UI, console warnings about missing translations.

### Pitfall 5: Audit Log Performance Degradation
**What goes wrong:** Audit log table grows massive (millions of rows), queries become slow, UI pagination times out.
**Why it happens:** No indexing strategy, no partitioning, querying unpartitioned append-only table.
**How to avoid:** Create composite index on `(Timestamp DESC, UserId, ActionType)`. Use SQL Server table partitioning by year/month for 10+ year retention. Implement cursor-based pagination for the admin UI (not offset). Consider archiving old partitions to cheaper storage.
**Warning signs:** Audit log query times exceeding 1 second, increasing disk usage alerts.

### Pitfall 6: TanStack Start Package Name Change
**What goes wrong:** shadcn/ui init or other tooling fails because it looks for `@tanstack/start` but the package has been renamed.
**Why it happens:** TanStack moved their package from `@tanstack/start` to `@tanstack/react-start`.
**How to avoid:** Check current package name at install time. Use `npm create @tanstack/start@latest` for scaffolding (CLI handles the rename). Update `package.json` if shadcn/ui init references old name.
**Warning signs:** "Package not found" errors, shadcn/ui init wizard failing.

### Pitfall 7: Schema-Per-Module Migration History Table Collision
**What goes wrong:** Multiple modules write their migration history to the same `__EFMigrationsHistory` table in `dbo` schema.
**Why it happens:** EF Core defaults migration history to `dbo.__EFMigrationsHistory` unless configured.
**How to avoid:** Set `modelBuilder.HasDefaultSchema("auth")` in each DbContext's `OnModelCreating`. This automatically scopes the migration history table to the module's schema (e.g., `auth.__EFMigrationsHistory`).
**Warning signs:** Migrations from different modules appearing in the same history table, migration ordering issues.

### Pitfall 8: Wolverine DbContextOptions ServiceLifetime
**What goes wrong:** Wolverine runtime pipeline is significantly slower than expected.
**Why it happens:** DbContextOptions is registered with default Scoped lifetime instead of Singleton.
**How to avoid:** When registering DbContext, set `optionsLifetime: ServiceLifetime.Singleton`: `builder.Services.AddDbContext<AuthDbContext>(x => x.UseSqlServer(cs), optionsLifetime: ServiceLifetime.Singleton)`. This is a documented Wolverine performance optimization.
**Warning signs:** Higher-than-expected handler execution times, excessive DI container resolution.

## Code Examples

Verified patterns from official sources:

### Bootstrapper Program.cs Setup

```csharp
// Source: Context7 /jasperfx/wolverine - EF Core integration + HTTP setup
// File: Bootstrapper/Program.cs

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SqlServer")!;

// Register module DbContexts
builder.Services.AddDbContext<AuthDbContext>(
    x => x.UseSqlServer(connectionString),
    optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddDbContext<AuditDbContext>(
    x => x.UseSqlServer(connectionString),
    optionsLifetime: ServiceLifetime.Singleton);

// Register validators from all module assemblies
builder.Services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();

// Configure Wolverine
builder.Host.UseWolverine(opts =>
{
    opts.PersistMessagesWithSqlServer(connectionString, "wolverine");
    opts.UseEntityFrameworkCoreTransactions();
    opts.Policies.UseDurableLocalQueues();
});

// Register application services
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IBranchContext, BranchContext>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Map Wolverine endpoints with FluentValidation + multi-tenancy
app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
    // BranchId detection from JWT claims or header
    opts.TenantId.IsClaimTypeNamed("branch_id");
    opts.TenantId.AssertExists();
});

app.Run();
```

### Shared.Domain Base Classes

```csharp
// File: Shared.Domain/AggregateRoot.cs

public abstract class AggregateRoot : Entity
{
    public Guid BranchId { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    protected void SetBranchId(Guid branchId)
        => BranchId = branchId;
}

public abstract class Entity
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

public interface IAuditable { }
```

### Auth Domain Entities

```csharp
// File: Auth.Domain/Entities/User.cs

public class User : AggregateRoot, IAuditable
{
    public string Email { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string PreferredLanguage { get; private set; } = "vi";
    public bool IsActive { get; private set; } = true;

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User() { } // EF Core

    public static User Create(string email, string fullName, string passwordHash, Guid branchId)
    {
        var user = new User
        {
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHash,
        };
        user.SetBranchId(branchId);
        user.AddDomainEvent(new UserCreatedEvent(user.Id, email));
        return user;
    }

    public void SetLanguagePreference(string language)
    {
        PreferredLanguage = language;
    }

    public void AssignRole(Role role)
    {
        if (!_userRoles.Any(ur => ur.RoleId == role.Id))
        {
            _userRoles.Add(new UserRole(Id, role.Id));
        }
    }

    public IEnumerable<Permission> GetEffectivePermissions()
    {
        return _userRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct();
    }
}
```

### TanStack Start SPA Mode Configuration

```tsx
// Source: Context7 /websites/tanstack_start_framework_react - SPA mode
// File: frontend/vite.config.ts

import { defineConfig } from 'vite'
import tanstackStart from '@tanstack/start-vite'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [
    tanstackStart({
      spa: {
        enabled: true,
      },
    }),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': '/src',
    },
  },
})
```

### Language Toggle Component

```tsx
// Source: Context7 /i18next/react-i18next - useTranslation, changeLanguage
// File: frontend/src/shared/components/LanguageToggle.tsx

import { useTranslation } from 'react-i18next'
import { Button } from '@/shared/components/ui/button'

export function LanguageToggle() {
  const { i18n } = useTranslation()

  const toggleLanguage = async () => {
    const newLang = i18n.language === 'vi' ? 'en' : 'vi'
    await i18n.changeLanguage(newLang)
    // Also persist to backend user profile
    await updateUserLanguagePreference(newLang)
  }

  return (
    <Button variant="ghost" size="sm" onClick={toggleLanguage}>
      {i18n.language === 'vi' ? 'EN' : 'VI'}
    </Button>
  )
}
```

### Audit Log Data Table with TanStack Table

```tsx
// File: frontend/src/features/audit/components/AuditLogTable.tsx

import { useReactTable, getCoreRowModel, flexRender } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'

const columns = [
  { accessorKey: 'timestamp', header: () => t('audit.columns.timestamp') },
  { accessorKey: 'userName', header: () => t('audit.columns.user') },
  { accessorKey: 'actionType', header: () => t('audit.columns.action') },
  { accessorKey: 'entityName', header: () => t('audit.columns.entity') },
  { accessorKey: 'details', header: () => t('audit.columns.details') },
]

export function AuditLogTable({ data, pageCount, onPaginationChange }) {
  const { t } = useTranslation('audit')

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
    pageCount,
    onPaginationChange,
  })

  // Render with shadcn/ui Table component
}
```

### openapi-typescript Type Generation Script

```json
// File: frontend/package.json (partial)
{
  "scripts": {
    "generate:api": "openapi-typescript http://localhost:5000/swagger/v1/swagger.json -o src/generated/api-types.ts"
  }
}
```

```tsx
// File: frontend/src/shared/lib/api-client.ts
import createClient from 'openapi-fetch'
import type { paths } from '@/generated/api-types'

export const apiClient = createClient<paths>({
  baseUrl: import.meta.env.VITE_API_URL,
})
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Single query filter per entity | Named query filters (multiple per entity) | EF Core 10, Nov 2025 | BranchId + SoftDelete filters can be managed independently |
| MediatR + ASP.NET Minimal APIs | Wolverine (mediator + HTTP + messaging unified) | Wolverine 5.0, Oct 2025 | Single framework handles commands, queries, HTTP, and messaging |
| ASP.NET Core Identity (PBKDF2) | Argon2id (via Konscious.Security.Cryptography) | OWASP 2025 recommendation | Memory-hard hashing, resistant to GPU attacks |
| Single monolithic DbContext | Schema-per-module with independent migrations | EF Core 5+ (mature pattern) | Module independence, clean domain boundaries |
| MediatR pipeline behaviors | Wolverine middleware chain | Wolverine 3.0+ | Built-in transaction, validation, logging middleware |
| Custom OpenAPI client generation | openapi-typescript + openapi-fetch | 2024-2025 | 6kb runtime, type-safe from spec, eliminates manual type definitions |
| React Router v6 | TanStack Router with file-based routes | 2024-2025 | Full type-safety, search params API, `beforeLoad` auth guards |

**Deprecated/outdated:**
- MediatR: Not deprecated per se, but redundant when using Wolverine (which IS the mediator). Do NOT add MediatR alongside Wolverine.
- `@tanstack/start` package name: Renamed to `@tanstack/react-start`. Some tooling (e.g., shadcn/ui) may still reference old name.
- NetArchTest.Rules (1.3.2): Use NetArchTest.eNhancedEdition (1.4.5) instead -- better diagnostics and more rules.
- EF Core single `HasQueryFilter()`: Still works but named filters are preferred for multi-concern scenarios (tenant + soft-delete).

## Open Questions

1. **Wolverine Result<T> auto-mapping to ProblemDetails**
   - What we know: Wolverine.HTTP can return HTTP responses, and Ardalis.Result maps to status codes via AspNetCore package.
   - What's unclear: Whether Wolverine natively understands `Ardalis.Result<T>` or if a custom result transformer is needed to bridge Wolverine.HTTP and Ardalis.Result.
   - Recommendation: Implement a thin adapter/policy in Wolverine that inspects `Result<T>` return types and maps to appropriate ProblemDetails responses. Test this early in Wave 1.

2. **TanStack Start stable release timing**
   - What we know: TanStack Start is at RC v1.154.0 as of Jan 2026. The RC is the build they expect to ship as 1.0.
   - What's unclear: Exact 1.0 release date. Package rename from `@tanstack/start` to `@tanstack/react-start` may cause tooling friction.
   - Recommendation: Pin to the RC version. SPA mode is stable and well-documented. Monitor TanStack blog for 1.0 announcement. Handle package name issues at setup time.

3. **Password hashing library choice**
   - What we know: Argon2id is the OWASP 2025 recommendation. BCrypt.Net-Next is simpler. ASP.NET Core Identity uses PBKDF2 by default.
   - What's unclear: Best .NET library for Argon2id with production-grade reliability.
   - Recommendation: Use `Konscious.Security.Cryptography.Argon2` (well-maintained, .NET Standard 2.0+). If issues arise, fall back to `BCrypt.Net-Next` with work factor 12+.

4. **Audit log write path -- same transaction or separate?**
   - What we know: EF Core interceptor captures changes during SaveChanges. Writing audit logs in the same transaction ensures consistency but couples modules.
   - What's unclear: Whether audit writes should go through Wolverine message bus (eventual consistency) or direct DbContext write (strong consistency).
   - Recommendation: Use direct write to AuditDbContext in the SaveChanges interceptor for field-level audit trail (AUD-01). This ensures audit record exists if and only if the data change committed. For access logging (AUD-02), use fire-and-forget via Wolverine local queue.

## Sources

### Primary (HIGH confidence)
- Context7 /jasperfx/wolverine - Wolverine.HTTP endpoints, EF Core integration, multi-tenancy, FluentValidation middleware, domain event publishing
- Context7 /dotnet/entityframework.docs - Named query filters (EF Core 10), multi-DbContext migrations, SaveChanges interceptors for audit
- Context7 /i18next/react-i18next - i18next initialization, useTranslation hook, language switching, namespace configuration
- Context7 /tanstack/router - Authentication guard with beforeLoad, redirect pattern, file-based routing
- Context7 /websites/tanstack_start_framework_react - SPA mode configuration, Vite plugin setup
- Context7 /shadcn-ui/ui - Vite installation, form with React Hook Form + Zod, components.json configuration
- Context7 /fluentvalidation/fluentvalidation - DI registration, ASP.NET Core integration, AbstractValidator pattern

### Secondary (MEDIUM confidence)
- [Microsoft Learn: What's new in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview) - .NET 10 GA Nov 2025, LTS until 2028
- [Microsoft Learn: What's new in EF Core 10](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew) - Named query filters feature
- [NuGet: WolverineFx.Http 5.2.0](https://www.nuget.org/packages/WolverineFx.Http/5.2.0) - Wolverine 5.x version confirmed
- [NuGet: Testcontainers.MsSql 4.6.0](https://www.nuget.org/packages/Testcontainers.MsSql/4.6.0) - Latest Testcontainers version
- [NuGet: NetArchTest.eNhancedEdition 1.4.5](https://www.nuget.org/packages/NetArchTest.eNhancedEdition) - Enhanced edition with better diagnostics
- [NuGet: Ardalis.Result 10.1.0](https://www.nuget.org/packages/Ardalis.Result) - Result pattern with Railway-Oriented Programming
- [NuGet: Bogus 35.6.5](https://www.nuget.org/packages/bogus) - Latest Bogus version
- [TanStack Start v1 RC announcement](https://tanstack.com/blog/announcing-tanstack-start-v1) - RC status confirmed
- [shadcn/ui TanStack Start installation](https://ui.shadcn.com/docs/installation/tanstack) - Official integration guide
- [openapi-typescript npm](https://www.npmjs.com/package/openapi-typescript) - Type generation from OpenAPI
- [openapi-fetch npm 0.17.0](https://www.npmjs.com/package/openapi-fetch) - Type-safe fetch client
- [Microsoft Learn: JWT bearer authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication) - Official JWT config

### Tertiary (LOW confidence)
- [OWASP 2025 password hashing recommendations](https://guptadeepak.com/the-complete-guide-to-password-hashing-argon2-vs-bcrypt-vs-scrypt-vs-pbkdf2-2026/) - Argon2id as gold standard (verified by multiple sources)
- Wolverine + Ardalis.Result integration specifics -- no official documentation found on direct integration; needs validation during implementation

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries verified via Context7, NuGet, and official docs. Versions confirmed current.
- Architecture: HIGH - Modular monolith with schema-per-module is a well-documented pattern. EF Core 10 named query filters confirmed via Microsoft Learn.
- Pitfalls: HIGH - Based on documented issues in Context7 (Wolverine discovery, EF migration scoping) and verified security best practices (JWT rotation).
- Frontend stack: MEDIUM-HIGH - TanStack Start RC is production-usable but not yet 1.0. Package rename may cause minor friction. All other frontend libraries are stable.
- Wolverine-Ardalis.Result integration: LOW - No official documentation on this specific pairing. Needs validation.

**Research date:** 2026-02-28
**Valid until:** 2026-03-30 (stable stack, 30-day validity)
