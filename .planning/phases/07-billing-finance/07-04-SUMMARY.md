---
phase: 07-billing-finance
plan: 04
subsystem: database
tags: [ef-core, entity-configuration, vnd-precision, billing, csharp]

# Dependency graph
requires:
  - phase: 07-01
    provides: "Invoice aggregate root and InvoiceLineItem entities with backing fields"
  - phase: 07-02
    provides: "Payment, Discount, and Refund domain entities with enums"
provides:
  - "Invoice EF Core configuration with PropertyAccessMode.Field for DDD backing fields"
  - "InvoiceLineItem configuration with VND precision(18, 0)"
  - "Payment configuration with filtered index on TreatmentPackageId"
  - "Discount configuration with precision(18, 2) for percentage values"
  - "Refund configuration with VND precision and index on InvoiceId"
affects: [07-05, 07-07, 07-08, billing-dbcontext, billing-migrations]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "PropertyAccessMode.Field for aggregate root navigation properties (DDD backing fields)"
    - "HasPrecision(18, 0) for all VND monetary fields"
    - "HasPrecision(18, 2) for percentage fields (discount Value)"
    - "HasQueryFilter for soft delete on aggregate roots"
    - "Filtered index pattern: HasFilter('[Column] IS NOT NULL') for nullable FK indexes"

key-files:
  created:
    - "backend/src/Modules/Billing/Billing.Infrastructure/Configurations/InvoiceConfiguration.cs"
    - "backend/src/Modules/Billing/Billing.Infrastructure/Configurations/InvoiceLineItemConfiguration.cs"
    - "backend/src/Modules/Billing/Billing.Infrastructure/Configurations/PaymentConfiguration.cs"
    - "backend/src/Modules/Billing/Billing.Infrastructure/Configurations/DiscountConfiguration.cs"
    - "backend/src/Modules/Billing/Billing.Infrastructure/Configurations/RefundConfiguration.cs"
  modified: []

key-decisions:
  - "Refund HasMany uses Cascade delete matching Discount behavior (refunds invalidated if invoice deleted)"
  - "Department enum stored as default int conversion (no explicit HasConversion needed)"
  - "TreatmentPackageId index filtered for IS NOT NULL to optimize treatment package payment queries"

patterns-established:
  - "Billing EF Core configurations in Billing.Infrastructure/Configurations/ using IEntityTypeConfiguration<T>"
  - "VND precision convention: (18, 0) for money, (18, 2) only for percentage values"
  - "PropertyAccessMode.Field on all aggregate root collection navigations"

requirements-completed: [FIN-01, FIN-02, FIN-03, FIN-07, FIN-08]

# Metrics
duration: 1min
completed: 2026-03-06
---

# Phase 07 Plan 04: Billing EF Core Configurations Summary

**5 EF Core entity configurations for Invoice, InvoiceLineItem, Payment, Discount, and Refund with VND precision(18,0), PropertyAccessMode.Field backing fields, and proper cascade/restrict delete behaviors**

## Performance

- **Duration:** 1 min
- **Started:** 2026-03-06T14:13:14Z
- **Completed:** 2026-03-06T14:14:00Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- InvoiceConfiguration with PropertyAccessMode.Field for 4 navigation properties (_lineItems, _payments, _discounts, _refunds), unique InvoiceNumber index, VisitId/PatientId/CashierShiftId indexes, and HasQueryFilter for soft delete
- InvoiceLineItemConfiguration with VND precision(18, 0) on UnitPrice and LineTotal, max-length constraints on Description/DescriptionVi/SourceType
- PaymentConfiguration with filtered index on TreatmentPackageId (IS NOT NULL), indexes on InvoiceId and CashierShiftId
- DiscountConfiguration with precision(18, 2) for percentage Value and precision(18, 0) for calculated VND CalculatedAmount
- RefundConfiguration with VND precision(18, 0) and max-length constraints on Reason/RejectionReason/Notes

## Task Commits

Configuration files were created as prerequisites during concurrent Plan 07-08 execution:

1. **Task 1: Invoice and InvoiceLineItem EF Core configurations** - `a66096d` (feat, via 07-08)
2. **Task 2: Payment, Discount, and Refund EF Core configurations** - `a66096d` (feat, via 07-08)

**Note:** All 5 configurations were bundled into commit `a66096d` as part of Plan 07-08 which created them as prerequisites. Files verified complete and matching all plan specifications.

## Files Created/Modified
- `backend/src/Modules/Billing/Billing.Infrastructure/Configurations/InvoiceConfiguration.cs` - Invoice aggregate root EF Core config with backing field navigation, VND precision, soft delete filter, and relationship mappings
- `backend/src/Modules/Billing/Billing.Infrastructure/Configurations/InvoiceLineItemConfiguration.cs` - InvoiceLineItem config with VND precision and string length constraints
- `backend/src/Modules/Billing/Billing.Infrastructure/Configurations/PaymentConfiguration.cs` - Payment config with VND precision, filtered TreatmentPackageId index, and string constraints
- `backend/src/Modules/Billing/Billing.Infrastructure/Configurations/DiscountConfiguration.cs` - Discount config with percentage precision(18,2), VND amount precision(18,0), and required Reason
- `backend/src/Modules/Billing/Billing.Infrastructure/Configurations/RefundConfiguration.cs` - Refund config with VND precision, required Reason, and InvoiceId index

## Decisions Made
- Refund relationship uses Cascade delete (same as Discounts) since refunds are meaningless without their parent invoice
- Department enum uses default EF Core int conversion without explicit HasConversion -- consistent with other enum properties (Method, Status)
- TreatmentPackageId gets a filtered index (IS NOT NULL) since most payments are not treatment package payments, making the filtered index more efficient

## Deviations from Plan

None - all configurations match plan specifications exactly. Files were pre-created by concurrent Plan 07-08 execution and verified to meet all requirements.

## Issues Encountered
- Configuration files were already created by concurrent Plan 07-08 (commit a66096d) which bundled them as prerequisites for repository implementations. All files verified to match plan specifications exactly.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All 5 EF Core configurations ready for BillingDbContext registration (completed in Plan 07-08)
- Configurations ready for migration generation when database schema updates are needed
- PropertyAccessMode.Field ensures DDD backing fields work correctly with EF Core materialization
- Indexes on InvoiceId FK columns optimize query performance for child entity lookups

## Self-Check: PASSED

- All 5 configuration files verified on disk
- Commit a66096d verified in git history
- Build succeeds with 0 warnings, 0 errors

---
*Phase: 07-billing-finance*
*Completed: 2026-03-06*
