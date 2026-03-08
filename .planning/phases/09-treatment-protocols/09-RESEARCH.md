# Phase 9: Treatment Protocols - Research

**Researched:** 2026-03-08
**Domain:** Treatment protocol management with session tracking, OSDI monitoring, consumable deduction, and approval workflows
**Confidence:** HIGH

## Summary

Phase 9 implements the core treatment protocol management system for IPL, LLLT, and lid care treatment packages. This is the clinic's primary differentiator and requires building a new bounded context (Treatment module) that integrates with Clinical (OSDI history), Pharmacy (consumable deduction), Billing (treatment charges), and Auth (doctor-only permissions).

The Treatment module backend scaffolding already exists with 4 empty projects (Domain, Application, Contracts, Infrastructure) plus a configured DbContext with "treatment" schema and Wolverine handler discovery. The frontend has no `treatments` feature directory yet but has a disabled sidebar entry ready to activate. All cross-module infrastructure (domain events, Wolverine message bus, FluentValidation, Result pattern) is battle-tested across 8 completed phases.

**Primary recommendation:** Follow the established vertical-slice pattern exactly. The Treatment module needs: (1) Domain entities as aggregate roots (TreatmentProtocol template + TreatmentPackage per-patient + TreatmentSession), (2) Application handlers with FluentValidation, (3) EF Core configurations in Infrastructure, (4) Minimal API endpoints in a new Treatment.Presentation project, and (5) Frontend feature directory with TanStack Query hooks and shadcn/ui components.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Global protocol templates: doctors configure reusable templates (e.g., "Standard IPL 4-session", "LLLT 6-session dry eye") with default parameters and pricing
- When assigning to a patient, doctor can customize session count, parameters, and pricing -- template is starting point, not rigid
- Packages can be created from both the visit detail page (post-exam) and the patient profile Treatments tab
- Template defines default package price and per-session price, overridable per patient when creating the package
- Only users with Doctor role can create or modify protocol templates and treatment packages (TRT-10)
- Each treatment type has its own defined fields: IPL (energy J/cm2, pulse count, spot size, treatment zones), LLLT (wavelength nm, power mW, duration min, treatment area), Lid care (procedure steps/checklist, products used, duration)
- Parameters on the template serve as defaults; actual values recorded per session can differ
- Treatment sessions are standalone by default (own record in Treatment module, not a Visit child entity)
- Optional visit link: session can be linked to that visit's VisitId
- Per session records: device parameters used, OSDI score, clinical observations (freeform text), optional photos, consumables used
- Either doctor or technician can record sessions -- system logs who performed it (PerformedBy)
- OSDI capture: both inline recording during session AND patient self-fill via QR/link (reuses Phase 4 public OSDI page pattern)
- Session is immutable after completion (similar to visit sign-off pattern)
- Dedicated /treatments page showing all active packages across patients
- Patient profile gets a "Treatments" tab
- "Due Soon" section at top of /treatments page: surfaces sessions whose minimum interval has passed
- Version history for protocol modifications: each change creates a version snapshot
- Cancellation workflow: doctor/staff requests cancellation -> status changes to "Pending Cancellation" -> manager approves/rejects
- Manager approval via dedicated approval queue page
- Configurable minimum intervals per treatment type: IPL 2-4 weeks, LLLT 1-2 weeks, lid care 1-2 weeks
- Intervals set on the protocol template, adjustable per patient package
- System warns but allows override when scheduling a session before minimum interval
- Package statuses: Active, Paused, Pending Cancellation, Cancelled, Switched, Completed
- Auto-marks as "Completed" when all sessions are done (TRT-04)
- Multiple concurrent active packages per patient supported (TRT-06)

### Claude's Discretion
- Treatments page layout (DataTable vs Kanban)
- Package detail progress visualization (timeline vs cards vs hybrid)
- Treatment type switching data model approach (close+create vs convert in-place)
- Cancellation deduction scope (per-type vs global)
- Consumable selection UI during session recording
- Session photo upload UX (reuse Phase 4 image upload pattern or simpler inline)
- Protocol template management page layout
- Default interval values and configuration UI
- Loading states and error handling

### Deferred Ideas (OUT OF SCOPE)
- Myopia Control treatment protocols -- post-launch (same template engine, different structured fields)
- Ortho-K follow-up auto-scheduling -- v2 notification feature (NTF-03)
- Treatment effectiveness reporting dashboards (OSDI improvement trends across patient cohorts) -- v2 (RPT-06)
- Per-patient treatment progress report export -- v2 (RPT-07)
- Zalo OA treatment reminders -- v2 (NTF-03)
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| TRT-01 | Doctor can create IPL, LLLT, or lid care treatment packages with 1-6 sessions and flexible pricing (per-session or per-package) | Domain model: TreatmentProtocol (template) + TreatmentPackage (per-patient). PricingMode enum (PerSession/PerPackage). Aggregate root with factory method, FluentValidation for session count 1-6 |
| TRT-02 | System tracks sessions completed and remaining per treatment course | TreatmentPackage tracks TotalSessions and has computed SessionsCompleted/SessionsRemaining from child TreatmentSession collection. Auto-recalculated on session completion |
| TRT-03 | System records OSDI score at each treatment session | TreatmentSession entity stores OsdiScore + OsdiSeverity. Reuses Clinical.Contracts GetOsdiHistoryQuery for trend display. Inline recording + QR self-fill via existing public OSDI pattern |
| TRT-04 | System auto-marks treatment course as "Completed" when all sessions are done | Domain logic in TreatmentPackage.CompleteSession() -- checks if SessionsCompleted == TotalSessions, auto-transitions Status to Completed |
| TRT-05 | System enforces minimum interval between sessions (configurable per type) | MinIntervalDays stored on TreatmentProtocol template and overridable on TreatmentPackage. Validation in RecordSessionCommand warns but allows override with reason logged |
| TRT-06 | Patient can have multiple active treatment courses simultaneously | No unique constraint on (PatientId, Status=Active). Query patterns filter by PatientId returning list. "Due Soon" aggregates across all active packages |
| TRT-07 | Doctor can modify treatment protocol mid-course (add/remove sessions, change parameters) | ProtocolVersion entity captures snapshot before each modification. TreatmentPackage.Modify() creates version record with what changed, who, when, reason |
| TRT-08 | Doctor can switch patient from one treatment type to another mid-course | Close-and-create approach: mark old package as "Switched" status, create new package with remaining session count. Preserves billing trail and completed session history |
| TRT-09 | Manager can process treatment cancellation with configurable refund deduction (10-20% fee) | Follows Billing.Refund approval pattern. CancellationRequest entity with Requested/Approved/Rejected status. Manager PIN verification via Auth cross-module query. Per-type deduction percentage stored on TreatmentProtocol |
| TRT-10 | Only users with Doctor role can create or modify treatment protocols | Permission system already seeded: PermissionModule.Treatment with all 6 actions, Doctor role has Treatment.Manage. Use RequireAuthorization with permission claims on endpoints |
| TRT-11 | System records consumables used per treatment session (linked to consumables warehouse) | SessionConsumable child entity on TreatmentSession. Domain event TreatmentSessionCompletedEvent triggers Pharmacy module handler to auto-deduct from ConsumableItem/ConsumableBatch using existing FEFO pattern |
</phase_requirements>

## Standard Stack

### Core (Backend)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET | 10.0 | Runtime | Already in use, all modules target net10.0 |
| EF Core | 10.0.* | ORM with SQL Server | TreatmentDbContext already scaffolded with "treatment" schema |
| Wolverine FX | 5.* | Message bus, handler discovery, transactional outbox | Used by all 8 existing modules for CQRS handlers and cross-module events |
| FluentValidation | 12.* | Command/query validation | Registered per-module via IoC, validated in handlers |
| xUnit | 2.* | Unit test framework | All existing test projects use xUnit |
| NSubstitute | 5.* | Mocking framework | Standard across all unit test projects |
| FluentAssertions | 8.* | Assertion library | Standard across all unit test projects |

### Core (Frontend)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| React | 19 | UI framework | Already in use |
| TanStack Router | 1.114+ | File-based routing | Existing routing pattern with /_authenticated/ guards |
| TanStack React Query | 5.64+ | Server state management | queryKey factories, mutation with cache invalidation |
| TanStack React Table | 8.21+ | DataTable component | Existing DataTable wrapper in shared/components |
| React Hook Form | 7.54+ | Form management | zodResolver with Controller pattern |
| Zod | 3.24+ | Schema validation | Frontend validation matching backend FluentValidation |
| shadcn/ui (Radix) | latest | UI components | Card, Dialog, Tabs, Select, etc. per CLAUDE.md mandate |
| Recharts | 3.7+ | Charts | For OSDI trend visualization on treatment packages |
| i18next | 24+ | Internationalization | EN/VI translations in public/locales/ |
| Tabler Icons | 3.37+ | Icon library | Consistent with existing UI (IconHeartbeat for treatments) |
| Sonner | 1.7+ | Toast notifications | onError toast pattern on all mutations |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| QuestPDF | 2025.* | PDF generation | Treatment consent forms (PRT-05 already implemented, extend for progress reports in v2) |
| qrcode.react | 4.2+ | QR code display | OSDI self-fill link QR (reuse OsdiSection pattern) |
| date-fns | 4.1+ | Date utilities | Interval calculation, "Due Soon" logic, relative time display |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| DataTable for treatments page | Kanban board | DataTable recommended -- consistent with other list views, better for filtering/sorting, less overhead. "Due Soon" section at top provides the proactive view |
| Close-and-create for type switching | In-place conversion | Close-and-create recommended -- preserves original billing trail, keeps completed session history intact, simpler data model (no need for polymorphic parameter changes) |
| Per-type cancellation deduction | Global setting | Per-type recommended -- stored on TreatmentProtocol template, IPL consumables cost more than lid care, aligns with template-level configuration pattern |
| Timeline visualization for progress | Card grid | Hybrid recommended -- card grid per session with OSDI mini-chart sidebar. Timeline is complex to build; card grid is simple and shows all needed data |

## Architecture Patterns

### Recommended Project Structure

#### Backend
```
backend/src/Modules/Treatment/
  Treatment.Domain/
    Entities/
      TreatmentProtocol.cs         # Template aggregate root
      TreatmentPackage.cs           # Per-patient package aggregate root
      TreatmentSession.cs           # Child entity of TreatmentPackage
      SessionConsumable.cs          # Child entity of TreatmentSession
      ProtocolVersion.cs            # Child entity of TreatmentPackage (version history)
      CancellationRequest.cs        # Child entity of TreatmentPackage
    Enums/
      TreatmentType.cs              # IPL, LLLT, LidCare
      PackageStatus.cs              # Active, Paused, PendingCancellation, Cancelled, Switched, Completed
      PricingMode.cs                # PerSession, PerPackage
      SessionStatus.cs              # Scheduled, InProgress, Completed, Cancelled
    Events/
      TreatmentSessionCompletedEvent.cs   # Cross-module: triggers consumable deduction + billing charge
      TreatmentPackageCompletedEvent.cs   # Internal: auto-completion notification
    ValueObjects/
      IplParameters.cs              # Structured: Energy, PulseCount, SpotSize, TreatmentZones
      LlltParameters.cs             # Structured: Wavelength, Power, Duration, TreatmentArea
      LidCareParameters.cs          # Structured: ProcedureSteps, ProductsUsed, Duration
  Treatment.Application/
    Features/
      # Protocol Templates
      CreateProtocolTemplate.cs
      UpdateProtocolTemplate.cs
      GetProtocolTemplates.cs
      GetProtocolTemplateById.cs
      # Treatment Packages
      CreateTreatmentPackage.cs
      ModifyTreatmentPackage.cs
      SwitchTreatmentType.cs
      PauseTreatmentPackage.cs
      # Sessions
      RecordTreatmentSession.cs
      GetTreatmentSessions.cs
      # Session OSDI
      RecordSessionOsdi.cs
      GenerateSessionOsdiLink.cs
      # Package Queries
      GetPatientTreatments.cs
      GetActiveTreatments.cs
      GetDueSoonSessions.cs
      GetTreatmentPackageById.cs
      # Cancellation
      RequestCancellation.cs
      ApproveCancellation.cs
      RejectCancellation.cs
      GetPendingCancellations.cs
    Interfaces/
      ITreatmentProtocolRepository.cs
      ITreatmentPackageRepository.cs
      IUnitOfWork.cs
    IoC.cs
    Marker.cs                       # Already exists
  Treatment.Contracts/
    Dtos/
      TreatmentPackageDto.cs
      TreatmentSessionDto.cs
      TreatmentProtocolDto.cs
    Queries/
      GetPatientTreatmentsQuery.cs  # Cross-module query for Patient profile tab
      GetTreatmentChargesQuery.cs   # Cross-module query for Billing charge collection
  Treatment.Infrastructure/
    Configurations/
      TreatmentProtocolConfiguration.cs
      TreatmentPackageConfiguration.cs
      TreatmentSessionConfiguration.cs
    Repositories/
      TreatmentProtocolRepository.cs
      TreatmentPackageRepository.cs
    TreatmentDbContext.cs           # Already exists with "treatment" schema
    IoC.cs
  Treatment.Presentation/          # NEW - must be created
    TreatmentApiEndpoints.cs
    IoC.cs
```

#### Frontend
```
frontend/src/features/treatment/
  api/
    treatment-api.ts               # Types + React Query hooks
    treatment-queries.ts           # queryKey factories
  components/
    ProtocolTemplateList.tsx        # Template management DataTable
    ProtocolTemplateForm.tsx        # Create/edit template dialog
    TreatmentPackageForm.tsx        # Create package from template for patient
    TreatmentPackageDetail.tsx      # Package detail with session cards + OSDI trend
    TreatmentSessionForm.tsx        # Record session dialog (structured params per type)
    TreatmentSessionCard.tsx        # Individual session display card
    TreatmentsPage.tsx             # Main /treatments page with DataTable + Due Soon
    DueSoonSection.tsx             # Due Soon alerts at top of treatments page
    PatientTreatmentsTab.tsx       # Treatments tab for patient profile
    CancellationApprovalQueue.tsx   # Manager approval page
    ConsumableSelector.tsx          # Consumable selection during session recording
    OsdiTrendChart.tsx             # Recharts line chart for OSDI scores across sessions
    SessionOsdiCapture.tsx          # Inline OSDI + QR link (reuses OsdiSection pattern)
    ModifyPackageDialog.tsx         # Mid-course modification dialog
    VersionHistoryDialog.tsx        # View protocol modification history
    SwitchTreatmentDialog.tsx       # Switch treatment type dialog
frontend/src/app/routes/_authenticated/
  treatments/
    index.tsx                      # /treatments - main page
    $packageId.tsx                 # /treatments/:packageId - package detail
  treatments.templates.tsx          # /treatments/templates - protocol management
  treatments.approvals.tsx          # /treatments/approvals - cancellation queue
frontend/public/locales/
  en/treatment.json                # English translations
  vi/treatment.json                # Vietnamese translations
```

### Pattern 1: Aggregate Root with Child Entities (TreatmentPackage)
**What:** TreatmentPackage is the primary aggregate root containing TreatmentSessions, ProtocolVersions, and CancellationRequests as child entities accessed through private backing fields.
**When to use:** For all treatment package operations -- sessions and modifications are always accessed through the package aggregate.
**Example:**
```csharp
// Source: Established pattern from Billing.Invoice and Optical.GlassesOrder
public class TreatmentPackage : AggregateRoot, IAuditable
{
    private readonly List<TreatmentSession> _sessions = [];
    private readonly List<ProtocolVersion> _versions = [];
    private CancellationRequest? _cancellationRequest;

    public Guid ProtocolTemplateId { get; private set; }
    public Guid PatientId { get; private set; }
    public string PatientName { get; private set; } = string.Empty;
    public TreatmentType TreatmentType { get; private set; }
    public PackageStatus Status { get; private set; }
    public int TotalSessions { get; private set; }
    public PricingMode PricingMode { get; private set; }
    public decimal PackagePrice { get; private set; }
    public decimal SessionPrice { get; private set; }
    public int MinIntervalDays { get; private set; }
    public string ParametersJson { get; private set; } = "{}"; // Default structured params
    public Guid? VisitId { get; private set; } // Optional link to originating visit
    public Guid CreatedById { get; private set; }

    public IReadOnlyList<TreatmentSession> Sessions => _sessions.AsReadOnly();
    public IReadOnlyList<ProtocolVersion> Versions => _versions.AsReadOnly();
    public CancellationRequest? CancellationRequest => _cancellationRequest;

    public int SessionsCompleted => _sessions.Count(s => s.Status == SessionStatus.Completed);
    public int SessionsRemaining => TotalSessions - SessionsCompleted;
    public bool IsComplete => SessionsCompleted >= TotalSessions;

    private TreatmentPackage() { }

    public static TreatmentPackage Create(/* params */) { /* factory */ }

    public TreatmentSession RecordSession(/* params */)
    {
        EnsureActive();
        // Interval enforcement (warn, not block)
        var session = TreatmentSession.Create(/* params */);
        _sessions.Add(session);

        if (IsComplete)
        {
            Status = PackageStatus.Completed;
            AddDomainEvent(new TreatmentPackageCompletedEvent(Id));
        }

        AddDomainEvent(new TreatmentSessionCompletedEvent(
            Id, session.Id, PatientId, TreatmentType));

        return session;
    }

    public void Modify(/* params, reason */)
    {
        EnsureModifiable();
        // Snapshot current state into ProtocolVersion
        _versions.Add(ProtocolVersion.Create(/* snapshot */));
        // Apply changes
        SetUpdatedAt();
    }

    public void MarkAsSwitched() { Status = PackageStatus.Switched; SetUpdatedAt(); }

    private void EnsureActive()
    {
        if (Status != PackageStatus.Active)
            throw new InvalidOperationException(
                $"Cannot record session on package in '{Status}' status.");
    }

    private void EnsureModifiable()
    {
        if (Status is PackageStatus.Completed or PackageStatus.Cancelled or PackageStatus.Switched)
            throw new InvalidOperationException(
                $"Cannot modify package in '{Status}' status.");
    }
}
```

### Pattern 2: Structured Parameters as JSON Column with Typed Value Objects
**What:** Treatment-type-specific parameters stored as JSON in a single column but with strongly-typed value objects for serialization/deserialization.
**When to use:** For IplParameters, LlltParameters, LidCareParameters -- both on protocol templates and on individual sessions.
**Example:**
```csharp
// Domain value objects
public sealed record IplParameters(
    decimal EnergyJoules,       // J/cm2
    int PulseCount,
    string SpotSize,            // e.g., "8mm", "10mm"
    string[] TreatmentZones);   // e.g., ["Upper lid", "Lower lid", "Periorbital"]

public sealed record LlltParameters(
    decimal WavelengthNm,       // e.g., 810
    decimal PowerMw,            // e.g., 100
    decimal DurationMinutes,    // e.g., 15
    string TreatmentArea);      // e.g., "Bilateral eyelids"

public sealed record LidCareParameters(
    string[] ProcedureSteps,    // Checklist items
    string[] ProductsUsed,
    decimal DurationMinutes);

// EF Core configuration
builder.Property(x => x.ParametersJson)
    .IsRequired()
    .HasMaxLength(4000)
    .HasColumnType("nvarchar(4000)");
// Application layer deserializes based on TreatmentType enum
```

### Pattern 3: Cross-Module Domain Event for Consumable Deduction (TRT-11)
**What:** When a treatment session is completed, a domain event triggers the Pharmacy module to auto-deduct consumables from the consumables warehouse.
**When to use:** On session completion in TreatmentPackage.RecordSession().
**Example:**
```csharp
// Treatment.Domain/Events/TreatmentSessionCompletedEvent.cs
public sealed record TreatmentSessionCompletedEvent(
    Guid PackageId,
    Guid SessionId,
    Guid PatientId,
    TreatmentType TreatmentType,
    List<ConsumableUsage> Consumables) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record ConsumableUsage(Guid ConsumableItemId, int Quantity);

// Handler in Pharmacy.Application (consumed via Wolverine)
// Pharmacy.Application references Treatment.Contracts for the event
public static class DeductTreatmentConsumablesHandler
{
    public static async Task Handle(
        TreatmentSessionCompletedEvent message,
        IConsumableRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        foreach (var usage in message.Consumables)
        {
            var item = await repository.GetByIdAsync(usage.ConsumableItemId, ct);
            // Use FEFO for ExpiryTracked, direct deduction for SimpleStock
            item.RemoveStock(usage.Quantity); // or FEFOAllocator for batch items
        }
        await unitOfWork.SaveChangesAsync(ct);
    }
}
```

### Pattern 4: Cancellation Approval Workflow (TRT-09)
**What:** Multi-step cancellation with manager PIN verification, matching the Billing.Refund approval pattern.
**When to use:** For treatment cancellation requests that require manager approval.
**Example:**
```csharp
// Follows exact same pattern as Billing.Refund and Billing.ApproveRefundHandler
public sealed record RequestCancellationCommand(
    Guid PackageId, string Reason);

public sealed record ApproveCancellationCommand(
    Guid PackageId, Guid ManagerId, string ManagerPin, decimal DeductionPercentage);

// Handler verifies PIN via cross-module query:
var pinResponse = await messageBus.InvokeAsync<VerifyManagerPinResponse>(
    new VerifyManagerPinQuery(command.ManagerId, command.ManagerPin), ct);
```

### Pattern 5: Treatment.Presentation Wiring
**What:** New Treatment.Presentation project must be created and wired into Bootstrapper.
**When to use:** This is a required infrastructure step before any endpoints work.
**Example:**
```csharp
// 1. Create Treatment.Presentation.csproj referencing Treatment.Application + Shared.Presentation
// 2. Create IoC.cs:
public static class IoC
{
    public static IServiceCollection AddTreatmentPresentation(this IServiceCollection services)
    {
        return services;
    }
}

// 3. Create TreatmentApiEndpoints.cs:
public static class TreatmentApiEndpoints
{
    public static IEndpointRouteBuilder MapTreatmentApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/treatments").RequireAuthorization();
        // Endpoints here...
        return app;
    }
}

// 4. Update Bootstrapper Program.cs:
//    - Add: using Treatment.Presentation;
//    - Add: builder.Services.AddTreatmentApplication();
//    - Add: builder.Services.AddTreatmentInfrastructure();
//    - Add: builder.Services.AddTreatmentPresentation();
//    - Add: app.MapTreatmentApiEndpoints();
```

### Anti-Patterns to Avoid
- **Making TreatmentSession a Visit child entity:** Sessions are standalone with an optional VisitId FK. Do NOT make them Visit aggregate children -- this was explicitly decided.
- **Polymorphic entity hierarchy for treatment types:** Do NOT create IplSession, LlltSession, LidCareSession subclasses. Use a single TreatmentSession entity with TreatmentType enum and JSON parameters column. Polymorphism adds EF Core complexity (TPH/TPT) for no benefit.
- **Storing parameters as separate columns per type:** Use JSON column with typed value objects. This keeps the schema clean and extensible (Myopia Control template in v2 adds a new type without schema changes).
- **Blocking session recording when interval not met:** The requirement says "warn but allow override with reason logged." Do NOT enforce intervals as hard constraints.
- **Skipping TDD:** CLAUDE.md mandates strict TDD. Write failing tests first, then implement (red-green-refactor). Minimum 80% coverage.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Consumable deduction with FEFO | Custom deduction logic | Pharmacy.Domain.Services.FEFOAllocator | Already implements batch selection and all-or-nothing semantics |
| Manager PIN verification | Custom auth check | Auth.Contracts VerifyManagerPinQuery via IMessageBus | Cross-module pattern established in Billing.ApproveRefund |
| OSDI score calculation | Custom calculator | Clinical.Application.Features.OsdiCalculator | Already handles the formula: (sum/questions*4)*100 with severity classification |
| OSDI self-fill via QR/token | New public endpoint | Reuse Clinical.Presentation.PublicOsdiEndpoints pattern | Public token with 24h expiry, rate limiting, same questionnaire UX |
| DataTable with sort/filter | Custom table component | shared/components/DataTable with TanStack React Table | Handles column definitions, sorting, filtering, row clicks |
| Collapsible section cards | Custom accordion | VisitSection component from clinical/components | Collapsible card wrapper with header/content pattern |
| Toast notifications on errors | Custom error handling | Sonner toast via onError callback on all React Query mutations | Established pattern across all frontend features |
| Form validation | Custom validators | React Hook Form + Zod + zodResolver | Standard form pattern with Controller components |
| Image upload | Custom upload UI | Reuse ImageUploader pattern from clinical/components | FormData + raw fetch + file type validation |
| RFC 7807 error display | Custom error parser | shared/components/ServerValidationAlert + handleServerValidationError | Standard error display pattern |

**Key insight:** This phase has 11 requirements but leverages patterns already built in 8 prior phases. The novel work is domain modeling (treatment entities, structured parameters, version history) and the treatments-specific UI. Infrastructure, cross-module communication, approval workflows, and OSDI recording are all proven patterns to reuse.

## Common Pitfalls

### Pitfall 1: Forgetting Treatment.Presentation Project
**What goes wrong:** Treatment module has no Presentation project (unlike all other modules). Code compiles but endpoints never register.
**Why it happens:** Treatment was scaffolded as 4 projects; Presentation was noted as "Needs to be created" in CONTEXT.md.
**How to avoid:** First task in the phase MUST create Treatment.Presentation.csproj, wire IoC, and register in Bootstrapper Program.cs.
**Warning signs:** Endpoints return 404 even though handlers exist.

### Pitfall 2: Complex Polymorphic Parameter Handling
**What goes wrong:** Over-engineering parameters as separate C# types per treatment type, leading to complex serialization and EF Core mapping issues.
**Why it happens:** Desire for type safety on structured fields (IPL vs LLLT vs Lid Care parameters).
**How to avoid:** Store as JSON string column (`ParametersJson nvarchar(4000)`). Deserialize to typed records in Application layer based on TreatmentType enum. Value objects for validation, JSON for storage.
**Warning signs:** Multiple migration files for parameter schema changes, complex generic constraints.

### Pitfall 3: Missing Cross-Module Event Handler Registration
**What goes wrong:** TreatmentSessionCompletedEvent fires but Pharmacy never deducts consumables.
**Why it happens:** Wolverine discovers handlers by assembly. If Pharmacy.Application doesn't reference Treatment.Contracts (where the event lives), the handler never gets registered.
**How to avoid:** Pharmacy.Application.csproj must add `<ProjectReference>` to Treatment.Contracts. Verify handler discovery in integration tests.
**Warning signs:** Consumable stock doesn't decrease after session completion. No errors (messages silently dropped).

### Pitfall 4: Version History Breaking Change Detection
**What goes wrong:** Protocol modification creates version records but doesn't capture the full diff of what changed.
**Why it happens:** Only storing the new values without the old values makes it impossible to show "Session count changed from 4 to 6."
**How to avoid:** ProtocolVersion entity stores both `PreviousJson` and `CurrentJson` snapshots plus a human-readable `ChangeDescription`.
**Warning signs:** Version history page shows "Modified by Dr. X" with no detail of what actually changed.

### Pitfall 5: Cancellation Deduction Calculation Errors
**What goes wrong:** Refund amount calculated incorrectly when pricing mode is PerPackage vs PerSession with partially completed sessions.
**Why it happens:** Not accounting for all pricing scenarios: per-session pricing (refund = remaining sessions * session price * (1 - deduction%)) vs per-package (refund = package price * remaining/total * (1 - deduction%)).
**How to avoid:** Domain method on TreatmentPackage calculates refund amount based on PricingMode, sessions completed, and deduction percentage. Test all scenarios.
**Warning signs:** Manager sees unexpected refund amounts in approval queue.

### Pitfall 6: Sidebar and Route Not Wired
**What goes wrong:** /treatments page works in dev but sidebar still shows "disabled" and route doesn't load in production.
**Why it happens:** Forgetting to change `disabled: true` to `false` in AppSidebar.tsx and not creating the route file.
**How to avoid:** Include sidebar activation and route creation as explicit tasks. Add permission guard (Treatment.View).
**Warning signs:** Sidebar entry is grayed out, 404 on /treatments.

### Pitfall 7: OSDI Link Reuse Without Session Context
**What goes wrong:** OSDI submissions via QR/token link are saved but not connected to the treatment session.
**Why it happens:** Phase 4 OSDI pattern links submissions to VisitId. Treatment sessions need a different linkage (SessionId or PackageId).
**How to avoid:** Either (a) extend OsdiSubmission to support optional TreatmentSessionId, or (b) create a separate treatment OSDI link generation endpoint that stores the session context in the token. Recommendation: option (b) with a TreatmentOsdiSubmission entity in the Treatment module that references the session.
**Warning signs:** OSDI scores recorded via QR link don't appear on the treatment session record.

## Code Examples

### Backend: Domain Entity with Status Machine
```csharp
// Source: Established pattern from Billing.Refund + Optical.GlassesOrder
public enum PackageStatus
{
    Active = 0,
    Paused = 1,
    PendingCancellation = 2,
    Cancelled = 3,
    Switched = 4,
    Completed = 5
}

// Status transitions (enforce in domain):
// Active -> Paused, PendingCancellation, Switched, Completed (auto)
// Paused -> Active
// PendingCancellation -> Cancelled, Active (rejection returns to Active)
// Cancelled, Switched, Completed -> terminal (no transitions)
```

### Backend: Handler with Cross-Module Query Pattern
```csharp
// Source: Established pattern from Billing.ApproveRefundHandler
public static class RecordTreatmentSessionHandler
{
    public static async Task<Result<TreatmentSessionDto>> Handle(
        RecordTreatmentSessionCommand command,
        ITreatmentPackageRepository packageRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var package = await packageRepository.GetByIdAsync(command.PackageId, ct);
        if (package is null)
            return Result.Failure<TreatmentSessionDto>(Error.NotFound("TreatmentPackage", command.PackageId));

        // Check interval warning (not blocking)
        var lastSession = package.Sessions
            .OrderByDescending(s => s.CompletedAt)
            .FirstOrDefault();

        IntervalWarning? warning = null;
        if (lastSession is not null)
        {
            var daysSinceLast = (DateTime.UtcNow - lastSession.CompletedAt).Days;
            if (daysSinceLast < package.MinIntervalDays)
                warning = new IntervalWarning(daysSinceLast, package.MinIntervalDays);
        }

        // Record session (domain validates status)
        var session = package.RecordSession(
            command.ParametersJson,
            command.OsdiScore,
            command.OsdiSeverity,
            command.ClinicalNotes,
            currentUser.UserId,
            command.VisitId,
            command.Consumables);

        await unitOfWork.SaveChangesAsync(ct);

        // Domain events dispatched by Wolverine outbox after SaveChanges
        return Result.Success(MapToDto(session, warning));
    }
}
```

### Backend: EF Core Configuration for Aggregate with Children
```csharp
// Source: Established pattern from Optical.GlassesOrderConfiguration
public class TreatmentPackageConfiguration : IEntityTypeConfiguration<TreatmentPackage>
{
    public void Configure(EntityTypeBuilder<TreatmentPackage> builder)
    {
        builder.ToTable("TreatmentPackages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PatientId).IsRequired();
        builder.Property(x => x.PatientName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TreatmentType).IsRequired().HasConversion<int>();
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.PricingMode).IsRequired().HasConversion<int>();
        builder.Property(x => x.PackagePrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SessionPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ParametersJson).HasMaxLength(4000);
        builder.Property(x => x.MinIntervalDays).IsRequired();
        builder.Property(x => x.TotalSessions).IsRequired();

        builder.Property(x => x.BranchId)
            .HasConversion(b => b.Value, v => new BranchId(v));

        // Backing field navigations
        builder.Navigation(x => x.Sessions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Sessions)
            .WithOne()
            .HasForeignKey(x => x.TreatmentPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey(x => x.TreatmentPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Computed properties not stored
        builder.Ignore(x => x.SessionsCompleted);
        builder.Ignore(x => x.SessionsRemaining);
        builder.Ignore(x => x.IsComplete);

        // Performance indexes
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.TreatmentType);
    }
}
```

### Frontend: React Query Hook Pattern
```typescript
// Source: Established pattern from billing-api.ts and consumables-api.ts
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { api } from "@/shared/lib/api-client"

// Query key factory
export const treatmentKeys = {
  all: ["treatments"] as const,
  packages: () => [...treatmentKeys.all, "packages"] as const,
  package: (id: string) => [...treatmentKeys.packages(), id] as const,
  patientPackages: (patientId: string) => [...treatmentKeys.all, "patient", patientId] as const,
  dueSoon: () => [...treatmentKeys.all, "due-soon"] as const,
  templates: () => [...treatmentKeys.all, "templates"] as const,
  pendingCancellations: () => [...treatmentKeys.all, "pending-cancellations"] as const,
}

// Query hook
export function useActiveTreatments() {
  return useQuery({
    queryKey: treatmentKeys.packages(),
    queryFn: async () => {
      const { data, error } = await api.GET("/api/treatments")
      if (error) throw new Error("Failed to fetch treatments")
      return data as TreatmentPackageDto[]
    },
  })
}

// Mutation hook with cache invalidation
export function useRecordSession(packageId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (command: RecordSessionCommand) => {
      const { data, error } = await api.POST(
        "/api/treatments/{packageId}/sessions" as any,
        { params: { path: { packageId } }, body: command as any }
      )
      if (error) throw error
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: treatmentKeys.package(packageId) })
      queryClient.invalidateQueries({ queryKey: treatmentKeys.dueSoon() })
    },
    onError: (error: any) => {
      toast.error(error?.detail || "Failed to record session")
    },
  })
}
```

### Frontend: Treatment Package Form with Type-Specific Parameters
```typescript
// Source: Established pattern from React Hook Form + Zod + Controller
const packageSchema = z.object({
  protocolTemplateId: z.string().uuid(),
  patientId: z.string().uuid(),
  totalSessions: z.number().min(1).max(6),
  pricingMode: z.enum(["PerSession", "PerPackage"]),
  packagePrice: z.number().min(0).optional(),
  sessionPrice: z.number().min(0).optional(),
  minIntervalDays: z.number().min(1),
  // Type-specific params render conditionally based on treatmentType
})

// The form renders different parameter fields based on treatment type
// IPL: energy, pulseCount, spotSize, treatmentZones
// LLLT: wavelength, power, duration, treatmentArea
// Lid care: procedureSteps (checklist), productsUsed, duration
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate DB table per treatment type | Single entity with JSON parameters + TreatmentType enum | Best practice for extensible treatment types | Enables adding Myopia Control in v2 without schema migration |
| Hard interval enforcement | Soft warning with override + reason logging | Clinical requirement | Doctors can override when clinically appropriate, system logs the exception |
| In-place modification history | Version snapshots (PreviousJson + CurrentJson) | DDD event sourcing pattern | Full audit trail for mid-course changes, critical for billing reconciliation |
| Direct consumable deduction in handler | Domain event + Wolverine outbox | Established project pattern | Decoupled modules, transactional guarantee via outbox, no direct Pharmacy dependency |

**Deprecated/outdated:**
- None -- this is a new module build. All patterns follow the latest project conventions established through Phases 1-8.

## Open Questions

1. **OSDI Submission Link to Treatment Session**
   - What we know: Phase 4 OSDI links to VisitId. Treatment sessions are standalone entities.
   - What's unclear: Whether to extend OsdiSubmission with optional TreatmentSessionId or create Treatment-specific OSDI storage.
   - Recommendation: Create a separate `TreatmentOsdiLink` endpoint in Treatment.Presentation that generates a token scoped to a session. On submission, store the OSDI score on the TreatmentSession entity directly (no separate OsdiSubmission for treatment). The public OSDI page can be reused with a different callback URL. This keeps modules decoupled.

2. **Billing Integration for Treatment Charges**
   - What we know: Billing uses GetVisitChargesQuery to collect charges. Treatment packages may not always be tied to a visit.
   - What's unclear: How treatment charges flow to invoicing (per-session charge vs one-time package charge).
   - Recommendation: Treatment charges are handled via the existing Payment model -- when creating a TreatmentPackage, an invoice is created for the full amount (or split per FIN-05/FIN-06). The treatment module publishes to Billing.Contracts for charge creation. This should be clarified during implementation but follows the existing cross-module charge pattern.

3. **Cancellation Deduction Percentage Storage**
   - What we know: 10-20% range per TRT-09. User left scope (per-type vs global) to Claude's discretion.
   - What's unclear: Whether this should be a clinic setting or per-protocol-template.
   - Recommendation: Store `CancellationDeductionPercent` on TreatmentProtocol template (defaulting to 15%). This allows IPL (higher consumable cost) to have different rates than lid care. Manager can override during approval.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.* + NSubstitute 5.* + FluentAssertions 8.* |
| Config file | `backend/tests/Treatment.Unit.Tests/Treatment.Unit.Tests.csproj` (Wave 0 creation) |
| Quick run command | `dotnet test backend/tests/Treatment.Unit.Tests --no-build -v q` |
| Full suite command | `dotnet test backend/Ganka28.slnx --no-build -v q` |

### Phase Requirements to Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| TRT-01 | Create treatment package with 1-6 sessions, pricing modes | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "CreateTreatmentPackage" -v q` | Wave 0 |
| TRT-02 | Track sessions completed/remaining | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "SessionTracking" -v q` | Wave 0 |
| TRT-03 | Record OSDI score at each session | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "RecordSessionOsdi" -v q` | Wave 0 |
| TRT-04 | Auto-complete when all sessions done | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "AutoComplete" -v q` | Wave 0 |
| TRT-05 | Minimum interval enforcement with override | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "IntervalEnforcement" -v q` | Wave 0 |
| TRT-06 | Multiple concurrent packages per patient | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "ConcurrentPackages" -v q` | Wave 0 |
| TRT-07 | Mid-course modification with version history | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "ModifyPackage" -v q` | Wave 0 |
| TRT-08 | Switch treatment type (close old + create new) | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "SwitchTreatment" -v q` | Wave 0 |
| TRT-09 | Cancellation with manager approval + deduction | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "Cancellation" -v q` | Wave 0 |
| TRT-10 | Doctor-only permission enforcement | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "Permission" -v q` | Wave 0 |
| TRT-11 | Consumable auto-deduction on session completion | unit | `dotnet test backend/tests/Treatment.Unit.Tests --filter "ConsumableDeduction" -v q` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Treatment.Unit.Tests --no-build -v q`
- **Per wave merge:** `dotnet test backend/Ganka28.slnx --no-build -v q`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `backend/tests/Treatment.Unit.Tests/Treatment.Unit.Tests.csproj` -- new test project with xUnit + NSubstitute + FluentAssertions references
- [ ] `backend/tests/Treatment.Unit.Tests/Features/` -- test directory for handler tests
- [ ] Add Treatment.Unit.Tests to solution (Ganka28.slnx)
- [ ] Treatment.Presentation project (not scaffolded yet)
- [ ] Treatment.Application IoC.cs (register FluentValidation validators)
- [ ] Treatment.Infrastructure IoC.cs (register repositories + UnitOfWork)

## Claude's Discretion Recommendations

Based on research of established patterns in this codebase:

### Treatments Page Layout: DataTable
**Recommendation:** Use DataTable (not Kanban). Reasons: (1) Consistent with all other list pages (patients, pharmacy, optical orders, billing), (2) Built-in sorting/filtering via shared DataTable component, (3) "Due Soon" section at top provides the proactive view without Kanban complexity. Add status badges with color coding (Active=green, Paused=yellow, PendingCancellation=orange, Completed=blue, Cancelled=red, Switched=gray).

### Package Detail Progress: Card Grid with OSDI Mini-Chart
**Recommendation:** Session cards in a responsive grid (2-3 columns) with a Recharts line chart for OSDI trend in a sidebar/header section. Each card shows session number, date, parameters summary, OSDI score with severity badge, and completion status. This mirrors the established card-based UI (e.g., patient overview, visit sections).

### Treatment Type Switching: Close + Create
**Recommendation:** Mark old package as "Switched" status, create new package with remaining session count. Reasons: (1) Preserves complete billing trail for the old package, (2) Completed sessions and their data remain intact, (3) Simpler domain model (no need to handle parameter type changes on existing sessions), (4) Consistent with the immutability pattern used for visit sign-off.

### Cancellation Deduction: Per-Type (on Protocol Template)
**Recommendation:** Store `CancellationDeductionPercent` (default 15%) on TreatmentProtocol template. IPL uses more expensive consumables than lid care, so different rates make business sense. Manager can override during approval. Range validated to 10-20% per TRT-09.

### Consumable Selection UI: Searchable Multi-Select
**Recommendation:** Reuse the existing Command/Combobox pattern (same as ICD-10 diagnosis selector) -- searchable dropdown showing active consumables from Pharmacy module via cross-module query. Each selected item gets a quantity input. Simple inline layout, not a separate dialog.

### Session Photo Upload: Simplified Inline
**Recommendation:** Simpler than Phase 4 ImageUploader. Single file input with drag-drop, no type/eye tag selection needed. Just session context. Reuse the FormData + raw fetch pattern but with a minimal UI (button + preview thumbnails).

### Protocol Template Management: DataTable + Dialog Form
**Recommendation:** DataTable listing all templates with create/edit via Dialog (same pattern as user management in admin). Template form uses conditional rendering for type-specific parameter defaults.

### Default Interval Values: Template-Level Configuration
**Recommendation:** Default intervals set when creating protocol templates: IPL default 21 days (3 weeks), LLLT default 10 days, Lid care default 10 days. Adjustable per-patient when creating a package. Display as human-readable "X weeks" in the UI with a number input in days.

## Sources

### Primary (HIGH confidence)
- Project codebase analysis: Billing module (approval workflow, invoice aggregate, refund pattern)
- Project codebase analysis: Pharmacy module (consumable entities, FEFO allocator, batch deduction)
- Project codebase analysis: Clinical module (OSDI submission, public OSDI endpoints, visit entity)
- Project codebase analysis: Optical module (GlassesOrder aggregate with children, EF configuration)
- Project codebase analysis: Shared domain (Entity, AggregateRoot, Result, Error, IDomainEvent)
- Project codebase analysis: Treatment module scaffolding (empty DbContext, Marker, csproj references)
- Project codebase analysis: Bootstrapper Program.cs (module wiring, Wolverine configuration)
- Project codebase analysis: Frontend patterns (api-client, DataTable, PatientProfilePage tabs, VisitSection)
- Project codebase analysis: Auth module (PermissionModule.Treatment seeded, Doctor role has Treatment.Manage)

### Secondary (MEDIUM confidence)
- 09-CONTEXT.md decisions and code context analysis (user-confirmed architectural decisions)

### Tertiary (LOW confidence)
- None -- all findings verified against actual codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - all libraries already in use across 8 completed phases, versions pinned in Directory.Packages.props
- Architecture: HIGH - domain model follows established patterns (Invoice aggregate, GlassesOrder children, Refund approval), verified against actual code
- Pitfalls: HIGH - identified from actual codebase gaps (missing Presentation project, cross-module references) and domain complexity (pricing modes, interval enforcement)

**Research date:** 2026-03-08
**Valid until:** 2026-04-08 (stable -- patterns established, no external dependency changes expected)
