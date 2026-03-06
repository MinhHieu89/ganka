---
phase: 06-pharmacy-consumables
verified: 2026-03-06T14:00:00Z
status: passed
score: 5/5 must-haves verified
re_verification:
  previous_status: passed
  previous_score: 5/5
  gaps_closed: []
  gaps_remaining: []
  regressions: []
notes:
  - "CON-03 auto-deduction is scoped to Phase 9 per ROADMAP line 347. REQUIREMENTS.md still marks CON-03 as [x] Complete under Phase 6 — this is a documentation inconsistency (not a code gap). The requirement text refers to auto-deduction from treatment sessions which is intentionally unimplemented in Phase 6. Scaffolding (ConsumableBatch.Deduct()) is present and ready for Phase 9 wiring."
human_verification:
  - test: "Verify dispensing workflow end-to-end with real prescription including PrescriptionCode"
    expected: "Pharmacist sees prescription queue with MOH prescription code visible, clicks to open dispensing dialog, sees FEFO batch suggestions, confirms dispensing, stock deducted from correct batches"
    why_human: "Cross-module integration with live Clinical prescriptions requires a non-null PrescriptionCode to be present in live data"
  - test: "Verify expiry alert banner appears and clears"
    expected: "After importing a batch with expiry date within 30 days, the ExpiryAlertBanner appears on /pharmacy with the correct drug name, batch number, and days until expiry"
    why_human: "Requires live database with expiry data"
  - test: "Verify low stock alert triggers correctly"
    expected: "After setting MinStockLevel=100 on a drug and batch total is below 100, LowStockAlertBanner appears"
    why_human: "Requires live database configuration"
  - test: "Verify consumables seeded items appear"
    expected: "12+ IPL/LLLT treatment supplies appear in /consumables on fresh database"
    why_human: "Requires running ConsumableCatalogSeeder hosted service"
  - test: "Verify Vietnamese translations display correctly"
    expected: "All pharmacy and consumables pages render with proper Vietnamese diacritics when language is set to VI"
    why_human: "Visual verification of diacritics and text quality"
---

# Phase 6: Pharmacy & Consumables Verification Report

**Phase Goal:** Pharmacist can manage drug inventory with batch/expiry tracking and dispense against prescriptions, with a separate consumables warehouse for treatment supplies
**Verified:** 2026-03-06T14:00:00Z
**Status:** passed
**Re-verification:** Yes — full re-verification against actual codebase (previous VERIFICATION.md also passed)

## Re-verification Summary

This is a full re-verification of the already-passed state. All artifacts and wiring were verified directly against the codebase.

**Previous status:** passed (5/5 truths verified)
**Current status:** passed (5/5 truths verified)
**Regressions:** None detected.

---

## Goal Achievement

### Observable Truths

| #   | Truth                                                                                                                         | Status     | Evidence                                                                                                                                                                        |
| --- | ----------------------------------------------------------------------------------------------------------------------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Staff can manage drug inventory with batch tracking and multiple suppliers                                                    | VERIFIED   | `Supplier.cs`, `DrugBatch.cs`, `FEFOAllocator.cs` all substantive; `CreateSupplier`, `GetSuppliers`, `UpdateSupplier`, inventory endpoints registered in `PharmacyApiEndpoints` |
| 2   | System alerts when drugs approach expiry (30/60/90 days) or fall below minimum stock levels                                   | VERIFIED   | `GetExpiryAlerts.cs` and `GetLowStockAlerts.cs` handlers present; `ExpiryAlertBanner` and `LowStockAlertBanner` components wired via `useExpiryAlerts`/`useLowStockAlerts` to API; rendered at `/pharmacy/index.tsx` lines 47-48 |
| 3   | Pharmacist can dispense drugs against HIS prescription with FEFO, 7-day validity enforcement, and MOH prescription code shown | VERIFIED   | `DispenseDrugs.cs` enforces `PrescriptionValidityDays=7`; `FEFOAllocator.Allocate()` called at line 180; `PrescriptionCode` flows from `DrugPrescription` → `ClinicalPendingPrescriptionDto` (line 20, 45) → `PendingPrescriptionDto` (line 67, 115) → `DispensingDialog.tsx` (lines 290-292); TDD test asserts `.PrescriptionCode.Should().Be("RX-2026-001")` at line 351 |
| 4   | Staff can process walk-in OTC sales without prescription                                                                      | VERIFIED   | `CreateOtcSale.cs` handler present; `OtcSaleForm` calls `useCreateOtcSale` → `createOtcSale.mutateAsync` at line 323; endpoint `POST /otc-sales` registered in `DispensingApiEndpoints.cs` line 71 |
| 5   | System maintains separate consumables warehouse with stock levels and alerts (CON-03 auto-deduction scoped to Phase 9)        | VERIFIED   | `ConsumableItem.cs`, `ConsumableBatch.cs` present and substantive; `ConsumableItemTable`, `ConsumableAlertBanner` wired to `/api/consumables` and `/api/consumables/alerts`; rendered at `/consumables/index.tsx` lines 33, 36; ROADMAP line 246 explicitly scopes auto-deduction to Phase 9 |

**Score:** 5/5 truths verified

---

## Required Artifacts

| Artifact | Expected | Status | Notes |
| -------- | -------- | ------ | ----- |
| `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/Supplier.cs` | Supplier AggregateRoot with CRUD factory | VERIFIED | Present and substantive |
| `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DrugBatch.cs` | DrugBatch with FEFO, Deduct, IsExpired | VERIFIED | Present and substantive |
| `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/DispensingRecord.cs` | Dispensing aggregate with lines | VERIFIED | Present and substantive |
| `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/ConsumableItem.cs` | ConsumableItem with two tracking modes | VERIFIED | Phase 9 comment uses permanent framing: "implemented in Phase 9 (Treatment Protocols)" |
| `backend/src/Modules/Pharmacy/Pharmacy.Domain/Entities/ConsumableBatch.cs` | ConsumableBatch with FEFO Deduct() | VERIFIED | Deduct() comment covers manual and Phase 9 auto flows |
| `backend/src/Modules/Pharmacy/Pharmacy.Domain/Services/FEFOAllocator.cs` | FEFO batch allocation domain service | VERIFIED | 59 lines, FEFO logic substantive, `Allocate()` used by DispenseDrugs handler |
| `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Dispensing/DispenseDrugs.cs` | 7-day validity + FEFO batch deduction | VERIFIED | `PrescriptionValidityDays=7` at line 87; `FEFOAllocator.Allocate()` at line 180 |
| `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Alerts/GetExpiryAlerts.cs` | Expiry alert handler with DaysThreshold | VERIFIED | `GetExpiryAlertsQuery(DaysThreshold)` delegates to repository |
| `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Alerts/GetLowStockAlerts.cs` | Low stock alert handler | VERIFIED | `GetLowStockAlertsHandler` delegates to repository |
| `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/StockImport/CreateStockImport.cs` | Supplier invoice import handler | VERIFIED | PHR-02 annotated; validator present |
| `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/StockImport/ImportStockFromExcel.cs` | Excel bulk import handler via MiniExcel | VERIFIED | Uses `MiniExcelLibs`; PHR-02 annotated |
| `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/` | Consumable CRUD + stock + alert handlers | VERIFIED | 6 handler files: Create, Update, GetItems, AddStock, AdjustStock, GetAlerts |
| `backend/src/Modules/Clinical/Clinical.Contracts/Dtos/PendingPrescriptionQuery.cs` | ClinicalPendingPrescriptionDto with PrescriptionCode | VERIFIED | `string? PrescriptionCode` at record param 5 (line 20) |
| `backend/src/Modules/Clinical/Clinical.Application/Features/GetPendingPrescriptions.cs` | Maps PrescriptionCode from DrugPrescription | VERIFIED | `PrescriptionCode: pw.Prescription.PrescriptionCode` at line 45 |
| `backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/DispensingDto.cs` | PendingPrescriptionDto with PrescriptionCode | VERIFIED | `string? PrescriptionCode` at line 67 |
| `backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Repositories/DispensingRepository.cs` | Cross-module query with PrescriptionCode mapping | VERIFIED | `PrescriptionCode: p.PrescriptionCode` at line 115 |
| `frontend/src/features/pharmacy/components/ExpiryAlertBanner.tsx` | Renders expiry alerts with threshold selector | VERIFIED | Real API call via `useExpiryAlerts(days)`; renders drug name, batch number, days until expiry |
| `frontend/src/features/pharmacy/components/LowStockAlertBanner.tsx` | Renders low stock alerts | VERIFIED | Real API call via `useLowStockAlerts()`; renders drug name, current stock, min level |
| `frontend/src/features/pharmacy/components/DispensingDialog.tsx` | Renders PrescriptionCode conditionally | VERIFIED | Lines 290-292 conditional render when non-null |
| `frontend/src/features/pharmacy/components/OtcSaleForm.tsx` | OTC sale form with real API submit | VERIFIED | `createOtcSale.mutateAsync({...})` called at line 323 on form submit |
| `frontend/src/features/consumables/components/ConsumableItemTable.tsx` | Consumable item table with CRUD | VERIFIED | Real data from `useConsumableItems()` hook |
| `frontend/src/features/consumables/components/ConsumableAlertBanner.tsx` | Consumable low stock alert banner | VERIFIED | Real data from `useConsumableAlerts()` hook |
| `backend/src/Modules/Pharmacy/Pharmacy.Presentation/DispensingApiEndpoints.cs` | Dispensing + OTC endpoints registered | VERIFIED | `MapGet /dispensing/pending`, `MapPost /dispensing`, `MapPost /otc-sales` all present |
| `backend/src/Modules/Pharmacy/Pharmacy.Presentation/ConsumablesApiEndpoints.cs` | Consumables endpoints registered | VERIFIED | `MapGet /`, `MapPost /`, `MapPut /{id}`, `MapPost /{id}/stock`, `MapPost /{id}/adjust`, `MapGet /alerts` all present |
| `backend/tests/Pharmacy.Unit.Tests/Features/DispensingHandlerTests.cs` | TDD tests including PrescriptionCode assertion | VERIFIED | Line 321: mock `PrescriptionCode: "RX-2026-001"`, line 351: `.PrescriptionCode.Should().Be("RX-2026-001")` |

---

## Key Link Verification

| From | To | Via | Status | Details |
| ---- | -- | --- | ------ | ------- |
| `GetPendingPrescriptionsHandler` | `DrugPrescription.PrescriptionCode` | `pw.Prescription.PrescriptionCode` | WIRED | `GetPendingPrescriptions.cs` line 45 |
| `ClinicalPendingPrescriptionDto` | `PendingPrescriptionDto` | `PrescriptionCode: p.PrescriptionCode` in DispensingRepository | WIRED | `DispensingRepository.cs` line 115 |
| `PendingPrescriptionDto` | `DispensingDialog` frontend | `prescription.prescriptionCode` conditional render | WIRED | `DispensingDialog.tsx` lines 290-292 |
| `PrescriptionCode` field | `PharmacyQueueTable` | `columnHelper.accessor("prescriptionCode")` | WIRED | `PharmacyQueueTable.tsx` line 70 — also searchable in filter at line 171 |
| `DispenseDrugs handler` | `FEFOAllocator` | `FEFOAllocator.Allocate(availableBatches, qty)` | WIRED | `DispenseDrugs.cs` line 180 |
| `ExpiryAlertBanner` | `/api/pharmacy/alerts/expiry` | `useExpiryAlerts(days)` → `pharmacy-queries.ts` line 116 | WIRED | Banner calls query; query calls API endpoint |
| `LowStockAlertBanner` | `/api/pharmacy/alerts/low-stock` | `useLowStockAlerts()` → `pharmacy-queries.ts` line 123 | WIRED | Banner calls query; query calls API endpoint |
| `ConsumableAlertBanner` | `/api/consumables/alerts` | `useConsumableAlerts()` → `consumables-queries.ts` line 39 | WIRED | `consumables-api.ts` line 132: `api.GET("/api/consumables/alerts")` |
| `OtcSaleForm` | `POST /api/pharmacy/otc-sales` | `useCreateOtcSale()` → `createOtcSale()` → `api.POST` | WIRED | `OtcSaleForm.tsx` line 323: `createOtcSale.mutateAsync({...})` |
| `ConsumableBatch.Deduct()` | Phase 9 Treatment Protocols | By design — not wired in Phase 6 | SCOPED | CON-03 explicitly Phase 9 per ROADMAP line 347 |

---

## Requirements Coverage

| Requirement | Source Plans | Description | Status | Evidence |
| ----------- | ------------ | ----------- | ------ | -------- |
| PHR-01 | 06-01, 06-11 | Staff can manage drug inventory with batch tracking and multiple suppliers | SATISFIED | `Supplier.cs`, `DrugBatch.cs`, supplier CRUD + inventory endpoints all present and wired |
| PHR-02 | 06-12 | Staff can import stock via supplier invoice or Excel bulk import | SATISFIED | `CreateStockImport.cs` (invoice) + `ImportStockFromExcel.cs` (MiniExcel) both present |
| PHR-03 | 06-10, 06-15 | System tracks expiry dates and alerts at configurable thresholds (30/60/90 days) | SATISFIED | `GetExpiryAlertsQuery(DaysThreshold)` handler + `ExpiryAlertBanner` with 3-button threshold selector |
| PHR-04 | 06-14, 06-15 | System alerts when drug stock falls below configurable minimum | SATISFIED | `GetLowStockAlertsHandler` + `LowStockAlertBanner`; `MinStockLevel` on `DrugCatalogItem` |
| PHR-05 | 06-13, 06-28 | Pharmacist can dispense drugs against HIS prescription with auto stock deduction | SATISFIED | `DispenseDrugs.cs` with FEFO + `PrescriptionCode` flows end-to-end through all layers |
| PHR-06 | 06-14 | Staff can process walk-in OTC sales without prescription | SATISFIED | `CreateOtcSale.cs` handler + `OtcSaleForm.tsx` + `DispensingApiEndpoints` line 71 |
| PHR-07 | 06-13 | System enforces 7-day prescription validity and warns on expired Rx | SATISFIED | `PrescriptionValidityDays=7` in `DispenseDrugs.cs` line 87; override reason required for expired Rx |
| CON-01 | 06-04, 06-09 | System maintains separate consumables warehouse independent from pharmacy stock | SATISFIED | `ConsumableItem`/`ConsumableBatch` in Pharmacy module; `/api/consumables` endpoints; `/consumables` route |
| CON-02 | 06-16, 06-22, 06-26 | Staff can manage treatment supplies inventory with stock levels and alerts | SATISFIED | `ConsumableItemTable`, `AddStockDialog`, `ConsumableAdjustDialog`, `ConsumableAlertBanner` all wired |
| CON-03 | 06-28 | Consumable usage per treatment session auto-deducts from consumables warehouse | DESCOPED TO PHASE 9 | Scaffolding (`ConsumableBatch.Deduct()`) ready; ROADMAP line 246 scopes delivery to Phase 9; Phase 9 criterion 5 explicitly owns this requirement. REQUIREMENTS.md marks as `[x] Complete` — this is a documentation inconsistency; the auto-deduction feature is not implemented in Phase 6 |

### CON-03 Documentation Note

`REQUIREMENTS.md` marks CON-03 as `[x]` Complete and traces it to Phase 6, but the actual auto-deduction behavior is not implemented. The ROADMAP correctly reflects reality at line 246: "auto-deduction from treatment sessions delivered in Phase 9." The domain scaffolding exists in `ConsumableBatch.Deduct()` and Phase 9 criterion 5 explicitly delivers the integration. This inconsistency in `REQUIREMENTS.md` does not block the Phase 6 goal — the goal does not include auto-deduction from treatment sessions.

---

## Anti-Patterns Found

None blocking. Scanned all key files:

- No `TODO`/`FIXME`/`PLACEHOLDER` found in pharmacy or consumables backend (`*.cs`) or frontend (`*.ts`, `*.tsx`) files.
- No stub return patterns (`return {}`, `return []` as sole body, `Not implemented` responses).
- `return null` at `DispensingDialog.tsx:266` is a legitimate guard clause (`if (!prescription) return null`).
- `ConsumableAlertBanner.tsx` uses hardcoded Vietnamese strings ("Cảnh báo tồn kho thấp", "Tồn kho ổn định") rather than `useTranslation` — this is a minor i18n inconsistency (the pharmacy equivalents `LowStockAlertBanner` correctly uses `t()`). Does not block functionality but flags as a warning.

| File | Pattern | Severity | Impact |
| ---- | ------- | -------- | ------ |
| `ConsumableAlertBanner.tsx` | Hardcoded Vietnamese strings instead of `useTranslation` | Warning | UI text won't switch to EN when user changes language |

---

## Human Verification Required

### 1. End-to-End Dispensing Workflow with PrescriptionCode

**Test:** Create a prescription via Phase 5 clinical workflow with a MOH prescription code assigned. Navigate to /pharmacy/queue. Verify the MOH prescription code appears in both the queue table column and the dispensing dialog header. Click the prescription, confirm FEFO batch suggestions appear, confirm dispensing, verify stock decremented.
**Expected:** Prescription disappears from queue after dispensing; PrescriptionCode visible in queue table and dialog; DrugBatch.CurrentQuantity decremented; oldest-expiry batch selected first.
**Why human:** Cross-module integration with live Clinical data requires a prescription with a non-null PrescriptionCode in the system.

### 2. Expiry Alert Banner Functional Verification

**Test:** Import a drug batch with expiry date within 30 days via /pharmacy/stock-import, then navigate to /pharmacy.
**Expected:** ExpiryAlertBanner appears at top of page with drug name, batch number, and days-until-expiry count.
**Why human:** Requires live database with qualifying batch data.

### 3. Low Stock Alert Trigger

**Test:** Set a drug's MinStockLevel to 100 via the pricing form, ensure total batch stock < 100, navigate to /pharmacy.
**Expected:** LowStockAlertBanner appears showing the drug as below minimum stock level.
**Why human:** Requires live database configuration.

### 4. Consumables Seeded Items

**Test:** On a fresh environment, start the backend and navigate to /consumables.
**Expected:** 12+ pre-seeded IPL/LLLT treatment supplies appear (IPL gel, eye shields, LLLT tips, etc.) with zero stock.
**Why human:** Requires running ConsumableCatalogSeeder hosted service against a database.

### 5. Vietnamese Translation Quality (ConsumableAlertBanner)

**Test:** Switch UI language to English, navigate to /consumables.
**Expected:** All labels in ConsumableAlertBanner switch to English (currently uses hardcoded Vietnamese strings).
**Why human:** Visual confirmation that hardcoded strings are acceptable or need i18n fix.

---

## Final Assessment

Phase 6 goal is fully achieved. All 5 observable truths are verified with substantive implementations and complete wiring:

1. Drug inventory management with batch tracking and supplier CRUD — fully implemented
2. Expiry and low stock alert system with configurable thresholds — fully wired end-to-end
3. Prescription dispensing with FEFO, 7-day validity, and PrescriptionCode display — complete pipeline confirmed across all 7 layers
4. OTC walk-in sales without prescription — form submits to API, stock deducted
5. Consumables warehouse with stock management and alerts — separate from pharmacy, full CRUD + alert pipeline

All 10 requirements (PHR-01 through PHR-07, CON-01 through CON-03) are accounted for. CON-03 auto-deduction is correctly scoped to Phase 9 with explicit ROADMAP notation and domain scaffolding ready for Phase 9 integration.

One documentation inconsistency noted: `REQUIREMENTS.md` marks CON-03 as `[x] Complete` mapped to Phase 6, but the auto-deduction behavior is intentionally unimplemented until Phase 9. This does not block Phase 6 goal achievement.

One minor anti-pattern: `ConsumableAlertBanner.tsx` uses hardcoded Vietnamese strings instead of `useTranslation`, inconsistent with `LowStockAlertBanner.tsx` which uses `t()`. This is a warning-level issue that does not block functionality but breaks EN language switching for the consumables alert banner.

---

_Verified: 2026-03-06T14:00:00Z_
_Verifier: Claude (gsd-verifier)_
