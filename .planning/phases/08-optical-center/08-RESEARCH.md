# Phase 8: Optical Center - Research

**Researched:** 2026-03-06
**Domain:** Optical center inventory management, glasses order lifecycle, barcode scanning, warranty claims, prescription history, stocktaking
**Confidence:** HIGH

## Summary

Phase 8 implements the Optical Center module -- a strategic revenue driver for the Ganka28 clinic. The module covers frame/lens inventory management with EAN-13 barcode scanning, a glasses order lifecycle (Ordered -> Processing -> Received -> Ready -> Delivered) with payment enforcement from Phase 7 Billing, combo pricing (preset and custom), warranty claim management with document uploads, lens prescription history with year-over-year comparison, and barcode-based stocktaking with discrepancy reports.

The existing codebase provides an empty but scaffolded Optical module (OpticalDbContext with "optical" schema, Wolverine handler discovery configured, DbContext registered in Bootstrapper). The established patterns from Pharmacy (vertical slices, FluentValidation, Repository/UnitOfWork, IHostedService seeders) and Billing (domain events, cross-module queries via Contracts, Invoice line items with Department.Optical) provide clear blueprints. Key integration points are: Clinical.Contracts for OpticalPrescription data, Pharmacy.Contracts for shared Supplier entity queries, and Billing.Contracts for payment confirmation (OPT-04 enforcement).

**Primary recommendation:** Follow the Pharmacy module structure exactly (Domain entities with factory methods -> Application handlers with FluentValidation -> Infrastructure repositories/seeders -> Presentation endpoints -> Frontend features), adding Billing integration for payment gating and Azure Blob for warranty documents. Use JsBarcode for frontend barcode generation, html5-qrcode for camera-based scanning fallback, and QuestPDF with ZXing.Net for backend barcode label PDF generation.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- EAN-13 barcode format for all frames
- Mixed barcode source: scan manufacturer barcodes when frames already have them, generate + print labels for untagged frames using clinic prefix
- Dual scanning support: USB barcode scanner as primary (keyboard input to focused field), phone/tablet camera as fallback (web-based scanner for mobile stocktaking)
- Full frame attribute set: brand, model, color, size (lens width/bridge/temple as separate fields), material, gender, frame type, selling price, cost price, barcode (EAN-13)
- Hybrid lens model: bulk stock for common powers + custom orders per prescription
- Per-piece quantity tracking for stocked lenses with low-stock alerts
- Custom lens orders placed with suppliers (Essilor, Hoya, Viet Phap) per patient prescription
- Shared supplier entity with Pharmacy module -- tag suppliers by type (drug/optical/both)
- Orders created from HIS optical Rx (Phase 5) -- no walk-in/external Rx path for v1
- Processing types: in-house vs outsourced to supplier lab
- Status transitions: Ordered -> Processing -> Received -> Ready -> Delivered
- Payment enforcement: blocks Processing until full payment confirmed (uses Phase 7 billing integration)
- Preset combo packages (admin creates) + custom combos (staff at order time)
- 12-month warranty on frame + lens per sale, starting from delivery date
- Three warranty resolution types: Replace, Repair, Discount
- Manager approval required for replacements only
- Warranty claim record includes: claim date, resolution type, notes, supporting photos/documents (Azure Blob)
- Lens prescription history per patient per glasses order (linked to optical Rx)
- Year-over-year comparison view
- Contact lenses prescribed via HIS, not sold through optical counter (OPT-05)
- Barcode-based stocktaking with physical count entry and discrepancy report

### Claude's Discretion
- Barcode label layout and paper sizing (thermal vs A4 sheet)
- Web-based barcode scanner library selection (quagga.js, html5-qrcode, or similar)
- Barcode generation approach (EAN-13 prefix allocation for clinic-generated barcodes)
- Lens catalog seed data (common power ranges to stock)
- Frame catalog admin page layout and filtering
- Glasses order detail page layout
- Stocktaking session workflow (start/pause/complete)
- Overdue order alert presentation
- Loading states and error handling

### Deferred Ideas (OUT OF SCOPE)
- Zalo OA "Glasses ready" notification (NTF-04) -- deferred to v2
- Walk-in orders from external prescriptions -- not supported for v1
- Trial lens inventory for Ortho-K fitting (OPX-01) -- deferred to v2
- Contact lens inventory management -- tracked separately if needed post-launch
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| OPT-01 | Staff can manage frame inventory with barcode scanning (brand, model, color, size, price, stock) | Frame entity with full attributes, EAN-13 barcode field, JsBarcode generation, USB scanner keyboard input pattern, camera fallback with html5-qrcode |
| OPT-02 | Staff can order lenses by prescription from suppliers (Essilor, Hoya, Viet Phap) | LensCatalogItem + LensOrder entities, shared Supplier entity via Pharmacy.Contracts cross-module query, optical supplier seeder |
| OPT-03 | System tracks glasses order lifecycle: Ordered -> Processing -> Received -> Ready -> Delivered | GlassesOrder aggregate root with status enum and transition methods, domain events for status changes, estimated delivery tracking |
| OPT-04 | System blocks lens processing until full payment is received | Cross-module query to Billing.Contracts.GetVisitInvoiceQuery to check Invoice.IsFullyPaid before allowing Processing transition |
| OPT-05 | Contact lenses (Ortho-K, soft) prescribed via HIS, not sold through optical counter | No implementation needed in Optical module -- already handled by Clinical OpticalPrescription with LensType enum |
| OPT-06 | Staff can create combo pricing (preset combos + custom frame+lens combinations) | ComboPackage entity (admin-created presets), custom combo pricing on GlassesOrder with manual price override |
| OPT-07 | System tracks warranty per sale (12 months frame + lens) with claim workflow | WarrantyClaim entity with resolution types, manager approval for replacements, Azure Blob for supporting documents, audit trail |
| OPT-08 | System stores lens prescription history per patient with year-over-year comparison | Query handler that fetches OpticalPrescription history from Clinical module via cross-module query, comparison DTO |
| OPT-09 | Staff can perform barcode-based stocktaking (physical count vs. system, discrepancy report) | StocktakingSession and StocktakingItem entities, barcode scan entry, discrepancy calculation, QuestPDF report |
</phase_requirements>

## Standard Stack

### Core (Backend)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET 10 | 10.0 | Runtime | Already used throughout project |
| EF Core | 10.0 | ORM with "optical" schema | Module-per-schema pattern established |
| Wolverine | (current) | Message bus, handler discovery | Static handler pattern with IMessageBus.InvokeAsync |
| FluentValidation | (current) | Command validation | AbstractValidator pattern per handler |
| QuestPDF | (current) | PDF generation for barcode labels and reports | Already used in Billing and Clinical for documents |
| ZXing.Net | 0.16.x+ | Backend EAN-13 barcode image generation for QuestPDF | Official QuestPDF barcode integration partner |
| NSubstitute | (current) | Unit test mocking | Project standard for test doubles |
| FluentAssertions | (current) | Test assertions | Project standard for readable assertions |
| xUnit | (current) | Test framework | Project standard |

### Core (Frontend)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| React | 19.x | UI framework | Project standard |
| TanStack Router | 1.114+ | Routing | File-based routing under _authenticated/ |
| TanStack Query | 5.64+ | Server state | QueryKey factory + mutation with invalidation pattern |
| TanStack Table | 8.21+ | Data tables | Used for inventory tables via DataTable component |
| React Hook Form + Zod | 7.x + 3.24 | Form handling | zodResolver with Controller pattern |
| shadcn/ui | (current) | UI components | Cards, Tables, Dialogs, Forms per CLAUDE.md |
| @tabler/icons-react | 3.37+ | Icons | IconEyeglass already imported for optical sidebar |
| sonner | 1.7+ | Toast notifications | onError toast pattern on all mutations |

### New Dependencies (to add)
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| JsBarcode | 3.11+ | Frontend EAN-13 barcode rendering to SVG/Canvas | Frame label display, barcode preview in catalog |
| html5-qrcode | 2.3.8 | Camera-based barcode scanning | Mobile stocktaking, fallback scanning when no USB scanner |
| ZXing.Net (NuGet) | 0.16.9+ | Backend barcode generation for PDF labels | QuestPDF barcode label generation |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| html5-qrcode | @AImageLab-zip/barcode-detector | barcode-detector uses native Barcode Detection API (Chrome-only); html5-qrcode has wider browser support despite being in maintenance mode -- sufficient for EAN-13 scanning which is well-tested |
| html5-qrcode | quagga2 | quagga2 is maintained fork but focused on 1D only; html5-qrcode simpler API, adequate for EAN-13 use case |
| JsBarcode | bwip-js | bwip-js more comprehensive but heavier; JsBarcode lighter, MIT license, excellent EAN-13 support |

**Installation:**
```bash
# Frontend
cd frontend && npm install jsbarcode html5-qrcode

# Backend (NuGet)
cd backend && dotnet add src/Modules/Optical/Optical.Infrastructure/Optical.Infrastructure.csproj package ZXing.Net
```

## Architecture Patterns

### Recommended Project Structure

#### Backend
```
backend/src/Modules/Optical/
  Optical.Domain/
    Entities/
      Frame.cs                    # AggregateRoot - frame catalog item with barcode
      LensCatalogItem.cs          # AggregateRoot - lens type in stock
      LensStockEntry.cs           # Entity - specific lens power in stock (child of LensCatalogItem)
      GlassesOrder.cs             # AggregateRoot - order lifecycle aggregate
      GlassesOrderItem.cs         # Entity - frame + lens selections on order
      LensOrder.cs                # Entity - custom lens order to supplier
      ComboPackage.cs             # AggregateRoot - preset combo pricing
      WarrantyClaim.cs            # Entity - warranty claim per glasses order
      StocktakingSession.cs       # AggregateRoot - stocktaking session
      StocktakingItem.cs          # Entity - individual count entry
    Enums/
      FrameMaterial.cs            # Metal, Plastic, Titanium
      FrameType.cs                # FullRim, SemiRimless, Rimless
      FrameGender.cs              # Male, Female, Unisex
      GlassesOrderStatus.cs       # Ordered, Processing, Received, Ready, Delivered
      ProcessingType.cs           # InHouse, Outsourced
      LensMaterial.cs             # CR39, Polycarbonate, HiIndex
      LensCoating.cs              # [Flags] AntiReflective, BlueCut, Photochromic, etc.
      WarrantyResolution.cs       # Replace, Repair, Discount
      WarrantyApprovalStatus.cs   # Pending, Approved, Rejected
      StocktakingStatus.cs        # InProgress, Completed, Cancelled
      SupplierType.cs             # Drug, Optical, Both (extend Supplier entity)
    Events/
      GlassesOrderStatusChangedEvent.cs
      LowStockAlertEvent.cs
  Optical.Application/
    Features/
      Frames/
        CreateFrame.cs
        UpdateFrame.cs
        GetFrames.cs
        SearchFrames.cs
        GenerateBarcode.cs
      Lenses/
        CreateLensCatalogItem.cs
        UpdateLensCatalogItem.cs
        GetLensCatalog.cs
        AdjustLensStock.cs
      Orders/
        CreateGlassesOrder.cs
        UpdateOrderStatus.cs
        GetGlassesOrders.cs
        GetGlassesOrderById.cs
        GetOverdueOrders.cs
      Combos/
        CreateComboPackage.cs
        UpdateComboPackage.cs
        GetComboPackages.cs
      Warranty/
        CreateWarrantyClaim.cs
        ApproveWarrantyClaim.cs
        GetWarrantyClaims.cs
        UploadWarrantyDocument.cs
      Prescriptions/
        GetPatientPrescriptionHistory.cs
        GetPrescriptionComparison.cs
      Stocktaking/
        StartStocktakingSession.cs
        RecordStocktakingItem.cs
        CompleteStocktaking.cs
        GetDiscrepancyReport.cs
      Alerts/
        GetLowLensStockAlerts.cs
    Interfaces/
      IFrameRepository.cs
      ILensCatalogRepository.cs
      IGlassesOrderRepository.cs
      IComboPackageRepository.cs
      IWarrantyClaimRepository.cs
      IStocktakingRepository.cs
      IUnitOfWork.cs
    IoC.cs
    Marker.cs
  Optical.Contracts/
    Dtos/
      FrameDto.cs
      LensCatalogItemDto.cs
      GlassesOrderDto.cs
      ComboPackageDto.cs
      WarrantyClaimDto.cs
      StocktakingReportDto.cs
    Queries/
      GetOpticalChargesQuery.cs   # Cross-module: Billing queries optical charges
  Optical.Infrastructure/
    OpticalDbContext.cs           # Already scaffolded with "optical" schema
    Configurations/
      FrameConfiguration.cs
      LensCatalogItemConfiguration.cs
      GlassesOrderConfiguration.cs
      ComboPackageConfiguration.cs
      WarrantyClaimConfiguration.cs
      StocktakingSessionConfiguration.cs
    Repositories/
      FrameRepository.cs
      LensCatalogRepository.cs
      GlassesOrderRepository.cs
      ComboPackageRepository.cs
      WarrantyClaimRepository.cs
      StocktakingRepository.cs
      UnitOfWork.cs
    Documents/
      BarcodeLabelDocument.cs     # QuestPDF barcode label (thermal + A4 sheet)
      StocktakingReportDocument.cs
    Seeding/
      OpticalSupplierSeeder.cs    # Seeds Essilor, Hoya, Viet Phap
    IoC.cs
  Optical.Presentation/
    OpticalApiEndpoints.cs        # Frame, lens, order, combo endpoints
    WarrantyApiEndpoints.cs       # Warranty claim endpoints
    StocktakingApiEndpoints.cs    # Stocktaking endpoints
    IoC.cs

backend/tests/
  Optical.Unit.Tests/
    Domain/
      FrameTests.cs
      GlassesOrderTests.cs
      WarrantyClaimTests.cs
    Features/
      FrameHandlerTests.cs
      LensHandlerTests.cs
      OrderHandlerTests.cs
      ComboHandlerTests.cs
      WarrantyHandlerTests.cs
      StocktakingHandlerTests.cs
      PrescriptionHistoryHandlerTests.cs
```

#### Frontend
```
frontend/src/features/optical/
  api/
    optical-api.ts              # API client functions
    optical-queries.ts          # TanStack Query hooks + key factory
  components/
    FrameCatalogPage.tsx
    FrameCatalogTable.tsx
    FrameFormDialog.tsx
    BarcodeScannerInput.tsx     # USB scanner keyboard input component
    CameraScanner.tsx           # html5-qrcode camera fallback
    BarcodeDisplay.tsx          # JsBarcode rendering component
    LensCatalogPage.tsx
    LensCatalogTable.tsx
    LensFormDialog.tsx
    GlassesOrdersPage.tsx
    GlassesOrderTable.tsx
    CreateGlassesOrderForm.tsx
    GlassesOrderDetailPage.tsx
    OrderStatusBadge.tsx
    ComboPackagePage.tsx
    ComboPackageForm.tsx
    WarrantyClaimsPage.tsx
    WarrantyClaimForm.tsx
    WarrantyDocumentUpload.tsx
    PrescriptionHistoryTab.tsx
    PrescriptionComparisonView.tsx
    StocktakingPage.tsx
    StocktakingScanner.tsx
    DiscrepancyReport.tsx
    OverdueOrderAlert.tsx

frontend/public/locales/en/optical.json
frontend/public/locales/vi/optical.json
```

### Pattern 1: Vertical Slice Feature Handler (Established)
**What:** Each feature is a single file with Command record, Validator class, and static Handler class.
**When to use:** Every backend feature in the Optical module.
**Example:**
```csharp
// Source: Pharmacy.Application/Features/Suppliers/CreateSupplier.cs pattern
public sealed record CreateFrameCommand(
    string Brand, string Model, string Color,
    int LensWidth, int BridgeWidth, int TempleLength,
    int Material, int FrameType, int Gender,
    decimal SellingPrice, decimal CostPrice,
    string? Barcode);

public class CreateFrameCommandValidator : AbstractValidator<CreateFrameCommand>
{
    public CreateFrameCommandValidator()
    {
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LensWidth).InclusiveBetween(40, 65);
        RuleFor(x => x.BridgeWidth).InclusiveBetween(12, 24);
        RuleFor(x => x.TempleLength).InclusiveBetween(120, 155);
        RuleFor(x => x.SellingPrice).GreaterThan(0);
        RuleFor(x => x.CostPrice).GreaterThan(0);
        RuleFor(x => x.Barcode)
            .Matches(@"^\d{13}$").When(x => x.Barcode is not null)
            .WithMessage("Barcode must be a 13-digit EAN-13 code.");
    }
}

public static class CreateFrameHandler
{
    public static async Task<Result<Guid>> Handle(
        CreateFrameCommand command,
        IFrameRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateFrameCommand> validator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result.Failure<Guid>(Error.ValidationWithDetails(errors));
        }

        var frame = Frame.Create(/* ... */, new BranchId(currentUser.BranchId));
        repository.Add(frame);
        await unitOfWork.SaveChangesAsync(ct);
        return frame.Id;
    }
}
```

### Pattern 2: Cross-Module Query via Contracts (Established)
**What:** Module A queries Module B by publishing a query record defined in B's Contracts project. Wolverine routes it to B's handler.
**When to use:** OPT-04 (query Billing for payment status), OPT-08 (query Clinical for optical Rx history), supplier queries (Pharmacy.Contracts).
**Example:**
```csharp
// In Billing.Contracts/Queries/GetVisitInvoiceQuery.cs (already exists)
public sealed record GetVisitInvoiceQuery(Guid VisitId);

// In Optical.Application - check payment before allowing Processing transition
var invoiceDto = await bus.InvokeAsync<InvoiceDto?>(new GetVisitInvoiceQuery(order.VisitId), ct);
if (invoiceDto is null || !invoiceDto.IsFullyPaid)
    return Result.Failure(Error.Validation("Payment must be completed before processing."));
```

### Pattern 3: Domain Events for State Changes (Established)
**What:** Aggregate roots raise domain events on significant state changes. Wolverine FX dispatches them.
**When to use:** GlassesOrder status transitions, low stock alerts.
**Example:**
```csharp
// In GlassesOrder.cs
public void TransitionTo(GlassesOrderStatus newStatus)
{
    // validate transition
    Status = newStatus;
    AddDomainEvent(new GlassesOrderStatusChangedEvent(Id, Status));
    SetUpdatedAt();
}
```

### Pattern 4: USB Barcode Scanner as Keyboard Input
**What:** USB barcode scanners emulate keyboard input. The scanned barcode value is typed into the focused input field, typically followed by Enter key.
**When to use:** Primary scanning method for frame catalog and stocktaking.
**Example:**
```typescript
// BarcodeScannerInput.tsx - simple focused text input
// USB scanner types barcode digits into the focused field + Enter
function BarcodeScannerInput({ onScan }: { onScan: (barcode: string) => void }) {
  const inputRef = useRef<HTMLInputElement>(null)
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault()
      const value = inputRef.current?.value?.trim()
      if (value && /^\d{13}$/.test(value)) {
        onScan(value)
        if (inputRef.current) inputRef.current.value = ''
      }
    }
  }
  return <Input ref={inputRef} onKeyDown={handleKeyDown} autoFocus />
}
```

### Pattern 5: TanStack Query Key Factory (Established)
**What:** Hierarchical query key factory for cache invalidation.
**When to use:** All frontend queries/mutations in optical module.
**Example:**
```typescript
// Source: pharmacy-queries.ts pattern
export const opticalKeys = {
  all: ["optical"] as const,
  frames: {
    all: () => [...opticalKeys.all, "frames"] as const,
    search: (term: string) => [...opticalKeys.all, "frames", "search", term] as const,
    detail: (id: string) => [...opticalKeys.all, "frames", id] as const,
  },
  lenses: {
    all: () => [...opticalKeys.all, "lenses"] as const,
  },
  orders: {
    all: () => [...opticalKeys.all, "orders"] as const,
    detail: (id: string) => [...opticalKeys.all, "orders", id] as const,
    overdue: () => [...opticalKeys.all, "orders", "overdue"] as const,
  },
  combos: {
    all: () => [...opticalKeys.all, "combos"] as const,
  },
  warranty: {
    all: () => [...opticalKeys.all, "warranty"] as const,
    byOrder: (orderId: string) => [...opticalKeys.all, "warranty", orderId] as const,
  },
  prescriptions: {
    byPatient: (patientId: string) => [...opticalKeys.all, "prescriptions", patientId] as const,
  },
  stocktaking: {
    current: () => [...opticalKeys.all, "stocktaking", "current"] as const,
  },
}
```

### Anti-Patterns to Avoid
- **Direct DB joins across modules:** Never join Optical tables with Clinical or Billing tables directly. Use cross-module queries via Contracts (IMessageBus.InvokeAsync).
- **Putting payment logic in Optical:** OPT-04 only checks payment status via Billing query. Optical never modifies invoices or payments.
- **Storing prescription data in Optical:** Lens prescription data lives in Clinical module (OpticalPrescription entity). Optical module queries it via cross-module query for history/comparison views.
- **Hardcoding barcode prefixes:** Use configurable clinic prefix for auto-generated barcodes, not a hardcoded constant.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| EAN-13 barcode rendering | Custom SVG drawing | JsBarcode | Check digit calculation, guard patterns, encoding tables are complex; JsBarcode handles all EAN variants correctly |
| EAN-13 barcode in PDF | Manual barcode drawing in QuestPDF | ZXing.Net + QuestPDF integration | QuestPDF's official barcode API uses ZXing.Net for vector SVG generation at any resolution |
| Camera barcode scanning | MediaDevices API + manual decoding | html5-qrcode | Camera permission handling, video stream management, barcode decoding from frames -- all handled by library |
| EAN-13 check digit | Manual modulo-10 calculation | JsBarcode validates automatically / simple utility | Easy to get wrong; alternating weights 1,3 pattern has subtle edge cases |
| File upload to Azure Blob | Custom multipart handling | IAzureBlobService (already exists) | SAS URLs, container management, content type handling all built |
| PDF document generation | HTML-to-PDF conversion | QuestPDF IDocument pattern (already established) | Consistent with existing ReceiptDocument, InvoiceDocument patterns |
| Audit logging | Custom change tracking | IAuditable marker interface + AuditInterceptor | Already intercepts SaveChanges for field-level audit -- just mark entities |

**Key insight:** The Optical module is architecturally identical to Pharmacy -- it's inventory management with order tracking. Every infrastructure concern (auditing, blob storage, PDF generation, cross-module queries) already has established patterns. The only truly new technical concern is barcode handling (generation + scanning), and well-tested libraries exist for both sides.

## Common Pitfalls

### Pitfall 1: Payment Gate Race Condition (OPT-04)
**What goes wrong:** Staff opens order form, payment is not yet confirmed. Meanwhile cashier confirms payment. Staff clicks "Start Processing" but the check was done on stale data.
**Why it happens:** Frontend caches payment status in TanStack Query.
**How to avoid:** Always re-check payment status server-side in the UpdateOrderStatus handler before transitioning to Processing. The frontend check is a UX optimization only.
**Warning signs:** Test only the frontend flow without server-side validation.

### Pitfall 2: Barcode Uniqueness Across Sources
**What goes wrong:** Two frames get the same barcode -- one scanned from manufacturer, one auto-generated.
**Why it happens:** No uniqueness constraint, or clinic-generated prefix overlaps with manufacturer codes.
**How to avoid:** Add unique index on Frame.Barcode (filtered for non-null). Use a clinic-specific GS1 prefix (e.g., "893" for Vietnam + clinic code) for generated barcodes. Validate uniqueness in CreateFrame/UpdateFrame handlers.
**Warning signs:** Duplicate scan results during stocktaking.

### Pitfall 3: Stock Quantity Going Negative
**What goes wrong:** Two concurrent orders deduct the same last lens from stock.
**Why it happens:** Check-then-update without concurrency control.
**How to avoid:** Use optimistic concurrency (RowVersion on stock entities) or pessimistic locking via EF Core's IsolationLevel.RepeatableRead for stock deduction operations. The Pharmacy module's FEFO allocator pattern shows how to handle this.
**Warning signs:** Negative stock quantities in inventory reports.

### Pitfall 4: Warranty Date Calculation Error
**What goes wrong:** Warranty period calculated from order creation date instead of delivery date.
**Why it happens:** Developer uses GlassesOrder.CreatedAt instead of the timestamp when status changed to Delivered.
**How to avoid:** Store DeliveredAt timestamp on GlassesOrder. Calculate warranty expiry as DeliveredAt + 12 months. The WarrantyClaim handler validates claim date is within warranty period.
**Warning signs:** Warranties expiring before glasses are even delivered.

### Pitfall 5: Stocktaking Session Concurrent Modification
**What goes wrong:** Two staff members scan the same item during stocktaking, doubling the physical count.
**Why it happens:** No check for duplicate barcode scans within a session.
**How to avoid:** StocktakingSession.RecordItem should upsert (update count if barcode already scanned in this session, not add a second entry). Use a unique index on (SessionId, Barcode).
**Warning signs:** Physical counts consistently higher than actual inventory.

### Pitfall 6: Missing Optical Module Bootstrapper Registration
**What goes wrong:** Build succeeds but endpoints return 404 or handlers are not discovered.
**Why it happens:** Optical module Application/Infrastructure/Presentation IoC not registered in Program.cs.
**How to avoid:** Follow exact Pharmacy registration pattern: AddOpticalApplication(), AddOpticalInfrastructure(), AddOpticalPresentation() + MapOpticalApiEndpoints(). The Wolverine discovery for Optical.Application.Marker is already configured (line 244 of Program.cs).
**Warning signs:** API returns 404, handlers never invoked.

### Pitfall 7: Forgetting BranchId on Aggregate Roots
**What goes wrong:** Multi-branch query filters silently exclude all optical data.
**Why it happens:** New aggregate roots don't call SetBranchId in factory method.
**How to avoid:** Every AggregateRoot.Create() must accept BranchId and call SetBranchId(). Follow Supplier.Create() pattern exactly.
**Warning signs:** Data disappears in queries filtered by BranchId global filter.

## Code Examples

### EAN-13 Barcode Generation (Frontend)
```typescript
// BarcodeDisplay.tsx - render EAN-13 barcode
import JsBarcode from 'jsbarcode'
import { useEffect, useRef } from 'react'

export function BarcodeDisplay({ value }: { value: string }) {
  const svgRef = useRef<SVGSVGElement>(null)

  useEffect(() => {
    if (svgRef.current && value) {
      JsBarcode(svgRef.current, value, {
        format: 'EAN13',
        width: 2,
        height: 60,
        displayValue: true,
        fontSize: 14,
        margin: 10,
      })
    }
  }, [value])

  return <svg ref={svgRef} />
}
```

### Camera Barcode Scanner (Frontend)
```typescript
// CameraScanner.tsx - html5-qrcode camera fallback
import { Html5QrcodeScanner, Html5QrcodeSupportedFormats } from 'html5-qrcode'
import { useEffect, useRef } from 'react'

interface CameraScannerProps {
  onScan: (barcode: string) => void
  onError?: (error: string) => void
}

export function CameraScanner({ onScan, onError }: CameraScannerProps) {
  const scannerRef = useRef<Html5QrcodeScanner | null>(null)

  useEffect(() => {
    const scanner = new Html5QrcodeScanner(
      'barcode-reader',
      {
        fps: 10,
        qrbox: { width: 300, height: 100 },
        formatsToSupport: [Html5QrcodeSupportedFormats.EAN_13],
      },
      false
    )

    scanner.render(
      (decodedText) => {
        onScan(decodedText)
        scanner.clear()
      },
      (errorMessage) => {
        onError?.(errorMessage)
      }
    )

    scannerRef.current = scanner

    return () => {
      scanner.clear().catch(() => {})
    }
  }, [onScan, onError])

  return <div id="barcode-reader" />
}
```

### EAN-13 Barcode Generation (Backend - for clinic-generated barcodes)
```csharp
// EAN-13 generation utility
public static class Ean13Generator
{
    private const string ClinicPrefix = "893"; // Vietnam GS1 prefix

    /// <summary>
    /// Generates a unique EAN-13 barcode with clinic prefix.
    /// Format: [3-digit prefix][9-digit sequence][1-digit check]
    /// </summary>
    public static string Generate(long sequenceNumber)
    {
        var withoutCheck = $"{ClinicPrefix}{sequenceNumber:D9}";
        var checkDigit = CalculateCheckDigit(withoutCheck);
        return $"{withoutCheck}{checkDigit}";
    }

    public static int CalculateCheckDigit(string first12)
    {
        if (first12.Length != 12) throw new ArgumentException("Must be 12 digits");

        var sum = 0;
        for (int i = 0; i < 12; i++)
        {
            var digit = first12[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }
        var remainder = sum % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }

    public static bool IsValid(string barcode)
    {
        if (barcode.Length != 13 || !barcode.All(char.IsDigit)) return false;
        var expected = CalculateCheckDigit(barcode[..12]);
        return (barcode[12] - '0') == expected;
    }
}
```

### QuestPDF Barcode Label Document (Backend)
```csharp
// BarcodeLabelDocument.cs - print barcode labels
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.Common;

public sealed class BarcodeLabelDocument : IDocument
{
    private readonly List<BarcodeLabelData> _labels;

    public record BarcodeLabelData(string Barcode, string Brand, string Model, string Color, string Size, decimal Price);

    public BarcodeLabelDocument(List<BarcodeLabelData> labels) => _labels = labels;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(10, Unit.Millimetre);

            page.Content().Grid(grid =>
            {
                grid.Columns(3); // 3 labels per row on A4
                grid.Spacing(5);

                foreach (var label in _labels)
                {
                    grid.Item().Border(0.5f).Padding(3).Column(col =>
                    {
                        col.Item().Text($"{label.Brand} {label.Model}").FontSize(8).Bold();
                        col.Item().Text($"{label.Color} | {label.Size}").FontSize(7);
                        col.Item().Image(GenerateBarcodeImage(label.Barcode));
                        col.Item().Text($"{label.Price:N0} VND").FontSize(8).Bold();
                    });
                }
            });
        });
    }

    private static byte[] GenerateBarcodeImage(string barcode)
    {
        var writer = new BarcodeWriterSvg
        {
            Format = BarcodeFormat.EAN_13,
            Options = new EncodingOptions { Width = 200, Height = 60, Margin = 5 }
        };
        var svgContent = writer.Write(barcode);
        // Convert SVG to PNG bytes for QuestPDF
        // ... (use SkiaSharp or similar for SVG->PNG conversion)
        return Array.Empty<byte>(); // placeholder
    }
}
```

### GlassesOrder Aggregate Root with Status Transitions
```csharp
// GlassesOrder.cs - domain entity with lifecycle management
public class GlassesOrder : AggregateRoot, IAuditable
{
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = string.Empty;
    public Guid VisitId { get; private set; }
    public Guid OpticalPrescriptionId { get; private set; }
    public GlassesOrderStatus Status { get; private set; }
    public ProcessingType ProcessingType { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public decimal TotalPrice { get; private set; }
    // ... frame, lens details, combo reference

    private static readonly Dictionary<GlassesOrderStatus, GlassesOrderStatus[]> AllowedTransitions = new()
    {
        [GlassesOrderStatus.Ordered] = [GlassesOrderStatus.Processing],
        [GlassesOrderStatus.Processing] = [GlassesOrderStatus.Received],
        [GlassesOrderStatus.Received] = [GlassesOrderStatus.Ready],
        [GlassesOrderStatus.Ready] = [GlassesOrderStatus.Delivered],
    };

    public void TransitionTo(GlassesOrderStatus newStatus)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to {newStatus}.");

        if (newStatus == GlassesOrderStatus.Delivered)
            DeliveredAt = DateTime.UtcNow;

        var oldStatus = Status;
        Status = newStatus;
        SetUpdatedAt();
        AddDomainEvent(new GlassesOrderStatusChangedEvent(Id, oldStatus, newStatus));
    }

    public bool IsUnderWarranty => DeliveredAt.HasValue &&
        DeliveredAt.Value.AddMonths(12) > DateTime.UtcNow;
}
```

### Supplier Type Extension Pattern
```csharp
// Extend existing Supplier entity with type tag
// Option: Add SupplierType flag in Pharmacy.Domain or use a join table
// Since Supplier lives in Pharmacy.Domain, add a SupplierTypes flags enum property:
[Flags]
public enum SupplierType
{
    Drug = 1,
    Optical = 2,
    // Both = Drug | Optical
}

// In Supplier.cs, add:
public SupplierType Types { get; private set; } = SupplierType.Drug;

// Optical module queries suppliers via Pharmacy.Contracts:
public sealed record GetOpticalSuppliersQuery();
// Handler filters: supplier.Types.HasFlag(SupplierType.Optical)
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| quagga.js (unmaintained) | html5-qrcode or Barcode Detection API | 2023+ | html5-qrcode widely used despite maintenance mode; native API Chrome-only |
| Backend barcode generation with System.Drawing | ZXing.Net with QuestPDF | 2024+ | System.Drawing deprecated on non-Windows; ZXing.Net cross-platform |
| Custom barcode SVG rendering | JsBarcode npm package | Stable since 2019 | MIT, zero dependencies, excellent EAN support |

**Deprecated/outdated:**
- quagga.js (original): unmaintained since 2020, use quagga2 fork or html5-qrcode
- html5-qrcode: in maintenance mode (no new releases since April 2023), but functional for EAN-13 use case
- System.Drawing.Common: deprecated on non-Windows .NET 7+; use SkiaSharp or ImageSharp for image processing

## Open Questions

1. **Supplier entity extension approach**
   - What we know: Supplier entity lives in Pharmacy.Domain. Optical needs to tag suppliers as optical-capable.
   - What's unclear: Whether to add a SupplierType flag to existing Supplier entity (requires Pharmacy migration) or create a separate OpticalSupplier entity in Optical module that references Pharmacy supplier by ID.
   - Recommendation: Add SupplierType flags enum to existing Supplier entity in Pharmacy.Domain -- simpler, avoids data duplication, and the context notes say "shared supplier entity." Create a Pharmacy migration to add the column, then Optical module uses cross-module query to filter by type.

2. **EAN-13 clinic prefix allocation**
   - What we know: Vietnam GS1 prefix is "893". Clinic needs a unique prefix for self-generated barcodes.
   - What's unclear: Whether the clinic has a registered GS1 company prefix.
   - Recommendation: Use a configurable prefix stored in clinic settings (default "8930000" -- 7-digit company prefix). This leaves 5 digits (99,999 items) + 1 check digit. For v1 this is sufficient. Make it configurable for future GS1 registration.

3. **Lens stock granularity**
   - What we know: Common lens powers are stocked in bulk; unusual prescriptions are custom-ordered.
   - What's unclear: Exact power ranges to pre-stock.
   - Recommendation: Seed with common single vision ranges (SPH -8.00 to +4.00 in 0.25 steps, CYL 0 to -2.00 in 0.25 steps). Make lens catalog fully admin-managed so clinic can adjust. Seed data is a starting point only.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + FluentAssertions + NSubstitute |
| Config file | backend/tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj (Wave 0) |
| Quick run command | `dotnet test backend/tests/Optical.Unit.Tests --no-build -v q` |
| Full suite command | `dotnet test backend/tests --no-build -v q` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| OPT-01 | Frame CRUD with barcode validation | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~FrameHandler" -v q` | Wave 0 |
| OPT-01 | EAN-13 barcode generation + validation | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~Barcode" -v q` | Wave 0 |
| OPT-02 | Lens catalog CRUD + stock management | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~LensHandler" -v q` | Wave 0 |
| OPT-03 | Glasses order lifecycle transitions | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~OrderHandler" -v q` | Wave 0 |
| OPT-03 | GlassesOrder domain - valid/invalid transitions | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~GlassesOrderTests" -v q` | Wave 0 |
| OPT-04 | Payment gate blocks Processing without payment | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~PaymentGate" -v q` | Wave 0 |
| OPT-05 | Contact lenses via HIS only | manual-only | N/A - architectural constraint, no optical code needed | N/A |
| OPT-06 | Combo package CRUD + pricing | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~ComboHandler" -v q` | Wave 0 |
| OPT-07 | Warranty claim create + approval workflow | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~WarrantyHandler" -v q` | Wave 0 |
| OPT-07 | Warranty period validation (12 months from delivery) | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~WarrantyTests" -v q` | Wave 0 |
| OPT-08 | Prescription history query + comparison | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~PrescriptionHistory" -v q` | Wave 0 |
| OPT-09 | Stocktaking session + discrepancy report | unit | `dotnet test backend/tests/Optical.Unit.Tests --filter "FullyQualifiedName~StocktakingHandler" -v q` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Optical.Unit.Tests --no-build -v q`
- **Per wave merge:** `dotnet test backend/tests --no-build -v q`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `backend/tests/Optical.Unit.Tests/Optical.Unit.Tests.csproj` -- new test project (reference Optical.Application, Optical.Domain, xUnit, FluentAssertions, NSubstitute)
- [ ] `backend/tests/Optical.Unit.Tests/Domain/GlassesOrderTests.cs` -- order lifecycle domain tests
- [ ] `backend/tests/Optical.Unit.Tests/Domain/FrameTests.cs` -- frame entity + barcode validation
- [ ] `backend/tests/Optical.Unit.Tests/Domain/WarrantyClaimTests.cs` -- warranty period validation
- [ ] `backend/tests/Optical.Unit.Tests/Features/` -- handler test files per feature area

## Sources

### Primary (HIGH confidence)
- Existing codebase: Pharmacy module (vertical slice pattern, Supplier entity, IHostedService seeder, API endpoints, TanStack Query hooks)
- Existing codebase: Billing module (Invoice entity, PaymentReceivedEvent, InvoiceFinalizedEvent, cross-module GetVisitInvoiceQuery, Department.Optical enum)
- Existing codebase: Clinical module (OpticalPrescription entity, LensType enum, WorkflowStage.PharmacyOptical)
- Existing codebase: Shared domain (AggregateRoot, Entity, Result<T>, IAuditable, ICurrentUser, IAzureBlobService)
- Existing codebase: Bootstrapper (OpticalDbContext registered line 111, Wolverine discovery for Optical.Application.Marker line 244)
- [QuestPDF Barcodes API Reference](https://www.questpdf.com/api-reference/barcodes.html) - barcode generation with ZXing.Net integration

### Secondary (MEDIUM confidence)
- [JsBarcode GitHub](https://github.com/lindell/JsBarcode) - EAN-13 support, MIT license, zero dependencies, latest v3.11
- [JsBarcode EAN Wiki](https://github.com/lindell/JsBarcode/wiki/EAN) - EAN-13/EAN-8 specific documentation
- [html5-qrcode GitHub](https://github.com/mebjas/html5-qrcode) - Cross-platform barcode scanning, supports EAN-13 format, in maintenance mode
- [ZXing.Net NuGet](https://www.nuget.org/packages/ZXing.Net) - .NET barcode generation library
- [EAN-13 specification](https://barcode-coder.com/en/ean-13-specification-102.html) - Check digit algorithm, format structure

### Tertiary (LOW confidence)
- html5-qrcode maintenance status: project is in maintenance mode since April 2023, no new releases. Functional for EAN-13 but no future fixes. Monitor for potential need to switch to Barcode Detection API + polyfill in future.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - all technologies already in use in the project; only new npm packages (JsBarcode, html5-qrcode) and one NuGet package (ZXing.Net) needed
- Architecture: HIGH - follows established Pharmacy module patterns exactly; cross-module queries via Contracts are proven pattern
- Domain model: HIGH - entity structure mirrors Pharmacy inventory pattern; glasses order lifecycle is standard state machine
- Integration points: HIGH - Billing.Contracts, Clinical.Contracts, Pharmacy.Contracts cross-module query patterns are well established
- Barcode handling: MEDIUM - JsBarcode and ZXing.Net are well-proven libraries; html5-qrcode is in maintenance mode but sufficient for EAN-13
- Pitfalls: HIGH - identified from domain knowledge and codebase analysis

**Research date:** 2026-03-06
**Valid until:** 2026-04-06 (stable domain, established patterns)
