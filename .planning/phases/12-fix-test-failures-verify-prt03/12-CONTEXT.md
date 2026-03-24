# Phase 12: Fix Test Failures & Verify PRT-03 - Context

**Gathered:** 2026-03-24
**Status:** Ready for planning

<domain>
## Phase Boundary

Fix all broken test suites across the codebase and formally verify PRT-03 (invoice/receipt printing). This is a gap closure phase — no new features, only restoring test health and closing verification gaps from the v1.0 audit.

Confirmed failures (scouted 2026-03-24):
1. **Optical.Unit.Tests** — Build fails: `WarrantyHandlerTests` missing `CancellationToken` parameter (5 call sites)
2. **Clinical.Unit.Tests** — Already passing (183/183) — no action needed
3. **Auth.Integration.Tests** — All 7 `AuthCookieEndpointTests` fail with `WolverineHasNotStartedException`
4. **Scheduling.Unit.Tests** — 2 tests fail: `DateTimeKind.Utc` not enforced in appointment projections

</domain>

<decisions>
## Implementation Decisions

### Auth Integration Tests Fix
- **D-01:** Fix the test host setup (WebApplicationFactory) to properly call `StartAsync()` so Wolverine initializes correctly
- **D-02:** The tests themselves are valid — only the host setup is broken. Do not mock Wolverine or rewrite the tests.

### Scheduling UTC Fix
- **D-03:** Fix at the EF Core level with a global UTC DateTime value converter, not per-query SpecifyKind calls
- **D-04:** Apply the UTC conversion globally across all modules (shared DbContext base/convention), not just Scheduling. This prevents the same issue in other modules.

### Optical Unit Tests Fix
- **D-05:** Fix test code only — update the 5 `WarrantyHandlerTests` call sites to pass `CancellationToken.None`. The handler signature change (adding CancellationToken) is standard Wolverine pattern and intentional.

### PRT-03 Verification
- **D-06:** Write automated integration tests for invoice (A4) and receipt (A5) print endpoints
- **D-07:** Tests verify HTTP 200, correct `application/pdf` content-type, and non-empty response body. No PDF content parsing — response-level validation only.
- **D-08:** No manual visual check required — automated tests are sufficient for verification

### Test Coverage Policy
- **D-09:** Fix all existing broken tests AND write new PRT-03 integration tests. Do not expand coverage beyond what's needed for this phase.
- **D-10:** Follow Phase 05.1 precedent: fix code to match tests, never relax tests

### Claude's Discretion
- Exact WebApplicationFactory configuration for Wolverine startup
- Where to place the global UTC DateTime converter (shared base DbContext vs convention)
- PRT-03 test data setup (seed invoice/receipt or create via handler)
- Whether to create a shared integration test base class or keep per-module test fixtures
- Any cascade fixes discovered during test repairs

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Test Failures
- `backend/tests/Optical.Unit.Tests/Features/WarrantyHandlerTests.cs` — 5 call sites needing CancellationToken
- `backend/tests/Auth.Integration.Tests/` — All 7 AuthCookieEndpointTests with Wolverine startup issue
- `backend/tests/Scheduling.Unit.Tests/` — 2 DateTimeKind.Utc assertion failures

### Prior Phase Context
- `.planning/phases/05.1-fix-architecture-test-failures-from-prior-phases/05.1-CONTEXT.md` — Precedent for "fix code, not relax tests" approach

### Printing Endpoints
- `backend/src/Modules/Billing/Billing.Presentation/BillingApiEndpoints.cs` — Invoice/receipt print endpoints
- `frontend/src/features/billing/components/InvoiceView.tsx` — Frontend print trigger
- `frontend/src/features/billing/api/shift-api.ts` — Related billing API calls

### Handler Under Test
- `backend/src/Modules/Clinical/Clinical.Application/Interfaces/IOsdiNotificationService.cs` — Interface used in Clinical tests (already passing)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `WebApplicationFactory` pattern already used in Auth.Integration.Tests — needs Wolverine startup fix, not rewrite
- Wolverine handler pattern with CancellationToken is standard across all modules
- EF Core DbContext per module — UTC converter can be applied at shared level

### Established Patterns
- Phase 05.1 established pattern of fixing code to comply with tests (enum duplication, namespace moves, interface extraction)
- Integration tests use xUnit with WebApplicationFactory
- Unit tests use xUnit with NSubstitute for mocking

### Integration Points
- Global UTC converter will affect all module DbContexts — need to verify no existing DateTime handling breaks
- PRT-03 integration tests will need a running test host with seeded billing data

</code_context>

<specifics>
## Specific Ideas

No specific requirements — straightforward fix-and-verify phase with clear success criteria from the roadmap.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 12-fix-test-failures-verify-prt03*
*Context gathered: 2026-03-24*
