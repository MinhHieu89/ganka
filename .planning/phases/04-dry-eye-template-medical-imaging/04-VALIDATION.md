---
phase: 4
slug: dry-eye-template-medical-imaging
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-05
---

# Phase 4 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit + FluentAssertions + NSubstitute (backend) |
| **Config file** | backend/tests/Clinical.Unit.Tests/Clinical.Unit.Tests.csproj |
| **Quick run command** | `dotnet test backend/tests/Clinical.Unit.Tests --filter "Category!=Integration" -x` |
| **Full suite command** | `dotnet test backend/tests/ --no-restore` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test backend/tests/Clinical.Unit.Tests --filter "Category!=Integration" -x`
- **After every plan wave:** Run `dotnet test backend/tests/ --no-restore`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 04-01-01 | 01 | 1 | DRY-01 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~UpdateDryEyeAssessment" -x` | ❌ W0 | ⬜ pending |
| 04-01-02 | 01 | 1 | DRY-02 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~OsdiCalculation" -x` | ❌ W0 | ⬜ pending |
| 04-01-03 | 01 | 1 | DRY-02 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~SubmitOsdiQuestionnaire" -x` | ❌ W0 | ⬜ pending |
| 04-01-04 | 01 | 1 | DRY-03 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~GetOsdiHistory" -x` | ❌ W0 | ⬜ pending |
| 04-01-05 | 01 | 1 | DRY-04 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~DryEyeComparison" -x` | ❌ W0 | ⬜ pending |
| 04-02-01 | 02 | 1 | IMG-01 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~UploadMedicalImage" -x` | ❌ W0 | ⬜ pending |
| 04-02-02 | 02 | 1 | IMG-02 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~UploadMedicalImage" -x` | ❌ W0 | ⬜ pending |
| 04-02-03 | 02 | 1 | IMG-03 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~GetVisitImages" -x` | ❌ W0 | ⬜ pending |
| 04-02-04 | 02 | 1 | IMG-04 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~ImageComparison" -x` | ❌ W0 | ⬜ pending |
| 04-02-05 | 02 | 1 | IMG-01 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~DeleteMedicalImage" -x` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `backend/tests/Clinical.Unit.Tests/Features/UpdateDryEyeAssessmentHandlerTests.cs` — stubs for DRY-01
- [ ] `backend/tests/Clinical.Unit.Tests/Features/OsdiCalculationTests.cs` — stubs for DRY-02
- [ ] `backend/tests/Clinical.Unit.Tests/Features/SubmitOsdiQuestionnaireHandlerTests.cs` — stubs for DRY-02
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetOsdiHistoryHandlerTests.cs` — stubs for DRY-03
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetDryEyeComparisonHandlerTests.cs` — stubs for DRY-04
- [ ] `backend/tests/Clinical.Unit.Tests/Features/UploadMedicalImageHandlerTests.cs` — stubs for IMG-01, IMG-02
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetVisitImagesHandlerTests.cs` — stubs for IMG-03
- [ ] `backend/tests/Clinical.Unit.Tests/Features/GetImageComparisonDataHandlerTests.cs` — stubs for IMG-04
- [ ] `backend/tests/Clinical.Unit.Tests/Features/DeleteMedicalImageHandlerTests.cs` — stubs for IMG-01

*Framework install: none needed — xUnit + FluentAssertions + NSubstitute already in Clinical.Unit.Tests.csproj*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| OSDI severity color coding renders correctly | DRY-02 | Visual CSS verification | Check severity badge shows correct color for each threshold |
| Image lightbox zoom works with touch/mouse | IMG-03 | Browser interaction | Open lightbox, verify zoom via scroll/pinch |
| QR code scans correctly | DRY-02 | Physical device test | Generate QR, scan with phone camera, verify page loads |
| Image comparison side-by-side layout | IMG-04 | Visual layout verification | Open comparison, verify two images render at correct size |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
