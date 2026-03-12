---
status: complete
phase: 06-pharmacy-consumables
source: 06-01 through 06-31 SUMMARY.md
started: 2026-03-10T10:00:00Z
updated: 2026-03-12T04:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Cold Start Smoke Test
expected: Kill any running server/service. Start backend from scratch. Server boots without errors, migrations run, ConsumableCatalogSeeder populates 12 consumable items, and primary endpoints respond: GET /api/inventory, GET /api/consumables, GET /api/dispensing/pending, GET /api/suppliers, GET /api/stock-imports, GET /api/otc-sales all return 200.
result: pass

### 2. Create and Manage Suppliers
expected: Navigate to /pharmacy/suppliers. Create supplier with name/contact info. View list of active suppliers. Edit supplier details. Toggle supplier active/inactive status.
result: pass
note: Re-tested 2026-03-12 after fix (06-30). Toggle now persists via dedicated PATCH /toggle-active endpoint.

### 3. Import Stock via Supplier Invoice
expected: Navigate to /pharmacy/stock-import. Create stock import with invoice number, add line items (drug, batch number, expiry date, quantity, purchase price). Import saves and appears in history.
result: pass
note: Multiple bugs found and fixed inline (CSV template, BOM stripping, preview button, drug combobox, template header)

### 4. Import Stock via Excel Bulk Upload
expected: On stock import page, upload Excel file with drug data. See preview of valid/invalid rows with specific error messages. Confirm import to apply valid rows as stock batches.
result: pass

### 5. View Drug Inventory with Batch Details
expected: Navigate to /pharmacy. Drug catalog shows total stock, batch count, selling price, min stock level columns. Expand a drug row to see FEFO-sorted batch list (batch number, expiry date, quantity).
result: pass
note: Bugs found and fixed inline (batchCount column added, supplier name in batch detail fixed)

### 6. Expiry Alert Banner
expected: On /pharmacy page, collapsible banner shows drugs expiring within configurable threshold (30/60/90 day toggles). Alert count badge visible. Shows drug name, earliest expiry date, days remaining.
result: pass

### 7. Low Stock Alert Banner (Drugs)
expected: On /pharmacy page, collapsible banner shows drugs below minimum stock level. Alert count badge visible. Shows drug name, current total stock, minimum level threshold.
result: pass

### 8. Dispense Drugs via Prescription Queue
expected: Navigate to /pharmacy/queue. Pending prescriptions listed sorted oldest first with patient name/code/item count. Click to open dispensing dialog with FEFO batch suggestions. Confirm dispense deducts stock from earliest-expiry batches.
result: pass
note: Fixed prescriptionId field name mismatch (frontend DTO had "id" but backend returns "prescriptionId"). Fixed stale batch data after dispensing (added inventory/batch/alert query invalidation to useDispenseDrugs).

### 9. Override Expired Prescription
expected: When dispensing an expired prescription (>7 days old), destructive red alert appears requiring override reason. Enter reason text, dispense succeeds with reason recorded.
result: pass

### 10. Skip Prescription Line Items
expected: In dispensing dialog, user can skip individual drug lines (out of stock, patient refusal). Off-catalog items auto-skipped. No stock deduction for skipped lines.
result: pass

### 11. Process Walk-in OTC Sales
expected: Navigate to /pharmacy/otc-sales. Create OTC sale for anonymous walk-in or named customer. Select drugs with auto-filled prices, adjust quantities. Reactive total calculation updates live. Confirm sale deducts stock via FEFO.
result: pass
note: Fixed DrugCombobox button missing type="button" (caused form auto-submit). Fixed "Required" error persisting after drug selection (added shouldValidate to setValue).

### 12. Adjust Drug Stock Manually
expected: On drug inventory, open stock adjustment for a drug batch. Select add/subtract, choose reason (Correction/WriteOff/Damage/Expired/Other), see before/after preview. Confirm saves with audit trail.
result: pass
note: Wired StockAdjustmentDialog into DrugBatchTable with adjust button per batch row. Fixed API field names (drugBatchId/quantityChange to match backend).

### 13. View Dispensing History
expected: Dispensing history shows paginated list of past records (date, dispensed by, item count). Can filter by patient. Expand to see drug names and quantities dispensed.
result: pass
note: Re-tested 2026-03-12 after fix (06-31). Global dispensing history page at /pharmacy/dispensing-history with sidebar link.

### 14. View OTC Sales History
expected: On /pharmacy/otc-sales, sales history shows paginated list (customer name with anonymous badge for walk-ins, date, item count, total amount) in reverse chronological order.
result: pass
note: Fixed OtcSaleDto field mapping (soldAt not saleDate, compute totals from lines). Fixed paged result unwrapping. Added expandable row detail view.

### 15. Update Drug Selling Price
expected: On drug inventory, edit selling price for a drug. Price update applies immediately to future OTC sales.
result: pass

### 16. Create and Manage Consumable Items
expected: Navigate to /consumables. View all active items with tracking mode badges (SimpleStock/ExpiryTracked). Can create new item with name, unit, tracking mode, min stock level. Can edit existing items.
result: pass

### 17. Add Consumable Stock (SimpleStock)
expected: For SimpleStock consumable, add stock by entering quantity only. Stock increments, IsLowStock flag updates.
result: pass
note: Fixed ConsumableTrackingMode enum values swapped in 3 frontend files (ExpiryTracked=0, SimpleStock=1 to match backend).

### 18. Add Consumable Stock (ExpiryTracked)
expected: For ExpiryTracked consumable, add stock with batch number and expiry date. New batch created, stock increments.
result: pass

### 19. Adjust Consumable Stock
expected: Adjust consumable batch quantity, select reason (Correction/WriteOff/Damage/Expired/Other), see before/after preview, saves with audit trail.
result: pass
note: Fixed field name mismatch (quantityChange/consumableBatchId). Added batch selector for ExpiryTracked items. Added GET /api/consumables/{id}/batches endpoint.

### 20. Consumable Low Stock Alerts
expected: On /consumables, collapsible banner shows items below minimum stock level (both tracking modes). Item count badge visible. Shows item name, current vs minimum levels.
result: pass
note: Fixed alert banner to show Vietnamese names via nameVi.

### 21. Prescriptions in Patient Profile
expected: Open patient profile, see Prescriptions tab with pending prescriptions and collapsible dispensing history expanded by default. Clickable rows show drug details with unit. 'View full queue' link navigates to pharmacy queue page. Vietnamese label shows "cấp phát" terminology.
result: pass

### 22. Pending Prescription Count Badge
expected: Pharmacy sidebar shows badge with count of pending (non-expired) prescriptions. Updates periodically via auto-refresh polling.
result: pass
note: Fixed black dot on hover (moved badge inline instead of SidebarMenuBadge). Fixed badge not updating (added sign-off invalidation). Fixed backend to only show prescriptions from Signed/Amended visits (not Draft).

### 23. Bilingual UI (English/Vietnamese)
expected: All pharmacy and consumables pages display correctly in both English and Vietnamese with proper diacritics. Sidebar nav shows localized labels. Language toggle switches all visible text.
result: pass
note: Added breadcrumb i18n mappings for all route segments (pharmacy, consumables, optical, billing, treatments, clinical). Removed duplicate user button from page header (already in sidebar).

### 24. Sidebar Navigation Structure
expected: Sidebar shows Pharmacy section with sub-items: Drug Inventory, Dispensing Queue, Suppliers, Stock Import, OTC Sales. Consumables section visible as standalone link. All links navigate correctly.
result: pass

## Summary

total: 24
passed: 24
issues: 0
pending: 0
skipped: 0

## Gaps

[none — all gaps closed on re-test 2026-03-12]
