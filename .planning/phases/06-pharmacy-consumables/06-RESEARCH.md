# Phase 6: Pharmacy & Consumables - Research

**Researched:** 2026-03-05
**Domain:** Pharmacy inventory management, batch/FEFO dispensing, consumables warehouse, Excel import
**Confidence:** HIGH

## Summary

Phase 6 extends the existing Pharmacy module (scaffolded in Phase 5 with DrugCatalogItem entity) to add full inventory management with batch tracking, supplier relationships, FEFO dispensing against HIS prescriptions, walk-in OTC sales, and a separate consumables warehouse. The Pharmacy module already has Application, Contracts, Domain, and Infrastructure layers with a PharmacyDbContext using the "pharmacy" schema. It is already registered in the Bootstrapper for both DbContext and Wolverine handler discovery.

The core technical challenges are: (1) implementing FEFO batch selection logic that auto-suggests earliest-expiry batches while allowing manual override, (2) cross-module queries from Clinical.Contracts (DrugPrescription) into Pharmacy for the dispensing queue, (3) Excel bulk import requiring a new NuGet library (MiniExcel recommended), and (4) a Consumables entity model within the same Pharmacy module but presented as a separate UI section. The codebase has strong established patterns (vertical slice handlers, IHostedService seeders, DataTable component, React Hook Form + Zod) that Phase 6 should follow exactly.

**Primary recommendation:** Extend the existing Pharmacy module with new entities (Supplier, DrugBatch, DispensingRecord, OtcSale, ConsumableItem, ConsumableBatch, StockAdjustment). Add selling price and min stock level to DrugCatalogItem. Use MiniExcel for Excel import. Consumables live in Pharmacy module with separate API route group and frontend feature directory.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Both import methods: supplier invoice form for day-to-day, Excel bulk import for large orders and initial stock load
- Purchase price tracked per batch; selling price is per drug (single selling price on catalog)
- Supplier entity with name, contact info, plus default purchase price per drug per supplier
- Batch fields: batch number, expiry date, quantity, purchase price, supplier reference
- Phase 5 drug catalog extended with: selling price, min stock level, supplier-drug pricing
- Both queue + patient lookup: dedicated pharmacy queue page as primary, plus accessible from patient profile
- Queue page shows count badge in sidebar navigation for pending prescriptions
- Auto FEFO with manual override
- All-or-nothing per drug line (no partial dispensing)
- 7-day prescription expiry: warn but allow override with reason logged
- Dispensing creates a dispensing record linking prescription line to batch(es) used to quantities deducted
- Customer linkage is optional for OTC sales
- No payment collection in Phase 6 (deferred to Phase 7)
- No receipt/invoice generation in Phase 6 (deferred to Phase 7)
- OTC sales still auto-deduct stock via same batch/FEFO mechanism
- Expiry alerts at configurable thresholds (30/60/90 days) per PHR-03
- Min stock alerts when drug falls below configurable minimum level per drug per PHR-04
- Consumables warehouse: fully separate UI section with own sidebar nav item
- Configurable per consumable: "expiry-tracked" (batch model with FEFO) or "simple stock" (quantity-only)
- Seeded with 10-15 core IPL/LLLT supplies
- Manual stock management only in Phase 6 (auto-deduction deferred to Phase 9)
- Same min stock alert pattern as pharmacy drugs

### Claude's Discretion
- OTC sale data model approach (quick sale form vs mini-prescription pipeline)
- Alert presentation design (dashboard widget, toast notifications, sidebar badges)
- Excel import template format and validation rules
- Pharmacy queue page layout and filtering options
- Consumables seed data selection (specific items to include)
- Stock adjustment workflow (manual corrections, write-offs)
- Dispensing confirmation UI details
- Loading states and error handling

### Deferred Ideas (OUT OF SCOPE)
- Auto-deduction of consumables from treatment sessions -- deferred to Phase 9
- Payment collection for OTC sales -- deferred to Phase 7
- Receipt/invoice generation for any pharmacy transaction -- deferred to Phase 7
- Drug interaction checking -- explicitly out of scope for v1
- Controlled substance tracking -- not needed per PROJECT.md
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| PHR-01 | Staff can manage drug inventory with batch tracking and multiple suppliers | Supplier, DrugBatch, SupplierDrugPrice entities; DrugCatalogItem extended with sellingPrice/minStockLevel |
| PHR-02 | Staff can import stock via supplier invoice or Excel bulk import | Supplier invoice form (CreateStockImport handler); MiniExcel for Excel bulk import with template validation |
| PHR-03 | System tracks expiry dates and alerts at configurable thresholds (30/60/90 days) | DrugBatch.ExpiryDate + background query or dashboard component; threshold stored per-drug or global setting |
| PHR-04 | System alerts when drug stock falls below configurable minimum level per drug | DrugCatalogItem.MinStockLevel + computed current stock from batches; alert component |
| PHR-05 | Pharmacist can dispense drugs against HIS prescription with auto stock deduction | Cross-module query for pending prescriptions via Clinical.Contracts; DispensingRecord entity; FEFO batch selection |
| PHR-06 | Staff can process walk-in OTC sales without prescription | OtcSale entity with optional PatientId; same FEFO batch deduction mechanism |
| PHR-07 | System enforces 7-day prescription validity and warns on expired prescriptions | PrescribedAt + 7 days validation in dispensing handler; warning banner + confirmation dialog with reason |
| CON-01 | System maintains separate consumables warehouse independent from pharmacy stock | ConsumableItem + ConsumableBatch entities in Pharmacy schema; separate API route group and frontend section |
| CON-02 | Staff can manage treatment supplies inventory with stock levels and alerts | CRUD for ConsumableItem; stock add/remove/adjust; same min stock alert pattern |
| CON-03 | Consumable usage per treatment session auto-deducts from warehouse | OUT OF SCOPE for Phase 6 (deferred to Phase 9). Phase 6 only builds manual stock management. Entity design should support future auto-deduction |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| EF Core | 10.0.*-* | ORM for Pharmacy entities (existing CPM version) | Project standard, PharmacyDbContext already registered |
| FluentValidation | 12.* | Command validation (existing CPM version) | Project standard for all handlers |
| WolverineFx | 5.* | Message bus for command/query handlers | Project standard, Pharmacy.Application.Marker already registered |
| QuestPDF | 2025.* | PDF generation (pharmacy labels already exist) | Already used in Phase 5 for drug Rx printing |
| MiniExcel | latest | Excel import/export for bulk stock import | Free MIT license, low memory, streaming row-by-row (see rationale below) |

### Supporting (Frontend)
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| @tanstack/react-table | ^8.21.2 | DataTable for inventory lists, queue, batches | All list/table views |
| react-hook-form + zod | ^7.54.2 / ^3.24.2 | Form validation for import, dispensing, OTC | All form views |
| recharts | ^3.7.0 | Stock level charts, expiry timeline (if needed) | Dashboard/alert views |
| sonner | ^1.7.4 | Toast notifications for alerts | Low stock and expiry notifications |

### New Dependency: MiniExcel
| Instead of | Chosen | Rationale |
|------------|--------|-----------|
| ClosedXML | MiniExcel | Streaming row-by-row processing, 10x lower memory for large imports, MIT license, simpler API for read-only import use case |
| EPPlus | MiniExcel | EPPlus requires commercial license for commercial use since v5. MiniExcel is fully free |
| NPOI | MiniExcel | NPOI has larger API surface and higher memory usage. MiniExcel is more focused |

**Installation (backend):**
```bash
# Add to Directory.Packages.props
<PackageVersion Include="MiniExcelLibs" Version="1.*" />

# Add to Pharmacy.Application.csproj or Pharmacy.Infrastructure.csproj
<PackageReference Include="MiniExcelLibs" />
```

**Installation (frontend):**
No new frontend packages needed. All existing dependencies (shadcn/ui, TanStack Table, React Hook Form, Zod) are sufficient.

## Architecture Patterns

### Backend Entity Model

```
Pharmacy.Domain/
  Entities/
    DrugCatalogItem.cs          # EXTEND: add SellingPrice, MinStockLevel
    Supplier.cs                 # NEW: AggregateRoot
    SupplierDrugPrice.cs        # NEW: Entity (junction: Supplier <-> DrugCatalogItem)
    DrugBatch.cs                # NEW: Entity (child of DrugCatalogItem)
    StockImport.cs              # NEW: AggregateRoot (tracks each import event)
    StockImportLine.cs          # NEW: Entity (child of StockImport)
    DispensingRecord.cs         # NEW: AggregateRoot
    DispensingLine.cs           # NEW: Entity (child of DispensingRecord)
    BatchDeduction.cs           # NEW: Entity (links dispensing line -> batch)
    OtcSale.cs                  # NEW: AggregateRoot
    OtcSaleLine.cs              # NEW: Entity (child of OtcSale)
    ConsumableItem.cs           # NEW: AggregateRoot
    ConsumableBatch.cs          # NEW: Entity (child of ConsumableItem, for expiry-tracked)
    StockAdjustment.cs          # NEW: Entity (tracks manual adjustments for audit)
  Enums/
    DrugForm.cs                 # EXISTS
    DrugRoute.cs                # EXISTS
    DispensingStatus.cs         # NEW: Pending, Dispensed, Skipped
    ImportSource.cs             # NEW: SupplierInvoice, ExcelBulk
    StockAdjustmentReason.cs    # NEW: Correction, WriteOff, Damage, Expired, Other
    ConsumableTrackingMode.cs   # NEW: ExpiryTracked, SimpleStock
```

### Backend Feature Structure (Vertical Slices)

```
Pharmacy.Application/
  Features/
    DrugCatalog/
      UpdateDrugCatalogPricing.cs     # Extend with sellingPrice, minStockLevel
    Suppliers/
      CreateSupplier.cs
      UpdateSupplier.cs
      GetSuppliers.cs
      GetSupplierDrugPrices.cs
    StockImport/
      CreateStockImport.cs            # Supplier invoice form
      ImportStockFromExcel.cs         # Excel bulk import
      GetStockImports.cs
    Inventory/
      GetDrugInventory.cs             # List all drugs with current stock levels
      GetDrugBatches.cs               # Batches for a specific drug
      AdjustStock.cs                  # Manual stock adjustment
    Dispensing/
      GetPendingPrescriptions.cs      # Cross-module query to Clinical
      DispenseDrugs.cs                # FEFO auto-select + manual override
      GetDispensingHistory.cs
    OtcSales/
      CreateOtcSale.cs               # Walk-in OTC sale
      GetOtcSales.cs
    Alerts/
      GetExpiryAlerts.cs
      GetLowStockAlerts.cs
    Consumables/
      CreateConsumableItem.cs
      UpdateConsumableItem.cs
      GetConsumableItems.cs
      AddConsumableStock.cs
      RemoveConsumableStock.cs
      AdjustConsumableStock.cs
      GetConsumableAlerts.cs
  Interfaces/
    ISupplierRepository.cs
    IDrugBatchRepository.cs
    IDispensingRepository.cs
    IOtcSaleRepository.cs
    IConsumableRepository.cs
    IStockImportRepository.cs
```

### Frontend Feature Structure

```
frontend/src/
  features/
    pharmacy/
      api/
        pharmacy-api.ts              # All pharmacy API functions
        pharmacy-queries.ts          # TanStack Query key factories + hooks
      components/
        DrugInventoryTable.tsx        # Drug list with stock levels
        DrugBatchTable.tsx            # Batches for a drug
        SupplierForm.tsx              # Create/edit supplier
        StockImportForm.tsx           # Supplier invoice import
        ExcelImportDialog.tsx         # Excel bulk import with template download
        PharmacyQueueTable.tsx        # Pending prescriptions queue
        DispensingDialog.tsx          # Dispense confirmation with FEFO display
        OtcSaleForm.tsx              # Walk-in OTC sale form
        ExpiryAlertBanner.tsx         # Expiry warning banner
        LowStockAlertBanner.tsx       # Low stock warning banner
    consumables/
      api/
        consumables-api.ts
        consumables-queries.ts
      components/
        ConsumableItemTable.tsx
        ConsumableItemForm.tsx
        AddStockDialog.tsx
        StockAdjustmentDialog.tsx
        ConsumableAlertBanner.tsx
  app/routes/_authenticated/
    pharmacy/
      index.tsx                      # Drug inventory page (default)
      suppliers.tsx                  # Supplier management
      queue.tsx                      # Dispensing queue
      dispensing/$prescriptionId.tsx  # Dispensing detail
      otc-sales.tsx                  # OTC sales
      stock-import.tsx               # Stock import page
    consumables/
      index.tsx                      # Consumables inventory page
```

### Key Patterns from Existing Codebase

**Pattern 1: Vertical Slice Handler (Command)**
```csharp
// Source: Established pattern from Phase 1-5 (e.g., CreateVisitCommand)
public sealed record CreateSupplierCommand(
    string Name, string? ContactInfo, string? Phone, string? Email);

public static class CreateSupplierHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateSupplierCommand command,
        IValidator<CreateSupplierCommand> validator,
        ISupplierRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Result<Guid>.Failure(Error.ValidationWithDetails(
                "Validation failed",
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var supplier = Supplier.Create(command.Name, command.ContactInfo, ...);
        repository.Add(supplier);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<Guid>.Success(supplier.Id);
    }
}
```

**Pattern 2: Cross-Module Query via Contracts**
```csharp
// Source: SearchDrugCatalogQuery in Pharmacy.Contracts (exists)
// Clinical module already queries Pharmacy via IMessageBus
// Phase 6 needs the REVERSE: Pharmacy queries Clinical for pending prescriptions

// In Clinical.Contracts (extend):
public sealed record GetPendingPrescriptionsQuery(Guid? PatientId = null);
public sealed record PendingPrescriptionDto(
    Guid PrescriptionId, Guid VisitId, Guid PatientId,
    string PatientName, DateTime PrescribedAt,
    List<PendingPrescriptionItemDto> Items);

// Handler lives in Clinical.Application, invoked from Pharmacy frontend via Pharmacy API
// OR Pharmacy.Presentation calls bus.InvokeAsync<List<PendingPrescriptionDto>>(query)
```

**Pattern 3: FEFO Batch Selection**
```csharp
// FEFO: Order batches by ExpiryDate ascending, filter out expired & zero-quantity
// Domain service in Pharmacy.Domain or Application service
public static List<BatchAllocation> AllocateFEFO(
    IReadOnlyList<DrugBatch> availableBatches,
    int requiredQuantity)
{
    var allocations = new List<BatchAllocation>();
    var remaining = requiredQuantity;

    var ordered = availableBatches
        .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow)
        .OrderBy(b => b.ExpiryDate)
        .ToList();

    foreach (var batch in ordered)
    {
        if (remaining <= 0) break;
        var take = Math.Min(remaining, batch.CurrentQuantity);
        allocations.Add(new BatchAllocation(batch.Id, batch.BatchNumber, take, batch.ExpiryDate));
        remaining -= take;
    }

    if (remaining > 0)
        return []; // Insufficient stock

    return allocations;
}
```

**Pattern 4: IHostedService Seeder (Consumables)**
```csharp
// Source: AllergyCatalogSeeder in Patient.Infrastructure (exists)
// Mirror for ConsumableCatalogSeeder
public sealed class ConsumableCatalogSeeder : IHostedService
{
    // Same pattern: check if data exists, if not, seed
    // Seed 10-15 IPL/LLLT supplies with proper Vietnamese diacritics
}
```

### Anti-Patterns to Avoid
- **Cross-module joins:** Never join Pharmacy tables with Clinical tables directly. Use IMessageBus cross-module queries via Contracts DTOs.
- **Batch quantity mutation without audit:** Every stock change (import, dispensing, OTC, adjustment) MUST create an entity record for audit trail. Never directly UPDATE batch quantity without a linked record.
- **Negative stock:** Domain guard must prevent batch quantity from going below zero. Use domain method `DrugBatch.Deduct(int qty)` that throws if insufficient.
- **Direct DbContext in handler:** Always use repository + unit of work pattern per established codebase convention.
- **WolverineFx.Http in Application:** Pharmacy.Application.csproj currently has WolverineFx.Http reference -- this MUST be removed per established pattern (Application layer is HTTP-free). Replace with FluentValidation.DependencyInjectionExtensions.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Excel parsing | Custom CSV/XLSX parser | MiniExcel (MiniExcelLibs NuGet) | Excel format is complex (XML-based zip), edge cases with dates/decimals/encoding |
| FEFO allocation | Inline LINQ in every handler | Domain service `FEFOAllocator` | Reused by dispensing AND OTC sales; testable in isolation; batch concurrency |
| Batch quantity tracking | Manual SUM queries everywhere | Computed property or dedicated query | `CurrentQuantity` on DrugBatch entity, decremented by domain methods |
| Expiry/stock alerts | Custom polling service | Query-based alerts on page load | No need for background service; compute alerts on demand from batch data |
| PDF labels | Custom HTML-to-PDF | QuestPDF (already in project) | Phase 5 already has PharmacyLabelDocument pattern |
| Form validation | Manual if/else checks | FluentValidation + Zod (existing stack) | Consistent with all other modules |
| Data table | Custom table component | DataTable + TanStack Table (existing) | Already has sorting, pagination, filtering support |

**Key insight:** The biggest risk is hand-rolling batch quantity management. Every mutation must go through domain methods that enforce invariants (no negative stock, FEFO ordering, audit trail). This is not just CRUD -- it is inventory lifecycle management.

## Common Pitfalls

### Pitfall 1: Race Condition on Batch Deduction
**What goes wrong:** Two pharmacists dispense from the same batch simultaneously, both see quantity=10, both deduct 8, resulting in -6 stock.
**Why it happens:** No concurrency control on batch quantity updates.
**How to avoid:** Use RowVersion (optimistic concurrency) on DrugBatch entity. EF Core throws DbUpdateConcurrencyException on conflict. Handler retries or returns conflict error. Same pattern as Visit.RowVersion in Clinical module.
**Warning signs:** Test with concurrent dispensing operations.

### Pitfall 2: Orphaned Prescription References
**What goes wrong:** Dispensing references a prescription that was amended or deleted in Clinical module after the pharmacist loaded the queue.
**Why it happens:** Cross-module eventual consistency -- Clinical and Pharmacy are separate bounded contexts.
**How to avoid:** Re-fetch prescription status at dispense time (not just at queue load). Validate that prescription is still valid and items match. If prescription was amended, force queue refresh.
**Warning signs:** Stale data in dispensing dialog.

### Pitfall 3: Excel Import Validation Failures
**What goes wrong:** User uploads Excel with missing columns, wrong date format, negative quantities, or duplicate batch numbers.
**Why it happens:** Excel data is inherently unstructured and user-generated.
**How to avoid:** Strict validation pipeline: (1) validate template structure (column headers), (2) validate each row with FluentValidation, (3) return all errors at once (not fail-fast), (4) require confirmation before applying valid rows. Provide downloadable template.
**Warning signs:** First real user import attempt.

### Pitfall 4: FEFO Across Multiple Batches
**What goes wrong:** Single prescription line (qty=20) needs to pull from 3 different batches (5+8+7). Handler only pulls from first batch and fails or under-deducts.
**Why it happens:** Naive implementation assumes one batch per line.
**How to avoid:** FEFO allocator must return List<BatchAllocation> and dispensing must create multiple BatchDeduction records per line. Model explicitly supports multi-batch deduction via DispensingLine -> BatchDeduction (1:N).
**Warning signs:** Test with low-stock scenarios requiring multi-batch allocation.

### Pitfall 5: Timezone Issues with Expiry Dates
**What goes wrong:** Drug expiring "today" at UTC might still be valid in Vietnam timezone (UTC+7), or vice versa.
**Why it happens:** Expiry dates are typically date-only (no time), but DateTime.UtcNow includes time component.
**How to avoid:** Store ExpiryDate as DateOnly (not DateTime). Compare using DateOnly.FromDateTime with Vietnam timezone conversion. Use established cross-platform timezone pattern: SE Asia Standard Time (Windows) / Asia/Ho_Chi_Minh (Linux).
**Warning signs:** Drugs showing as expired that pharmacist insists are still valid.

### Pitfall 6: Pharmacy Module Missing Presentation Layer
**What goes wrong:** Pharmacy module has no Presentation project (Application, Contracts, Domain, Infrastructure only). Cannot add Minimal API endpoints.
**Why it happens:** Scaffolded modules omitted Presentation layer when not yet needed.
**How to avoid:** Create Pharmacy.Presentation project following exact pattern from Clinical.Presentation (MapPharmacyApiEndpoints, MapConsumablesApiEndpoints). Register in Bootstrapper.
**Warning signs:** Compilation error when trying to add endpoint mapping.

## Code Examples

### Entity: DrugBatch with FEFO Support
```csharp
// Domain entity with concurrency control and domain-guarded deduction
public class DrugBatch : Entity
{
    public Guid DrugCatalogItemId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string BatchNumber { get; private set; } = string.Empty;
    public DateOnly ExpiryDate { get; private set; }
    public int InitialQuantity { get; private set; }
    public int CurrentQuantity { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public Guid? StockImportId { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private DrugBatch() { }

    public static DrugBatch Create(
        Guid drugCatalogItemId, Guid supplierId, string batchNumber,
        DateOnly expiryDate, int quantity, decimal purchasePrice,
        Guid? stockImportId = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        if (expiryDate <= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Expiry date must be in the future.");

        return new DrugBatch
        {
            DrugCatalogItemId = drugCatalogItemId,
            SupplierId = supplierId,
            BatchNumber = batchNumber,
            ExpiryDate = expiryDate,
            InitialQuantity = quantity,
            CurrentQuantity = quantity,
            PurchasePrice = purchasePrice,
            StockImportId = stockImportId
        };
    }

    public void Deduct(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Deduction must be positive.");
        if (quantity > CurrentQuantity)
            throw new InvalidOperationException(
                $"Insufficient stock in batch {BatchNumber}. Available: {CurrentQuantity}, Requested: {quantity}");

        CurrentQuantity -= quantity;
        SetUpdatedAt();
    }

    public bool IsExpired => ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsNearExpiry(int daysThreshold) =>
        !IsExpired && ExpiryDate <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(daysThreshold);
}
```

### Entity: DispensingRecord
```csharp
public class DispensingRecord : AggregateRoot, IAuditable
{
    public Guid PrescriptionId { get; private set; }       // Clinical DrugPrescription.Id
    public Guid VisitId { get; private set; }
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = string.Empty;  // Denormalized
    public Guid DispensedById { get; private set; }
    public DateTime DispensedAt { get; private set; }
    public string? OverrideReason { get; private set; }    // If expired prescription override

    private readonly List<DispensingLine> _lines = [];
    public IReadOnlyCollection<DispensingLine> Lines => _lines.AsReadOnly();

    // Factory + AddLine methods following established pattern
}

public class DispensingLine : Entity
{
    public Guid DispensingRecordId { get; private set; }
    public Guid PrescriptionItemId { get; private set; }   // Clinical PrescriptionItem.Id
    public Guid DrugCatalogItemId { get; private set; }
    public string DrugName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public DispensingStatus Status { get; private set; }   // Dispensed or Skipped

    private readonly List<BatchDeduction> _batchDeductions = [];
    public IReadOnlyCollection<BatchDeduction> BatchDeductions => _batchDeductions.AsReadOnly();
}

public class BatchDeduction : Entity
{
    public Guid DispensingLineId { get; private set; }
    public Guid DrugBatchId { get; private set; }
    public string BatchNumber { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
}
```

### OTC Sale as Quick Sale (Claude's Discretion Decision)
```csharp
// Recommendation: Quick Sale form approach (NOT mini-prescription pipeline)
// Rationale: OTC sales don't need doctor involvement, prescription lifecycle,
// or 7-day validity. A simple sale record with line items is cleaner.
// This avoids polluting the Clinical prescription model with non-clinical data.

public class OtcSale : AggregateRoot, IAuditable
{
    public Guid? PatientId { get; private set; }          // Optional walk-in customer
    public string? CustomerName { get; private set; }     // For anonymous sales
    public Guid SoldById { get; private set; }
    public DateTime SoldAt { get; private set; }

    private readonly List<OtcSaleLine> _lines = [];
    public IReadOnlyCollection<OtcSaleLine> Lines => _lines.AsReadOnly();
}

public class OtcSaleLine : Entity
{
    public Guid OtcSaleId { get; private set; }
    public Guid DrugCatalogItemId { get; private set; }
    public string DrugName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }        // SellingPrice at time of sale

    private readonly List<BatchDeduction> _batchDeductions = [];
    public IReadOnlyCollection<BatchDeduction> BatchDeductions => _batchDeductions.AsReadOnly();
}
```

### Excel Import with MiniExcel
```csharp
// Source: MiniExcel GitHub documentation
// Streaming read - low memory usage for large files
using MiniExcelLibs;

public static async Task<Result<List<StockImportLineDto>>> ParseExcelImport(
    Stream fileStream, CancellationToken ct)
{
    var rows = MiniExcel.Query<StockImportRow>(fileStream).ToList();
    var errors = new List<string>();
    var validLines = new List<StockImportLineDto>();

    foreach (var (row, index) in rows.Select((r, i) => (r, i)))
    {
        // Validate each row
        // Collect all errors, don't fail fast
        // Return validated DTOs for confirmation
    }

    if (errors.Count > 0)
        return Result<List<StockImportLineDto>>.Failure(
            Error.Validation($"Import has {errors.Count} errors"));

    return Result<List<StockImportLineDto>>.Success(validLines);
}
```

### Alert Presentation (Claude's Discretion Decision)
```
Recommendation: In-context banners on pharmacy pages + sidebar badge count

Approach:
1. Pharmacy inventory page: Banner at top showing expiry + low stock alert counts
2. Sidebar: Badge count on Pharmacy nav item (pending prescriptions count)
3. Alert detail: Dedicated alerts tab/section in pharmacy showing full list
4. No background polling/toast -- alerts computed on page load/refresh
5. ConsumableItems page: Same banner pattern for consumable alerts

Rationale: Matches existing AllergyAlert banner pattern. Dashboard widgets add
complexity without proportional value for a small clinic with ~1 pharmacist.
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| EPPlus (free) | MiniExcel | EPPlus v5 (2020) went commercial | Must use MiniExcel or ClosedXML for free |
| DateTime for expiry | DateOnly (.NET 6+) | .NET 6 (2021) | Avoids timezone confusion on date-only values |
| Manual FEFO tracking | Domain-enforced FEFO | Ongoing best practice | Batch selection logic in domain service, not ad-hoc queries |
| Separate Consumables module | Consumables within Pharmacy module | Architecture decision | Avoids module proliferation; shared patterns; separate UI section |

**Deprecated/outdated:**
- EPPlus free license: No longer available for commercial use since v5 (2020)
- WolverineFx.Http in Application layer: Must be removed per project pattern

## Open Questions

1. **BatchDeduction shared between DispensingLine and OtcSaleLine?**
   - What we know: Both dispensing and OTC sales need to record which batches were deducted
   - What's unclear: Whether BatchDeduction should have a polymorphic parent (DispensingLineId OR OtcSaleLineId) or separate tables
   - Recommendation: Use separate tables (DispensingBatchDeduction and OtcBatchDeduction) to avoid nullable FK complexity. OR use a single StockDeduction table with discriminator column. Planner should choose simpler option (separate tables).

2. **Consumables: Same schema or separate schema?**
   - What we know: CONTEXT.md says "fully separate section" in UI. Backend module structure is Pharmacy.
   - What's unclear: Whether consumable tables should be in "pharmacy" schema or a new "consumables" schema
   - Recommendation: Same "pharmacy" schema, separate entity prefix (ConsumableItem vs DrugCatalogItem). Keeps DbContext simple. UI separation is frontend-only concern.

3. **Pending prescription count for sidebar badge: polling vs page-load?**
   - What we know: Queue page shows badge count in sidebar
   - What's unclear: Should it poll for real-time updates or only update on navigation?
   - Recommendation: TanStack Query with refetchInterval (30s) for the badge count query. Lightweight single-number query, acceptable for small clinic.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.* + FluentAssertions 8.* + NSubstitute 5.* |
| Config file | backend/tests/ directory structure |
| Quick run command | `dotnet test backend/tests/Pharmacy.Unit.Tests --no-build -v q` |
| Full suite command | `dotnet test backend/tests/ --no-build -v q` |

### Phase Requirements to Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| PHR-01 | Drug inventory CRUD with batch tracking | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "DrugBatch" -x` | Wave 0 |
| PHR-01 | Supplier CRUD | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "Supplier" -x` | Wave 0 |
| PHR-02 | Stock import via supplier invoice | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "StockImport" -x` | Wave 0 |
| PHR-02 | Excel bulk import validation | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "ExcelImport" -x` | Wave 0 |
| PHR-03 | Expiry alert threshold detection | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "ExpiryAlert" -x` | Wave 0 |
| PHR-04 | Low stock level alert detection | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "LowStock" -x` | Wave 0 |
| PHR-05 | FEFO batch allocation | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "FEFO" -x` | Wave 0 |
| PHR-05 | Dispensing handler with stock deduction | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "Dispensing" -x` | Wave 0 |
| PHR-06 | OTC sale with stock deduction | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "OtcSale" -x` | Wave 0 |
| PHR-07 | 7-day prescription expiry validation | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "PrescriptionExpiry" -x` | Wave 0 |
| CON-01 | Consumable item CRUD | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "Consumable" -x` | Wave 0 |
| CON-02 | Consumable stock management + alerts | unit | `dotnet test backend/tests/Pharmacy.Unit.Tests --filter "ConsumableStock" -x` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Pharmacy.Unit.Tests --no-build -v q`
- **Per wave merge:** `dotnet test backend/tests/ --no-build -v q`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `backend/tests/Pharmacy.Unit.Tests/` -- entire test project needs creation
- [ ] `backend/tests/Pharmacy.Unit.Tests/Pharmacy.Unit.Tests.csproj` -- project file with xUnit/FluentAssertions/NSubstitute references
- [ ] `backend/tests/Pharmacy.Unit.Tests/Features/` -- handler test directory
- [ ] `backend/tests/Pharmacy.Unit.Tests/Domain/` -- domain entity test directory (FEFO allocator, batch deduction, etc.)
- [ ] Framework install: Already available via CPM (xunit, FluentAssertions, NSubstitute, Bogus)

## Codebase-Specific Notes

### Existing Pharmacy Module State
The Pharmacy module currently has:
- **Pharmacy.Domain**: DrugCatalogItem entity, DrugForm/DrugRoute enums
- **Pharmacy.Contracts**: DrugCatalogItemDto, SearchDrugCatalogQuery
- **Pharmacy.Application**: Marker.cs only (no handlers yet -- handlers for drug catalog search are likely in this module but need verification)
- **Pharmacy.Infrastructure**: PharmacyDbContext (empty, "pharmacy" schema)
- **NO Pharmacy.Presentation**: Must be created

### Pharmacy.Application.csproj Cleanup Required
Current references include WolverineFx.Http which must be removed (Application layer is HTTP-free). Replace with FluentValidation.DependencyInjectionExtensions.

### Bootstrapper Registration Gaps
- PharmacyDbContext: Already registered with AuditInterceptor (line 95 of Program.cs)
- Wolverine handler discovery: Already registered (line 228 of Program.cs)
- IoC extension methods: NOT yet registered (no AddPharmacyApplication/Infrastructure/Presentation calls)
- Endpoint mapping: NOT yet registered (no MapPharmacyApiEndpoints call)

### i18n Namespace
- `pharmacy.json` exists in en/ but NOT in vi/ -- need to add vi/pharmacy.json
- Need to add `consumables.json` in both en/ and vi/ (or extend pharmacy.json with consumables section)
- Need to add 'pharmacy' and 'consumables' to i18n ns array in i18n.ts

### Sidebar Navigation Updates
- Pharmacy: Change from `disabled: true` to active, update icon to IconMedicineSyrup (already assigned)
- Consumables: Add new entry with IconPackage or similar icon under Operations group
- Badge: Add pending prescriptions count badge to Pharmacy nav item

### Cross-Module Integration Points
1. **Clinical -> Pharmacy**: Existing SearchDrugCatalogQuery in Pharmacy.Contracts (Phase 5)
2. **Pharmacy -> Clinical**: NEW - Need GetPendingPrescriptionsQuery in Clinical.Contracts for dispensing queue
3. **Pharmacy -> Patient**: Walk-in customer lookup for OTC sales -- query via Patient.Contracts
4. **Pharmacy -> Auth**: Permission check for dispensing, stock management, OTC sale actions

### Permission Additions Needed
PermissionModule.Pharmacy already exists in Auth.Domain.Enums. Need to add specific permissions:
- Pharmacy.ManageInventory (stock import, adjustment)
- Pharmacy.Dispense (dispense prescriptions)
- Pharmacy.OtcSale (process OTC sales)
- Pharmacy.ViewQueue (view dispensing queue)
- Pharmacy.ManageSuppliers (supplier CRUD)
- Pharmacy.ManageConsumables (consumables CRUD)

## Sources

### Primary (HIGH confidence)
- Existing codebase: DrugCatalogItem.cs, DrugPrescription.cs, PrescriptionItem.cs, Visit.cs -- entity model and cross-module patterns
- Existing codebase: AllergyCatalogSeeder.cs -- IHostedService seeder pattern
- Existing codebase: ClinicalApiEndpoints.cs -- Minimal API endpoint pattern
- Existing codebase: Program.cs (Bootstrapper) -- registration patterns, already includes Pharmacy module scaffolding
- Existing codebase: AppSidebar.tsx -- sidebar navigation with disabled items and badge pattern
- Existing codebase: Directory.Packages.props -- CPM versions for all dependencies

### Secondary (MEDIUM confidence)
- [MiniExcel GitHub](https://github.com/mini-software/MiniExcel) -- MIT license, streaming Excel read, .NET support
- [FEFO Inventory Management](https://dclcorp.com/blog/inventory/fefo-first-expired-first-out/) -- FEFO pharmaceutical best practices
- [Microsoft Q&A: Free Excel Libraries](https://learn.microsoft.com/en-gb/answers/questions/2236589/free-library-recommendation-for-excel-operations-i) -- Confirms MiniExcel and ClosedXML as free options

### Tertiary (LOW confidence)
- None -- all findings verified against codebase or official sources

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in project except MiniExcel (verified free/MIT)
- Architecture: HIGH - Follows exact patterns from Phases 1-5 codebase
- Pitfalls: HIGH - Based on inventory management domain knowledge + codebase concurrency patterns
- Entity model: HIGH - Extends existing DrugCatalogItem with clear relationship model
- Cross-module integration: MEDIUM - Pending prescription query pattern inferred from existing SearchDrugCatalogQuery pattern

**Research date:** 2026-03-05
**Valid until:** 2026-04-05 (30 days - stable domain, established codebase patterns)
