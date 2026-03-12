---
phase: 06-pharmacy-consumables
verified: 2026-03-12T03:33:48Z
status: passed
score: 7/7 must-haves verified
re_verification:
  previous_status: passed
  previous_score: 7/7
  gaps_closed: []
  gaps_remaining: []
  regressions: []
notes:
  - "CON-03 auto-deduction: DeductTreatmentConsumablesHandler exists in the Pharmacy module (DeductTreatmentConsumables.cs). This file was created in Phase 9 plan 18 (per 09-18-SUMMARY.md), not Phase 6. In Phase 6, the scaffolding (ConsumableBatch.Deduct(), IConsumableRepository.GetBatchesAsync) was ready; the full cross-module handler was delivered in Phase 9. REQUIREMENTS.md marks CON-03 as Complete under Phase 6 which is a documentation inconsistency — the actual delivery was Phase 9. This does not block Phase 6 goal."
  - "ConsumableAlertBanner.tsx still uses hardcoded Vietnamese strings instead of useTranslation (lines 33, 43, 64-66). Warning level — does not block functionality. Identified in 2026-03-06 verification, confirmed still present in 2026-03-12 re-verification."
human_verification:
  - test: "Verify supplier toggle persists across page reload"
    expected: "Click toggle on an active supplier, see inactive badge, reload page, supplier still shows as inactive. Click toggle again, supplier becomes active again."
    why_human: "Requires live database to confirm persistence through full request lifecycle"
  - test: "Verify global dispensing history page renders real data"
    expected: "Navigate to /pharmacy/dispensing-history via sidebar. Paginated list shows past dispensing records (patient name, date, line count). Expand a row to see individual drug lines with quantities and optional override reason."
    why_human: "Requires existing dispensing records in the live database"
  - test: "Verify dispensing workflow end-to-end with PrescriptionCode"
    expected: "Pharmacist sees prescription queue with MOH prescription code visible, clicks to open dispensing dialog, sees FEFO batch suggestions, confirms dispensing, stock deducted from correct batches"
    why_human: "Cross-module integration with live Clinical prescriptions requires a non-null PrescriptionCode in live data"
  - test: "Verify expiry alert banner appears and clears"
    expected: "After importing a batch with expiry date within 30 days, the ExpiryAlertBanner appears on /pharmacy with the correct drug name, batch number, and days until expiry"
    why_human: "Requires live database with expiry data"
  - test: "Verify low stock alert triggers correctly"
    expected: "After setting MinStockLevel=100 on a drug and batch total is below 100, LowStockAlertBanner appears"
    why_human: "Requires live database configuration"
  - test: "Verify consumables seeded items appear"
    expected: "12+ IPL/LLLT treatment supplies appear in /consumables on fresh database"
    why_human: "Requires running ConsumableCatalogSeeder hosted service"
  - test: "Verify ConsumableAlertBanner language switching"
    expected: "Switch UI to English — ConsumableAlertBanner labels switch to English (currently hardcoded Vietnamese: 'Cảnh báo tồn kho thấp', 'Tồn kho ổn định', 'Tên vật tư', 'Tồn kho hiện tại', 'Tối thiểu')"
    why_human: "Visual verification; hardcoded strings are an accepted Warning-level issue"
---

# Phase 6: Pharmacy & Consumables Verification Report

**Phase Goal:** Pharmacist can manage drug inventory with batch/expiry tracking and dispense against prescriptions, with a separate consumables warehouse for treatment supplies
**Verified:** 2026-03-12T03:33:48Z
**Status:** passed
**Re-verification:** Yes — fresh re-verification after UAT gap closure (plans 06-30 and 06-31) and git status showing modified files

## Re-verification Summary

The previous VERIFICATION.md (2026-03-11) was already status `passed` (7/7) after two UAT gaps were closed:

1. Supplier toggle active/inactive persistence (fixed in 06-30)
2. Global dispensing history page (fixed in 06-31)

This re-verification confirms the current codebase matches all 7 truths. All recently modified files (from git status) have been inspected. No regressions detected.

**Previous status:** passed (7/7)
**Current status:** passed (7/7)
**Regressions:** None detected.

One correction to previous VERIFICATION.md: `DeductTreatmentConsumablesHandler` (CON-03) was delivered in Phase 9 plan 18, not Phase 6. The Phase 6 note claiming "scaffolding ready for Phase 9 wiring" was incorrect — Phase 9 plan 18 created the full handler. Phase 6 provided the domain infrastructure (ConsumableBatch.Deduct(), IConsumableRepository).

---

## Goal Achievement

### Observable Truths

| #   | Truth                                                                                                                                    | Status     | Evidence                                                                                                                                                                                        |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------- | ---------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Staff can manage drug inventory with batch tracking and multiple suppliers, including toggling supplier active/inactive status            | VERIFIED   | `ToggleSupplierActiveHandler` (36 lines) calls `Deactivate()`/`Activate()` then `SaveChangesAsync`; `PATCH /api/pharmacy/suppliers/{id}/toggle-active` at `PharmacyApiEndpoints.cs` line 98; `useToggleSupplierActive()` wired at `suppliers.tsx` line 57 via `toggleActive.mutateAsync(supplier.id)` |
| 2   | System alerts when drugs approach expiry (30/60/90 days) or fall below minimum stock levels                                              | VERIFIED   | `GetExpiryAlerts.cs` + `GetLowStockAlerts.cs` handlers substantive; `ExpiryAlertBanner` and `LowStockAlertBanner` imported and rendered at `pharmacy/index.tsx` lines 53-54                    |
| 3   | Pharmacist can dispense drugs against HIS prescription with FEFO, 7-day validity enforcement, and MOH prescription code shown            | VERIFIED   | `DispenseDrugs.cs` line 87 `PrescriptionValidityDays=7`; line 180 `FEFOAllocator.Allocate()`; `DispensingDto.cs` line 68 carries `PrescriptionCode`; `DispensingRepository.cs` line 132 maps it; `DispensingDialog.tsx` line 290 renders it; `PharmacyQueueTable.tsx` line 70-71 shows it in queue column |
| 4   | Staff can process walk-in OTC sales without prescription                                                                                 | VERIFIED   | `CreateOtcSale.cs` handler with FEFO; `OtcSaleForm.tsx` line 322 calls `createOtcSale.mutateAsync`; `POST /api/pharmacy/otc-sales` registered in `DispensingApiEndpoints.cs` line 71           |
| 5   | System maintains separate consumables warehouse with stock levels, batch management, and alerts                                          | VERIFIED   | `ConsumableItem.cs`, `ConsumableBatch.cs` domain entities; all 7 endpoints in `ConsumablesApiEndpoints.cs` (GET, POST, PUT, GET batches, POST stock, POST adjust, GET alerts); `ConsumableAlertBanner` wired to `useConsumableAlerts()`; `ConsumableAdjustDialog` uses `useConsumableBatches` for batch selection |
| 6   | Pharmacist can view global dispensing history across all patients in a paginated view accessible from sidebar                            | VERIFIED   | `dispensing-history.tsx` (208 lines, substantive); `useDispensingHistory(page)` called without patientId; sidebar at `AppSidebar.tsx` line 131 links `/pharmacy/dispensing-history`; EN key `pharmacyDispensingHistory: "Dispensing History"` at `en/common.json` line 20 |
| 7   | Supplier active/inactive toggle persists to database and refreshes UI                                                                    | VERIFIED   | `ToggleSupplierActive.cs` persists via `unitOfWork.SaveChangesAsync`; `useToggleSupplierActive()` at `pharmacy-queries.ts` line 222 invalidates `pharmacyKeys.suppliers.all()` on success so UI re-fetches current state |

**Score:** 7/7 truths verified

---

## Required Artifacts

| Artifact | Expected | Status | Notes |
| -------- | -------- | ------ | ----- |
| `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Suppliers/ToggleSupplierActive.cs` | Toggle handler calling Activate()/Deactivate() + SaveChanges | VERIFIED | 36 lines; branches on `supplier.IsActive`, calls domain method, then `unitOfWork.SaveChangesAsync` |
| `backend/src/Modules/Pharmacy/Pharmacy.Presentation/PharmacyApiEndpoints.cs` line 98 | PATCH /suppliers/{id}/toggle-active endpoint | VERIFIED | `group.MapPatch("/suppliers/{id:guid}/toggle-active", ...)` confirmed present |
| `frontend/src/features/pharmacy/api/pharmacy-api.ts` line 354 | `toggleSupplierActive()` API function | VERIFIED | `api.PATCH(...toggle-active...)` present |
| `frontend/src/features/pharmacy/api/pharmacy-queries.ts` line 222 | `useToggleSupplierActive()` mutation hook | VERIFIED | Calls `toggleSupplierActive(id)`, invalidates suppliers cache on success |
| `frontend/src/app/routes/_authenticated/pharmacy/suppliers.tsx` | Suppliers page wires toggle mutation | VERIFIED | `toggleActive.mutateAsync(supplier.id)` at line 57 |
| `frontend/src/app/routes/_authenticated/pharmacy/dispensing-history.tsx` | Paginated global dispensing history page | VERIFIED | 208 lines; `useDispensingHistory(page)` without patientId; expandable rows showing drug lines and override reason |
| `frontend/src/shared/components/AppSidebar.tsx` line 131 | Sidebar link to dispensing history | VERIFIED | `{ titleKey: "sidebar.pharmacyDispensingHistory", to: "/pharmacy/dispensing-history" }` |
| `frontend/public/locales/en/common.json` line 20 | EN translation for dispensing history nav | VERIFIED | `"pharmacyDispensingHistory": "Dispensing History"` |
| `frontend/public/locales/en/pharmacy.json` | EN translations for dispensing history page | VERIFIED | `"dispensingHistory": "Dispensing History"`, `"dispensingHistorySubtitle": "All dispensing records across all patients"` |
| `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/GetConsumableBatches.cs` | Consumable batch retrieval for AdjustStock dialog | VERIFIED | 41 lines, FEFO-ordered; `GET /api/consumables/{id}/batches` registered at `ConsumablesApiEndpoints.cs` line 54 |
| `frontend/src/features/consumables/components/ConsumableAdjustDialog.tsx` | Batch selector for ExpiryTracked consumable adjustments | VERIFIED | 297 lines; calls `useConsumableBatches(itemId)` when `isExpiryTracked && open`; renders batch selector with batch number, qty, expiry; passes `consumableBatchId` to mutation |

---

## Key Link Verification

| From | To | Via | Status | Details |
| ---- | -- | --- | ------ | ------- |
| `suppliers.tsx` (line 57) | `PATCH /api/pharmacy/suppliers/{id}/toggle-active` | `toggleActive.mutateAsync(supplier.id)` → `toggleSupplierActive(id)` in `pharmacy-api.ts` line 354 | WIRED | Full chain confirmed; cache invalidated on success |
| `ToggleSupplierActiveHandler` | `supplier.Activate()`/`supplier.Deactivate()` | Conditional on `supplier.IsActive` (lines 28-31) | WIRED | Persisted via `unitOfWork.SaveChangesAsync(ct)` line 33 |
| `dispensing-history.tsx` (line 22) | `GET /api/pharmacy/dispensing/history` | `useDispensingHistory(page)` → `getDispensingHistory(page, 20, undefined)` in `pharmacy-api.ts` line 495 | WIRED | `api.GET("/api/pharmacy/dispensing/history", ...)` confirmed |
| `AppSidebar.tsx` (line 131) | `/pharmacy/dispensing-history` | `sidebar.pharmacyDispensingHistory` i18n key | WIRED | Link renders via `renderNavItems`; EN + VI translations confirmed |
| `ConsumableAdjustDialog.tsx` | `GET /api/consumables/{id}/batches` | `useConsumableBatches(itemId)` → `getConsumableBatches(id)` in `consumables-api.ts` line 133 | WIRED | `api.GET("/api/consumables/${id}/batches")` → endpoint at `ConsumablesApiEndpoints.cs` line 54 → `GetConsumableBatchesHandler` |
| `DispensingDialog.tsx` (line 290) | `PrescriptionCode` from backend | `DispensingRecordDto.prescriptionCode` → `DispensingRepository.cs` line 132 → `DrugPrescription.PrescriptionCode` DB column | WIRED | Field flows through all layers; rendered conditionally when non-null |
| `PharmacyQueueTable.tsx` (line 70-71) | `prescriptionCode` column | `columnHelper.accessor("prescriptionCode")` | WIRED | Present in queue table and filterable |
| `OtcSaleForm.tsx` (line 322) | `POST /api/pharmacy/otc-sales` | `createOtcSale.mutateAsync(...)` → `DispensingApiEndpoints.cs` line 71 | WIRED | OTC sale form submits to registered endpoint |
| `pharmacy/index.tsx` (lines 53-54) | Alert data from backend | `ExpiryAlertBanner` → `useExpiryAlerts()` + `LowStockAlertBanner` → `useLowStockAlerts()` | WIRED | Both banners imported and rendered; hooks call respective GET endpoints |
| `ConsumableAlertBanner` | `GET /api/consumables/alerts` | `useConsumableAlerts()` → `getConsumableAlerts()` in `consumables-api.ts` line 139 | WIRED | Alert endpoint registered at `ConsumablesApiEndpoints.cs` line 88 |

---

## Requirements Coverage

| Requirement | Source Plans | Description | Status | Evidence |
| ----------- | ------------ | ----------- | ------ | -------- |
| PHR-01 | 06-01, 06-11, 06-30 | Staff can manage drug inventory with batch tracking and multiple suppliers | SATISFIED | `Supplier.cs` + `DrugBatch.cs` domain entities; full CRUD endpoints; `ToggleSupplierActive` gap closed in 06-30 |
| PHR-02 | 06-12 | Staff can import stock via supplier invoice (manual entry) or Excel bulk import | SATISFIED | `CreateStockImport.cs` (invoice) + `ImportStockFromExcel.cs` (MiniExcel) both substantive and wired |
| PHR-03 | 06-10, 06-15 | System tracks expiry dates and alerts at configurable thresholds (30/60/90 days) | SATISFIED | `GetExpiryAlerts.cs` with `DaysThreshold` parameter; `ExpiryAlertBanner` with 3-button threshold selector wired to `/pharmacy` |
| PHR-04 | 06-14, 06-15 | System alerts when drug stock falls below configurable minimum level per drug | SATISFIED | `GetLowStockAlerts.cs` + `LowStockAlertBanner`; `MinStockLevel` on `DrugCatalogItem` |
| PHR-05 | 06-13, 06-28, 06-31 | Pharmacist can dispense drugs against HIS prescription with auto stock deduction | SATISFIED | `DispenseDrugs.cs` with FEFO + PrescriptionCode pipeline; global dispensing history page added in 06-31 |
| PHR-06 | 06-14 | Staff can process walk-in OTC sales without prescription | SATISFIED | `CreateOtcSale.cs` handler + `OtcSaleForm.tsx` + `POST /api/pharmacy/otc-sales` |
| PHR-07 | 06-13 | System enforces 7-day prescription validity and warns on expired Rx | SATISFIED | `PrescriptionValidityDays=7` in `DispenseDrugs.cs` line 87; override reason required for expired prescriptions |
| CON-01 | 06-04, 06-09 | System maintains separate consumables warehouse independent from pharmacy stock | SATISFIED | `ConsumableItem`/`ConsumableBatch` in Pharmacy module; `/api/consumables` endpoints; `/consumables` route separate from `/pharmacy` |
| CON-02 | 06-16, 06-22, 06-26 | Staff can manage treatment supplies inventory with stock levels and alerts | SATISFIED | `ConsumableItemTable`, `AddStockDialog`, `ConsumableAdjustDialog` (with batch selector in 06-26), `ConsumableAlertBanner` all wired; `GetConsumableBatches` endpoint added for adjust dialog |
| CON-03 | 06-28 (descoped); delivered in Phase 9 plan 18 | Consumable usage per treatment session auto-deducts from consumables warehouse | SATISFIED IN PHASE 9 | `DeductTreatmentConsumablesHandler` created in Phase 9 plan 18 (per 09-18-SUMMARY.md). Phase 6 plan 06-28 correctly descoped this to Phase 9. Domain scaffolding (`ConsumableBatch.Deduct()`, `IConsumableRepository.GetBatchesAsync`) was ready in Phase 6. REQUIREMENTS.md marks CON-03 as Complete under Phase 6 — documentation inconsistency only. Auto-deduction is fully functional as of Phase 9. |

### CON-03 Traceability Note

The `DeductTreatmentConsumablesHandler` file is present at `backend/src/Modules/Pharmacy/Pharmacy.Application/Features/Consumables/DeductTreatmentConsumables.cs`. This file was created in Phase 9 plan 18 (confirmed in `09-18-SUMMARY.md` key-files.created), not Phase 6. The Treatment module publishes `TreatmentSessionCompletedIntegrationEvent` (from `TreatmentPackage.cs` line 192) → `PublishSessionCompletedIntegrationEventHandler` bridges the domain event → `DeductTreatmentConsumablesHandler` deducts stock via FEFO. The full CON-03 pipeline is functional in the system but was not a Phase 6 deliverable.

---

## Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
| ---- | ---- | ------- | -------- | ------ |
| `frontend/src/features/consumables/components/ConsumableAlertBanner.tsx` | 33 | Hardcoded `"Cảnh báo tồn kho thấp"` string instead of `useTranslation` | Warning | Banner title does not switch to English |
| `frontend/src/features/consumables/components/ConsumableAlertBanner.tsx` | 43 | Hardcoded `"Tồn kho ổn định"` string instead of `useTranslation` | Warning | OK-state label does not switch to English |
| `frontend/src/features/consumables/components/ConsumableAlertBanner.tsx` | 64-66 | Hardcoded table headers `"Tên vật tư"`, `"Tồn kho hiện tại"`, `"Tối thiểu"` instead of `useTranslation` | Warning | Column headers do not switch to English |

No blocking anti-patterns found. All other Pharmacy and Consumables components verified to use `useTranslation` consistently. The hardcoded strings in `ConsumableAlertBanner.tsx` were identified in the 2026-03-06 verification and remain unfixed. No new anti-patterns introduced by plans 06-30 or 06-31.

---

## Human Verification Required

### 1. Supplier Toggle Persistence (Regression Check)

**Test:** Navigate to /pharmacy/suppliers. Click the toggle button on an active supplier. Verify the row shows the supplier as inactive. Reload the page. Verify the supplier still shows as inactive. Click the toggle again to restore.
**Expected:** Toggle persists across page reloads; supplier row reflects current database state on refresh.
**Why human:** Requires a live database to confirm `Activate()`/`Deactivate()` state changes survive a full HTTP round-trip and EF Core `SaveChangesAsync` call.

### 2. Global Dispensing History Page

**Test:** Navigate to /pharmacy/dispensing-history via the sidebar "Dispensing History" link. Verify a paginated list of past dispensing records appears with patient name, dispensed date, and line count. Click a row to expand it and see individual drug lines.
**Expected:** Real data from `GET /api/pharmacy/dispensing/history`; expandable rows show drug name, quantity, unit; override reason shown in yellow when present; pagination controls work.
**Why human:** Requires existing dispensing records in the live database.

### 3. End-to-End Dispensing Workflow with PrescriptionCode

**Test:** Create a prescription via Phase 5 clinical workflow with a MOH prescription code assigned. Navigate to /pharmacy/queue. Verify the MOH prescription code appears in the queue table column. Click the prescription, confirm the code appears in the dispensing dialog header. Confirm dispensing. Verify stock decremented.
**Expected:** PrescriptionCode visible in both queue table and dialog; batch quantities decremented from oldest-expiry batch first; prescription disappears from queue.
**Why human:** Cross-module integration with live Clinical data; requires a prescription with a non-null `PrescriptionCode` in the system.

### 4. Expiry Alert Banner Functional Verification

**Test:** Import a drug batch via /pharmacy/stock-import with expiry date within 30 days. Navigate to /pharmacy.
**Expected:** `ExpiryAlertBanner` appears at top of page with drug name, batch number, and days-until-expiry count. Clicking the 30/60/90 day toggle buttons changes the alert threshold and count.
**Why human:** Requires live database with a qualifying batch.

### 5. Low Stock Alert Trigger

**Test:** Set a drug's `MinStockLevel` to a value greater than its current total batch stock. Navigate to /pharmacy.
**Expected:** `LowStockAlertBanner` appears showing the drug as below minimum stock level with current stock and minimum values.
**Why human:** Requires live database configuration.

### 6. Consumables Seeded Items

**Test:** On a fresh environment (empty `ConsumableItems` table), start the backend and navigate to /consumables.
**Expected:** 12+ pre-seeded IPL/LLLT treatment supplies appear with zero stock (seeded by `ConsumableCatalogSeeder` hosted service).
**Why human:** Requires running backend against a database to trigger the hosted service.

### 7. ConsumableAlertBanner Language Switching (Warning Item)

**Test:** Switch UI language to English via language toggle. Navigate to /consumables. If any consumable item is below its minimum stock level, confirm the alert banner appears.
**Expected:** Banner title, OK-state label, and table column headers display in English. Currently hardcoded Vietnamese: "Cảnh báo tồn kho thấp", "Tồn kho ổn định", "Tên vật tư", "Tồn kho hiện tại", "Tối thiểu" do not switch.
**Why human:** Visual confirmation; accepted as Warning-level defect.

---

## Final Assessment

Phase 6 goal is fully achieved. All 7 observable truths verified with substantive implementations and complete wiring:

1. Drug inventory management with batch tracking and supplier CRUD — fully implemented, supplier toggle persistence confirmed in code
2. Expiry and low stock alert system with configurable thresholds — end-to-end wiring confirmed
3. Prescription dispensing with FEFO, 7-day validity, and PrescriptionCode display — full pipeline confirmed across all 7 layers (domain → handler → repository → DTO → endpoint → API function → React component)
4. OTC walk-in sales without prescription — form submits to wired endpoint, FEFO allocation in handler
5. Consumables warehouse with separate stock management and alerts — complete CRUD, batch selection in adjust dialog, alert banner
6. Global dispensing history page — standalone paginated route, sidebar link, both EN and VI translations
7. Supplier toggle persists — backend handler calls domain methods + saves, frontend mutation invalidates cache on success

All 10 requirements (PHR-01 through PHR-07, CON-01 through CON-03) are accounted for. CON-03 auto-deduction was descoped from Phase 6 delivery per plan 06-28 and delivered in Phase 9 plan 18; the full handler is now present in the codebase.

One persistent warning: `ConsumableAlertBanner.tsx` uses hardcoded Vietnamese strings instead of `useTranslation` (lines 33, 43, 64-66). Identified in March 6 verification, still present. Functionality not blocked; only EN language switching is affected for this one component.

---

_Verified: 2026-03-12T03:33:48Z_
_Verifier: Claude (gsd-verifier)_
_Re-verification: Yes — fresh re-verification after plans 06-30 (supplier toggle) and 06-31 (dispensing history page)_
