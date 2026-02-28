# Architecture Research

**Domain:** Ophthalmology Clinic Management System (Modular Monolith)
**Researched:** 2026-02-28
**Confidence:** HIGH (core patterns) / MEDIUM (Wolverine-specific, medical image pipeline)

## Standard Architecture

### System Overview

```
                            ┌──────────────────────────────────┐
                            │         TanStack Start           │
                            │  (SSR public / SPA staff pages)  │
                            └──────────────┬───────────────────┘
                                           │ REST + SignalR
                            ┌──────────────┴───────────────────┐
                            │       ASP.NET Core API Host      │
                            │   (Bootstrapper / Composition)   │
                            │   Auth, Routing, Middleware       │
                            ├──────────────────────────────────┤
                            │       Wolverine FX Message Bus   │
                            │  Commands / Queries / Events     │
                            │  Outbox / Inbox / Sagas          │
                            ├─────┬──────┬──────┬──────┬───────┤
                            │     │      │      │      │       │
              ┌─────────────┤  HIS│ TxPr │Pharm │Optic │Report │
              │  SharedKern │     │      │      │      │       │
              │  (Contracts,│     │      │      │      │       │
              │   BuildBlk) │     │      │      │      │       │
              └─────────────┤     │      │      │      │       │
                            ├─────┴──────┴──────┴──────┴───────┤
                            │        Entity Framework Core     │
                            │   Schema-per-module isolation    │
                            ├──────────────────────────────────┤
                            │          Azure SQL Server        │
                            │  ┌─────┐┌─────┐┌─────┐┌───────┐ │
                            │  │ his ││txpro││pharm││optical│ │
                            │  └─────┘└─────┘└─────┘└───────┘ │
                            │  ┌────────┐┌────────┐┌────────┐ │
                            │  │report  ││finance ││wolvrin │ │
                            │  └────────┘└────────┘└────────┘ │
                            └──────────────────────────────────┘
                                           │
                            ┌──────────────┴───────────────────┐
                            │      Azure Blob Storage          │
                            │  Medical Images (OCT, Meibo,     │
                            │  Fluorescein, Fundus)            │
                            └──────────────────────────────────┘
```

### Bounded Context Map

```
    ┌─────────────────────────────────────────────────────────────────┐
    │                        CONTEXT MAP                              │
    │                                                                 │
    │   ┌───────────┐    events     ┌─────────────────┐              │
    │   │           │──────────────>│   Treatment     │              │
    │   │   HIS     │<─────────────│   Protocols     │              │
    │   │  (Core)   │   queries     │  (Downstream)   │              │
    │   │           │               └─────────────────┘              │
    │   │           │    events     ┌─────────────────┐              │
    │   │           │──────────────>│   Pharmacy      │              │
    │   │           │               │  (Downstream)   │              │
    │   │           │               └─────────────────┘              │
    │   │           │    events     ┌─────────────────┐              │
    │   │           │──────────────>│ Optical Center  │              │
    │   │           │               │  (Downstream)   │              │
    │   │           │               └─────────────────┘              │
    │   └───────────┘                                                │
    │        │ events                                                 │
    │        v                                                        │
    │   ┌───────────┐              ┌─────────────────┐              │
    │   │ Finance   │<────events───│ All modules     │              │
    │   │(Billing)  │              │ emit billable   │              │
    │   └───────────┘              │ events          │              │
    │        │ events               └─────────────────┘              │
    │        v                                                        │
    │   ┌───────────┐                                                │
    │   │ Reporting │<────────── reads denormalized projections      │
    │   │ (CQRS)    │              from all modules                  │
    │   └───────────┘                                                │
    └─────────────────────────────────────────────────────────────────┘
```

**Relationship types:**

| Upstream | Downstream | Relationship | Rationale |
|----------|-----------|--------------|-----------|
| HIS | Treatment Protocols | Customer/Supplier | Treatment needs patient + visit data; HIS publishes events, Treatment subscribes |
| HIS | Pharmacy | Customer/Supplier | Prescriptions originate in HIS; Pharmacy consumes prescription events |
| HIS | Optical Center | Customer/Supplier | Refraction Rx originates in HIS; Optical consumes Rx events |
| HIS | Finance | Customer/Supplier | Visit completion triggers billing; Finance consumes billable events |
| Treatment Protocols | Finance | Conformist | Treatment packages produce billable items in Finance's format |
| Pharmacy | Finance | Conformist | Dispensing produces billable items |
| Optical Center | Finance | Conformist | Orders produce billable items |
| All Modules | Reporting | Published Language | All emit standardized events; Reporting projects into read models |

### Component Responsibilities

| Component | Responsibility | Owns (Data) |
|-----------|---------------|-------------|
| **HIS** (Hospital Information System) | Patient registration, visits, medical records, appointments, refraction data, diagnoses (ICD-10), medical images, allergy tracking | Patients, Visits, MedicalRecords, Appointments, Images metadata, Allergies |
| **Treatment Protocols** | IPL/LLLT/lid care packages, session scheduling, OSDI tracking per session, protocol enforcement (min intervals), auto-complete | TreatmentPackages, Sessions, ProtocolDefinitions |
| **Pharmacy** | Drug inventory, prescription dispensing, walk-in sales, stock management, expiry alerts, supplier management | DrugCatalog, Inventory, Prescriptions (dispensing side), Suppliers |
| **Optical Center** | Frame/lens catalog, barcode management, glasses orders, contact lens tracking, warranty, combo pricing | FrameCatalog, LensCatalog, GlassesOrders, Warranties |
| **Finance** | Unified billing, payment processing, VIP membership, discounts, MISA export, revenue tracking per department | Invoices, Payments, VIPMemberships, DiscountRules |
| **Reporting** | Revenue dashboards, gross margin, doctor performance, treatment effectiveness (OSDI trends), data export | Denormalized read models (projections from all modules) |
| **Shared Kernel** | Patient ID resolution, money value object, ICD-10 codes, common enums, Wolverine message contracts | None (stateless contracts only) |
| **Auth (cross-cutting)** | ASP.NET Identity, JWT, RBAC, granular permissions | Users, Roles, Permissions |
| **Notifications (cross-cutting)** | Zalo OA integration, appointment reminders, post-visit summaries | NotificationLog, Templates |

## Recommended Project Structure

```
Ganka28.sln
│
├── src/
│   ├── Bootstrapper/
│   │   └── Ganka28.Api/                        # ASP.NET Core host
│   │       ├── Program.cs                       # Composition root, Wolverine config
│   │       ├── appsettings.json
│   │       ├── Endpoints/                       # Thin REST endpoints (delegate to modules)
│   │       └── Hubs/                            # SignalR hubs for real-time
│   │
│   ├── Modules/
│   │   ├── HIS/
│   │   │   ├── Ganka28.HIS.Domain/             # Entities, Value Objects, Domain Events
│   │   │   ├── Ganka28.HIS.Application/        # Wolverine Handlers (Commands/Queries)
│   │   │   ├── Ganka28.HIS.Infrastructure/     # EF Core DbContext, Repos, Blob access
│   │   │   └── Ganka28.HIS.Contracts/          # Integration events, shared DTOs
│   │   │
│   │   ├── TreatmentProtocols/
│   │   │   ├── Ganka28.Treatment.Domain/
│   │   │   ├── Ganka28.Treatment.Application/
│   │   │   ├── Ganka28.Treatment.Infrastructure/
│   │   │   └── Ganka28.Treatment.Contracts/
│   │   │
│   │   ├── Pharmacy/
│   │   │   ├── Ganka28.Pharmacy.Domain/
│   │   │   ├── Ganka28.Pharmacy.Application/
│   │   │   ├── Ganka28.Pharmacy.Infrastructure/
│   │   │   └── Ganka28.Pharmacy.Contracts/
│   │   │
│   │   ├── OpticalCenter/
│   │   │   ├── Ganka28.Optical.Domain/
│   │   │   ├── Ganka28.Optical.Application/
│   │   │   ├── Ganka28.Optical.Infrastructure/
│   │   │   └── Ganka28.Optical.Contracts/
│   │   │
│   │   ├── Finance/
│   │   │   ├── Ganka28.Finance.Domain/
│   │   │   ├── Ganka28.Finance.Application/
│   │   │   ├── Ganka28.Finance.Infrastructure/
│   │   │   └── Ganka28.Finance.Contracts/
│   │   │
│   │   └── Reporting/
│   │       ├── Ganka28.Reporting.Application/   # Full CQRS: read model projections
│   │       ├── Ganka28.Reporting.Infrastructure/ # Denormalized views, export logic
│   │       └── Ganka28.Reporting.Contracts/
│   │
│   ├── Shared/
│   │   ├── Ganka28.SharedKernel/                # Value Objects (Money, PatientId, etc.)
│   │   ├── Ganka28.BuildingBlocks/              # Base classes, interfaces, common infra
│   │   │   ├── Domain/                          # IAggregateRoot, IDomainEvent, Entity base
│   │   │   ├── Application/                     # IUnitOfWork, pagination, result types
│   │   │   └── Infrastructure/                  # EF Core base config, Blob helpers
│   │   └── Ganka28.IntegrationEvents/           # Cross-module event contracts only
│   │
│   └── CrossCutting/
│       ├── Ganka28.Auth/                        # ASP.NET Identity, JWT, RBAC
│       └── Ganka28.Notifications/               # Zalo OA client, notification handlers
│
├── tests/
│   ├── Ganka28.HIS.Domain.Tests/
│   ├── Ganka28.HIS.Application.Tests/
│   ├── Ganka28.HIS.Integration.Tests/
│   ├── Ganka28.Treatment.Domain.Tests/
│   ├── ... (mirror per module)
│   ├── Ganka28.Architecture.Tests/              # ArchUnit tests for dependency rules
│   └── Ganka28.E2E.Tests/                       # Playwright E2E tests
│
└── frontend/
    └── ganka28-web/                             # TanStack Start app
        ├── app/
        │   ├── routes/                          # File-based routing
        │   ├── components/                      # shadcn/ui components
        │   └── lib/                             # API clients, utils
        └── package.json
```

### Structure Rationale

- **Bootstrapper/**: Single API host acts as composition root. All Wolverine configuration, middleware, SignalR hubs, and module registration live here. Thin endpoints delegate immediately to module handlers via Wolverine's `IMessageBus`.
- **Modules/{Name}/**: Each module has 4 projects following Clean Architecture. The `.Contracts` project is the **only** assembly other modules may reference -- it contains integration event types and read-only DTOs. Domain and Application are `internal` by default.
- **Shared/**: `SharedKernel` holds value objects used across modules (Money, PatientId). `BuildingBlocks` holds base classes and infrastructure abstractions. `IntegrationEvents` holds cross-module event marker interfaces.
- **CrossCutting/**: Auth and Notifications are not bounded contexts; they are infrastructure services consumed by all modules.
- **Reporting has no Domain project**: It is a pure projection/read model module. It subscribes to integration events from all other modules and builds denormalized read models. No domain logic.

### Dependency Rules (enforce with ArchUnit tests)

```
Module.Domain         → SharedKernel, BuildingBlocks.Domain (ONLY)
Module.Application    → Module.Domain, BuildingBlocks.Application, other Module.Contracts
Module.Infrastructure → Module.Domain, Module.Application, BuildingBlocks.Infrastructure
Module.Contracts      → SharedKernel (ONLY, no domain/application references)
Bootstrapper.Api      → All Module.Infrastructure (for DI registration), CrossCutting
```

**Critical rule**: No module may reference another module's Domain, Application, or Infrastructure projects. Only `.Contracts` assemblies cross module boundaries.

## Architectural Patterns

### Pattern 1: Wolverine Handler-per-Message (Vertical Slice within Module)

**What:** Each command or query gets a dedicated handler class. No service layer. The handler IS the use case. Wolverine auto-discovers handlers by convention.

**When to use:** Every command and query in the system. This is the primary code organization pattern.

**Trade-offs:** (+) Minimal boilerplate, easy to locate code, testable in isolation. (-) Unfamiliar to developers used to service-layer patterns.

**Example:**
```csharp
// In Ganka28.HIS.Application/Visits/CreateVisit.cs
namespace Ganka28.HIS.Application.Visits;

// Command (message)
public record CreateVisitCommand(
    Guid PatientId,
    Guid DoctorId,
    VisitType Type,
    DateTime ScheduledAt);

// Integration event published after visit creation
// Lives in Ganka28.HIS.Contracts
public record VisitCreated(
    Guid VisitId,
    Guid PatientId,
    Guid DoctorId,
    VisitType Type,
    DateTime CreatedAt);

// Handler - Wolverine discovers this automatically
public static class CreateVisitHandler
{
    // Cascading return = Wolverine publishes VisitCreated after commit
    public static async Task<(Visit, VisitCreated)> Handle(
        CreateVisitCommand command,
        HisDbContext db)
    {
        var patient = await db.Patients.FindAsync(command.PatientId)
            ?? throw new PatientNotFoundException(command.PatientId);

        var visit = Visit.Create(
            patient.Id,
            command.DoctorId,
            command.Type,
            command.ScheduledAt);

        db.Visits.Add(visit);

        return (visit, new VisitCreated(
            visit.Id, visit.PatientId,
            visit.DoctorId, visit.Type,
            DateTime.UtcNow));
    }
}
```

### Pattern 2: Integration Events with Outbox (Cross-Module Communication)

**What:** Modules communicate asynchronously through integration events. Wolverine's transactional outbox guarantees events are published if and only if the database transaction commits. Downstream modules subscribe to these events via their own handlers.

**When to use:** Every cross-module interaction. Never call another module's handler directly.

**Trade-offs:** (+) Decoupled modules, reliable delivery, supports eventual consistency. (-) Eventual consistency requires UI patterns (optimistic updates, polling).

**Example:**
```csharp
// In Ganka28.HIS.Contracts/Events/PrescriptionIssued.cs
public record PrescriptionIssued(
    Guid PrescriptionId,
    Guid VisitId,
    Guid PatientId,
    IReadOnlyList<PrescriptionLineDto> Lines,
    DateTime IssuedAt,
    DateTime ExpiresAt);   // 7-day validity

public record PrescriptionLineDto(
    string DrugCode,
    string DrugName,
    decimal Quantity,
    string Dosage,
    string Instructions);

// In Ganka28.Pharmacy.Application/Handlers/PrescriptionIssuedHandler.cs
// Wolverine routes PrescriptionIssued to this handler automatically
public static class PrescriptionIssuedHandler
{
    public static async Task Handle(
        PrescriptionIssued @event,
        PharmacyDbContext db)
    {
        var pendingDispensing = PendingDispensing.CreateFrom(
            @event.PrescriptionId,
            @event.PatientId,
            @event.Lines,
            @event.ExpiresAt);

        db.PendingDispensings.Add(pendingDispensing);
        // Wolverine commits via transactional middleware
    }
}
```

### Pattern 3: Immutable Visit Records with Amendment Chain

**What:** Medical records are never updated in place. The original visit record is sealed (locked) after the doctor signs off. Any corrections create a new Amendment record that references the original, with full audit metadata.

**When to use:** All clinical data in HIS: visit notes, diagnoses, prescriptions, refraction data, images.

**Trade-offs:** (+) Full legal compliance, tamper-evident audit trail, supports litigation defense. (-) Queries must compose original + amendments, more complex read logic, storage grows over time.

**Example:**
```csharp
// Domain model
public class VisitRecord : Entity
{
    public Guid Id { get; private set; }
    public Guid VisitId { get; private set; }
    public Guid AuthoredBy { get; private set; }     // Doctor ID
    public DateTime AuthoredAt { get; private set; }
    public VisitRecordStatus Status { get; private set; } // Draft, Signed, Amended
    public string ClinicalNotes { get; private set; }
    public string DiagnosisCodes { get; private set; }  // JSON ICD-10 codes

    // Once signed, record is sealed
    public void Sign(Guid doctorId)
    {
        if (AuthoredBy != doctorId)
            throw new UnauthorizedRecordAccessException();
        Status = VisitRecordStatus.Signed;
        // Raises VisitRecordSigned domain event
    }

    // Corrections create amendments, never mutate original
    public Amendment CreateAmendment(
        Guid amendedBy, string reason, string correctedFields)
    {
        if (Status != VisitRecordStatus.Signed)
            throw new InvalidOperationException("Can only amend signed records");

        Status = VisitRecordStatus.Amended;

        return new Amendment(
            originalRecordId: this.Id,
            amendedBy: amendedBy,
            reason: reason,
            correctedFields: correctedFields,
            amendedAt: DateTime.UtcNow);
    }
}

public class Amendment : Entity
{
    public Guid Id { get; private set; }
    public Guid OriginalRecordId { get; private set; }
    public Guid AmendedBy { get; private set; }
    public string Reason { get; private set; }         // Required: why the correction
    public string CorrectedFields { get; private set; } // JSON: field-level changes
    public DateTime AmendedAt { get; private set; }
}
```

### Pattern 4: Wolverine Saga for Multi-Step Workflows

**What:** Long-running processes that span multiple events and may need timeouts. Wolverine persists saga state in SQL Server and correlates incoming events to the correct saga instance by convention (matching `{SagaType}Id` property on messages).

**When to use:** Treatment protocol sessions (multi-session packages), glasses order lifecycle, Ortho-K follow-up scheduling.

**Trade-offs:** (+) Explicit state machine, timeout support, survives process restarts. (-) Adds complexity; only use for genuinely multi-step workflows, not simple request/response.

**Example:**
```csharp
// Treatment package lifecycle saga
public class TreatmentPackageSaga : Saga
{
    public Guid Id { get; set; }  // = TreatmentPackageId
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public DateTime? LastSessionDate { get; set; }
    public TimeSpan MinInterval { get; set; }

    // Saga starts when a treatment package is purchased
    public static TreatmentPackageSaga Start(
        TreatmentPackagePurchased message)
    {
        return new TreatmentPackageSaga
        {
            Id = message.TreatmentPackageId,
            TotalSessions = message.TotalSessions,
            CompletedSessions = 0,
            MinInterval = TimeSpan.FromDays(message.MinIntervalDays)
        };
    }

    // Each completed session advances the saga
    public void Handle(TreatmentSessionCompleted message)
    {
        CompletedSessions++;
        LastSessionDate = message.CompletedAt;

        if (CompletedSessions >= TotalSessions)
        {
            MarkCompleted(); // Wolverine deletes saga state
        }
    }

    // Timeout: if patient hasn't returned after expected window
    public ScheduleNextReminder Handle(
        TreatmentReminderTimeout timeout)
    {
        if (CompletedSessions >= TotalSessions) return null;

        // Trigger notification to patient
        return new ScheduleNextReminder(
            Id, LastSessionDate?.Add(MinInterval) ?? DateTime.UtcNow);
    }
}
```

### Pattern 5: Mixed CQRS (Light for Operations, Full for Reporting)

**What:** Operational modules (HIS, Pharmacy, Optical, Treatment) use "Light CQRS" -- separate command and query handlers but reading from the same normalized tables via EF Core. The Reporting module uses "Full CQRS" -- it subscribes to integration events from all modules and projects data into denormalized read models optimized for dashboards and analytics.

**When to use:** Light CQRS for all operational modules. Full CQRS only for Reporting.

**Trade-offs:** Light CQRS avoids the overhead of separate read models for straightforward operational queries. Full CQRS in Reporting enables fast, pre-computed dashboard queries without joining across module schemas.

**Example:**
```csharp
// Light CQRS in HIS - query reads from same tables
public record GetPatientVisitHistory(Guid PatientId);

public static class GetPatientVisitHistoryHandler
{
    public static async Task<IReadOnlyList<VisitSummaryDto>> Handle(
        GetPatientVisitHistory query,
        HisDbContext db)
    {
        return await db.Visits
            .Where(v => v.PatientId == query.PatientId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VisitSummaryDto(
                v.Id, v.Type, v.DoctorName,
                v.CreatedAt, v.Status))
            .ToListAsync();
    }
}

// Full CQRS in Reporting - projected read model
// Subscribes to events from HIS, Finance, Treatment, etc.
public static class RevenueProjectionHandler
{
    public static async Task Handle(
        InvoicePaid @event,      // From Finance.Contracts
        ReportingDbContext db)
    {
        var projection = await db.DailyRevenue
            .FindAsync(@event.PaidDate.Date, @event.Department);

        if (projection == null)
        {
            projection = new DailyRevenueProjection(
                @event.PaidDate.Date, @event.Department);
            db.DailyRevenue.Add(projection);
        }

        projection.AddRevenue(@event.Amount, @event.PaymentMethod);
    }
}
```

### Pattern 6: Wolverine + SignalR for Real-Time Staff Notifications

**What:** Wolverine's SignalR transport enables the backend to push real-time updates to the staff SPA. When domain events occur (new patient checked in, prescription ready, glasses order status change), Wolverine handlers publish SignalR messages that reach connected browser clients.

**When to use:** Staff dashboard real-time updates: queue changes, prescription ready alerts, glasses order status, appointment reminders.

**Trade-offs:** (+) Native Wolverine integration, no custom plumbing, supports groups for role-based notifications. (-) Requires WebSocket connectivity; fallback to polling needed for unreliable connections.

**Example:**
```csharp
// Server-side: Wolverine handler cascades to SignalR
public static class NotifyStaffOfNewCheckin
{
    public static SignalRMessage<PatientCheckedInNotification> Handle(
        PatientCheckedIn @event)
    {
        return new PatientCheckedInNotification(
            @event.PatientId,
            @event.PatientName,
            @event.VisitType,
            @event.CheckedInAt)
        .ToWebSocketGroup("reception-staff");
    }
}
```

## Data Flow

### Core Clinical Flow

```
Patient Arrival
    │
    v
[HIS] Registration/Check-in
    │
    ├──> PatientCheckedIn event ──> [SignalR] ──> Staff dashboard
    │
    v
[HIS] Refraction Testing (Technician enters data)
    │  SPH, CYL, AXIS, ADD, PD, VA, IOP, Axial Length
    │
    v
[HIS] Doctor Examination
    │  Clinical notes, Diagnoses (ICD-10), Image capture
    │
    ├──> (if treatment needed)
    │    PrescriptionIssued event ──> [Pharmacy] creates PendingDispensing
    │    TreatmentPlanCreated event ──> [Treatment] creates package
    │    RefractionRxIssued event ──> [Optical] creates pending order
    │
    v
[HIS] Visit Signed (record sealed, immutable)
    │
    ├──> VisitCompleted event ──> [Finance] creates consolidated invoice
    │                          ──> [Reporting] projects visit data
    │
    v
[Finance] Billing + Payment
    │
    ├──> InvoicePaid event ──> [Reporting] projects revenue
    │                       ──> [Notifications] sends post-visit summary via Zalo
    │
    v
[Pharmacy] Dispense drugs  ───> DispensingCompleted event ──> [Finance] marks line paid
[Optical] Process glasses   ───> GlassesReady event ──> [Notifications] sends Zalo alert
[Treatment] Execute session ───> SessionCompleted event ──> [Reporting] updates OSDI trend
```

### Medical Image Pipeline

```
Diagnostic Device (OCT, Meibography, Fluorescein, Fundus camera)
    │
    │  Manual upload (v1) or device API (future)
    │
    v
[HIS] Image Upload Handler
    │
    ├── 1. Validate file type (JPEG/PNG for v1, DICOM future-ready)
    ├── 2. Generate metadata: patientId, visitId, imageType, capturedAt, eye (L/R)
    ├── 3. Store original in Azure Blob Storage
    │      Container: medical-images/{patientId}/{visitId}/{imageType}/{filename}
    ├── 4. Generate thumbnail (resize to 300px width)
    │      Container: medical-images-thumbnails/...
    ├── 5. Save metadata to HIS database (ImageRecord entity)
    │      - BlobUrl, ThumbnailUrl, metadata fields
    │      - NO image bytes in SQL Server
    │
    v
[HIS] Image Display (Frontend)
    │
    ├── List images: query metadata from SQL, display thumbnails
    ├── Full view: generate short-lived SAS token (5-15 min), load from Blob
    ├── Side-by-side comparison:
    │   - Query images by patient + imageType, sorted by visit date
    │   - Frontend renders two images with synchronized zoom/pan
    │   - Display OSDI/measurement overlays from visit data
    │
    v
[Future: DICOM support]
    - Add DICOM parser (fo-dicom NuGet)
    - Extract DICOM metadata into ImageRecord
    - Use Cornerstone.js or custom viewer for DICOM rendering
    - Current architecture accommodates this without restructuring
```

### Cross-Module Data Access Pattern

Modules never share tables. When Module B needs data from Module A:

```
Option 1: Integration Event (preferred for state changes)
    [HIS] ──PrescriptionIssued──> [Pharmacy]
    Pharmacy stores its own copy of prescription data

Option 2: Synchronous Query via Contracts (for reads, sparingly)
    [Treatment] needs patient name for display
    → Calls IPatientQueryService (defined in HIS.Contracts)
    → Implemented in HIS.Infrastructure
    → Registered in DI by Bootstrapper
    → Returns read-only DTO, never domain entity

Option 3: Shared Kernel Value Objects (for identity)
    PatientId, Money, VisitId are in SharedKernel
    All modules use the same value types for correlation
```

**Rule of thumb**: Use events for state propagation, synchronous queries for display-only lookups, shared kernel for identity and value types. Never share database tables.

### Key Data Flows

1. **Prescription flow**: HIS issues prescription -> PrescriptionIssued event -> Pharmacy creates pending dispensing -> Pharmacist dispenses -> DispensingCompleted event -> Finance marks line item fulfilled
2. **Glasses order flow**: HIS issues Refraction Rx -> RefractionRxIssued event -> Optical creates order -> Order progresses (ordered -> processing -> ready -> delivered) -> Each status change emits event -> Notifications sends Zalo when ready
3. **Treatment package flow**: Patient purchases package -> TreatmentPackagePurchased event -> Saga starts -> Each session completed -> SessionCompleted event -> OSDI recorded -> Saga tracks progress -> Auto-complete when all sessions done
4. **Revenue reporting flow**: Finance emits InvoicePaid -> Reporting handler projects into DailyRevenue read model -> Dashboard queries pre-computed aggregates
5. **Appointment reminder flow**: HIS creates appointment -> Scheduler checks next-day appointments -> Notifications sends Zalo OA reminder -> Day of: reception sees today's list on dashboard

## Wolverine FX Configuration

### Recommended Bootstrapper Setup

```csharp
// Program.cs - Composition Root
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SqlServer");

// EF Core DbContexts - one per module, each targeting its own schema
builder.Services.AddDbContextWithWolverineIntegration<HisDbContext>(
    opts => opts.UseSqlServer(connectionString));
builder.Services.AddDbContextWithWolverineIntegration<PharmacyDbContext>(
    opts => opts.UseSqlServer(connectionString));
builder.Services.AddDbContextWithWolverineIntegration<OpticalDbContext>(
    opts => opts.UseSqlServer(connectionString));
builder.Services.AddDbContextWithWolverineIntegration<TreatmentDbContext>(
    opts => opts.UseSqlServer(connectionString));
builder.Services.AddDbContextWithWolverineIntegration<FinanceDbContext>(
    opts => opts.UseSqlServer(connectionString));
builder.Services.AddDbContextWithWolverineIntegration<ReportingDbContext>(
    opts => opts.UseSqlServer(connectionString));

// SignalR
builder.Services.AddSignalR();

// Wolverine
builder.Host.UseWolverine(opts =>
{
    // Modular monolith essentials
    opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
    opts.Durability.MessageIdentity = MessageIdentity.IdAndDestination;
    opts.Durability.MessageStorageSchemaName = "wolverine";

    // SQL Server persistence for outbox/inbox/sagas
    opts.PersistMessagesWithSqlServer(connectionString, "wolverine");

    // Make all local queues durable (survive restarts)
    opts.Policies.UseDurableLocalQueues();

    // Auto-apply transactional middleware to handlers using DbContext
    opts.Policies.AutoApplyTransactions();

    // SignalR transport for real-time notifications
    opts.UseSignalR();
    opts.Publish(x =>
    {
        x.MessagesImplementing<IStaffNotification>();
        x.ToSignalR();
    });

    // Handler discovery from module assemblies
    opts.Discovery.IncludeAssembly(typeof(CreateVisitHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(DispensePrescriptionHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(ProcessGlassesOrderHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(RecordTreatmentSessionHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(CreateInvoiceHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(RevenueProjectionHandler).Assembly);
});

var app = builder.Build();

// SignalR hub
app.MapWolverineSignalRHub("/api/notifications");

// REST endpoints
app.MapHisEndpoints();
app.MapPharmacyEndpoints();
app.MapOpticalEndpoints();
app.MapTreatmentEndpoints();
app.MapFinanceEndpoints();
app.MapReportingEndpoints();

app.Run();
```

### Schema-per-Module DbContext Configuration

```csharp
public class HisDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("his");
        modelBuilder.MapWolverineEnvelopeStorage(); // Outbox integration

        modelBuilder.Entity<Patient>(cfg => { /* ... */ });
        modelBuilder.Entity<Visit>(cfg => { /* ... */ });
        modelBuilder.Entity<VisitRecord>(cfg => { /* ... */ });
        modelBuilder.Entity<ImageRecord>(cfg => { /* ... */ });
        modelBuilder.Entity<Appointment>(cfg => { /* ... */ });
    }
}

public class PharmacyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("pharmacy");
        modelBuilder.MapWolverineEnvelopeStorage();

        modelBuilder.Entity<Drug>(cfg => { /* ... */ });
        modelBuilder.Entity<InventoryItem>(cfg => { /* ... */ });
        modelBuilder.Entity<PendingDispensing>(cfg => { /* ... */ });
    }
}
// Same pattern for Optical, Treatment, Finance, Reporting DbContexts
```

## Scaling Considerations

| Scale | Architecture Adjustments |
|-------|--------------------------|
| 1-10 concurrent users (launch) | Current architecture is overkill but correct. Single Azure App Service Basic tier handles this. No caching needed. |
| 10-50 concurrent users (growth) | Add Redis for session caching and SignalR backplane. Consider upgrading to Standard tier. Monitor SQL query performance. |
| 50-200 concurrent users (multi-branch future) | Extract Reporting into separate read replica. Add Azure CDN for medical image thumbnails. Consider moving from Azure SQL Basic to Standard with more DTUs. |
| 200+ concurrent users (unlikely for boutique clinic) | Consider splitting hot modules (HIS, Finance) into separate services. Wolverine's message-based architecture makes this straightforward -- change local queues to Azure Service Bus transport. |

### Scaling Priorities

1. **First bottleneck: Medical image loading.** Large OCT/Meibography images over slow connections. Mitigation: aggressive thumbnail usage, lazy-load full images, CDN for frequently accessed images, client-side image compression on upload.
2. **Second bottleneck: Reporting queries.** Dashboard queries across denormalized tables grow expensive with data volume. Mitigation: Full CQRS with pre-computed projections already addresses this. Add indexed views if needed.
3. **Third bottleneck: Concurrent appointment scheduling.** Optimistic concurrency conflicts during busy periods. Mitigation: pessimistic locking on time slots (SELECT FOR UPDATE equivalent via EF Core concurrency tokens).

## Anti-Patterns

### Anti-Pattern 1: Cross-Module Database Joins

**What people do:** Write SQL queries that JOIN tables from HIS schema with Pharmacy schema because "it's the same database."
**Why it's wrong:** Destroys module boundaries. When you extract a module to a separate service, every cross-schema query breaks. Also creates hidden coupling that makes schema changes dangerous.
**Do this instead:** Use integration events to replicate needed data into the consuming module's schema. If Pharmacy needs patient names, subscribe to PatientRegistered events and maintain a local PatientLookup table.

### Anti-Pattern 2: Synchronous Command Calls Between Modules

**What people do:** Use Wolverine's `IMessageBus.InvokeAsync()` to synchronously call a handler in another module, creating a distributed transaction across module boundaries.
**Why it's wrong:** Creates tight temporal coupling. If the called module is slow or fails, the calling module blocks or fails too. Negates the benefits of modular separation.
**Do this instead:** Use `PublishAsync()` for asynchronous events between modules. Reserve `InvokeAsync()` for within-module command handling only. If you need a response, use a saga or callback event pattern.

### Anti-Pattern 3: Putting Business Logic in the API Endpoints

**What people do:** Write validation, domain logic, and data access directly in REST endpoint methods because it's "faster" during development.
**Why it's wrong:** Logic becomes untestable without HTTP context. Duplicates logic when the same operation is triggered by an event vs. an API call. Bypasses Wolverine's middleware pipeline (transactions, retries, logging).
**Do this instead:** Endpoints should only: parse the request, create a command/query message, send it via Wolverine, return the response. All logic lives in Wolverine handlers.

### Anti-Pattern 4: Shared Mutable State in Shared Kernel

**What people do:** Put entities, repositories, or service interfaces in the SharedKernel project, allowing modules to share mutable state.
**Why it's wrong:** SharedKernel becomes a dumping ground. Every module depends on it, so changes ripple everywhere. Defeats module isolation.
**Do this instead:** SharedKernel contains ONLY immutable value objects (Money, PatientId, DateRange) and marker interfaces (IDomainEvent). If two modules need the same entity, they each define their own version with only the fields they need.

### Anti-Pattern 5: Skipping the Outbox for "Simple" Events

**What people do:** Publish integration events directly (without the transactional outbox) because "it's just a local queue, what could go wrong?"
**Why it's wrong:** If the process crashes between database commit and event publish, the event is lost. This leads to inconsistent state across modules (e.g., invoice created but pharmacy never notified of prescription).
**Do this instead:** Always use durable local queues (`opts.Policies.UseDurableLocalQueues()`). Wolverine's outbox ensures events are persisted in the same transaction as domain changes and published after commit.

## Integration Points

### External Services

| Service | Integration Pattern | Notes |
|---------|---------------------|-------|
| Azure Blob Storage | SDK (Azure.Storage.Blobs) via Infrastructure service | Abstracted behind `IImageStorageService`. SAS tokens for frontend access. Container structure: `medical-images/{patientId}/{visitId}/` |
| Zalo OA | REST API client in Notifications module | Rate limits apply. Queue outbound messages. Retry with exponential backoff. Store message status for delivery tracking. |
| MISA (e-invoicing) | Manual CSV/Excel export (Phase 1) | Finance module generates export files. No real-time integration. Future: MISA API in Phase 2. |
| VNPay/MoMo/ZaloPay | Payment gateway webhooks | Finance module processes callbacks. Idempotent webhook handlers (check payment ID before processing). |
| Sở Y tế | API integration (future, before 31/12/2026) | ICD-10 from Day 1 ensures data readiness. Actual API spec TBD -- architecture must be flexible. |

### Internal Module Boundaries

| Boundary | Communication | Notes |
|----------|---------------|-------|
| HIS -> Pharmacy | Async events (PrescriptionIssued, PrescriptionCancelled) | Pharmacy stores its own copy of prescription data |
| HIS -> Optical | Async events (RefractionRxIssued) | Optical stores its own Rx copy |
| HIS -> Treatment | Async events (TreatmentPlanCreated) | Treatment creates package from plan |
| HIS -> Finance | Async events (VisitCompleted) | Finance creates draft invoice |
| Treatment -> HIS | Sync query (IPatientQueryService) for display only | Treatment needs patient name/info for UI |
| Pharmacy -> Finance | Async events (DispensingCompleted with line items) | Finance adds to invoice |
| Optical -> Finance | Async events (GlassesOrderCreated, GlassesDelivered) | Finance adds to invoice |
| Finance -> Reporting | Async events (InvoicePaid, RefundIssued) | Reporting projects revenue data |
| All -> Reporting | Async events (various domain events) | Reporting subscribes broadly |
| Finance -> Notifications | Async events (PaymentConfirmed) | Triggers post-visit Zalo message |
| Optical -> Notifications | Async events (GlassesReady) | Triggers pickup Zalo message |

## Suggested Build Order

Based on dependency analysis, the recommended implementation order:

```
Phase 1: Foundation + HIS Core
├── SharedKernel, BuildingBlocks
├── Auth (ASP.NET Identity, RBAC)
├── Wolverine + EF Core + SQL Server infrastructure
├── HIS: Patient registration, visit workflow, refraction data
├── HIS: Appointment scheduling
└── Frontend: Staff login, patient registration, visit screen

Phase 2: HIS Clinical + Medical Images
├── HIS: Clinical notes, ICD-10 diagnoses, immutable records
├── HIS: Medical image upload/storage/display pipeline
├── HIS: Allergy tracking
├── HIS: Dry Eye template (OSDI, TBUT, Schirmer)
└── Frontend: Clinical workflow, image viewer, side-by-side comparison

Phase 3: Pharmacy + Finance Core
├── Pharmacy: Drug catalog, inventory, dispensing
├── Finance: Billing, payment processing, invoice generation
├── Integration: HIS → Pharmacy (PrescriptionIssued)
├── Integration: Pharmacy → Finance (DispensingCompleted)
└── Frontend: Pharmacy dispensing, cashier screen

Phase 4: Optical Center + Treatment Protocols
├── Optical: Frame/lens catalog, barcode, orders, warranty
├── Treatment: IPL/LLLT packages, session tracking, OSDI per session
├── Integration: HIS → Optical (RefractionRxIssued)
├── Integration: HIS → Treatment (TreatmentPlanCreated)
├── Sagas: Treatment package lifecycle, glasses order lifecycle
└── Frontend: Optical order tracking, treatment session recording

Phase 5: Reporting + Notifications + Polish
├── Reporting: Full CQRS projections, dashboards, export
├── Finance: VIP membership, discounts
├── Notifications: Zalo OA integration
├── SignalR: Real-time staff dashboard updates
├── Printing: Prescriptions, invoices, Rx forms
└── Frontend: Dashboards, reports, bilingual UI
```

**Build order rationale:**
- HIS is the **core upstream context**. Every other module depends on events from HIS. It must be built first.
- Auth is foundational -- needed before any staff-facing feature.
- Pharmacy and Finance together because dispensing immediately needs billing integration.
- Optical and Treatment can parallel each other (independent contexts) but both need HIS to exist first.
- Reporting is last because it only consumes events -- it needs the event producers to exist and emit real data.
- Notifications are last because they are enhancement/polish, not core workflow.

## Sources

- [Wolverine Modular Monolith Tutorial](https://wolverinefx.net/tutorials/modular-monolith.html) - HIGH confidence (official docs)
- [Wolverine 5 and Modular Monoliths](https://jeremydmiller.com/2025/10/27/wolverine-5-and-modular-monoliths/) - HIGH confidence (framework author)
- [Wolverine 3.6 Modular Monolith Features](https://jeremydmiller.com/2025/01/12/wolverine-3-6-modular-monolith-and-vertical-slice-architecture-goodies/) - HIGH confidence (framework author)
- [Wolverine EF Core Outbox/Inbox](https://wolverinefx.net/guide/durability/efcore/outbox-and-inbox.html) - HIGH confidence (official docs)
- [Wolverine Sagas](https://wolverinefx.net/guide/durability/sagas.html) - HIGH confidence (official docs)
- [Wolverine SQL Server Integration](https://wolverinefx.net/guide/durability/sqlserver) - HIGH confidence (official docs)
- [Wolverine SignalR Transport](https://wolverinefx.net/guide/messaging/transports/signalr) - HIGH confidence (official docs)
- [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd) - HIGH confidence (reference implementation, 11k+ stars)
- [Kamil Grzybek: Modular Monolith Integration Styles](https://www.kamilgrzybek.com/blog/posts/modular-monolith-integration-styles) - HIGH confidence (DDD authority)
- [DDD Context Mapping Patterns](https://opus.ch/ddd-concepts-and-patterns-context-map/) - MEDIUM confidence
- [Immutable Audit Logs for Health SaaS](https://dev.to/beck_moulton/immutable-by-design-building-tamper-proof-audit-logs-for-health-saas-22dc) - MEDIUM confidence
- [DICOM in Medical Imaging](https://pmc.ncbi.nlm.nih.gov/articles/PMC61235/) - MEDIUM confidence (academic)
- [Ophthalmology EMR Guide](https://www.thinkitive.com/blog/ophthalmology-emr-software-development-complete-guide/) - LOW confidence (marketing content)
- [Cornerstone.js](https://www.cornerstonejs.org/) - MEDIUM confidence (official site, but relevance to non-DICOM images unclear)

---
*Architecture research for: Ganka28 Ophthalmology Clinic Management System*
*Researched: 2026-02-28*
