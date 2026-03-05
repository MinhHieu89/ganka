---
phase: 6
slug: pharmacy-consumables
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-05
---

# Phase 6 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.* + FluentAssertions 8.* + NSubstitute 5.* |
| **Config file** | backend/tests/ directory structure |
| **Quick run command** | `dotnet test backend/tests/Pharmacy.Unit.Tests --no-build -v q` |
| **Full suite command** | `dotnet test backend/tests/ --no-build -v q` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test backend/tests/Pharmacy.Unit.Tests --no-build -v q`
- **After every plan wave:** Run `dotnet test backend/tests/ --no-build -v q`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 06-01-01 | 01 | 1 | PHR-01 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "DrugBatch" -x` | ❌ W0 | ⬜ pending |
| 06-01-02 | 01 | 1 | PHR-01 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "Supplier" -x` | ❌ W0 | ⬜ pending |
| 06-02-01 | 02 | 1 | PHR-02 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "StockImport" -x` | ❌ W0 | ⬜ pending |
| 06-02-02 | 02 | 1 | PHR-02 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "ExcelImport" -x` | ❌ W0 | ⬜ pending |
| 06-03-01 | 03 | 2 | PHR-03 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "ExpiryAlert" -x` | ❌ W0 | ⬜ pending |
| 06-03-02 | 03 | 2 | PHR-04 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "LowStock" -x` | ❌ W0 | ⬜ pending |
| 06-04-01 | 04 | 2 | PHR-05 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "FEFO" -x` | ❌ W0 | ⬜ pending |
| 06-04-02 | 04 | 2 | PHR-05 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "Dispensing" -x` | ❌ W0 | ⬜ pending |
| 06-05-01 | 05 | 3 | PHR-06 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "OtcSale" -x` | ❌ W0 | ⬜ pending |
| 06-05-02 | 05 | 3 | PHR-07 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "PrescriptionExpiry" -x` | ❌ W0 | ⬜ pending |
| 06-06-01 | 06 | 3 | CON-01 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "Consumable" -x` | ❌ W0 | ⬜ pending |
| 06-06-02 | 06 | 3 | CON-02 | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "ConsumableStock" -x` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `backend/tests/Pharmacy.Unit.Tests/` — entire test project needs creation
- [ ] `backend/tests/Pharmacy.Unit.Tests/Pharmacy.Unit.Tests.csproj` — project file with xUnit/FluentAssertions/NSubstitute references
- [ ] `backend/tests/Pharmacy.Unit.Tests/Features/` — handler test directory
- [ ] `backend/tests/Pharmacy.Unit.Tests/Domain/` — domain entity test directory (FEFO allocator, batch deduction, etc.)
- [ ] Framework install: Already available via CPM (xunit, FluentAssertions, NSubstitute, Bogus)

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Excel file upload UI | PHR-02 | Browser file input interaction | Upload .xlsx via stock import page, verify parsed preview |
| Sidebar badge display | PHR-05 | Visual badge rendering | Check pharmacy nav item shows pending count |
| Print prescription receipt | PHR-05 | PDF rendering in browser | Dispense a drug, verify receipt prints correctly |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
