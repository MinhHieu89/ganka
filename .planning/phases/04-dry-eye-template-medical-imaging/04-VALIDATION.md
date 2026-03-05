---
phase: 4
slug: dry-eye-template-medical-imaging
status: ready
nyquist_compliant: true
wave_0_complete: true
created: 2026-03-05
---

# Phase 4 -- Validation Strategy

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

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | Test Created By | Status |
|---------|------|------|-------------|-----------|-------------------|-----------------|--------|
| 04-02-T1 | 02 | 3 | DRY-01, DRY-02 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~UpdateDryEyeAssessment\|FullyQualifiedName~OsdiCalculation\|FullyQualifiedName~SubmitOsdi" -x` | TDD in Plan 02 Task 1 | pending |
| 04-02-T2 | 02 | 3 | DRY-03, DRY-04 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~GetOsdiHistory\|FullyQualifiedName~DryEyeComparison" -x` | TDD in Plan 02 Task 2 | pending |
| 04-03-T1 | 03 | 3 | IMG-01, IMG-02, IMG-03, IMG-04 | unit | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~UploadMedicalImage\|FullyQualifiedName~GetVisitImages\|FullyQualifiedName~ImageComparison\|FullyQualifiedName~DeleteMedicalImage" -x` | TDD in Plan 03 Task 1 | pending |
| 04-04-T1 | 04 | 4 | DRY-01, DRY-02 | build | `cd frontend && npx tsc --noEmit && npm run build` | Plan 04 Task 1 | pending |
| 04-04-T2 | 04 | 4 | DRY-03, DRY-04 | build | `cd frontend && npx tsc --noEmit && npm run build` | Plan 04 Task 2 | pending |
| 04-05-T1 | 05 | 4 | IMG-01, IMG-02, IMG-03 | build | `cd frontend && npx tsc --noEmit && npm run build` | Plan 05 Task 1 | pending |
| 04-05-T2 | 05 | 4 | IMG-04 | build | `cd frontend && npx tsc --noEmit && npm run build` | Plan 05 Task 2 | pending |
| 04-06-T1 | 06 | 5 | ALL | e2e | `dotnet test backend/tests/ --no-restore && cd frontend && npx tsc --noEmit` | Plan 06 Task 1 | pending |
| 04-06-T2 | 06 | 5 | ALL | manual | Human verification | Plan 06 Task 2 | pending |

*Status: pending -- green -- red -- flaky*

**Note:** Wave 0 test stubs are not needed as separate tasks. Plans 02 and 03 use TDD (test-first), so tests are created as part of the RED phase within those plan tasks. Tests exist before production code.

---

## Wave 0 Requirements

Not applicable -- Plans 02 and 03 are TDD plans where tests are written first (RED phase) as part of each task. No separate Wave 0 test scaffold is needed because:
- Plan 02 Task 1 creates: UpdateDryEyeAssessmentHandlerTests.cs, OsdiCalculationTests.cs, SubmitOsdiQuestionnaireHandlerTests.cs
- Plan 02 Task 2 creates: GetOsdiHistoryHandlerTests.cs, GetDryEyeComparisonHandlerTests.cs
- Plan 03 Task 1 creates: UploadMedicalImageHandlerTests.cs, GetVisitImagesHandlerTests.cs, GetImageComparisonDataHandlerTests.cs, DeleteMedicalImageHandlerTests.cs

xUnit + FluentAssertions + NSubstitute already in Clinical.Unit.Tests.csproj from Phase 3.

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

- [x] All tasks have `<automated>` verify or TDD-created tests
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] TDD plans (02, 03) create all test files during RED phase (no Wave 0 needed)
- [x] No watch-mode flags
- [x] Feedback latency < 30s
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** approved
