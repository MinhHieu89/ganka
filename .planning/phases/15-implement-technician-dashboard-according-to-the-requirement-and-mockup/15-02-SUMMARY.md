---
phase: 15-implement-technician-dashboard
plan: 02
subsystem: clinical
tags: [tdd, wolverine, cqrs, api, technician-dashboard, query-handler, command-handler]
dependency_graph:
  requires:
    - phase: 15-01
      provides: TechnicianOrder entity, WorkflowStage.PreExam, auto-creation hook
  provides:
    - GetTechnicianDashboard query handler with status derivation per D-08
    - GetTechnicianKpiStats handler with today-filtered counts per D-09
    - AcceptTechnicianOrder command with concurrency rejection per D-15
    - CompleteTechnicianOrder command with DoctorExam stage advancement
    - ReturnToQueue command (clears assignment, no stage change)
    - RedFlagTechnicianOrder command with DoctorExam stage advancement
    - 6 API endpoints under /api/clinical/technician
    - ITechnicianOrderQueryService CQRS read-side interface
  affects: [frontend-technician-dashboard, clinical-api]
tech_stack:
  added: []
  patterns: [cqrs-query-service, in-memory-db-integration-tests]
key_files:
  created:
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/TechnicianDashboardDto.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/TechnicianKpiDto.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetTechnicianDashboard.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetTechnicianKpiStats.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/AcceptTechnicianOrder.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/CompleteTechnicianOrder.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/ReturnToQueue.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/RedFlagTechnicianOrder.cs
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/ITechnicianOrderQueryService.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Services/TechnicianOrderQueryService.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetTechnicianDashboardTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetTechnicianKpiStatsTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/AcceptTechnicianOrderTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/CompleteTechnicianOrderTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/ReturnToQueueTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/RedFlagTechnicianOrderTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/IoC.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
key_decisions:
  - "Used ITechnicianOrderQueryService read-side abstraction instead of direct ClinicalDbContext in Application layer to avoid circular project dependency (Application<->Infrastructure)"
  - "Dashboard and KPI tests use InMemory DB with real TechnicianOrderQueryService (integration-style) rather than mocked repository for more realistic query testing"
  - "Red flag validation done inline in handler rather than FluentValidation to keep it simple for a single field check"
patterns_established:
  - "CQRS query service pattern: ITechnicianOrderQueryService in Application, implementation in Infrastructure, for complex read-side queries that bypass repository abstraction"
  - "InMemory DB tests for query handlers: when testing queries with joins, use real DbContext + InMemory provider instead of mocked repositories"
requirements_completed: [TECH-04, TECH-05, TECH-06, TECH-07]
duration: 11min
completed: "2026-03-29T05:10:00Z"
---

# Phase 15 Plan 02: Backend Handlers for Technician Dashboard (TDD) Summary

**6 Wolverine handlers (2 queries + 4 commands) with 25 unit tests, all wired to /api/clinical/technician endpoints with CQRS query service pattern to avoid circular dependency.**

## Commits

| # | Hash | Message | Files |
|---|------|---------|-------|
| 1 | 03af621 | test(15-02): add failing tests for dashboard and KPI handlers (TDD RED) | 4 |
| 2 | d940c5c | feat(15-02): implement dashboard and KPI handlers (TDD GREEN) | 7 |
| 3 | 7953afc | test(15-02): add failing tests for action commands (TDD RED) | 4 |
| 4 | 82977be | feat(15-02): implement action commands and API endpoints (TDD GREEN) | 7 |

## Task Results

### Task 1: DTOs, Dashboard Query Handler, KPI Handler (TDD)

**RED:** 14 failing tests written covering status derivation (waiting/in_progress/red_flag/completed), status filtering, patient name search, row field mapping, visit type detection (new/follow_up), and sort order (in_progress pinned first).

**GREEN:**
- `TechnicianDashboardDto.cs`: Query record + response + row DTO with all required fields
- `TechnicianKpiDto.cs`: Query record + KPI response (Waiting, InProgress, Completed, RedFlag)
- `GetTechnicianDashboard.cs`: Wolverine handler with status derivation per D-08, visit type per D-10, search, filter, pagination, sort
- `GetTechnicianKpiStats.cs`: Wolverine handler counting today's orders per D-09 (InProgress = current technician only)
- `ITechnicianOrderQueryService.cs`: CQRS read-side interface for complex joins
- `TechnicianOrderQueryService.cs`: EF Core implementation with Vietnam timezone date filtering
- 14 tests passing

### Task 2: Action Command Handlers + API Endpoints (TDD)

**RED:** 11 failing tests written covering accept (success, already-accepted D-15, not-found), complete (success + DoctorExam advance, not-accepted, not-found), return-to-queue (success, not-found), red-flag (success + DoctorExam advance, empty reason, not-found).

**GREEN:**
- `AcceptTechnicianOrder.cs`: Assigns technician, returns Conflict error with existing name (D-15)
- `CompleteTechnicianOrder.cs`: Marks complete, advances visit to DoctorExam
- `ReturnToQueue.cs`: Clears assignment, keeps visit at PreExam
- `RedFlagTechnicianOrder.cs`: Sets red flag with reason, advances to DoctorExam
- `IVisitRepository.GetByTechnicianOrderIdAsync()`: New method loading visit with TechnicianOrders
- `ClinicalApiEndpoints.MapTechnicianEndpoints()`: 6 endpoints under `/api/clinical/technician`
- 11 tests passing

## API Endpoints

| Method | Route | Handler |
|--------|-------|---------|
| GET | /api/clinical/technician/dashboard | GetTechnicianDashboardHandler |
| GET | /api/clinical/technician/kpi | GetTechnicianKpiStatsHandler |
| POST | /api/clinical/technician/orders/{orderId}/accept | AcceptTechnicianOrderHandler |
| POST | /api/clinical/technician/orders/{orderId}/complete | CompleteTechnicianOrderHandler |
| POST | /api/clinical/technician/orders/{orderId}/return-to-queue | ReturnToQueueHandler |
| POST | /api/clinical/technician/orders/{orderId}/red-flag | RedFlagTechnicianOrderHandler |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Avoided Application->Infrastructure circular dependency**
- **Found during:** Task 1
- **Issue:** Plan specified using ClinicalDbContext directly in Application handlers, but Application cannot reference Infrastructure (Infrastructure already references Application)
- **Fix:** Created ITechnicianOrderQueryService interface in Application, implementation in Infrastructure. CQRS read-side pattern that keeps proper layer separation.
- **Files created:** ITechnicianOrderQueryService.cs, TechnicianOrderQueryService.cs
- **Commit:** d940c5c

**2. [Rule 1 - Bug] Fixed missing CreatePreExamOrder() call in test setup**
- **Found during:** Task 1
- **Issue:** AdvanceStage(PreExam) does not auto-create TechnicianOrder (that happens in AdvanceWorkflowStageHandler). Tests needed explicit CreatePreExamOrder() call.
- **Fix:** Added visit.CreatePreExamOrder() after AdvanceStage in test helpers.
- **Commit:** d940c5c

## Known Stubs

None -- all endpoints return real data, no placeholders.

## Verification

- `dotnet build backend/src/Bootstrapper`: 0 errors, 2 pre-existing warnings
- `dotnet test Clinical.Unit.Tests --filter "Technician"`: 34 passed, 0 failed
- `dotnet test Clinical.Unit.Tests --no-restore`: 324 passed, 0 failed (full suite)

## Self-Check: PASSED
