---
phase: 06-pharmacy-consumables
plan: 05b
type: execute
wave: 2
depends_on:
  - 06-03
  - 06-04
files_modified:
  - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DispensingRecordConfiguration.cs
  - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/OtcSaleConfiguration.cs
  - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/ConsumableItemConfiguration.cs
  - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/StockAdjustmentConfiguration.cs
autonomous: true
requirements:
  - PHR-05
  - PHR-06
  - CON-01
  - CON-02
must_haves:
  truths:
    - "EF Core configurations exist for DispensingRecord/Lines/BatchDeductions, OtcSale/Lines, ConsumableItem/Batches, StockAdjustment"
    - "Backing field navigation for _lines and _batchDeductions properly configured"
    - "ConsumableBatch has RowVersion for concurrent stock operations"
  artifacts:
    - path: "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DispensingRecordConfiguration.cs"
      provides: "EF config for dispensing hierarchy"
    - path: "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/OtcSaleConfiguration.cs"
      provides: "EF config for OTC sale hierarchy"
    - path: "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/ConsumableItemConfiguration.cs"
      provides: "EF config for ConsumableItem and ConsumableBatch"
    - path: "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/StockAdjustmentConfiguration.cs"
      provides: "EF config for StockAdjustment with dual nullable FKs"
  key_links:
    - from: "DispensingRecordConfiguration"
      to: "DispensingLine/BatchDeduction"
      via: "HasMany WithOne cascading"
      pattern: "HasMany.*Lines.*WithOne"
---

<objective>
Create EF Core configurations for dispensing, OTC sale, consumable, and stock adjustment entities.

Purpose: Complete the database schema for all remaining Phase 6 entities.
Output: 4 new configuration files covering 8 entities total.
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/phases/06-pharmacy-consumables/06-RESEARCH.md
</context>

<tasks>

<task type="auto">
  <name>Task 1: Create DispensingRecord and OtcSale EF configurations</name>
  <files>
    backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DispensingRecordConfiguration.cs,
    backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/OtcSaleConfiguration.cs
  </files>
  <action>
    **DispensingRecordConfiguration.cs** (covers DispensingRecord + DispensingLine + BatchDeduction):
    - DispensingRecord table: "DispensingRecords", HasKey(Id)
    - PrescriptionId, VisitId, PatientId: IsRequired
    - PatientName: IsRequired, MaxLength(200)
    - DispensedById: IsRequired
    - DispensedAt: IsRequired
    - OverrideReason: MaxLength(500)
    - BranchId: HasConversion
    - Navigation _lines: UsePropertyAccessMode(PropertyAccessMode.Field)
    - HasMany(Lines).WithOne().HasForeignKey(l => l.DispensingRecordId)

    DispensingLine (configure via OwnsMany or separate entity config):
    - Table: "DispensingLines", HasKey(Id)
    - DrugName: MaxLength(200)
    - Status: HasConversion<int>()
    - Navigation _batchDeductions: UsePropertyAccessMode(PropertyAccessMode.Field)
    - HasMany(BatchDeductions).WithOne().HasForeignKey(bd => bd.DispensingLineId)

    BatchDeduction:
    - Table: "BatchDeductions", HasKey(Id)
    - DispensingLineId: nullable
    - OtcSaleLineId: nullable
    - DrugBatchId: IsRequired
    - BatchNumber: MaxLength(100)
    - Quantity: IsRequired

    **OtcSaleConfiguration.cs** (covers OtcSale + OtcSaleLine):
    - OtcSale table: "OtcSales", HasKey(Id)
    - PatientId: nullable
    - CustomerName: MaxLength(200)
    - SoldById: IsRequired
    - SoldAt: IsRequired
    - Notes: MaxLength(1000)
    - BranchId: HasConversion
    - Navigation _lines: UsePropertyAccessMode(PropertyAccessMode.Field)

    OtcSaleLine:
    - Table: "OtcSaleLines", HasKey(Id)
    - DrugName: MaxLength(200)
    - UnitPrice: HasPrecision(18, 2)
    - Navigation _batchDeductions: UsePropertyAccessMode(PropertyAccessMode.Field)
    - HasMany(BatchDeductions).WithOne().HasForeignKey(bd => bd.OtcSaleLineId)
  </action>
  <verify>
    <automated>dotnet build backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj --no-restore -v q</automated>
  </verify>
  <done>DispensingRecord and OtcSale hierarchies fully configured with backing field access and cascading relationships.</done>
</task>

<task type="auto">
  <name>Task 2: Create ConsumableItem and StockAdjustment EF configurations</name>
  <files>
    backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/ConsumableItemConfiguration.cs,
    backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/StockAdjustmentConfiguration.cs
  </files>
  <action>
    **ConsumableItemConfiguration.cs** (covers ConsumableItem + ConsumableBatch):
    - ConsumableItem table: "ConsumableItems", HasKey(Id)
    - Name: IsRequired, MaxLength(200), UseCollation("Vietnamese_CI_AI")
    - NameVi: IsRequired, MaxLength(200), UseCollation("Vietnamese_CI_AI")
    - Unit: IsRequired, MaxLength(50)
    - TrackingMode: IsRequired, HasConversion<int>()
    - CurrentStock: IsRequired, HasDefaultValue(0)
    - MinStockLevel: IsRequired, HasDefaultValue(0)
    - IsActive: IsRequired, HasDefaultValue(true)
    - BranchId: HasConversion
    - Index on Name

    ConsumableBatch:
    - Table: "ConsumableBatches", HasKey(Id)
    - ConsumableItemId: IsRequired
    - BatchNumber: IsRequired, MaxLength(100)
    - ExpiryDate: IsRequired (DateOnly)
    - InitialQuantity: IsRequired
    - CurrentQuantity: IsRequired
    - RowVersion: IsRowVersion()
    - HasOne<ConsumableItem>().WithMany().HasForeignKey(cb => cb.ConsumableItemId)
    - Index on (ConsumableItemId, ExpiryDate)

    **StockAdjustmentConfiguration.cs**:
    - Table: "StockAdjustments", HasKey(Id)
    - DrugBatchId: nullable
    - ConsumableBatchId: nullable
    - QuantityChange: IsRequired
    - Reason: IsRequired, HasConversion<int>()
    - Notes: MaxLength(1000)
    - AdjustedById: IsRequired
    - AdjustedAt: IsRequired
  </action>
  <verify>
    <automated>dotnet build backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj --no-restore -v q</automated>
  </verify>
  <done>ConsumableItem/Batch and StockAdjustment configurations exist. ConsumableBatch has RowVersion.</done>
</task>

</tasks>

<verification>
All EF configurations compile. Full schema coverage for Phase 6 entities.
</verification>

<success_criteria>
- 4 configuration files covering 8 entities
- Backing field access configured for all collection navigations
- RowVersion on ConsumableBatch for concurrency
- `dotnet build Pharmacy.Infrastructure` succeeds
</success_criteria>

<output>
After completion, create `.planning/phases/06-pharmacy-consumables/06-05b-SUMMARY.md`
</output>
