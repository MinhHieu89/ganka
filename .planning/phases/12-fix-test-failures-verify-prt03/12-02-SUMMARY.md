---
phase: 12-fix-test-failures-verify-prt03
plan: 02
subsystem: testing
tags: [wolverine, integration-tests, webapplicationfactory, auth, xunit]

requires:
  - phase: 08-auth-identity
    provides: Auth module with JWT cookie-based authentication endpoints
provides:
  - All 7 AuthCookieEndpointTests pass (login, refresh, logout with cookie-based tokens)
  - Correct test host setup pattern for Wolverine-based modular monolith
affects: [auth, testing]

tech-stack:
  added: []
  patterns:
    - "Remove DomainEventDispatcherInterceptor in test factories to avoid Wolverine startup race"
    - "Remove hosted service seeders from test host when they trigger domain events via SaveChangesAsync"
    - "Remove IDbContextOptionsConfiguration to fully clear interceptor config from test DbContexts"

key-files:
  created: []
  modified:
    - backend/tests/Auth.Integration.Tests/CustomWebApplicationFactory.cs

key-decisions:
  - "Remove AuthDataSeeder from test host instead of trying to start Wolverine during test initialization"
  - "Remove DomainEventDispatcherInterceptor from test services since domain event dispatch is unnecessary for endpoint tests"

patterns-established:
  - "Test factory pattern: remove DomainEventDispatcherInterceptor + AuthDataSeeder + IDbContextOptionsConfiguration when Wolverine is not needed"

requirements-completed: [AUTH-01]

duration: 15min
completed: 2026-03-24
---

# Phase 12 Plan 02: Fix Auth Integration Tests Summary

**Fixed WolverineHasNotStartedException by removing DomainEventDispatcherInterceptor and AuthDataSeeder from test host, enabling all 7 AuthCookieEndpointTests to pass**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-24T10:03:44Z
- **Completed:** 2026-03-24T10:18:46Z
- **Tasks:** 1
- **Files modified:** 1

## Accomplishments
- All 7 AuthCookieEndpointTests pass (Login_Success, WithRememberMe, WithoutRememberMe, Refresh_WithCookie, Refresh_WithoutCookie, Logout_Clears, Refresh_PreservesRememberMe)
- Root-caused the WolverineHasNotStartedException: AuthDataSeeder hosted service calls SaveChangesAsync during host startup, triggering DomainEventDispatcherInterceptor which requires Wolverine to be fully initialized, but Wolverine's hosted service hasn't completed startup yet
- Established a reusable test factory pattern for Wolverine-based projects

## Task Commits

Each task was committed atomically:

1. **Task 1: Fix CustomWebApplicationFactory to start the host for Wolverine initialization** - `77090cf` (fix)

## Files Created/Modified
- `backend/tests/Auth.Integration.Tests/CustomWebApplicationFactory.cs` - Removed AuthDataSeeder hosted service, DomainEventDispatcherInterceptor, and IDbContextOptionsConfiguration from test host to prevent Wolverine startup race condition

## Decisions Made
- **Remove AuthDataSeeder instead of fixing startup order:** The seeder runs during StartAsync alongside Wolverine. Controlling hosted service order is fragile. Removing the seeder and seeding manually in InitializeAsync is deterministic and clear.
- **Remove DomainEventDispatcherInterceptor entirely:** Auth integration tests verify HTTP endpoint behavior, not domain event dispatch. Removing the interceptor prevents any Wolverine dependency during test data operations.
- **Also remove IDbContextOptionsConfiguration:** The main app's ConfigureDbContext uses `(sp, options)` overload which registers IDbContextOptionsConfiguration with interceptor config. Just removing DbContextOptions wasn't sufficient to prevent the interceptor from leaking through.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Server.Host not available with minimal hosting model**
- **Found during:** Task 1
- **Issue:** Plan specified `await server.Host.StartAsync()` but `TestServer.Host` throws `InvalidOperationException` with the minimal hosting model (WebApplicationBuilder) because the server wasn't created with IWebHostBuilder
- **Fix:** Used `CreateClient()` instead to trigger full host startup, removed AuthDataSeeder and DomainEventDispatcherInterceptor to prevent Wolverine race condition during startup
- **Files modified:** backend/tests/Auth.Integration.Tests/CustomWebApplicationFactory.cs
- **Verification:** All 7 tests pass
- **Committed in:** 77090cf

**2. [Rule 1 - Bug] IDbContextOptionsConfiguration leaking interceptor config**
- **Found during:** Task 1
- **Issue:** RemoveDbContextServices only removed DbContextOptions and the DbContext service type, but IDbContextOptionsConfiguration<TContext> (registered by the `(sp, options)` AddDbContext overload) still carried interceptor configuration
- **Fix:** Added IDbContextOptionsConfiguration<TContext> to the removal filter
- **Files modified:** backend/tests/Auth.Integration.Tests/CustomWebApplicationFactory.cs
- **Verification:** All 7 tests pass
- **Committed in:** 77090cf

---

**Total deviations:** 2 auto-fixed (2 bugs)
**Impact on plan:** Both fixes necessary for correctness. The plan's approach (Server.Host.StartAsync) is not compatible with the minimal hosting model used by this project. The alternative approach (remove interceptor + seeder) is more robust.

## Issues Encountered
- Initial attempt with `Server.Host.StartAsync()` failed because TestServer.Host is unavailable in the minimal hosting model
- Required deeper investigation into why DomainEventDispatcherInterceptor was still active after DbContext re-registration (IDbContextOptionsConfiguration leak)

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Auth integration tests are fully passing, satisfying roadmap success criterion 3
- Test factory pattern established for future integration test suites

## Self-Check: PASSED

- FOUND: backend/tests/Auth.Integration.Tests/CustomWebApplicationFactory.cs
- FOUND: commit 77090cf

---
*Phase: 12-fix-test-failures-verify-prt03*
*Completed: 2026-03-24*
