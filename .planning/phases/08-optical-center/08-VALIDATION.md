---
phase: 8
slug: optical-center
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-06
---

# Phase 8 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit + FluentAssertions + NSubstitute |
| **Config file** | backend/tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj (Wave 0) |
| **Quick run command** | `dotnet test backend/tests/Optical.Unit.Tests --no-build -v q` |
| **Full suite command** | `dotnet test backend/tests --no-build -v q` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test backend/tests/Optical.Unit.Tests --no-build -v q`
- **After every plan wave:** Run `dotnet test backend/tests --no-build -v q`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 08-01-01 | 01 | 1 | OPT-01 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~FrameHandler" -v q` | ❌ W0 | ⬜ pending |
| 08-01-02 | 01 | 1 | OPT-01 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~Barcode" -v q` | ❌ W0 | ⬜ pending |
| 08-02-01 | 02 | 1 | OPT-02 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~LensHandler" -v q` | ❌ W0 | ⬜ pending |
| 08-03-01 | 03 | 2 | OPT-03 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~OrderHandler" -v q` | ❌ W0 | ⬜ pending |
| 08-03-02 | 03 | 2 | OPT-03 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~GlassesOrderTests" -v q` | ❌ W0 | ⬜ pending |
| 08-04-01 | 04 | 2 | OPT-04 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~PaymentGate" -v q` | ❌ W0 | ⬜ pending |
| 08-05-01 | 05 | 2 | OPT-06 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~ComboHandler" -v q` | ❌ W0 | ⬜ pending |
| 08-06-01 | 06 | 3 | OPT-07 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~WarrantyHandler" -v q` | ❌ W0 | ⬜ pending |
| 08-06-02 | 06 | 3 | OPT-07 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~WarrantyTests" -v q` | ❌ W0 | ⬜ pending |
| 08-07-01 | 07 | 3 | OPT-08 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~PrescriptionHistory" -v q` | ❌ W0 | ⬜ pending |
| 08-08-01 | 08 | 3 | OPT-09 | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~StocktakingHandler" -v q` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `backend/tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj` — new test project (reference Optical.Application, Optical.Domain, xUnit, FluentAssertions, NSubstitute)
- [ ] `backend/tests/Optical.Unit.Tests/Domain/GlassesOrderTests.cs` — order lifecycle domain tests
- [ ] `backend/tests/Optical.Unit.Tests/Domain/FrameTests.cs` — frame entity + barcode validation
- [ ] `backend/tests/Optical.Unit.Tests/Domain/WarrantyClaimTests.cs` — warranty period validation
- [ ] `backend/tests/Optical.Unit.Tests/Features/` — handler test files per feature area

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Contact lenses via HIS only | OPT-05 | Architectural constraint — no optical code needed | Verify no contact lens inventory management exists in optical module |
| USB barcode scanner input | OPT-01 | Hardware-dependent keyboard emulation | Focus barcode input field, scan physical barcode, verify value populates |
| Phone camera scanning | OPT-09 | Device-dependent camera access | Open stocktaking on mobile device, scan frame barcode via camera |
| Barcode label printing | OPT-01 | Printer hardware required | Generate label, send to thermal printer / print A4 sheet |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
