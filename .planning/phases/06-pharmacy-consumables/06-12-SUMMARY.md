---
phase: 06-pharmacy-consumables
plan: 12
subsystem: pharmacy-stock-import
tags: [tdd, stock-import, excel-import, mini-excel, cpm, handlers]
dependency_graph:
  requires: [06-08, 06-10]
  provides: [CreateStockImport, ImportStockFromExcel, GetStockImports]
  affects: [Pharmacy.Application, Pharmacy.Unit.Tests]
tech_stack:
  added: [MiniExcel 1.42.0]
  patterns: [TDD red-green, Wolverine static handler, vertical slice, FluentValidation, streaming Excel parse]
key_files:
  created:
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/StockImport/CreateStockImport.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/StockImport/ImportStockFromExcel.cs
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Features/StockImport/GetStockImports.cs
    - backend/tests/Pharmacy.Unit.Tests/Features/StockImportHandlerTests.cs
  modified:
    - backend/Directory.Packages.props
    - backend/src/Modules/Pharmacy/Pharmacy.Application/Pharmacy.Application.csproj
decisions:
  - "MiniExcel package name is 'MiniExcel' (not 'MiniExcelLibs') on NuGet.org; Query<T> hasHeader parameter matches handler rows"
  - "ImportStockFromExcel returns ExcelImportPreview (not fail-fast) -- valid lines + all errors for user confirmation before applying"
  - "DrugCatalogItemDto.Id (not DrugCatalogItemId) used for drug catalog item identifier in match step"
  - "MiniExcel.Query<T>(stream, hasHeader: true) is the correct static API (not extension method with useHeaderRow)"
metrics:
  duration: 6min
  completed: 2026-03-06
  tasks_completed: 1
  files_created: 4
  files_modified: 2
---

# Phase 06 Plan 12: Stock Import Handlers Summary

**One-liner:** Stock import handlers (supplier invoice + Excel bulk) using MiniExcel streaming with row-level validation preview, all tests passing (8 new + 47 total).

## What Was Built

### CreateStockImport Handler
- `CreateStockImportCommand(SupplierId, InvoiceNumber?, Notes?, Lines)` with `StockImportLineInput` for each line
- Validates via FluentValidation: SupplierId required, Lines not empty, each line has BatchNumber, Quantity > 0, ExpiryDate in future, PurchasePrice >= 0
- Verifies supplier exists via `ISupplierRepository.GetByIdAsync`
- Creates `StockImport` aggregate (sets ImportSource = SupplierInvoice)
- For each line: verifies drug exists, calls `stockImport.AddLine()`, creates `DrugBatch.Create()` linked to StockImport.Id
- All saved in one `SaveChangesAsync` call

### ImportStockFromExcel Handler
- `ImportStockFromExcelCommand(Stream, SupplierId)` parsed via `MiniExcel.Query<StockImportRow>(stream, hasHeader: true)`
- Expected Excel columns: DrugName, BatchNumber, ExpiryDate, Quantity, PurchasePrice
- Non-fail-fast validation: collects ALL row errors before returning
- Date parsing supports yyyy-MM-dd, dd/MM/yyyy, MM/dd/yyyy formats
- Drug matching via `SearchAsync` with exact name/NameVi comparison
- Returns `ExcelImportPreview { ValidLines, Errors }` for user confirmation step
- Handler does NOT persist -- confirmation step uses `CreateStockImportCommand`

### GetStockImports Handler
- `GetStockImportsQuery(Page, PageSize)` with sane defaults (page=1, pageSize=20, max=100)
- Returns `PagedStockImportsResult { Items, TotalCount, Page, PageSize, TotalPages }`

## TDD Cycle

**RED phase:** Tests written first against non-existent handlers → build failed with CS0234/CS0246 errors (expected)

**GREEN phase:** Implemented all 3 handlers → 8/8 tests pass, 47/47 total Pharmacy.Unit.Tests pass

## Tests Written (8)

| Test | Handler | Scenario |
|------|---------|----------|
| CreateStockImport_ValidInvoice_CreatesStockImportAndBatches | CreateStockImport | Happy path |
| CreateStockImport_InvalidData_ReturnsValidationErrors | CreateStockImport | Validation failure |
| CreateStockImport_FutureExpiryRequired_RejectsExpiredDates | CreateStockImport | Past expiry date |
| CreateStockImport_SupplierNotFound_ReturnsNotFoundError | CreateStockImport | Supplier not found |
| CreateStockImport_MultipleLines_CreatesMultipleBatches | CreateStockImport | Multi-line import |
| ImportStockFromExcel_ValidData_ReturnsPreviewLines | ImportStockFromExcel | Valid Excel file |
| ImportStockFromExcel_InvalidRows_CollectsAllErrors | ImportStockFromExcel | Invalid rows collected |
| GetStockImports_ReturnsPaginated | GetStockImports | Pagination result |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] MiniExcel package name correction**
- **Found during:** Task 1 (CPM setup)
- **Issue:** Research docs referenced `MiniExcelLibs` but the actual NuGet package name is `MiniExcel`
- **Fix:** Updated CPM entry from `MiniExcelLibs` to `MiniExcel` in Directory.Packages.props
- **Files modified:** backend/Directory.Packages.props, backend/src/Modules/Pharmacy/Pharmacy.Application/Pharmacy.Application.csproj
- **Commit:** 9d1b0a1

**2. [Rule 1 - Bug] MiniExcel Query API parameter name**
- **Found during:** Task 1 (handler implementation)
- **Issue:** Reflection showed `Query<T>(Stream, sheetName, excelType, startCell, configuration, hasHeader)` -- parameter is `hasHeader` not `useHeaderRow`
- **Fix:** Updated handler to use `MiniExcel.Query<StockImportRow>(stream, hasHeader: true)` (static class call)
- **Files modified:** ImportStockFromExcel.cs

**3. [Rule 1 - Bug] DrugCatalogItemDto property name**
- **Found during:** Task 1 (test + handler implementation)
- **Issue:** DrugCatalogItemDto uses `Id` (not `DrugCatalogItemId`) for the drug identifier
- **Fix:** Updated both handler (`exactMatch.Id`) and test (correct positional args) to use `Id`
- **Files modified:** ImportStockFromExcel.cs, StockImportHandlerTests.cs

## Self-Check: PASSED

All created files exist and commit 9d1b0a1 confirmed in git history.
