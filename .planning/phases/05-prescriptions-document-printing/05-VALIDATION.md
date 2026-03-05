---
phase: 5
slug: prescriptions-document-printing
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-05
---

# Phase 5 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xunit 2.* + FluentAssertions 8.* + NSubstitute 5.* |
| **Config file** | Clinical.Unit.Tests.csproj (exists), Pharmacy.Unit.Tests.csproj (Wave 0) |
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

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 05-01-01 | 01 | 1 | RX-01 | unit | `dotnet test --filter "AddDrugPrescription" -x` | ❌ W0 | ⬜ pending |
| 05-01-02 | 01 | 1 | RX-02 | unit | `dotnet test --filter "PrescriptionItem*CatalogFlag" -x` | ❌ W0 | ⬜ pending |
| 05-02-01 | 02 | 1 | RX-03 | unit | `dotnet test --filter "OpticalPrescription" -x` | ❌ W0 | ⬜ pending |
| 05-03-01 | 03 | 1 | RX-05 | unit | `dotnet test --filter "DrugAllergy" -x` | ❌ W0 | ⬜ pending |
| 05-04-01 | 04 | 1 | RX-04 | unit | `dotnet test --filter "DrugPrescriptionDocument" -x` | ❌ W0 | ⬜ pending |
| 05-05-01 | 05 | 2 | PRT-01 | unit | `dotnet test --filter "DrugPrescriptionDocument" -x` | ❌ W0 | ⬜ pending |
| 05-05-02 | 05 | 2 | PRT-02 | unit | `dotnet test --filter "OpticalPrescriptionDocument" -x` | ❌ W0 | ⬜ pending |
| 05-06-01 | 06 | 2 | PRT-04 | unit | `dotnet test --filter "ReferralLetterDocument" -x` | ❌ W0 | ⬜ pending |
| 05-06-02 | 06 | 2 | PRT-05 | unit | `dotnet test --filter "ConsentFormDocument" -x` | ❌ W0 | ⬜ pending |
| 05-06-03 | 06 | 2 | PRT-06 | unit | `dotnet test --filter "PharmacyLabelDocument" -x` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `backend/tests/Pharmacy.Unit.Tests/` — new test project for pharmacy handlers
- [ ] `backend/tests/Pharmacy.Unit.Tests/Pharmacy.Unit.Tests.csproj` — test project setup
- [ ] `backend/tests/Clinical.Unit.Tests/Features/AddDrugPrescriptionHandlerTests.cs` — stubs for RX-01, RX-02
- [ ] `backend/tests/Clinical.Unit.Tests/Features/AddOpticalPrescriptionHandlerTests.cs` — stubs for RX-03
- [ ] `backend/tests/Clinical.Unit.Tests/Features/CheckDrugAllergyHandlerTests.cs` — stubs for RX-05
- [ ] `backend/tests/Clinical.Unit.Tests/Documents/DrugPrescriptionDocumentTests.cs` — stubs for RX-04, PRT-01
- [ ] QuestPDF NuGet package added to solution

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| PDF visual layout matches MOH format | RX-04 | Visual inspection of generated PDF | Generate sample drug Rx PDF, verify A5 size, clinic header, required fields placement |
| Print dialog opens correctly | PRT-01–PRT-06 | Browser print behavior | Click print button for each document type, verify PDF opens in new tab |
| Vietnamese diacritics render correctly in PDF | RX-04 | Font rendering verification | Generate PDF with Vietnamese drug names, verify diacritics display |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
