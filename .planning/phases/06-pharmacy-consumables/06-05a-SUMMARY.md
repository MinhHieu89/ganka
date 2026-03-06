---
phase: 06-pharmacy-consumables
plan: 05a
subsystem: pharmacy-infrastructure
tags: [ef-core, configurations, database-schema, pharmacy, concurrency]
dependency_graph:
  requires: [06-01, 06-02]
  provides: [DrugBatches table, Suppliers table, SupplierDrugPrices table, StockImports table, StockImportLines table, SellingPrice/MinStockLevel on DrugCatalogItems]
  affects: [PharmacyDbContext, DrugCatalogItemConfiguration, pharmacy schema]
tech_stack:
  added: []
  patterns: [IEntityTypeConfiguration, IsRowVersion concurrency, Vietnamese_CI_AI collation, HasPrecision(18,2), composite index for FEFO, PropertyAccessMode.Field, EF Core migration]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/SupplierConfiguration.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/SupplierDrugPriceConfiguration.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugBatchConfiguration.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/StockImportConfiguration.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Migrations/20260306065900_AddPharmacyStockEntities.cs
  modified:
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugCatalogItemConfiguration.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/PharmacyDbContext.cs
decisions:
  - "DrugBatchConfiguration uses IsRowVersion() for optimistic concurrency -- prevents concurrent dispensing double-deduction race conditions"
  - "FEFO composite index on (DrugCatalogItemId, ExpiryDate) -- primary query pattern for FEFO batch selection"
  - "StockImportConfiguration and StockImportLineConfiguration in single file -- colocation reflects parent-child relationship"
  - "DrugCatalogItem SellingPrice is nullable decimal -- null until pricing is configured via UpdatePricing"
  - "MinStockLevel HasDefaultValue(0) -- no alert by default; explicit configuration required"
  - "Migration applied to database -- schema updated in one migration covering all 5 new tables + 2 new columns"
metrics:
  duration: 3min
  completed_date: 2026-03-06
  tasks_completed: 2
  files_created: 5
  files_modified: 2
---

# Phase 06 Plan 05a: Pharmacy EF Core Configurations Summary

**One-liner:** EF Core entity configurations for Supplier, SupplierDrugPrice, DrugBatch with RowVersion concurrency, and StockImport/Line tables, plus SellingPrice/MinStockLevel columns on DrugCatalogItem — all applied via a single migration.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Create Supplier, SupplierDrugPrice, DrugBatch EF configurations | 889a82e | SupplierConfiguration.cs, SupplierDrugPriceConfiguration.cs, DrugBatchConfiguration.cs, PharmacyDbContext.cs |
| 2 | Create StockImport configuration and update DrugCatalogItem configuration | dc21431 | StockImportConfiguration.cs, DrugCatalogItemConfiguration.cs, PharmacyDbContext.cs |
| Migration | Database schema update | 41d39ee | 20260306065900_AddPharmacyStockEntities.cs, PharmacyDbContextModelSnapshot.cs |

## Key Configurations Implemented

### SupplierConfiguration
- Table: `Suppliers`, `HasKey(Id)`
- `Name`: `IsRequired`, `HasMaxLength(200)`, `UseCollation("Vietnamese_CI_AI")`
- `ContactInfo`: `HasMaxLength(500)` (nullable)
- `Phone`: `HasMaxLength(20)` (nullable)
- `Email`: `HasMaxLength(200)` (nullable)
- `IsActive`: `IsRequired`, `HasDefaultValue(true)`
- `BranchId`: value object conversion `BranchId <-> Guid`
- Index on `Name` for search performance

### SupplierDrugPriceConfiguration
- Table: `SupplierDrugPrices`, `HasKey(Id)`
- `SupplierId`, `DrugCatalogItemId`: `IsRequired`
- `DefaultPurchasePrice`: `IsRequired`, `HasPrecision(18, 2)`
- Unique composite index on `(SupplierId, DrugCatalogItemId)`

### DrugBatchConfiguration
- Table: `DrugBatches`, `HasKey(Id)`
- `BatchNumber`: `IsRequired`, `HasMaxLength(100)`
- `ExpiryDate`: `IsRequired` (DateOnly — native mapping in EF Core 8+)
- `PurchasePrice`: `IsRequired`, `HasPrecision(18, 2)`
- `RowVersion`: `IsRowVersion()` — optimistic concurrency for concurrent dispensing
- FEFO index: `(DrugCatalogItemId, ExpiryDate)` — primary batch selection query
- Expiry alert index: `ExpiryDate` — threshold alert queries

### StockImportConfiguration
- Table: `StockImports`, `HasKey(Id)`
- `ImportSource`: `HasConversion<int>()`
- `InvoiceNumber`: `HasMaxLength(100)` (nullable)
- `Notes`: `HasMaxLength(1000)` (nullable)
- `BranchId`: value object conversion
- `Lines` navigation: `PropertyAccessMode.Field` for backing field `_lines`
- `HasMany(Lines).WithOne().HasForeignKey(l => l.StockImportId)` with cascade delete

### StockImportLineConfiguration (inline in same file)
- Table: `StockImportLines`, `HasKey(Id)`
- `DrugName`: `IsRequired`, `HasMaxLength(200)`
- `BatchNumber`: `IsRequired`, `HasMaxLength(100)`
- `ExpiryDate`: `IsRequired` (DateOnly)
- `PurchasePrice`: `IsRequired`, `HasPrecision(18, 2)`

### DrugCatalogItemConfiguration (updated)
- Added: `SellingPrice` with `HasPrecision(18, 2)` (nullable)
- Added: `MinStockLevel` with `IsRequired`, `HasDefaultValue(0)`

## Deviations from Plan

### Auto-added: Database Migration
- **Found during:** Post-task verification
- **Issue:** CLAUDE.md requires creating and running migrations when models change. The plan did not include a migration step.
- **Fix:** Created migration `20260306065900_AddPharmacyStockEntities` and applied it to the database via `dotnet ef database update`.
- **Files modified:** `Migrations/20260306065900_AddPharmacyStockEntities.cs`, `Migrations/20260306065900_AddPharmacyStockEntities.Designer.cs`, `Migrations/PharmacyDbContextModelSnapshot.cs`
- **Commit:** 41d39ee

## Verification

- `dotnet build backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj --no-restore -v q` — PASSED (0 errors, 0 warnings)
- EF Core migration created and applied to database successfully

## Self-Check: PASSED

Files created/modified verified:
- SupplierConfiguration.cs: FOUND
- SupplierDrugPriceConfiguration.cs: FOUND
- DrugBatchConfiguration.cs: FOUND
- StockImportConfiguration.cs: FOUND
- DrugCatalogItemConfiguration.cs: FOUND (updated)
- PharmacyDbContext.cs: FOUND (updated)
- Migration 20260306065900_AddPharmacyStockEntities.cs: FOUND

Commits verified:
- 889a82e: Task 1 — Supplier, SupplierDrugPrice, DrugBatch EF configurations
- dc21431: Task 2 — StockImport configuration + DrugCatalogItem pricing columns
- 41d39ee: Migration — database schema updated
