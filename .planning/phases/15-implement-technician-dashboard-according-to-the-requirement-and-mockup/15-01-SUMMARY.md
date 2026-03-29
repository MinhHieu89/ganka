---
phase: 15-implement-technician-dashboard
plan: 01
subsystem: clinical
tags: [domain-model, enum-rename, entity, ef-core, migration, tdd]
dependency_graph:
  requires: []
  provides: [TechnicianOrder-entity, WorkflowStage-PreExam, TechnicianOrders-table]
  affects: [clinical-domain, clinical-infrastructure, scheduling-application]
tech_stack:
  added: []
  patterns: [factory-method, idempotency-guard, unique-filtered-index, auto-creation-hook]
key_files:
  created:
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/TechnicianOrder.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/TechnicianOrderType.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/TechnicianOrderConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Migrations/20260329045304_AddTechnicianOrder.cs
    - backend/tests/Clinical.Unit.Tests/Domain/TechnicianOrderTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/WorkflowStage.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/AdvanceWorkflowStage.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs
    - frontend/src/features/clinical/api/clinical-api.ts
decisions:
  - "Used FindAsync (no Include) in AdvanceWorkflowStage; idempotency enforced by DB unique filtered index rather than in-memory check"
metrics:
  duration: 11min
  completed: "2026-03-29T04:54:00Z"
---

# Phase 15 Plan 01: Domain Model Foundation (PreExam Rename + TechnicianOrder Entity) Summary

**One-liner:** Renamed WorkflowStage.RefractionVA to PreExam across 22 files and created TechnicianOrder entity with TDD (9 domain tests), EF Core string enum config with unique filtered index, migration, and auto-creation hook in AdvanceWorkflowStage.

## Commits

| # | Hash | Message | Files |
|---|------|---------|-------|
| 1 | 5d1e6a1 | refactor(15-01): rename WorkflowStage.RefractionVA to PreExam | 22 |
| 2 | 116a9c4 | test(15-01): add failing tests for TechnicianOrder (TDD RED) | 2 |
| 3 | 0cd1c17 | feat(15-01): add TechnicianOrder entity, EF config, migration, auto-creation (TDD GREEN) | 10 |

## Task Results

### Task 1: Rename WorkflowStage.RefractionVA to PreExam

- Renamed enum value `RefractionVA = 1` to `PreExam = 1` (integer unchanged, no data migration)
- Updated 22 files: 9 source, 11 test, 1 frontend, 1 contracts
- All 289 existing Clinical unit tests pass
- Zero grep matches for `RefractionVA` in backend/src, backend/tests, frontend/src

### Task 2: Create TechnicianOrder Entity (TDD)

**RED:** 9 failing tests written covering CreatePreExam, Accept, Complete, ReturnToQueue, MarkRedFlag, Visit.CreatePreExamOrder, and idempotency guard.

**GREEN:**
- `TechnicianOrder.cs`: Entity with factory method (`CreatePreExam`), state transitions (`Accept`, `Complete`, `ReturnToQueue`, `MarkRedFlag`)
- `TechnicianOrderType.cs`: Enum with `PreExam` and `AdditionalExam`
- `TechnicianOrderConfiguration.cs`: String enum conversion, unique filtered index `[OrderType] = 'PreExam'`
- `Visit.cs`: Added `_technicianOrders` collection and `CreatePreExamOrder()` method
- `AdvanceWorkflowStage.cs`: Auto-creates TechnicianOrder when visit enters PreExam stage
- `ClinicalDbContext.cs`: Added `DbSet<TechnicianOrder>`
- `VisitConfiguration.cs`: Added HasMany/WithOne navigation
- Migration `20260329045304_AddTechnicianOrder` created and applied
- 1 additional handler test: AdvanceToPreExam auto-creates TechnicianOrder
- All 299 Clinical unit tests pass

## Deviations from Plan

None - plan executed exactly as written.

## Pre-existing Issues Noted

- `Scheduling.Unit.Tests/Features/GetReceptionistDashboardTests.cs` has pre-existing compilation errors (missing `patientRepository` and `CancellationToken` parameters) unrelated to this plan's changes. Logged but not fixed (out of scope).

## Known Stubs

None.

## Decisions Made

1. **Idempotency via DB index**: The `AdvanceWorkflowStageHandler` uses `GetByIdAsync` (FindAsync, no Include), so `_technicianOrders` collection is empty in memory. The idempotency guard in `CreatePreExamOrder` works for in-memory cases, while the unique filtered index `[OrderType] = 'PreExam'` enforces uniqueness at the database level for concurrent scenarios.

## Verification

- `dotnet build` on Clinical.Domain, Clinical.Application, Clinical.Contracts, Scheduling.Application: all succeed with 0 errors
- `dotnet test Clinical.Unit.Tests`: 299 passed, 0 failed
- `grep -r "RefractionVA" backend/src/ backend/tests/ frontend/src/`: 0 matches
- TechnicianOrders table exists in database with correct schema
- Migration file `20260329045304_AddTechnicianOrder.cs` exists

## Self-Check: PASSED

- [x] `backend/src/Modules/Clinical/Clinical.Domain/Entities/TechnicianOrder.cs` exists
- [x] `backend/src/Modules/Clinical/Clinical.Domain/Enums/TechnicianOrderType.cs` exists
- [x] `backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/TechnicianOrderConfiguration.cs` exists
- [x] `backend/tests/Clinical.Unit.Tests/Domain/TechnicianOrderTests.cs` exists
- [x] Migration file exists
- [x] Commit 5d1e6a1 exists
- [x] Commit 116a9c4 exists
- [x] Commit 0cd1c17 exists
