---
phase: 5
slug: prescriptions-document-printing
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-03-05
---

# Phase 5 -- Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xunit 2.* + FluentAssertions 8.* + NSubstitute 5.* |
| **Config file** | Clinical.Unit.Tests.csproj (exists), Pharmacy.Unit.Tests.csproj (Plan 19) |
| **Quick run command** | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~Prescription" -x` |
| **Full suite command** | `dotnet test backend/tests/Clinical.Unit.Tests && dotnet test backend/tests/Pharmacy.Unit.Tests` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~Prescription" -x`
- **After every plan wave:** Run `dotnet test backend/tests/Clinical.Unit.Tests && dotnet test backend/tests/Pharmacy.Unit.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Wave 0 Strategy

Wave 0 test stubs are created as part of the TDD plans themselves (Plans 06, 07, 19). These plans follow the RED-GREEN-REFACTOR cycle per CLAUDE.md:

- **Plan 06** (Wave 2, TDD): Drug prescription handler tests -- writes failing tests first for AddDrugPrescription, UpdateDrugPrescription, RemoveDrugPrescription, CheckDrugAllergy
- **Plan 07** (Wave 2, TDD): Optical prescription handler tests -- writes failing tests first for AddOpticalPrescription, UpdateOpticalPrescription
- **Plan 19** (Wave 3, TDD): Pharmacy handler tests -- creates Pharmacy.Unit.Tests project with SearchDrugCatalog and CRUD handler tests

This approach embeds Wave 0 into the TDD plans rather than requiring a separate test scaffold plan. Each TDD plan starts with the RED phase (failing tests) before implementing GREEN (passing code).

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | Status |
|---------|------|------|-------------|-----------|-------------------|--------|
| 05-06-T1 | 06 | 2 | RX-01 | unit (TDD) | `dotnet test --filter "AddDrugPrescription" -x` | pending |
| 05-06-T2 | 06 | 2 | RX-05 | unit (TDD) | `dotnet test --filter "CheckDrugAllergy" -x` | pending |
| 05-07-T1 | 07 | 2 | RX-03 | unit (TDD) | `dotnet test --filter "OpticalPrescription" -x` | pending |
| 05-19-T1 | 19 | 3 | RX-01 | unit (TDD) | `dotnet test --filter "SearchDrugCatalog" -x` | pending |
| 05-19-T2 | 19 | 3 | RX-01 | unit (TDD) | `dotnet test --filter "DrugCatalogCrud" -x` | pending |
| 05-10-T2 | 10 | 4 | RX-04 | build | `dotnet build backend/src/Bootstrapper/Bootstrapper.csproj` | pending |
| 05-11-T1 | 11 | 5 | PRT-01 | build | `dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj` | pending |
| 05-11-T2 | 11 | 5 | PRT-01 | build | `dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj` | pending |
| 05-12a-T1 | 12a | 6 | PRT-02, PRT-04 | build | `dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj` | pending |
| 05-12a-T2 | 12a | 6 | PRT-05, PRT-06 | build | `dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj` | pending |
| 05-12b-T1 | 12b | 6 | PRT-01..06 | build | `dotnet build backend/src/Bootstrapper/Bootstrapper.csproj` | pending |
| 05-15-T1 | 15 | 6 | RX-01, RX-05 | typecheck | `cd frontend && npx tsc --noEmit` | pending |
| 05-16-T1 | 16 | 7 | RX-03 | typecheck | `cd frontend && npx tsc --noEmit` | pending |
| 05-17a-T1 | 17a | 8 | PRT-01..06 | typecheck | `cd frontend && npx tsc --noEmit` | pending |
| 05-21-T1 | 21 | 9 | ALL | e2e | `dotnet test && tsc --noEmit` | pending |
| 05-21-T2 | 21 | 9 | ALL | human | Human verification | pending |

*Status: pending / green / red / flaky*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| PDF visual layout matches MOH format | RX-04 | Visual inspection of generated PDF | Generate sample drug Rx PDF, verify A5 size, clinic header, required fields placement |
| Print dialog opens correctly | PRT-01--PRT-06 | Browser print behavior | Click print button for each document type, verify PDF opens in new tab |
| Vietnamese diacritics render correctly in PDF | RX-04 | Font rendering verification | Generate PDF with Vietnamese drug names, verify diacritics display |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or TDD plan coverage
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covered by TDD plans (06, 07, 19)
- [x] No watch-mode flags
- [x] Feedback latency < 30s
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
