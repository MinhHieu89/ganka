# Phase 7: Billing & Finance - Research

**Researched:** 2026-03-06
**Domain:** Financial billing, invoice generation, payment processing, shift management, Vietnamese e-invoice compliance
**Confidence:** HIGH

## Summary

Phase 7 implements the Billing & Finance module for the Ganka28 ophthalmology clinic. The module must generate consolidated invoices per visit (combining charges from medical, optical, pharmacy, and treatment departments), accept multiple payment methods with manual confirmation, produce Vietnamese e-invoices (hoa don dien tu) for MISA export, handle treatment package split payments with enforcement rules, and manage cashier shifts with cash reconciliation.

The existing codebase provides a strong foundation: BillingDbContext is scaffolded with "billing" schema, QuestPDF is already integrated for document generation with Vietnamese font support (Noto Sans), PermissionModule.Billing is defined in the Auth module, and cross-module query patterns via Wolverine IMessageBus are established. The Billing module follows the same vertical-slice architecture (Domain/Application/Contracts/Infrastructure/Presentation) as all other modules.

**Primary recommendation:** Build the billing domain model with Invoice as the aggregate root containing InvoiceLineItems, a separate Payment entity linked to invoices, and a CashierShift aggregate for shift management. Use QuestPDF for PDF invoice/receipt/e-invoice/shift report generation. Cross-module charge collection via Contracts queries through Wolverine message bus. Manual approval workflows for discounts/refunds using a simple status-based pattern (no complex queue system -- the clinic has ~8 staff with manager often present).

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Progressively built invoice: invoice starts when visit begins, charges added as each service is performed (refraction, exam, drugs dispensed, optical orders, etc.). Cashier sees accumulated total at checkout
- Line items grouped by department on printed invoice: Kham benh, Duoc pham, Kinh, Dieu tri -- patient sees which charges come from where (standard Vietnamese clinic convention)
- Internal revenue allocation tracked per department on each line item (not shown to patient, used for reporting)
- E-invoice output: both PDF (hoa don dien tu format for printing/filing) AND structured data export (JSON/XML for MISA import). Phase 1 approach = manual export, no MISA API
- All payment methods use manual confirmation -- no API integration with VNPay/MoMo/ZaloPay or POS terminals for v1
- QR payment: cashier selects method, patient scans clinic's static QR, cashier manually confirms receipt
- Multi-method split payment supported: cashier can split payment across methods (e.g., 500k cash + 300k QR). Each payment recorded separately against the invoice
- System enforces 50/50 rule: 2nd payment must be received before mid-course session (5-session -> before session 3, 3-session -> before session 2)
- Both percentage and fixed-amount discounts supported
- Discounts can apply per line item OR per invoice total -- cashier has both options
- Refund approval requires manager/owner approval with full audit trail
- Price change audit log: all price changes tracked with who, when, old/new values (FIN-09)
- Shift definition: pre-configured templates as defaults (Morning, Afternoon matching clinic hours), cashier can adjust start/end times when opening
- Cash reconciliation at shift close: cashier enters physical cash count, system compares against expected cash (opening balance + cash received - cash refunds), shows discrepancy with manager note field
- Printable PDF shift report: revenue by payment method, transaction count, cash discrepancy, notes -- uses QuestPDF (already integrated from Phase 5)

### Claude's Discretion
- OTC sale invoice data model (nullable VisitId vs separate invoice type)
- Invoice numbering format and sequence
- Card payment recording details
- Treatment package payment tracking approach (two invoices vs two payments on one invoice)
- Manager approval workflow style (in-app queue vs PIN override)
- Refund scope (partial vs full)
- Concurrent shift support
- Opening cash balance entry workflow
- Tax calculation display (if applicable for Vietnamese clinic invoices)
- Loading states and error handling
- Cashier dashboard layout and navigation

### Deferred Ideas (OUT OF SCOPE)
- VIP membership auto-discounts -- deferred to v2 (VIP-01 through VIP-06)
- Revenue dashboards and reporting -- deferred to v2 (RPT-01 through RPT-08)
- MISA API auto-sync -- v1 is manual export, v2 considers API integration
- Payment gateway API integration (VNPay/MoMo/ZaloPay) -- v1 uses manual confirmation
- POS terminal integration for card payments -- v1 uses manual entry
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| FIN-01 | Single consolidated invoice per visit with charges from all departments | Invoice aggregate with InvoiceLineItem entities, Department enum for grouping, cross-module Contracts queries for charge collection |
| FIN-02 | Internal revenue allocation per department on each invoice line item | Department field on InvoiceLineItem, not exposed in patient-facing PDF but stored for reporting |
| FIN-03 | Payment methods: cash, bank transfer, QR (VNPay/MoMo/ZaloPay), card (Visa/MC) | PaymentMethod enum, Payment entity linked to Invoice, manual confirmation pattern |
| FIN-04 | E-invoice (hoa don dien tu) per Vietnamese tax law | QuestPDF PDF generation + JSON/XML structured export, Vietnamese e-invoice format per Decree 123/2020 |
| FIN-05 | Treatment package payments: full upfront or 50/50 split | Single invoice with two Payment records (recommendation), TreatmentPackagePaymentId linking |
| FIN-06 | 50/50 split enforcement: 2nd payment before mid-course session | Domain rule on payment validation, cross-module query to Treatment for session count |
| FIN-07 | Manual discounts require manager approval | Discount entity with ApprovalStatus, manager PIN override pattern (recommendation) |
| FIN-08 | Refund processing requires manager/owner approval with full audit trail | Refund entity with approval workflow, IAuditable for audit interceptor tracking |
| FIN-09 | Price change audit log (who, when, old/new values) | IAuditable on all price-bearing entities, existing AuditInterceptor captures field-level changes automatically |
| FIN-10 | Shift management: define shifts, assign staff, track revenue per shift, cash reconciliation | CashierShift aggregate, ShiftTemplate seed data, reconciliation logic |
</phase_requirements>

## Standard Stack

### Core (Backend)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET 10 | 10.0 | Runtime framework | Already in use across all modules |
| EF Core | 10.0 | ORM with billing schema | Schema-per-module pattern established |
| FluentValidation | 12.x | Command validation | Handler-invoked pattern established |
| WolverineFx | latest | Message bus for cross-module queries | Cross-module Contracts pattern established |
| QuestPDF | 2026.x (Community) | PDF generation for invoices, receipts, e-invoices, shift reports | Already integrated in Phase 5 with Vietnamese font support |
| NSubstitute | latest | Test mocking | Used across all unit test projects |
| FluentAssertions | latest | Test assertions | Used across all unit test projects |

### Core (Frontend)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| React | 19.x | UI framework | Already in use |
| TanStack Router | latest | File-based routing | Already in use |
| TanStack Query | latest | Server state management | Query key factories, mutation pattern established |
| React Hook Form + Zod | latest | Form management | zodResolver pattern established |
| shadcn/ui | latest | UI components | Wrapper import pattern established |
| @tabler/icons-react | latest | Icon library | Already in use (IconReceipt for billing) |
| react-i18next | latest | Internationalization | Namespace-per-module pattern |
| sonner | latest | Toast notifications | onError toast pattern established |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Text.Json | built-in | JSON serialization for MISA export | E-invoice structured data export |
| System.Xml.Linq | built-in | XML generation for MISA export | Alternative export format |
| date-fns | latest | Date formatting | Vietnamese dd/MM/yyyy locale, shift time display |
| openapi-fetch | latest | API client | Standard api.GET/POST/PUT pattern |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Manager PIN override | In-app approval queue | PIN is simpler for ~8 staff clinic where manager is often present; queue adds unnecessary complexity |
| Two payments on one invoice | Two linked invoices for 50/50 split | Single invoice with multiple payments is cleaner data model, avoids cross-invoice reconciliation |
| Nullable VisitId on Invoice | Separate OTCInvoice type | Nullable VisitId keeps one Invoice table; OTC sales are minority and don't justify a separate entity hierarchy |
| Invoice number via application code | SQL sequence | Application-level (MAX+1 per year) matches PatientCode pattern established in Phase 2; allows year-boundary reset |

## Architecture Patterns

### Recommended Project Structure (Backend)
```
Modules/Billing/
├── Billing.Domain/
│   ├── Entities/
│   │   ├── Invoice.cs              # Aggregate root
│   │   ├── InvoiceLineItem.cs      # Child entity
│   │   ├── Payment.cs              # Linked entity
│   │   ├── Discount.cs             # Child entity with approval
│   │   ├── Refund.cs               # Entity with approval workflow
│   │   ├── CashierShift.cs         # Aggregate root
│   │   └── ShiftTemplate.cs        # Configuration entity
│   ├── Enums/
│   │   ├── InvoiceStatus.cs        # Draft, Finalized, Voided
│   │   ├── PaymentMethod.cs        # Cash, BankTransfer, QR, Card
│   │   ├── PaymentStatus.cs        # Pending, Confirmed, Refunded
│   │   ├── Department.cs           # Medical, Pharmacy, Optical, Treatment
│   │   ├── DiscountType.cs         # Percentage, FixedAmount
│   │   ├── ApprovalStatus.cs       # Pending, Approved, Rejected
│   │   ├── RefundStatus.cs         # Requested, Approved, Processed, Rejected
│   │   └── ShiftStatus.cs          # Open, Closed
│   └── Events/
│       ├── InvoiceFinalizedEvent.cs
│       └── PaymentReceivedEvent.cs
├── Billing.Application/
│   ├── Features/
│   │   ├── CreateInvoice.cs
│   │   ├── AddInvoiceLineItem.cs
│   │   ├── RemoveInvoiceLineItem.cs
│   │   ├── FinalizeInvoice.cs
│   │   ├── RecordPayment.cs
│   │   ├── ApplyDiscount.cs
│   │   ├── ApproveDiscount.cs
│   │   ├── RequestRefund.cs
│   │   ├── ApproveRefund.cs
│   │   ├── ProcessRefund.cs
│   │   ├── OpenShift.cs
│   │   ├── CloseShift.cs
│   │   ├── GetInvoiceById.cs
│   │   ├── GetInvoicesByVisit.cs
│   │   ├── GetCurrentShift.cs
│   │   ├── GetShiftReport.cs
│   │   ├── ExportEInvoice.cs
│   │   └── GetPendingApprovals.cs
│   ├── Interfaces/
│   │   ├── IInvoiceRepository.cs
│   │   ├── IPaymentRepository.cs
│   │   ├── ICashierShiftRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── IBillingDocumentService.cs
│   ├── IoC.cs
│   └── Marker.cs
├── Billing.Contracts/
│   ├── Dtos/
│   │   ├── InvoiceDto.cs
│   │   ├── InvoiceLineItemDto.cs
│   │   ├── PaymentDto.cs
│   │   ├── DiscountDto.cs
│   │   ├── RefundDto.cs
│   │   ├── CashierShiftDto.cs
│   │   ├── ShiftReportDto.cs
│   │   ├── EInvoiceExportDto.cs
│   │   └── GetVisitChargesQuery.cs  # Cross-module query
├── Billing.Infrastructure/
│   ├── BillingDbContext.cs          # Already scaffolded
│   ├── Configurations/
│   │   ├── InvoiceConfiguration.cs
│   │   ├── InvoiceLineItemConfiguration.cs
│   │   ├── PaymentConfiguration.cs
│   │   ├── DiscountConfiguration.cs
│   │   ├── RefundConfiguration.cs
│   │   ├── CashierShiftConfiguration.cs
│   │   └── ShiftTemplateConfiguration.cs
│   ├── Repositories/
│   │   ├── InvoiceRepository.cs
│   │   ├── PaymentRepository.cs
│   │   └── CashierShiftRepository.cs
│   ├── Documents/
│   │   ├── InvoiceDocument.cs       # QuestPDF invoice PDF
│   │   ├── ReceiptDocument.cs       # QuestPDF payment receipt
│   │   ├── EInvoiceDocument.cs      # QuestPDF e-invoice PDF
│   │   └── ShiftReportDocument.cs   # QuestPDF shift summary
│   ├── Services/
│   │   ├── BillingDocumentService.cs
│   │   └── EInvoiceExportService.cs # JSON/XML export
│   ├── Seeding/
│   │   └── ShiftTemplateSeeder.cs
│   ├── Migrations/
│   ├── IoC.cs
│   └── UnitOfWork.cs
├── Billing.Presentation/
│   ├── BillingApiEndpoints.cs
│   └── IoC.cs
```

### Recommended Project Structure (Frontend)
```
frontend/src/
├── features/billing/
│   ├── api/
│   │   └── billing-api.ts          # API functions, hooks, query keys
│   └── components/
│       ├── InvoiceView.tsx          # Invoice detail with line items
│       ├── InvoiceLineItemsTable.tsx
│       ├── PaymentForm.tsx          # Multi-method payment entry
│       ├── PaymentMethodSelector.tsx
│       ├── DiscountDialog.tsx       # Apply discount with approval
│       ├── RefundDialog.tsx         # Request refund
│       ├── ApprovalPinDialog.tsx    # Manager PIN entry
│       ├── ShiftOpenDialog.tsx      # Open shift with cash balance
│       ├── ShiftCloseDialog.tsx     # Close shift, cash count
│       ├── ShiftReportView.tsx      # Shift summary display
│       ├── PendingApprovalsPanel.tsx # Discount/refund approvals
│       └── EInvoiceExportButton.tsx # Export e-invoice
├── app/routes/_authenticated/
│   ├── billing/
│   │   ├── index.tsx               # Cashier dashboard
│   │   ├── invoices.$invoiceId.tsx  # Invoice detail
│   │   └── shifts.tsx              # Shift management
```

### Pattern 1: Invoice as Progressive Aggregate
**What:** Invoice is created when a visit begins (or when first charge is added) and accumulates line items as services are performed. The invoice remains in Draft status until the cashier finalizes it at checkout.
**When to use:** Always for visit-based invoices. OTC sales create an invoice directly without a visit.
**Example:**
```csharp
// Domain entity pattern (following established AggregateRoot pattern)
public class Invoice : AggregateRoot, IAuditable
{
    private readonly List<InvoiceLineItem> _lineItems = [];
    private readonly List<Payment> _payments = [];

    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid? VisitId { get; private set; }       // Nullable for OTC sales
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public Guid? CashierShiftId { get; private set; }

    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();

    public void AddLineItem(string description, decimal unitPrice, int quantity,
        Department department, Guid? sourceId = null, string? sourceType = null)
    {
        EnsureDraft();
        var item = new InvoiceLineItem(description, unitPrice, quantity, department, sourceId, sourceType);
        _lineItems.Add(item);
        RecalculateTotals();
        SetUpdatedAt();
    }

    public void Finalize(Guid cashierShiftId)
    {
        if (BalanceDue > 0)
            throw new InvalidOperationException("Cannot finalize invoice with outstanding balance.");
        Status = InvoiceStatus.Finalized;
        CashierShiftId = cashierShiftId;
        SetUpdatedAt();
        AddDomainEvent(new InvoiceFinalizedEvent(Id));
    }

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Invoice is not in draft status.");
    }

    private void RecalculateTotals()
    {
        SubTotal = _lineItems.Sum(i => i.LineTotal);
        TotalAmount = SubTotal - DiscountAmount;
    }
}
```

### Pattern 2: Manager PIN Override for Approvals
**What:** For discount and refund approvals, the manager enters their PIN (a short numeric code) directly in the dialog. This avoids a separate approval queue system which is overkill for a small clinic.
**When to use:** Discount approval (FIN-07), refund approval (FIN-08).
**Example:**
```csharp
// Approval via manager PIN -- keeps it simple for small clinic
public sealed record ApproveDiscountCommand(
    Guid InvoiceId,
    Guid DiscountId,
    string ManagerPin);

// Handler verifies PIN against the manager's stored PIN hash
public static class ApproveDiscountHandler
{
    public static async Task<Result> Handle(
        ApproveDiscountCommand command,
        IInvoiceRepository invoiceRepository,
        IUserRepository userRepository,  // Cross-module via Auth.Contracts
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        // Verify PIN, approve discount, save
    }
}
```

### Pattern 3: Cross-Module Charge Collection
**What:** Billing queries other modules for charges via Contracts queries through Wolverine message bus. Each module exposes a query in its Contracts project that returns charges relevant to a visit.
**When to use:** When building/updating invoice line items from services performed during a visit.
**Example:**
```csharp
// In Clinical.Contracts (new query)
public sealed record GetVisitChargesQuery(Guid VisitId);
public sealed record VisitChargeDto(
    string Description,
    decimal UnitPrice,
    int Quantity,
    int Department,  // Department enum as int
    Guid SourceId,
    string SourceType);

// In Billing.Application handler -- collects charges from all modules
var clinicalCharges = await bus.InvokeAsync<List<VisitChargeDto>>(
    new GetVisitChargesQuery(command.VisitId), ct);
```

### Pattern 4: Payment Recording (Manual Confirmation)
**What:** Each payment is recorded separately against an invoice. For QR/card/bank transfer, the cashier manually confirms receipt and records a reference number.
**When to use:** All payment collection.
**Example:**
```csharp
public class Payment : Entity, IAuditable
{
    public Guid InvoiceId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? ReferenceNumber { get; private set; }  // Bank transfer ref, QR transaction ID
    public string? CardLast4 { get; private set; }        // For card payments
    public string? CardType { get; private set; }         // Visa, MC
    public string? Notes { get; private set; }
    public Guid RecordedById { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public Guid? CashierShiftId { get; private set; }
}
```

### Anti-Patterns to Avoid
- **Direct cross-module DB queries:** Never query Clinical/Pharmacy/Optical DbContexts directly from Billing. Always use Contracts + message bus pattern.
- **Mutable finalized invoices:** Once finalized, an invoice should not be modified. Corrections are handled via credit notes/refunds.
- **Hardcoded prices in billing:** Billing records the price at time of charge, not a reference to a catalog price. Price changes after billing should not retroactively affect existing invoices.
- **Complex approval queue for small clinic:** An in-app approval request/response queue is overengineered for ~8 staff where manager is usually present. PIN override is appropriate.
- **Storing payment gateway responses:** V1 is manual confirmation -- do not build infrastructure for future API responses.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| PDF invoice generation | Custom PDF rendering | QuestPDF (already integrated) | Complex layout, Vietnamese diacritics, pagination handled |
| Field-level audit trail | Custom change tracking | AuditInterceptor + IAuditable | Already captures all field changes on IAuditable entities automatically |
| VND currency formatting | Manual string formatting | Standard decimal with Vietnamese locale formatting | VND has no decimal places, uses dot as thousands separator (1.500.000) |
| Invoice number generation | Manual counter table | Application-level MAX+1 pattern per year | Matches PatientCode pattern (Phase 2), handles year-boundary reset |
| Cross-module data access | Direct DbContext references | Wolverine IMessageBus + Contracts queries | Preserves module isolation, established pattern |
| Form validation | Manual field checking | FluentValidation (backend) + Zod (frontend) | Established pattern with RFC 7807 error mapping |

**Key insight:** The billing domain has many moving parts (invoices, payments, discounts, refunds, shifts) but the project's established patterns (AggregateRoot, IAuditable, vertical slices, cross-module queries) handle most complexity. The biggest risk is over-engineering the approval workflow and payment gateway preparation -- v1 is deliberately simple (manual confirmation, PIN override).

## Common Pitfalls

### Pitfall 1: Premature Payment Gateway Integration
**What goes wrong:** Building abstraction layers for future VNPay/MoMo/ZaloPay API integration when v1 is manual confirmation only.
**Why it happens:** Anticipating v2 requirements and wanting to "prepare" the architecture.
**How to avoid:** Record payment method and manual confirmation only. V2 can add adapter pattern (ARC-01) when gateway APIs are needed.
**Warning signs:** PaymentGateway interfaces, webhook endpoints, transaction status polling.

### Pitfall 2: Cross-Module Circular Dependencies
**What goes wrong:** Billing references Clinical.Domain or Pharmacy.Domain directly for charge data.
**Why it happens:** It seems easier than creating Contracts queries.
**How to avoid:** ALL cross-module data flows through Contracts projects + Wolverine message bus. Billing.Application references Clinical.Contracts, Pharmacy.Contracts, etc.
**Warning signs:** Direct DbContext injection from other modules, references to other modules' Domain projects.

### Pitfall 3: Invoice Totals Drift from Line Items
**What goes wrong:** Invoice total doesn't match sum of line items after discounts/modifications.
**Why it happens:** Totals calculated independently of line items, or discount applied without recalculating.
**How to avoid:** Single RecalculateTotals() method called after every mutation. TotalAmount is always derived from SubTotal - DiscountAmount.
**Warning signs:** Separate "update total" endpoints, manual total entry.

### Pitfall 4: Missing Audit Trail on Financial Changes
**What goes wrong:** Price changes, discounts, or refunds not captured in audit log.
**Why it happens:** Forgetting IAuditable marker on new entities, or modifying prices through direct SQL.
**How to avoid:** Mark all billing entities with IAuditable. The existing AuditInterceptor automatically captures field-level changes. Never bypass EF Core for financial data changes.
**Warning signs:** Entities without IAuditable, raw SQL UPDATE statements for price fields.

### Pitfall 5: E-Invoice Format Non-Compliance
**What goes wrong:** Generated e-invoice missing required fields per Vietnamese tax law.
**Why it happens:** Not following Decree 123/2020/ND-CP and Circular 78/2021 (now Circular 32/2025) requirements.
**How to avoid:** Include all mandatory fields: seller info (name, tax code, address), buyer info (name, tax code if business), item description, quantity, unit price, total, tax amount. Vietnamese language primary.
**Warning signs:** Missing tax code fields, no Vietnamese text, no sequential invoice numbering.

### Pitfall 6: Shift Cash Reconciliation Race Conditions
**What goes wrong:** Payment recorded between cash count and shift close, causing discrepancy.
**Why it happens:** No atomicity between counting cash and closing shift.
**How to avoid:** When closing a shift, lock the shift from accepting new payments first, then calculate expected cash, then record physical count and close.
**Warning signs:** "Close shift" and "record payment" as independent operations without coordination.

## Code Examples

### Invoice Domain Entity (Full Pattern)
```csharp
// Source: Established AggregateRoot pattern from Phase 1-5
public class Invoice : AggregateRoot, IAuditable
{
    private readonly List<InvoiceLineItem> _lineItems = [];
    private readonly List<Payment> _payments = [];
    private readonly List<Discount> _discounts = [];

    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid? VisitId { get; private set; }
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public decimal SubTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public Guid? CashierShiftId { get; private set; }
    public Guid? FinalizedById { get; private set; }
    public DateTime? FinalizedAt { get; private set; }

    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();
    public IReadOnlyList<Discount> Discounts => _discounts.AsReadOnly();

    public decimal BalanceDue => TotalAmount - PaidAmount;
    public bool IsFullyPaid => BalanceDue <= 0;

    private Invoice() { }

    public static Invoice Create(
        string invoiceNumber,
        Guid patientId,
        string patientName,
        Guid? visitId,
        BranchId branchId)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            PatientId = patientId,
            PatientName = patientName,
            VisitId = visitId,
            Status = InvoiceStatus.Draft
        };
        invoice.SetBranchId(branchId);
        return invoice;
    }

    public void AddLineItem(
        string description,
        string? descriptionVi,
        decimal unitPrice,
        int quantity,
        Department department,
        Guid? sourceId = null,
        string? sourceType = null)
    {
        EnsureDraft();
        var item = InvoiceLineItem.Create(
            description, descriptionVi, unitPrice, quantity, department, sourceId, sourceType);
        _lineItems.Add(item);
        RecalculateTotals();
        SetUpdatedAt();
    }

    public void RecordPayment(Payment payment)
    {
        if (Status == InvoiceStatus.Voided)
            throw new InvalidOperationException("Cannot add payment to voided invoice.");
        _payments.Add(payment);
        PaidAmount = _payments.Where(p => p.Status == PaymentStatus.Confirmed)
            .Sum(p => p.Amount);
        SetUpdatedAt();
    }

    public void Finalize(Guid cashierShiftId, Guid userId)
    {
        EnsureDraft();
        if (!IsFullyPaid)
            throw new InvalidOperationException("Cannot finalize invoice with outstanding balance.");
        Status = InvoiceStatus.Finalized;
        CashierShiftId = cashierShiftId;
        FinalizedById = userId;
        FinalizedAt = DateTime.UtcNow;
        SetUpdatedAt();
        AddDomainEvent(new InvoiceFinalizedEvent(Id, InvoiceNumber, TotalAmount));
    }

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Invoice is not in draft status.");
    }

    private void RecalculateTotals()
    {
        SubTotal = _lineItems.Sum(i => i.LineTotal);
        DiscountTotal = _discounts
            .Where(d => d.ApprovalStatus == ApprovalStatus.Approved)
            .Sum(d => d.CalculatedAmount);
        TotalAmount = SubTotal - DiscountTotal;
        if (TotalAmount < 0) TotalAmount = 0;
    }
}
```

### EF Core Configuration Pattern
```csharp
// Source: Established configuration pattern from Phase 2-5
public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InvoiceNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(e => e.InvoiceNumber).IsUnique();

        builder.Property(e => e.PatientName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.SubTotal).HasPrecision(18, 0);      // VND has no decimals
        builder.Property(e => e.DiscountTotal).HasPrecision(18, 0);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 0);
        builder.Property(e => e.PaidAmount).HasPrecision(18, 0);

        builder.HasMany(e => e.LineItems)
            .WithOne()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Payments)
            .WithOne()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Discounts)
            .WithOne()
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation via backing field for DDD
        builder.Navigation(e => e.LineItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(e => e.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(e => e.Discounts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Global query filter for multi-branch and soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
```

### Frontend API Hook Pattern
```typescript
// Source: Established pharmacy-api.ts pattern from Phase 5
export const billingKeys = {
  all: ["billing"] as const,
  invoices: () => [...billingKeys.all, "invoices"] as const,
  invoice: (id: string) => [...billingKeys.all, "invoice", id] as const,
  visitInvoice: (visitId: string) => [...billingKeys.all, "visit", visitId] as const,
  shifts: () => [...billingKeys.all, "shifts"] as const,
  currentShift: () => [...billingKeys.all, "currentShift"] as const,
  pendingApprovals: () => [...billingKeys.all, "pendingApprovals"] as const,
}

export function useVisitInvoice(visitId: string) {
  return useQuery({
    queryKey: billingKeys.visitInvoice(visitId),
    queryFn: () => getInvoiceByVisit(visitId),
    enabled: !!visitId,
  })
}

export function useRecordPayment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (command: RecordPaymentCommand) => recordPayment(command),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: billingKeys.invoice(variables.invoiceId) })
      queryClient.invalidateQueries({ queryKey: billingKeys.currentShift() })
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}
```

### QuestPDF Invoice Document Pattern
```csharp
// Source: Established DrugPrescriptionDocument pattern from Phase 5
public sealed class InvoiceDocument : IDocument
{
    private readonly InvoiceData _data;
    private readonly ClinicHeaderData _header;

    public InvoiceDocument(InvoiceData data, ClinicHeaderData header)
    {
        _data = data;
        _header = header;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(15, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Noto Sans"));

            page.Header().Component(new ClinicHeaderComponent(_header));

            page.Content().PaddingTop(10).Column(col =>
            {
                col.Item().AlignCenter().Text("HOA DON BAN HANG").FontSize(14).Bold();
                // ... grouped by department (Kham benh, Duoc pham, Kinh, Dieu tri)
                // ... payment summary
                // ... cashier signature
            });
        });
    }
}
```

### VND Currency Formatting
```typescript
// Vietnamese Dong formatting (no decimal places, dot thousands separator)
export function formatVND(amount: number): string {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(amount)
}
// Example: formatVND(1500000) => "1.500.000 d"
```

## Discretion Recommendations

Based on the codebase patterns and domain context, here are my recommendations for the Claude's Discretion items:

### OTC Sale Invoice Data Model
**Recommendation:** Nullable VisitId on Invoice. OTC pharmacy sales create an Invoice with VisitId = null. This keeps a single Invoice table and avoids a parallel invoice hierarchy. The Pharmacy module's dispensing handler can create an invoice directly for walk-in OTC sales.

### Invoice Numbering Format
**Recommendation:** Format: `HD-YYYY-NNNNN` (e.g., HD-2026-00001). Application-level MAX+1 per year, matching the PatientCode pattern from Phase 2. Reset numbering each year. Store the sequence number separately for efficient MAX+1 queries.

### Card Payment Recording
**Recommendation:** Record card type (Visa/MC as enum), last 4 digits (string), and external POS reference number. No card processing -- cashier uses an external POS terminal and records the details manually.

### Treatment Package Payment Tracking
**Recommendation:** Single invoice with two Payment records. The first Payment covers 50%, the second covers the remaining 50%. A TreatmentPackageId field on Invoice links it to the treatment package. The domain enforces that the second payment must be received before the mid-course session via a cross-module query to Treatment module.

### Manager Approval Workflow
**Recommendation:** Manager PIN override. A 4-6 digit PIN stored as a hashed field on the User entity (via Auth module). When a cashier applies a discount or requests a refund, the manager enters their PIN in a dialog. This is validated in real-time. Simpler than an async approval queue for a clinic where the manager is typically present.

### Refund Scope
**Recommendation:** Support both partial (per line item) and full invoice refunds. Vietnamese e-invoice regulations allow for adjustment invoices (hoa don dieu chinh) which can be partial. The Refund entity references specific InvoiceLineItem IDs and amounts for partial refunds, or the entire invoice amount for full refunds.

### Concurrent Shift Support
**Recommendation:** Allow only one open shift per branch at a time. The data model includes BranchId on CashierShift for future multi-branch, but the business rule enforces single concurrent shift per branch. This matches the clinic's single-cashier operation.

### Opening Cash Balance Entry
**Recommendation:** When opening a shift, the cashier enters the opening cash balance (carried over from previous shift's closing balance as a suggested default). This is stored on the CashierShift entity and used as the baseline for cash reconciliation at shift close.

### Tax Calculation
**Recommendation:** Vietnamese clinic invoices typically include VAT at 8% for healthcare services (per Vietnamese tax law). Display pre-tax amount, VAT amount, and total on the invoice. However, for v1, make VAT rate configurable (stored in clinic settings or as a constant) since some private clinics have different tax arrangements. The e-invoice must clearly separate pre-tax, tax rate, and tax amount.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Circular 78/2021 e-invoice format | Circular 32/2025 e-invoice format | June 2025 | Updated e-invoice field requirements; new regulations effective from June 1, 2025 |
| Paper invoices (hoa don giay) | Mandatory e-invoices (hoa don dien tu) | 2022 (Decree 123/2020) | All businesses must use electronic invoices |
| MISA desktop-only | MISA AMIS Cloud + meInvoice | 2023+ | Cloud-based accounting with e-invoice integration |

**Deprecated/outdated:**
- Paper invoices: Fully replaced by e-invoices per Decree 123/2020/ND-CP
- Circular 78/2021: Replaced by Circular 32/2025/TT-BTC effective June 2025

## Vietnamese E-Invoice Required Fields

Per Decree 123/2020/ND-CP and Circular 32/2025/TT-BTC:

| Field | Vietnamese | Required | Notes |
|-------|-----------|----------|-------|
| Invoice name | Ten hoa don | Yes | "HOA DON GIA TRI GIA TANG" or "HOA DON BAN HANG" |
| Invoice template symbol | Ky hieu mau so hoa don | Yes | e.g., 1C26TBB |
| Invoice symbol | Ky hieu hoa don | Yes | e.g., AA/26E |
| Invoice number | So hoa don | Yes | Sequential, unique per symbol |
| Seller name | Ten nguoi ban | Yes | From clinic settings |
| Seller tax code | Ma so thue nguoi ban | Yes | From clinic settings |
| Seller address | Dia chi nguoi ban | Yes | From clinic settings |
| Buyer name | Ten nguoi mua | Yes | Patient name |
| Buyer tax code | Ma so thue nguoi mua | Conditional | Required if buyer is a business |
| Item description | Ten hang hoa, dich vu | Yes | Service/product description |
| Unit | Don vi tinh | Yes | e.g., Lan, Chai, Cai |
| Quantity | So luong | Yes | |
| Unit price | Don gia | Yes | |
| Amount | Thanh tien | Yes | Quantity x Unit Price |
| Tax rate | Thue suat | Yes | 8% for healthcare, or "KCT" if not subject |
| Tax amount | Tien thue | Yes | |
| Total amount | Tong tien thanh toan | Yes | Including tax |
| Currency | Dong tien | Yes | VND |
| Date of issue | Ngay, thang, nam | Yes | |

## Open Questions

1. **Manager PIN Storage Location**
   - What we know: PIN needs to be stored as a hash on User entity in Auth module
   - What's unclear: Whether to extend the existing User entity or create a separate ManagerPin entity in Auth
   - Recommendation: Add ApprovalPinHash nullable field to User entity in Auth module. Only set for Manager/Owner roles. Simpler than a separate entity.

2. **Treatment Module Dependency (Phase 9)**
   - What we know: FIN-05 and FIN-06 require treatment package data (session count, package price, mid-course session number) which is Phase 9
   - What's unclear: How to implement 50/50 enforcement when Treatment module isn't built yet
   - Recommendation: Define the cross-module Contracts query interface now (GetTreatmentPackageQuery in Treatment.Contracts). Implement with stub data or skip enforcement until Phase 9 is complete. The data model and payment split logic can be built now.

3. **Optical Module Dependency (Phase 8)**
   - What we know: Invoices should include optical charges, but Optical is Phase 8
   - What's unclear: Whether to implement optical charge collection stubs now
   - Recommendation: Define GetOpticalChargesQuery in Optical.Contracts. Billing handler gracefully handles null/empty responses. Optical charges will flow automatically once Phase 8 is implemented.

4. **VAT Rate for Private Eye Clinic**
   - What we know: Vietnamese healthcare services are generally subject to 8% VAT
   - What's unclear: Whether this specific private clinic has VAT exemptions or special arrangements
   - Recommendation: Make VAT rate configurable. Default to 8% but allow clinic settings to override. Some private clinics may have different arrangements.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + FluentAssertions + NSubstitute |
| Config file | backend/tests/Billing.Unit.Tests/Billing.Unit.Tests.csproj (Wave 0) |
| Quick run command | `dotnet test backend/tests/Billing.Unit.Tests --no-build -v q` |
| Full suite command | `dotnet test backend/Ganka28.slnx --no-build -v q` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| FIN-01 | Consolidated invoice with multi-department line items | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "InvoiceLineItem" -v q` | Wave 0 |
| FIN-02 | Internal department revenue allocation per line item | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "DepartmentAllocation" -v q` | Wave 0 |
| FIN-03 | Multiple payment method recording | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "Payment" -v q` | Wave 0 |
| FIN-04 | E-invoice generation with required fields | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "EInvoice" -v q` | Wave 0 |
| FIN-05 | Treatment package payment (full/50-50) | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "TreatmentPayment" -v q` | Wave 0 |
| FIN-06 | 50/50 split enforcement before mid-course | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "SplitEnforcement" -v q` | Wave 0 |
| FIN-07 | Discount requires manager approval | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "Discount" -v q` | Wave 0 |
| FIN-08 | Refund with manager/owner approval audit trail | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "Refund" -v q` | Wave 0 |
| FIN-09 | Price change audit log | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "PriceAudit" -v q` | Wave 0 |
| FIN-10 | Shift management and cash reconciliation | unit | `dotnet test backend/tests/Billing.Unit.Tests --filter "Shift" -v q` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Billing.Unit.Tests --no-build -v q`
- **Per wave merge:** `dotnet test backend/Ganka28.slnx --no-build -v q`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `backend/tests/Billing.Unit.Tests/Billing.Unit.Tests.csproj` -- new test project
- [ ] `backend/tests/Billing.Unit.Tests/Features/InvoiceCrudHandlerTests.cs` -- covers FIN-01, FIN-02
- [ ] `backend/tests/Billing.Unit.Tests/Features/PaymentHandlerTests.cs` -- covers FIN-03, FIN-05
- [ ] `backend/tests/Billing.Unit.Tests/Features/DiscountHandlerTests.cs` -- covers FIN-07
- [ ] `backend/tests/Billing.Unit.Tests/Features/RefundHandlerTests.cs` -- covers FIN-08
- [ ] `backend/tests/Billing.Unit.Tests/Features/ShiftHandlerTests.cs` -- covers FIN-10
- [ ] `backend/tests/Billing.Unit.Tests/Domain/InvoiceTests.cs` -- domain logic tests for aggregate
- [ ] `backend/tests/Billing.Unit.Tests/Domain/CashierShiftTests.cs` -- shift domain tests

## Sources

### Primary (HIGH confidence)
- Existing codebase analysis -- BillingDbContext, AggregateRoot, Entity, IAuditable, AuditInterceptor patterns
- Clinical.Contracts, Patient.Contracts -- cross-module query pattern
- DrugPrescriptionDocument, ClinicHeaderComponent -- QuestPDF document generation pattern
- pharmacy-api.ts, clinical-api.ts -- frontend API hook pattern
- AppSidebar.tsx -- billing sidebar item (currently disabled)
- ICurrentUser, CurrentUser -- user identity and branch ID pattern
- PermissionModule.Billing -- already defined in Auth.Domain
- AuthDataSeeder -- billing permissions already seeded for cashier, doctor, accountant roles

### Secondary (MEDIUM confidence)
- [Vietnamese e-invoice requirements](https://www.vietnam-briefing.com/news/e-invoice-compliance-in-vietnam-regulations-requirements-and-best-practices.html/) -- Decree 123/2020, Circular 32/2025
- [Circular 78 invoice templates](https://1c.com.vn/en/news/mau-hoa-don-dien-tu) -- 7 e-invoice template types
- [MISA meInvoice integration](https://viindoo.com/apps/app/17.0/l10n_vn_viin_accounting_meinvoice) -- XML export format for MISA
- [QuestPDF documentation](https://www.questpdf.com/) -- fluent API, component pattern, invoice examples

### Tertiary (LOW confidence)
- Vietnamese VAT rate for private clinics -- 8% is standard but clinic-specific arrangements may vary (needs validation with clinic owner)
- Circular 32/2025 specific field changes vs Circular 78/2021 -- effective June 2025, exact differences need validation

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all libraries already in use in the project
- Architecture: HIGH -- patterns directly replicated from established modules (Phase 1-5)
- Domain model: HIGH -- clear requirements from CONTEXT.md with detailed decisions
- E-invoice format: MEDIUM -- Vietnamese regulations confirmed via web search, specific field requirements from Decree 123/2020
- Pitfalls: HIGH -- based on established patterns and common billing system issues
- Tax/VAT: LOW -- 8% standard rate needs validation with clinic owner

**Research date:** 2026-03-06
**Valid until:** 2026-04-06 (stable -- established patterns, no fast-moving dependencies)
