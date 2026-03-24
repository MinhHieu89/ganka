# Phase 12: Fix Test Failures & Verify PRT-03 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-24
**Phase:** 12-fix-test-failures-verify-prt03
**Areas discussed:** Auth test fix strategy, Scheduling UTC fix, PRT-03 verification scope, Test coverage policy

---

## Auth Test Fix Strategy

| Option | Description | Selected |
|--------|-------------|----------|
| Fix test host setup | Update WebApplicationFactory to call StartAsync() so Wolverine initializes correctly | ✓ |
| Mock Wolverine in tests | Register no-op Wolverine stub — faster but less integration coverage | |
| You decide | Claude picks best approach | |

**User's choice:** Fix test host setup
**Notes:** All 7 tests fail with identical root cause (WolverineHasNotStartedException). Tests are valid, only host setup is wrong.

---

## Scheduling UTC Fix

| Option | Description | Selected |
|--------|-------------|----------|
| Fix projection code | Add DateTime.SpecifyKind(Utc) in query handler/projection | |
| Fix at EF Core level | Configure EF Core value converter for UTC DateTimes | ✓ |

**User's choice:** Fix at EF Core level

**Follow-up — UTC converter scope:**

| Option | Description | Selected |
|--------|-------------|----------|
| Global (all modules) | Configure in shared DbContext base/convention | ✓ |
| Scheduling only | Add only in SchedulingDbContext | |

**User's choice:** Global (all modules)
**Notes:** Prevents same issue recurring in other modules.

---

## PRT-03 Verification Scope

| Option | Description | Selected |
|--------|-------------|----------|
| Automated endpoint test | Integration tests verifying HTTP 200, PDF content-type, non-empty body | ✓ |
| Manual visual check | Run app, trigger print from UI, visually confirm | |
| Both automated + manual | Write tests AND do manual visual check | |

**User's choice:** Automated endpoint test

**Follow-up — PDF assertion depth:**

| Option | Description | Selected |
|--------|-------------|----------|
| Response only | Verify HTTP 200, content-type, non-empty body | ✓ |
| Content assertions too | Parse PDF and assert key fields present | |

**User's choice:** Response only
**Notes:** PDF rendering libraries can change formatting without breaking correctness — response-level validation is sufficient.

---

## Test Coverage Policy

| Option | Description | Selected |
|--------|-------------|----------|
| Fix existing + add PRT-03 tests | Fix broken suites AND write new PRT-03 integration tests | ✓ |
| Fix existing only | Only fix broken tests, verify PRT-03 separately | |
| Fix + expand coverage broadly | Fix, add PRT-03, and audit for other gaps | |

**User's choice:** Fix existing + add PRT-03 tests

**Follow-up — Optical fix approach:**

| Option | Description | Selected |
|--------|-------------|----------|
| Fix test code only | Update 5 call sites to pass CancellationToken.None | ✓ |
| Audit handler change first | Review why signature changed before fixing | |

**User's choice:** Fix test code only
**Notes:** Handler adding CancellationToken is standard Wolverine pattern — straightforward fix.

---

## Claude's Discretion

- WebApplicationFactory configuration details for Wolverine
- UTC converter placement (shared base DbContext vs convention)
- PRT-03 test data seeding strategy
- Shared test base class decisions

## Deferred Ideas

None — discussion stayed within phase scope
