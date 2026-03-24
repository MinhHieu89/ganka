---
phase: 11-granular-permission-enforcement
plan: 00
subsystem: backend-architecture-tests
tags: [tdd, red-phase, architecture-tests, permissions, authorization]
dependency_graph:
  requires: []
  provides: [permission-enforcement-architecture-test]
  affects: [all-api-endpoint-files]
tech_stack:
  added: []
  patterns: [file-based-source-scanning, architecture-test]
key_files:
  created:
    - backend/tests/Ganka28.ArchitectureTests/PermissionEnforcementTests.cs
  modified: []
decisions:
  - Used file-based source scanning (Directory.GetFiles + File.ReadAllText) instead of assembly reflection for permission checks, since RequirePermissions is a runtime extension method not detectable via NetArchTest
metrics:
  duration: 5min
  completed: "2026-03-24T09:08:00Z"
---

# Phase 11 Plan 00: Permission Enforcement Architecture Tests (RED) Summary

Architecture tests that validate all API endpoint files have RequirePermissions calls using Permissions.* constants, currently in RED state as expected for TDD.

## What Was Done

### Task 1: Create failing architecture tests for permission enforcement (RED phase)
**Commit:** d2973a7

Created `PermissionEnforcementTests.cs` with two test methods:

1. **AllNonPublicEndpointFiles_MustContain_RequirePermissionsCalls** (FAILS - RED)
   - Scans all `*Endpoints.cs` files under `backend/src/`
   - Excludes `PublicBookingEndpoints.cs` and `PublicOsdiEndpoints.cs`
   - Asserts each file contains `RequirePermissions(Permissions.`
   - Currently fails listing 10 endpoint files missing permission enforcement:
     - SettingsApiEndpoints.cs, AuditApiEndpoints.cs, AuthApiEndpoints.cs
     - ClinicalApiEndpoints.cs, OpticalApiEndpoints.cs, PatientApiEndpoints.cs
     - ConsumablesApiEndpoints.cs, DispensingApiEndpoints.cs, PharmacyApiEndpoints.cs
     - SchedulingApiEndpoints.cs

2. **NoEndpointFile_ShouldUse_StringLiteralPermissions** (PASSES)
   - Verifies no endpoint file contains `RequirePermissions("` (string literal)
   - Passes because no files use string literals

Files already passing Test 1 (have RequirePermissions calls):
- TreatmentApiEndpoints.cs (reference implementation)
- BillingApiEndpoints.cs (already guarded)
- StocktakingApiEndpoints.cs (already guarded)
- WarrantyApiEndpoints.cs (already guarded)

## Deviations from Plan

None - plan executed exactly as written.

## Decisions Made

1. **File-based source scanning over assembly reflection** - RequirePermissions is a runtime extension method on IEndpointConventionBuilder, not detectable via NetArchTest assembly scanning. Reading source files directly is the reliable approach.

## Known Stubs

None.

## Verification Results

- `dotnet build backend/tests/Ganka28.ArchitectureTests/` compiles successfully
- Test 1 FAILS (RED) as expected - 10 endpoint files lack RequirePermissions calls
- Test 2 PASSES - no files use string literal permissions
- PublicBookingEndpoints.cs and PublicOsdiEndpoints.cs correctly excluded
- Test uses `Permissions.` constant pattern check, not hardcoded strings

## Self-Check: PASSED
