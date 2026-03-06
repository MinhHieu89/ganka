---
status: resolved
phase: 06-pharmacy-consumables
source: 06-01 through 06-28 SUMMARY.md
started: 2026-03-06T12:10:00Z
updated: 2026-03-06T14:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test (API)
expected: Server boots, all pharmacy/consumables endpoints respond 200, seed data loads
result: pass
note: Auto-tested. Auth login 200, suppliers 200, inventory 200 (77 drugs), dispensing/pending 200, stock-imports 200, otc-sales 200, consumables 200 (12 items seeded).

### 2. Supplier CRUD (API)
expected: Create supplier 201, update supplier 200, data persists correctly
result: pass
note: Auto-tested. Created "UAT Test Supplier" (201), updated name/phone/email (200), verified fields persisted.

### 3. Stock Import via Invoice (API)
expected: Create stock import with drug lines, 201 returned, batch records created, stock increases
result: pass
note: Auto-tested. Created import for Acetazolamide (100 qty, batch BATCH-UAT-001, expiry 2027-06-15). Stock went 0->100, batchCount 0->1.

### 4. Drug Batch Details (API)
expected: GET /inventory/{drugId}/batches returns batch list with FEFO data (batchNumber, expiryDate, currentQuantity, isExpired, isNearExpiry)
result: pass
note: Auto-tested. Returns batch with correct FEFO fields. Batch shows initialQuantity=100, currentQuantity=95 after OTC sale.

### 5. OTC Walk-in Sale (API)
expected: Create anonymous OTC sale 201, stock deducted via FEFO, sale appears in history
result: pass
note: Auto-tested. Created walk-in sale (5 qty Acetazolamide). Stock 100->95. OTC history shows sale with null patientId/customerName.

### 6. Stock Adjustment +/- (API)
expected: Positive adjustment increases batch stock, negative decreases, both create audit records (201)
result: pass
note: Auto-tested. +10 adjustment -> stock 95->105 (201). -5 adjustment -> stock 105->100 (201). Reason enum and notes accepted.

### 7. Drug Selling Price Update (API)
expected: PUT /inventory/{drugId}/pricing updates sellingPrice and minStockLevel
result: pass
note: Auto-tested. Updated price to 120000 and minStock to 10. Verified in inventory list.

### 8. Consumable SimpleStock Add (API)
expected: POST /{id}/stock with quantity only increases CurrentStock for SimpleStock items
result: pass
note: Auto-tested. Added 200 to Disposable Gloves (SimpleStock). Stock 0->200. LowStock flipped false.

### 9. Consumable ExpiryTracked Add (API)
expected: POST /{id}/stock with batchNumber+expiryDate creates ConsumableBatch for ExpiryTracked items
result: pass
note: Auto-tested. Added batch AED-BATCH-001 (30 qty, exp 2027-06-30) to Anesthetic Eye Drops. Stock 0->30. Returns batch ID.

### 10. Consumable ExpiryTracked Validation (API)
expected: Adding stock to ExpiryTracked item without batchNumber returns 400 validation error
result: pass
note: Auto-tested. Got 400 "Batch number is required for expiry-tracked consumables." - correct validation.

### 11. Consumable Alerts (API)
expected: GET /consumables/alerts returns items that are low stock or near expiry
result: pass
note: Auto-tested. Returns all 12 seeded items (all initially zero stock = low stock).

### 12. Consumable Seeder (API)
expected: 12 consumable items auto-seeded on startup with correct tracking modes
result: pass
note: Auto-tested. Found 12 items: 3 ExpiryTracked (mode 0), 9 SimpleStock (mode 1). Names include Anesthetic Eye Drops, Cotton Applicators, IPL Gel, etc.

### 13. Frontend Routes Load (Auto)
expected: All 6 pharmacy/consumables routes return HTTP 200
result: pass
note: Auto-tested. /pharmacy 200, /pharmacy/queue 200, /pharmacy/suppliers 200, /pharmacy/stock-import 200, /pharmacy/otc-sales 200, /consumables 200.

### 14. Dispensing Queue API (API)
expected: GET /dispensing/pending returns list (empty if no prescriptions exist)
result: pass
note: Auto-tested. Returns empty array []. No pending prescriptions in test data. Endpoint structure is correct.

### 15. Stock Import History (API)
expected: GET /stock-imports returns paginated list with supplier, invoice, date, item count
result: pass
note: Auto-tested. Returns paginated response with totalCount, page, pageSize, totalPages fields.

### 16. Drug Inventory Page UI
expected: Navigate to /pharmacy. Page displays all active drugs in DataTable with name, total stock, batch count, selling price, min stock level columns. Acetazolamide shows stock=100, price=120000, minStock=10. Low stock and expiry alert banners are visible.
result: pass
note: Playwright-tested. DataTable shows all columns correctly. Acetazolamide displays stock=100, price=120,000d, minStock=10, status "Binh thuong". Expiry alert banner with 30/60/90 day toggles. Low stock alert banner visible.

### 17. Drug Batch Expand UI
expected: Click expand button on Acetazolamide row. Nested table shows batch BATCH-UAT-001 with qty=100, expiry=2027-06-15, no expired/near-expiry flags. Batches ordered by expiry date (FEFO).
result: issue
reported: "Expand panel shows 'Khong co lo thuoc' (No batches) despite API returning batch data. Network log confirms no /inventory/{drugId}/batches API call is made when expanding. Frontend DrugInventoryDto has 'id' field but backend returns 'drugCatalogItemId' - field name mismatch causes drugId to be undefined, so useDrugBatches query never fires."
severity: major

### 18. Suppliers Page UI
expected: Navigate to /pharmacy/suppliers. DataTable shows suppliers with name, contact, phone, email columns. "UAT Test Supplier Updated" appears with updated details.
result: pass
note: Playwright-tested. Shows both suppliers with name, contact info, active status badges (green "Hoat dong"), edit and toggle action buttons.

### 19. Stock Import Page UI
expected: Navigate to /pharmacy/stock-import. Shows import history with our UAT-TEST-001 import. "Create Import" button opens form with supplier dropdown, invoice field, and drug line items.
result: pass
note: Playwright-tested. Dual tabs (Invoice import / History). Form has supplier dropdown, invoice number, date, notes, drug line items with search/batch/expiry/qty/price. Excel import button present. Add line button works.

### 20. OTC Sales Page UI
expected: Navigate to /pharmacy/otc-sales. Shows sales history with our UAT walk-in sale. "New Sale" button opens form with customer toggle, drug combobox with prices, reactive total calculation.
result: pass
note: Playwright-tested. Split layout: left=new sale form with customer toggle (Walk-in/Named), drug combobox, qty, price, reactive total (Tong cong), notes, confirm button. Right=history table with customer/date/drugs/total columns.

### 21. Dispensing Queue Page UI
expected: Navigate to /pharmacy/queue. Shows pending prescriptions list (may be empty). If prescriptions exist: patient name, prescription code, prescribed date, item count visible.
result: pass
note: Playwright-tested. Correct columns: Patient, Prescription Code, Date, Drugs, Expiry, Status. 30s auto-refresh indicator visible. Search bar present. Empty state "Khong co don thuoc nao dang cho".

### 22. Consumables Warehouse Page UI
expected: Navigate to /consumables. DataTable shows 12 consumable items with tracking mode badge, stock, min stock. Low stock alert banner visible for items with stock < minStockLevel.
result: pass
note: Playwright-tested. 12 items with VI/EN names, unit, tracking mode badges (Don gian/Theo lo), stock, min stock, status (OK/Sap het). Low stock alert banner "9 items". Action buttons (+add, adjust, edit). Stock values reflect UAT test data correctly.

### 23. Sidebar Navigation
expected: Sidebar shows Pharmacy section with sub-items: Drug Inventory, Dispensing Queue, Suppliers, Stock Import, OTC Sales. Consumables section visible. All links navigate correctly.
result: pass
note: Playwright-tested. Sidebar VI: Nha thuoc > Kho thuoc, Hang doi pha che, Nha cung cap, Nhap kho, Ban le khong don. Vat tu tieu hao as standalone link. EN: Pharmacy > Inventory, Dispensing Queue, Suppliers, Stock Import, OTC Sales. Consumables link works.

### 24. i18n EN/VI Toggle
expected: Language toggle switches all labels. Vietnamese shows correct diacritics. English shows translated labels.
result: issue
reported: "Vietnamese is fully translated and correct. English toggle works for sidebar, table headers, alert banners, search placeholders. But some elements stay Vietnamese in English mode: page subtitle 'Quan ly kho thuoc va theo doi ton kho', action buttons 'Quan ly nha cung cap' and 'Nhap kho'. Partial English translation."
severity: minor

## Summary

total: 24
passed: 22
issues: 2
pending: 0
skipped: 0

## Gaps

- truth: "Clicking expand on drug row shows batch details in nested table"
  status: resolved
  reason: "User reported: Expand panel shows 'Khong co lo thuoc' (No batches) despite API returning batch data. Network log confirms no /inventory/{drugId}/batches API call is made when expanding. Frontend DrugInventoryDto has 'id' field but backend returns 'drugCatalogItemId' - field name mismatch causes drugId to be undefined, so useDrugBatches query never fires."
  severity: major
  test: 17
  root_cause: "Frontend DrugInventoryDto.id does not match backend DrugInventoryDto.DrugCatalogItemId. The frontend uses drug.id to pass to DrugBatchTable which calls useDrugBatches(drugId). Since 'id' is undefined (backend sends 'drugCatalogItemId'), the query is disabled and no API call is made."
  artifacts:
    - path: "frontend/src/features/pharmacy/api/pharmacy-api.ts"
      issue: "DrugInventoryDto has 'id: string' but backend sends 'drugCatalogItemId'"
    - path: "frontend/src/features/pharmacy/components/DrugInventoryTable.tsx"
      issue: "Line 336 passes drug.id which is undefined"
    - path: "backend/src/Modules/Pharmacy/Pharmacy.Contracts/Dtos/DrugBatchDto.cs"
      issue: "DrugInventoryDto record has DrugCatalogItemId field (line 28)"
  missing:
    - "Rename DrugInventoryDto.id to drugCatalogItemId in frontend, or alias it in the API response"
  debug_session: ""

- truth: "English mode shows all labels in English"
  status: resolved
  reason: "User reported: Vietnamese is fully translated and correct. English toggle works for sidebar, table headers, alert banners, search placeholders. But some elements stay Vietnamese in English mode: page subtitle, action buttons 'Quan ly nha cung cap' and 'Nhap kho'. Partial English translation."
  severity: minor
  test: 24
  root_cause: "Missing translation keys in English locale file for pharmacy page subtitle and action buttons"
  artifacts:
    - path: "frontend/src/app/routes/_authenticated/pharmacy/index.tsx"
      issue: "Some strings may be hardcoded in Vietnamese or missing EN translation keys"
    - path: "frontend/public/locales/en/pharmacy.json"
      issue: "Missing translation keys for page subtitle and action buttons"
  missing:
    - "Add missing English translation keys for pharmacy page subtitle and action buttons"
  debug_session: ""
