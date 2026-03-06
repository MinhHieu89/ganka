# Deferred Items - Phase 05

## Pre-existing Architecture Test Failures (Out of Scope)

Discovered during 05-21 verification. These are from prior phases and documented in STATE.md decisions.

1. **All_Domain_Entities_Should_Have_Private_Setters** - `FieldChange` entity has public setters (from Phase 3)
2. **Contracts_Should_Not_Reference_Module_Internals(Patient)** - Patient.Contracts references Patient.Domain for enum reuse (Phase 2 decision)
3. **Application_Should_Not_Depend_On_Infrastructure** - Clinical.Application references Shared.Infrastructure for ReferenceDbContext (Phase 3 decision)
4. **Contracts_Should_Be_Independent** - Patient.Contracts references Patient.Domain (same as #2)
5. **Presentation_Should_Not_Depend_On_Domain_Directly** - Patient.Presentation references Domain (Phase 2)

**Recommendation:** Address in a future refactoring plan to either update tests to account for accepted deviations or refactor code to match architecture rules.
