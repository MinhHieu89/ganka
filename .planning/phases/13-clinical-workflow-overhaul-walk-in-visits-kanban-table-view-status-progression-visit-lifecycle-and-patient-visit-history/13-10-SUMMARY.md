---
phase: 13-clinical-workflow-overhaul
plan: 10
subsystem: clinical-infrastructure-application
tags: [ef-core, cqrs, handlers, api-endpoints, tdd, workflow-actions]
dependency_graph:
  requires: [workflow-stage-enum, visit-parallel-tracks, imaging-request-entity, stage-skip-entity]
  provides: [workflow-api-endpoints, skip-refraction-handler, confirm-payment-handler, dispense-pharmacy-handler, optical-order-handler, handoff-handler]
  affects: [clinical-kanban-frontend, visit-lifecycle, pharmacy-module, optical-module]
tech_stack:
  added: []
  patterns: [domain-method-child-entities, post-payment-track-routing, prescription-branching-auto-advance]
key_files:
  created:
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/ImagingRequestConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/ImagingServiceConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/StageSkipConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitPaymentConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/PharmacyDispensingConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DispensingLineItemConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/OpticalOrderConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/HandoffChecklistConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/SkipRefraction.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/UndoRefractionSkip.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/RequestImaging.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/CompleteImagingServices.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/ConfirmVisitPayment.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/DispensePharmacy.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/ConfirmOpticalOrder.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/CompleteOpticalLab.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/CompleteHandoff.cs
    - backend/tests/Clinical.Unit.Tests/Features/SkipRefractionHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/RequestImagingHandlerTests.cs
    - backend/tests/Clinical.Unit.Tests/Features/ConfirmVisitPaymentHandlerTests.cs
  modified:
    - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs
    - backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs
    - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
    - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/CreateVisitCommand.cs
    - backend/src/Modules/Clinical/Clinical.Application/Features/SignOffVisit.cs
    - backend/src/Modules/Clinical/Clinical.Presentation/ClinicalApiEndpoints.cs
decisions:
  - "Added domain methods (AddVisitPayment, AddPharmacyDispensing, etc.) to Visit aggregate instead of using reflection to access backing fields"
  - "SignOffVisit auto-advance at Prescription stage now branches based on OpticalPrescriptions.Any() -- OpticalCenter if glasses, Cashier if no glasses"
  - "ConfirmVisitPayment uses GetByIdWithDetailsAsync to check DrugPrescriptions.Any() and OpticalPrescriptions.Any() for track activation"
metrics:
  duration: 11min
  completed: "2026-03-25T12:34:40Z"
  tasks_completed: 2
  tasks_total: 2
  files_created: 20
  files_modified: 6
  tests_added: 16
  tests_total: 278
---

# Phase 13 Plan 10: Infrastructure + Application Layer for Workflow Spec Summary

Complete backend EF Core configurations, migration, CQRS handlers, and API endpoints for all workflow stage actions including parallel post-payment tracks.

## Commits

| # | Hash | Message |
|---|------|---------|
| 1 | f386d77 | feat(13-10): EF Core configurations for workflow entities + migration |
| 2 | 9bdf666 | feat(13-10): workflow action handlers + API endpoints with TDD tests |

## Task Details

### Task 1: EF Core configurations + migration + repository updates

- Created 8 IEntityTypeConfiguration classes: ImagingRequest, ImagingService, StageSkip, VisitPayment, PharmacyDispensing, DispensingLineItem, OpticalOrder, HandoffChecklist
- Updated VisitConfiguration with 6 new navigation collections (ImagingRequests, StageSkips, VisitPayments, PharmacyDispensings, OpticalOrders, HandoffChecklists) with PropertyAccessMode.Field
- Added DrugTrackStatus and GlassesTrackStatus int conversion properties
- Added 8 DbSets to ClinicalDbContext for all new entities
- Migration AddWorkflowV2EntityConfigurations applies MaxLength constraints on string columns (was nvarchar(max), now nvarchar(200) etc.)

### Task 2: Application handlers + API endpoints (TDD)

- Created 9 CQRS handlers following existing Wolverine pattern:
  - SkipRefractionHandler: skips RefractionVA, creates StageSkip, advances to DoctorExam
  - UndoRefractionSkipHandler: reverses refraction skip
  - RequestImagingHandler: creates ImagingRequest with services, advances to Imaging
  - CompleteImagingServicesHandler: advances from Imaging to DoctorReviewsResults
  - ConfirmVisitPaymentHandler: creates VisitPayment, activates post-payment tracks, auto-Done if no tracks
  - DispensePharmacyHandler: creates PharmacyDispensing, completes drug track, auto-Done if all tracks done
  - ConfirmOpticalOrderHandler: creates OpticalOrder, advances OpticalCenter to Cashier
  - CompleteOpticalLabHandler: advances OpticalLab to ReturnGlasses
  - CompleteHandoffHandler: creates HandoffChecklist, completes glasses track, auto-Done if all tracks done
- Added 10 command DTOs to Clinical.Contracts
- Registered 9 new API endpoints in ClinicalApiEndpoints under workflow action group
- Updated SignOffVisit auto-advance: Prescription now branches to OpticalCenter (if glasses) or Cashier (if no glasses)
- Added Visit domain methods for child entity additions (AddVisitPayment, AddPharmacyDispensing, AddOpticalOrder, AddHandoffChecklist, SetHasGlassesPrescription)
- 16 new unit tests covering: skip refraction (4 tests), request imaging (4 tests), confirm payment (5 tests), and edge cases

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing critical functionality] Added Visit domain methods for child entity additions**
- **Found during:** Task 2
- **Issue:** Visit aggregate had no public methods to add workflow child entities (VisitPayment, PharmacyDispensing, etc.). Initial ConfirmVisitPayment handler used reflection to access backing fields.
- **Fix:** Added AddVisitPayment, AddPharmacyDispensing, AddOpticalOrder, AddHandoffChecklist, and SetHasGlassesPrescription domain methods to Visit aggregate. These follow the same pattern as existing AddDrugPrescription.
- **Files modified:** backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
- **Commit:** 9bdf666

**2. [Rule 2 - Missing critical functionality] Added DispensingLineItemConfiguration**
- **Found during:** Task 1
- **Issue:** Plan listed PharmacyDispensingConfiguration but DispensingLineItem (child of PharmacyDispensing) also needed its own EF configuration for MaxLength constraints.
- **Fix:** Created DispensingLineItemConfiguration with proper MaxLength for DrugName(200) and Instruction(500).
- **Files modified:** backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DispensingLineItemConfiguration.cs
- **Commit:** f386d77

## Known Stubs

None -- all handlers are fully implemented with domain method calls and proper track completion logic.

## Self-Check: PASSED
