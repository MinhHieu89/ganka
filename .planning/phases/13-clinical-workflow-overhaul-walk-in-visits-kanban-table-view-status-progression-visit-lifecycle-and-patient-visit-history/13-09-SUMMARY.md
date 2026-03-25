---
phase: 13-clinical-workflow-overhaul
plan: 09
subsystem: clinical-domain
tags: [domain-model, workflow, tdd, enums, entities, parallel-tracks]
dependency_graph:
  requires: []
  provides: [workflow-stage-enum, visit-parallel-tracks, imaging-request-entity, stage-skip-entity]
  affects: [clinical-kanban, visit-lifecycle, pharmacy-dispensing, optical-orders]
tech_stack:
  added: []
  patterns: [branching-workflow-validation, parallel-track-completion, factory-method-entities]
key_files:
  created:
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/VisitTrack.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/TrackStatus.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/SkipReason.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/PaymentType.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/PaymentMethod.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/ImagingRequest.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/ImagingService.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/StageSkip.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/VisitPayment.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/PharmacyDispensing.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/OpticalOrder.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/HandoffChecklist.cs
    - backend/tests/Clinical.Unit.Tests/Domain/VisitWorkflowStageTests.cs
    - backend/tests/Clinical.Unit.Tests/Domain/VisitParallelTrackTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Domain/Enums/WorkflowStage.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/ActiveVisitDto.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/GetActiveVisits.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/SignOffVisit.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
    - backend/tests/Clinical.Unit.Tests/Domain/VisitReverseStageTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/AdvanceWorkflowStageHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/GetActiveVisitsHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/ReverseWorkflowStageHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/SignOffVisitAutoAdvanceTests.cs
decisions:
  - "WorkflowStage enum values are NOT sequential with flow order -- OpticalCenter(8) routes to Cashier(6) via AdvanceStage branching"
  - "No CashierGlasses stage -- single combined payment at Cashier per original spec"
  - "SignOffVisit auto-advance updated from simple +1 increment to branching-aware next-stage logic"
metrics:
  duration: 13min
  completed: "2026-03-25T10:15:32Z"
  tasks_completed: 2
  tasks_total: 2
  files_created: 14
  files_modified: 11
  tests_added: 55
  tests_total: 262
---

# Phase 13 Plan 09: Domain Model Overhaul Summary

Redesigned backend domain model from 8-stage linear workflow to 12-stage branching/parallel model with imaging loop, refraction skip, and dual post-payment tracks.

## Commits

| # | Hash | Message |
|---|------|---------|
| 1 | 6d17460 | feat(13-09): redesign WorkflowStage enum + new domain enums and child entities |
| 2 | ce2a065 | feat(13-09): update Visit aggregate for parallel tracks, imaging loop, and skip support |

## Task Details

### Task 1: Redesign WorkflowStage enum + new domain enums and child entities

- Replaced 8-stage linear WorkflowStage enum with 12-member branching model (Reception through Done=99)
- Renamed enum members: Diagnostics->Imaging, DoctorReads->DoctorReviewsResults, Rx->Prescription, PharmacyOptical->Pharmacy
- Added 5 new enums: VisitTrack, TrackStatus, SkipReason, PaymentType, PaymentMethod
- Created 7 child entities: ImagingRequest, ImagingService, StageSkip, VisitPayment, PharmacyDispensing (with DispensingLineItem), OpticalOrder, HandoffChecklist
- Added 3 new stage values: OpticalCenter(8), OpticalLab(9), ReturnGlasses(10)
- Updated AllowedReversals dictionary for renamed stages
- Updated all existing references across application, infrastructure, and test layers
- 31 new domain tests for enums and entities

### Task 2: Update Visit aggregate for parallel tracks, imaging loop, and skip support

- Added DrugTrackStatus and GlassesTrackStatus properties (default NotApplicable)
- Added ImagingRequested and RefractionSkipped branching flags
- Added 6 child entity collections (ImagingRequests, StageSkips, VisitPayments, PharmacyDispensings, OpticalOrders, HandoffChecklists)
- Added domain methods: RequestImaging, SkipRefraction, UndoRefractionSkip, ActivatePostPaymentTracks, CompleteDrugTrack, CompleteGlassesTrack
- Added IsComplete computed property for parallel track completion check
- Rewrote AdvanceStage with branching validation (DoctorExam->Imaging requires ImagingRequested, OpticalCenter->Cashier backward jump allowed)
- Updated ActiveVisitDto with 4 new fields: drugTrackStatus, glassesTrackStatus, imagingRequested, refractionSkipped
- 24 new parallel track tests

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed SignOffVisit auto-advance for branching workflow**
- **Found during:** Task 2
- **Issue:** SignOffVisit handler used simple `(int)CurrentStage + 1` for auto-advance, which crashes with branching validation (DoctorExam+1=Imaging fails when ImagingRequested=false)
- **Fix:** Replaced with branching-aware `GetNextStage()` method that routes DoctorExam to either Imaging or Prescription based on ImagingRequested flag
- **Files modified:** backend/src/Modules/Clinical/Clinical.Application/Features/SignOffVisit.cs
- **Commit:** ce2a065

**2. [Rule 1 - Bug] Updated all test helpers for branching-aware stage advancement**
- **Found during:** Task 2
- **Issue:** Test helpers used linear for-loop advancement through Imaging/DoctorReviewsResults, which fails with new branching validation
- **Fix:** Updated CreateVisitAtStage helpers in all test files to use no-imaging path (DoctorExam->Prescription) or imaging path (with RequestImaging) based on target stage
- **Files modified:** 5 test files
- **Commit:** ce2a065

## Known Stubs

None -- all domain methods are fully implemented with business logic.

## Self-Check: PASSED

All 14 created files verified. Both commit hashes (6d17460, ce2a065) confirmed in git log.
