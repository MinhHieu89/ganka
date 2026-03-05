---
phase: 06-pharmacy-consumables
plan: 05a
type: execute
wave: 2
depends_on:
  - 06-01
  - 06-02
files_modified:
  - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/SupplierConfiguration.cs
  - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/SupplierDrugPriceConfiguration.cs
  - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugBatchConfiguration.cs
  - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/StockImportConfiguration.cs
  - backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugCatalogItemConfiguration.cs
autonomous: true
requirements:
  - PHR-01
  - PHR-02
must_haves:
  truths:
    - "EF Core configurations exist for Supplier, SupplierDrugPrice, DrugBatch, StockImport with proper table names"
    - "DrugBatch has RowVersion concurrency token configured"
    - "DrugCatalogItem configuration updated with SellingPrice and MinStockLevel columns"
    - "Vietnamese_CI_AI collation on searchable name fields"
  artifacts:
    - path: "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/SupplierConfiguration.cs"
      provides: "EF Core config for Suppliers table"
    - path: "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugBatchConfiguration.cs"
      provides: "EF Core config for DrugBatches table with RowVersion"
    - path: "backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugCatalogItemConfiguration.cs"
      provides: "Updated config with SellingPrice/MinStockLevel columns"
  key_links:
    - from: "DrugBatchConfiguration"
      to: "DrugBatch entity"
      via: "IEntityTypeConfiguration"
      pattern: "IEntityTypeConfiguration<DrugBatch>"
---

<objective>
Create EF Core configurations for pharmacy stock entities (Supplier, SupplierDrugPrice, DrugBatch, StockImport/Lines) and update DrugCatalogItem configuration.

Purpose: Database schema definition for all inventory-related entities. RowVersion on DrugBatch prevents concurrent dispensing race conditions.
Output: 4 new configuration files + 1 updated configuration file.
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/phases/06-pharmacy-consumables/06-RESEARCH.md

<interfaces>
From DrugCatalogItemConfiguration.cs (existing pattern):
```csharp
public class DrugCatalogItemConfiguration : IEntityTypeConfiguration<DrugCatalogItem>
{
    public void Configure(EntityTypeBuilder<DrugCatalogItem> builder)
    {
        builder.ToTable("DrugCatalogItems");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200).UseCollation("Vietnamese_CI_AI");
        builder.Property(d => d.BranchId).HasConversion(b => b.Value, v => new BranchId(v));
        // ... etc
    }
}
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Create Supplier, SupplierDrugPrice, DrugBatch EF configurations</name>
  <files>
    backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/SupplierConfiguration.cs,
    backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/SupplierDrugPriceConfiguration.cs,
    backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugBatchConfiguration.cs
  </files>
  <action>
    Follow exact pattern from DrugCatalogItemConfiguration:

    **SupplierConfiguration.cs**:
    - Table: "Suppliers", HasKey(Id)
    - Name: IsRequired, MaxLength(200), UseCollation("Vietnamese_CI_AI")
    - ContactInfo: MaxLength(500)
    - Phone: MaxLength(20)
    - Email: MaxLength(200)
    - IsActive: IsRequired, HasDefaultValue(true)
    - BranchId: HasConversion (BranchId value object)
    - Index on Name

    **SupplierDrugPriceConfiguration.cs**:
    - Table: "SupplierDrugPrices", HasKey(Id)
    - SupplierId: IsRequired
    - DrugCatalogItemId: IsRequired
    - DefaultPurchasePrice: IsRequired, HasPrecision(18, 2)
    - Unique index on (SupplierId, DrugCatalogItemId)

    **DrugBatchConfiguration.cs**:
    - Table: "DrugBatches", HasKey(Id)
    - DrugCatalogItemId: IsRequired
    - SupplierId: IsRequired
    - BatchNumber: IsRequired, MaxLength(100)
    - ExpiryDate: IsRequired (DateOnly maps natively in EF Core 10)
    - InitialQuantity: IsRequired
    - CurrentQuantity: IsRequired
    - PurchasePrice: IsRequired, HasPrecision(18, 2)
    - StockImportId: nullable
    - RowVersion: IsRowVersion() for optimistic concurrency
    - Index on (DrugCatalogItemId, ExpiryDate) for FEFO queries
    - Index on ExpiryDate for expiry alert queries
  </action>
  <verify>
    <automated>dotnet build backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj --no-restore -v q</automated>
  </verify>
  <done>Supplier, SupplierDrugPrice, DrugBatch configurations exist with proper constraints, indexes, and RowVersion on DrugBatch.</done>
</task>

<task type="auto">
  <name>Task 2: Create StockImport configuration and update DrugCatalogItem configuration</name>
  <files>
    backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/StockImportConfiguration.cs,
    backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Configurations/DrugCatalogItemConfiguration.cs
  </files>
  <action>
    **StockImportConfiguration.cs** (covers both StockImport and StockImportLine):
    - StockImport table: "StockImports", HasKey(Id)
    - SupplierId: IsRequired
    - SupplierName: IsRequired, MaxLength(200)
    - ImportSource: IsRequired, HasConversion<int>()
    - InvoiceNumber: MaxLength(100)
    - ImportedById: IsRequired
    - ImportedAt: IsRequired
    - Notes: MaxLength(1000)
    - BranchId: HasConversion
    - Configure _lines backing field: builder.Navigation(e => e.Lines).UsePropertyAccessMode(PropertyAccessMode.Field)
    - HasMany(e => e.Lines).WithOne().HasForeignKey(l => l.StockImportId)

    Create separate StockImportLineConfiguration or configure inline:
    - StockImportLine table: "StockImportLines", HasKey(Id)
    - All string props: MaxLength constraints
    - ExpiryDate: IsRequired (DateOnly)
    - PurchasePrice: HasPrecision(18, 2)

    **Update DrugCatalogItemConfiguration.cs**:
    - Add: builder.Property(d => d.SellingPrice).HasPrecision(18, 2); (nullable decimal)
    - Add: builder.Property(d => d.MinStockLevel).IsRequired().HasDefaultValue(0);
  </action>
  <verify>
    <automated>dotnet build backend/src/Modules/Pharmacy/Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj --no-restore -v q</automated>
  </verify>
  <done>StockImport/Line configurations exist. DrugCatalogItem now maps SellingPrice and MinStockLevel columns.</done>
</task>

</tasks>

<verification>
All EF configurations compile. DrugBatch has RowVersion. DrugCatalogItem has new pricing columns.
</verification>

<success_criteria>
- 4 new configuration files + 1 updated file
- RowVersion on DrugBatch for concurrency control
- FEFO-optimized index on (DrugCatalogItemId, ExpiryDate)
- `dotnet build Pharmacy.Infrastructure` succeeds
</success_criteria>

<output>
After completion, create `.planning/phases/06-pharmacy-consumables/06-05a-SUMMARY.md`
</output>
