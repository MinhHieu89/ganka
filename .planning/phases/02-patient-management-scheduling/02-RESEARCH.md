# Phase 02: Patient Management & Scheduling - Research

**Researched:** 2026-03-01
**Domain:** Patient registration, profile management, allergy tracking, search, appointment scheduling, public self-booking
**Confidence:** HIGH

## Summary

Phase 02 builds two closely related modules -- Patient and Scheduling -- that form the backbone of the clinic's HIS. The Patient module covers medical patient registration (auto-generated GK-YYYY-NNNN IDs), walk-in pharmacy customer registration, allergy management, and patient search with Vietnamese diacritics support. The Scheduling module covers per-doctor appointment calendars with configurable durations, double-booking prevention via database constraints, drag-and-drop rescheduling, and a public self-booking page with staff confirmation workflow.

The backend follows the established modular monolith patterns from Phase 01 (DDD entities, repository-per-aggregate, Wolverine handlers, Minimal API endpoints, EF Core with schema-per-module). Both Patient and Scheduling module scaffolds already exist with empty DbContexts. The frontend follows the established patterns (TanStack Router file-based routing, React Query, shadcn/ui wrappers, i18n, Zustand stores). The primary new frontend dependency is a calendar component -- FullCalendar with its free timeGrid plugins is recommended over alternatives.

**Primary recommendation:** Use EF Core sequences for GK-YYYY-NNNN patient ID generation, SQL Server accent-insensitive collation for Vietnamese search, FullCalendar (free standard plugins) for the weekly calendar view, and a shadcn Command/Popover combination for autocomplete patient search.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Claude decides best approach for medical vs walk-in pharmacy distinction (single form with toggle vs separate entry points)
- After registration, redirect to patient profile -- no special toast for the generated GK-YYYY-NNNN ID
- Mandatory fields (Address, CCCD) are hardcoded: always optional at registration, always required for referrals/legal export -- no admin settings page
- Allergy entry is available inline during registration (optional section)
- Allergy input uses combo approach: autocomplete from predefined list + free-text for custom entries
- Each allergy has name + severity level (mild/moderate/severe)
- Live autocomplete search -- results appear as you type (debounced dropdown)
- Diacritics-insensitive matching for Vietnamese names (e.g. 'Nguyen' matches 'Nguyen')
- Phone number search uses prefix matching (e.g. '0912' matches '0912345678')
- Global search bar in site header + dedicated Patient List page
- Global search shows typed results with icons (patient icon for patients, prepared for future entity types)
- When global search bar is focused before typing, show recent patients
- Recent patients also appear as a dashboard widget
- Paginated DataTable patient list with filters (gender, has allergies, date range)
- Uses existing generic DataTable patterns from Phase 01.2
- Tabbed patient profile with header (name/ID/photo), tabs (Overview, Allergies, Appointments, etc.)
- Optional photo upload with initials avatar fallback
- Show both DOB and calculated age, e.g. '15/03/1985 (40 tuoi)'
- Edit via "Edit" button that switches to form mode with Save/Cancel
- Soft-delete only (deactivate) -- patients never truly deleted, staff can reactivate
- Default weekly view per doctor
- Click empty time slot -> dialog for quick booking (search/select patient, choose type, confirm)
- Also available: "Book Appointment" button for full form booking without calendar context
- Double-booking prevention: occupied slots visually distinct + server-side validation error
- Color coding: background = appointment type (New Patient blue, Follow-up green, Treatment orange, Ortho-K purple), border/badge = status
- Drag-and-drop rescheduling with double-booking validation
- Clinic operating hours visually grayed out (Mon closed, Tue-Fri 13-20h, Sat-Sun 8-12h)
- Appointment cancellation requires mandatory reason
- Appointment durations configurable by type (defaults: new 30min, follow-up 20min, treatment 30-45min, Ortho-K 60-90min)
- Separate public URL (e.g. /book) -- no login required, shareable as link or QR code
- Zalo integration is link sharing only (no Zalo API/Mini App)
- Bilingual: Vietnamese + English
- Fully branded with Ganka28 logo, clinic colors, address, phone number
- Confirmation page shown after submission (no SMS/Zalo notifications in this phase)
- Anti-spam: rate limit to max pending bookings per phone number

### Claude's Discretion
- Medical vs walk-in form distinction approach
- Allergy alert design for downstream workflows (prioritize patient safety)
- Calendar single-doctor vs multi-doctor column layout
- Self-booking form fields
- Staff confirmation workflow for self-bookings
- Doctor selection on public booking page
- Booking status check page
- Whether to include a Notes tab on patient profile
- Appointment history display on profile (past + upcoming vs separate tabs)

### Deferred Ideas (OUT OF SCOPE)
- Insurance information (provider, policy number) -- Phase 7: Billing & Finance
- Contact preferences (Phone/Zalo/Email for reminders) -- future notification feature
- SMS/Zalo appointment notifications -- future notification feature
- Zalo Mini App integration -- future enhancement
- Patient notes/comments tab -- Claude may include if scope fits, otherwise defer
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| PAT-01 | Staff can register medical patient with name, phone, DOB, gender; system auto-generates GK-YYYY-NNNN ID | EF Core SQL Server sequence for NNNN counter, computed column or domain logic for format. Patient aggregate root with factory method. |
| PAT-02 | Staff can register walk-in pharmacy customer with name + phone only (lightweight record, no full medical profile) | Same Patient entity with a PatientType discriminator (Medical vs WalkIn). Toggle in registration form. Fewer required fields for WalkIn. |
| PAT-03 | System supports configurable mandatory fields (Address, CCCD) that become required for referrals/legal export | Hardcoded per user decision: optional at registration, required for export. FluentValidation conditional rules. No admin settings page. |
| PAT-04 | Staff can search patients by name, phone, or patient ID with results in <=3 seconds | SQL Server accent-insensitive collation (Latin1_General_CI_AI) or EF.Functions.Collate() for diacritics-insensitive search. Composite index on NormalizedName + Phone + PatientCode. Debounced frontend autocomplete. |
| PAT-05 | System stores structured allergy list per patient and displays allergy alerts during prescribing | Allergy as owned entity collection on Patient. Predefined allergy catalog table + free-text support. Severity enum (Mild/Moderate/Severe). Allergy alert component for downstream reuse. |
| SCH-01 | Staff can book appointments for patients (walk-in registration + pre-booked slots) | Appointment aggregate in Scheduling module. References PatientId and DoctorId (Guid foreign keys, no cross-module EF navigation). Quick-book dialog from calendar + full form booking. |
| SCH-02 | Patients can self-book via public website/Zalo with staff confirmation workflow | Public /book route (no auth layout). SelfBookingRequest entity with Pending status. Staff dashboard shows pending requests for approve/reject. Rate limiting by phone number. |
| SCH-03 | System enforces no double-booking (1 patient per doctor per time slot) | Database-level unique filtered index on (DoctorId, StartTime, EndTime) WHERE Status != 'Cancelled'. Plus application-level overlap check before insert. EF Core concurrency token. |
| SCH-04 | System displays calendar view per doctor, color-coded by appointment type | FullCalendar free timeGridWeek plugin with appointment type colors. Filter by doctor dropdown. Events fetched via React Query with date range parameters. |
| SCH-05 | Appointment durations configurable by type (default: new 30min, follow-up 20min, treatment 30-45min, Ortho-K 60-90min) | AppointmentType configuration stored in database or seeded reference data. Duration lookup used in both booking form and calendar slot sizing. |
| SCH-06 | System respects clinic operating hours (Tue-Fri 13-20h, Sat-Sun 8-12h, closed Monday) as configurable schedule | ClinicSchedule entity or configuration. FullCalendar businessHours prop for visual display. Server-side validation rejects bookings outside hours. |
</phase_requirements>

## Standard Stack

### Core (Backend -- already in project)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| EF Core (Microsoft.EntityFrameworkCore.SqlServer) | 10.0.*-* (preview, via CPM) | ORM, migrations, query filters | Already in project. Schema-per-module pattern established. |
| WolverineFx | 5.* (via CPM) | Message bus, handler discovery | Already in project. Handles commands/queries/events. |
| FluentValidation | 12.* (via CPM) | Input validation | Already in project. Validator-per-command pattern established. |
| Shared.Domain (AggregateRoot, Entity, Result, Error, ValueObject) | N/A (project code) | DDD building blocks | Already in project. All entities extend these base classes. |

### Core (Frontend -- already in project)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| @tanstack/react-router | ^1.114.3 | File-based routing | Already in project. Routes under app/routes/. |
| @tanstack/react-query | ^5.64.2 | Server state management | Already in project. Query/mutation hooks per feature. |
| @tanstack/react-table | ^8.21.2 | DataTable with sorting/pagination | Already in project. Pre-configured table instance pattern. |
| react-hook-form + zod | ^7.54.2 / ^3.24.2 | Form management + schema validation | Already in project. Zod schemas inside component for i18n. |
| openapi-fetch | ^0.13.5 | Type-safe API client | Already in project. api-client.ts with auth middleware. |
| zustand | ^5.0.3 | Client-side state | Already in project. authStore, sidebarStore patterns. |
| i18next + react-i18next | ^24.2.2 / ^15.4.1 | Internationalization (vi/en) | Already in project. Translation files in public/locales/. |
| @tabler/icons-react | ^3.37.1 | Icons | Already in project. Used throughout. |
| shadcn/ui primitives | N/A | UI components | Already in project. 20 primitives with wrapper pattern. |

### New Dependencies (Frontend)

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| @fullcalendar/core | ^6.1.20 | Calendar engine | Required for appointment calendar view |
| @fullcalendar/react | ^6.1.20 | React wrapper for FullCalendar | Required for React integration |
| @fullcalendar/timegrid | ^6.1.20 | Weekly/daily time grid view | Free plugin -- provides timeGridWeek and timeGridDay |
| @fullcalendar/daygrid | ^6.1.20 | Month grid view (optional) | Free plugin -- month overview if needed |
| @fullcalendar/interaction | ^6.1.20 | Drag-and-drop + click interactions | Free plugin -- enables drag-drop rescheduling and click-to-create |
| @radix-ui/react-popover | latest | Popover primitive | Needed for global search dropdown (shadcn Popover wrapper) |
| cmdk | ^1.0.4 | Command palette / autocomplete | Needed for global search combobox (shadcn Command wrapper) |
| @radix-ui/react-tabs | latest | Tabs primitive | Needed for patient profile tabs (shadcn Tabs wrapper) |
| react-day-picker | ^9.x | Date picker for booking forms | Already a shadcn dependency pattern; for date fields in forms |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| FullCalendar (free) | Schedule-X | Schedule-X has modern design and resource scheduler, but resource view is premium (EUR 299/yr). FullCalendar free timeGridWeek covers the weekly view requirement without cost. |
| FullCalendar (free) | react-big-calendar | react-big-calendar is fully free with resource views, but requires more manual implementation (no built-in event editor, dialogs). Less maintained than FullCalendar. |
| FullCalendar Premium (resource-timegrid) | FullCalendar free (timeGridWeek) | Premium adds side-by-side doctor columns. But for a boutique clinic with few doctors, a single-doctor-at-a-time filter with a doctor dropdown is sufficient and free. |
| cmdk (Command) | Custom autocomplete | cmdk is the shadcn standard for command/search patterns. Building custom would be reinventing the wheel. |

**Installation (frontend new dependencies):**
```bash
cd frontend
npm install @fullcalendar/core @fullcalendar/react @fullcalendar/timegrid @fullcalendar/daygrid @fullcalendar/interaction
npm install cmdk @radix-ui/react-popover @radix-ui/react-tabs react-day-picker
```

**No new backend NuGet packages needed** -- all required packages are already in the project via Central Package Management.

## Architecture Patterns

### Recommended Project Structure

**Backend -- Patient Module:**
```
backend/src/Modules/Patient/
├── Patient.Domain/
│   ├── Entities/
│   │   ├── Patient.cs              # Aggregate root (medical + walk-in)
│   │   ├── Allergy.cs              # Owned entity on Patient
│   │   └── AllergyCatalogItem.cs   # Predefined allergy reference data
│   ├── Enums/
│   │   ├── PatientType.cs          # Medical, WalkIn
│   │   ├── Gender.cs               # Male, Female, Other
│   │   └── AllergySeverity.cs      # Mild, Moderate, Severe
│   ├── ValueObjects/
│   │   └── PatientCode.cs          # GK-YYYY-NNNN value object
│   └── Events/
│       ├── PatientRegisteredEvent.cs
│       └── PatientUpdatedEvent.cs
├── Patient.Application/
│   ├── Features/
│   │   ├── RegisterPatient.cs      # Command + Validator + Handler
│   │   ├── UpdatePatient.cs
│   │   ├── DeactivatePatient.cs    # Soft delete
│   │   ├── ReactivatePatient.cs
│   │   ├── SearchPatients.cs       # Autocomplete query
│   │   ├── GetPatientById.cs
│   │   ├── GetPatientList.cs       # Paginated with filters
│   │   ├── AddAllergy.cs
│   │   ├── RemoveAllergy.cs
│   │   ├── GetRecentPatients.cs    # For dashboard widget + search focus
│   │   └── UploadPatientPhoto.cs
│   ├── Interfaces/
│   │   ├── IPatientRepository.cs
│   │   ├── IAllergyCatalogRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── IoC.cs
│   └── Marker.cs
├── Patient.Infrastructure/
│   ├── PatientDbContext.cs          # Schema: "patient"
│   ├── Configurations/
│   │   ├── PatientConfiguration.cs
│   │   ├── AllergyConfiguration.cs
│   │   └── AllergyCatalogItemConfiguration.cs
│   ├── Repositories/
│   │   ├── PatientRepository.cs
│   │   └── AllergyCatalogRepository.cs
│   ├── UnitOfWork.cs
│   ├── Seeding/
│   │   └── AllergyCatalogSeeder.cs  # Seed predefined allergies
│   └── IoC.cs
├── Patient.Presentation/
│   ├── PatientApiEndpoints.cs
│   └── IoC.cs
└── Patient.Contracts/
    └── Dtos/
        ├── RegisterPatientCommand.cs
        ├── PatientDto.cs
        ├── PatientSearchResult.cs
        ├── AllergyDto.cs
        └── PatientRegisteredEvent.cs  # Integration event for other modules
```

**Backend -- Scheduling Module:**
```
backend/src/Modules/Scheduling/
├── Scheduling.Domain/
│   ├── Entities/
│   │   ├── Appointment.cs           # Aggregate root
│   │   ├── SelfBookingRequest.cs    # Public booking (pending confirmation)
│   │   ├── ClinicSchedule.cs        # Operating hours per day of week
│   │   └── AppointmentType.cs       # New, FollowUp, Treatment, OrthoK
│   ├── Enums/
│   │   ├── AppointmentStatus.cs     # Pending, Confirmed, Cancelled, Completed
│   │   ├── CancellationReason.cs    # PatientNoShow, PatientRequest, DoctorUnavailable, Other
│   │   └── BookingStatus.cs         # Pending, Approved, Rejected
│   ├── ValueObjects/
│   │   └── TimeSlot.cs              # Start + End time value object
│   └── Events/
│       ├── AppointmentBookedEvent.cs
│       ├── AppointmentCancelledEvent.cs
│       └── AppointmentRescheduledEvent.cs
├── Scheduling.Application/
│   ├── Features/
│   │   ├── BookAppointment.cs
│   │   ├── CancelAppointment.cs
│   │   ├── RescheduleAppointment.cs
│   │   ├── GetAppointmentsByDoctor.cs  # Calendar data for date range
│   │   ├── GetAppointmentsByPatient.cs
│   │   ├── SubmitSelfBooking.cs        # Public booking endpoint (no auth)
│   │   ├── ApproveSelfBooking.cs
│   │   ├── RejectSelfBooking.cs
│   │   ├── GetPendingSelfBookings.cs
│   │   ├── GetClinicSchedule.cs
│   │   ├── GetAppointmentTypes.cs
│   │   └── CheckBookingStatus.cs       # Public status check by reference
│   ├── Interfaces/
│   │   ├── IAppointmentRepository.cs
│   │   ├── ISelfBookingRepository.cs
│   │   ├── IClinicScheduleRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── IoC.cs
│   └── Marker.cs
├── Scheduling.Infrastructure/
│   ├── SchedulingDbContext.cs        # Schema: "scheduling"
│   ├── Configurations/
│   │   ├── AppointmentConfiguration.cs
│   │   ├── SelfBookingRequestConfiguration.cs
│   │   ├── ClinicScheduleConfiguration.cs
│   │   └── AppointmentTypeConfiguration.cs
│   ├── Repositories/
│   │   ├── AppointmentRepository.cs
│   │   ├── SelfBookingRepository.cs
│   │   └── ClinicScheduleRepository.cs
│   ├── UnitOfWork.cs
│   ├── Seeding/
│   │   ├── AppointmentTypeSeeder.cs
│   │   └── ClinicScheduleSeeder.cs
│   └── IoC.cs
├── Scheduling.Presentation/
│   ├── SchedulingApiEndpoints.cs     # Authenticated endpoints
│   ├── PublicBookingEndpoints.cs     # Unauthenticated endpoints (/api/public/booking)
│   └── IoC.cs
└── Scheduling.Contracts/
    └── Dtos/
        ├── BookAppointmentCommand.cs
        ├── AppointmentDto.cs
        ├── SelfBookingRequestDto.cs
        ├── ClinicScheduleDto.cs
        └── AppointmentBookedEvent.cs  # Integration event
```

**Frontend:**
```
frontend/src/
├── features/
│   ├── patient/
│   │   ├── api/
│   │   │   └── patient-api.ts          # API functions + React Query hooks
│   │   ├── components/
│   │   │   ├── PatientRegistrationForm.tsx
│   │   │   ├── PatientListPage.tsx
│   │   │   ├── PatientProfilePage.tsx
│   │   │   ├── PatientProfileHeader.tsx
│   │   │   ├── PatientOverviewTab.tsx
│   │   │   ├── PatientAllergyTab.tsx
│   │   │   ├── PatientAppointmentTab.tsx
│   │   │   ├── AllergyForm.tsx
│   │   │   ├── AllergyAlert.tsx         # Reusable alert banner/badge
│   │   │   └── PatientTable.tsx
│   │   └── hooks/
│   │       ├── usePatients.ts
│   │       └── usePatientSearch.ts
│   ├── scheduling/
│   │   ├── api/
│   │   │   └── scheduling-api.ts
│   │   ├── components/
│   │   │   ├── AppointmentCalendar.tsx   # FullCalendar wrapper
│   │   │   ├── AppointmentBookingDialog.tsx
│   │   │   ├── AppointmentDetailDialog.tsx
│   │   │   ├── PendingBookingsPanel.tsx
│   │   │   └── DoctorSelector.tsx
│   │   └── hooks/
│   │       ├── useAppointments.ts
│   │       └── useSelfBookings.ts
│   └── booking/                          # Public self-booking
│       ├── api/
│       │   └── booking-api.ts            # No auth needed
│       ├── components/
│       │   ├── PublicBookingPage.tsx
│       │   ├── BookingForm.tsx
│       │   ├── BookingConfirmation.tsx
│       │   └── BookingStatusCheck.tsx
│       └── hooks/
│           └── usePublicBooking.ts
├── shared/
│   ├── components/
│   │   ├── Command.tsx                   # shadcn Command wrapper (new)
│   │   ├── Popover.tsx                   # shadcn Popover wrapper (new)
│   │   ├── Tabs.tsx                      # shadcn Tabs wrapper (new)
│   │   ├── Calendar.tsx                  # shadcn Calendar wrapper (new, react-day-picker)
│   │   ├── GlobalSearch.tsx              # Global search combobox in SiteHeader
│   │   └── DatePicker.tsx                # Date picker form component (new)
│   └── stores/
│       └── recentPatientsStore.ts        # Zustand store for recent patients
├── app/
│   └── routes/
│       ├── _authenticated/
│       │   ├── patients/
│       │   │   ├── index.tsx             # Patient list page
│       │   │   └── $patientId.tsx        # Patient profile page
│       │   ├── appointments/
│       │   │   └── index.tsx             # Appointment calendar page
│       │   └── dashboard.tsx             # Updated with recent patients widget
│       └── book/
│           ├── index.tsx                 # Public booking page (no auth layout)
│           └── status.tsx                # Booking status check page
```

### Pattern 1: GK-YYYY-NNNN Patient ID Generation

**What:** Auto-generate patient codes in format GK-2026-0001, GK-2026-0002, etc. Counter resets each year.
**When to use:** On every new medical patient registration.

**Approach:** Use a SQL Server sequence per year + domain logic to format the code.

```csharp
// Domain: Patient.Domain/ValueObjects/PatientCode.cs
public class PatientCode : ValueObject
{
    public string Value { get; }

    private PatientCode(string value) => Value = value;

    public static PatientCode Create(int year, int sequenceNumber)
    {
        var code = $"GK-{year}-{sequenceNumber:D4}";
        return new PatientCode(code);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

// Infrastructure: PatientConfiguration.cs
// Use SQL sequence for the counter
modelBuilder.HasSequence<int>("PatientCodeSequence", "patient")
    .StartsAt(1)
    .IncrementsBy(1);

// The sequence number is stored alongside the formatted code
modelBuilder.Entity<Patient>(cfg =>
{
    cfg.Property(p => p.PatientCode).HasMaxLength(15).IsRequired();
    cfg.HasIndex(p => p.PatientCode).IsUnique();
    cfg.Property(p => p.SequenceNumber).HasDefaultValueSql("NEXT VALUE FOR patient.PatientCodeSequence");
});

// Application: RegisterPatientHandler
// After saving (sequence assigned), format the code
patient.SetPatientCode(PatientCode.Create(DateTime.UtcNow.Year, patient.SequenceNumber));
await unitOfWork.SaveChangesAsync(cancellationToken); // second save to persist code
```

**Alternative (simpler):** Query MAX(SequenceNumber) WHERE Year = currentYear, then +1 with a unique constraint as safety net. Less performant under concurrency but adequate for a boutique clinic.

### Pattern 2: Vietnamese Diacritics-Insensitive Search

**What:** Search for "Nguyen" and match "Nguyen" (Vietnamese diacritics-insensitive).
**When to use:** Patient name search.

**Approach:** Use SQL Server accent-insensitive collation at the column or query level.

```csharp
// Option A: Column-level collation in EF Core configuration
modelBuilder.Entity<Patient>(cfg =>
{
    cfg.Property(p => p.FullName)
        .HasMaxLength(200)
        .UseCollation("Vietnamese_CI_AI"); // Case-insensitive, accent-insensitive
});

// Option B: Query-level collation (more flexible, no migration needed)
public async Task<List<Patient>> SearchAsync(string term, CancellationToken ct)
{
    return await _dbContext.Patients
        .Where(p =>
            EF.Functions.Like(
                EF.Functions.Collate(p.FullName, "Vietnamese_CI_AI"),
                $"%{term}%")
            || p.Phone.StartsWith(term)
            || p.PatientCode == term)
        .OrderBy(p => p.FullName)
        .Take(20)
        .AsNoTracking()
        .ToListAsync(ct);
}
```

**Important:** SQL Server's `Vietnamese_CI_AI` collation is specifically designed for Vietnamese text. It handles all Vietnamese diacritical marks correctly. Alternative: `Latin1_General_CI_AI` also works for accent-insensitive matching but `Vietnamese_CI_AI` is more precise for Vietnamese-specific character ordering.

### Pattern 3: Double-Booking Prevention (Database-Level)

**What:** Prevent two appointments from overlapping for the same doctor.
**When to use:** Every appointment creation and reschedule.

```csharp
// Domain: Appointment overlap check
public class Appointment : AggregateRoot, IAuditable
{
    // ... properties ...

    public bool OverlapsWith(DateTime start, DateTime end)
    {
        return StartTime < end && EndTime > start;
    }
}

// Infrastructure: Database unique constraint approach
// Since time slots can be variable-length, use an exclusion-style check.
// SQL Server doesn't have native range exclusion constraints like PostgreSQL,
// so use a combination of:
// 1. Application-level overlap query before insert
// 2. Optimistic concurrency via RowVersion on Appointment
// 3. Serializable transaction for the booking operation

// AppointmentConfiguration.cs
modelBuilder.Entity<Appointment>(cfg =>
{
    cfg.HasIndex(a => new { a.DoctorId, a.StartTime })
        .HasFilter("[Status] <> 3")  // Exclude cancelled (enum value)
        .IsUnique()
        .HasDatabaseName("IX_Appointment_DoctorId_StartTime_NoCancelled");

    cfg.Property(a => a.RowVersion).IsRowVersion(); // Concurrency token
});

// Application: BookAppointmentHandler
// Step 1: Check for overlaps
var overlapping = await appointmentRepository.HasOverlappingAsync(
    command.DoctorId, command.StartTime, command.EndTime, cancellationToken);
if (overlapping)
    return Result.Failure(Error.Conflict("Time slot is already booked for this doctor."));

// Step 2: Validate within clinic operating hours
var schedule = await clinicScheduleRepository.GetForDayAsync(
    command.StartTime.DayOfWeek, cancellationToken);
if (schedule is null || !schedule.IsWithinHours(command.StartTime, command.EndTime))
    return Result.Failure(Error.Validation("Appointment is outside clinic operating hours."));

// Step 3: Create and save (unique index catches race conditions)
var appointment = Appointment.Create(...);
appointmentRepository.Add(appointment);
await unitOfWork.SaveChangesAsync(cancellationToken);
```

### Pattern 4: Public Self-Booking (Unauthenticated Endpoint)

**What:** Public endpoint that doesn't require JWT authentication.
**When to use:** Patient self-booking page.

```csharp
// Presentation: PublicBookingEndpoints.cs
public static class PublicBookingEndpoints
{
    public static IEndpointRouteBuilder MapPublicBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public/booking");
        // NO .RequireAuthorization() -- intentionally public

        group.MapPost("/", async (SubmitSelfBookingCommand command, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result<Guid>>(command);
            return result.ToCreatedHttpResult("/api/public/booking");
        });

        group.MapGet("/status/{referenceNumber}", async (string referenceNumber, IMessageBus bus) =>
        {
            var result = await bus.InvokeAsync<Result<BookingStatusDto>>(
                new CheckBookingStatusQuery(referenceNumber));
            return result.ToHttpResult();
        });

        group.MapGet("/doctors", async (IMessageBus bus) =>
        {
            // Return list of doctors with available time slots
            var doctors = await bus.InvokeAsync<List<PublicDoctorDto>>(new GetPublicDoctorsQuery());
            return Results.Ok(doctors);
        });

        group.MapGet("/schedule", async (IMessageBus bus) =>
        {
            var schedule = await bus.InvokeAsync<ClinicScheduleDto>(new GetClinicScheduleQuery());
            return Results.Ok(schedule);
        });

        return app;
    }
}

// Frontend: Public booking route (outside _authenticated layout)
// app/routes/book/index.tsx -- not under _authenticated, no auth check
```

### Pattern 5: FullCalendar React Integration

**What:** Weekly calendar view per doctor with appointment events.
**When to use:** Appointment calendar page.

```typescript
// features/scheduling/components/AppointmentCalendar.tsx
import FullCalendar from '@fullcalendar/react'
import timeGridPlugin from '@fullcalendar/timegrid'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import type { EventClickArg, DateSelectArg, EventDropArg } from '@fullcalendar/core'

const APPOINTMENT_COLORS = {
  NewPatient: { backgroundColor: '#3b82f6', borderColor: '#2563eb' },    // blue
  FollowUp: { backgroundColor: '#22c55e', borderColor: '#16a34a' },      // green
  Treatment: { backgroundColor: '#f97316', borderColor: '#ea580c' },     // orange
  OrthoK: { backgroundColor: '#a855f7', borderColor: '#9333ea' },        // purple
}

export function AppointmentCalendar({ doctorId, onSlotClick, onEventClick, onEventDrop }) {
  const { data: appointments } = useAppointmentsByDoctor(doctorId, dateRange)
  const { data: schedule } = useClinicSchedule()

  const events = appointments?.map(apt => ({
    id: apt.id,
    title: apt.patientName,
    start: apt.startTime,
    end: apt.endTime,
    ...APPOINTMENT_COLORS[apt.type],
    extendedProps: { ...apt },
  }))

  return (
    <FullCalendar
      plugins={[timeGridPlugin, dayGridPlugin, interactionPlugin]}
      initialView="timeGridWeek"
      headerToolbar={{
        left: 'prev,next today',
        center: 'title',
        right: 'timeGridWeek,timeGridDay'
      }}
      slotMinTime="08:00:00"
      slotMaxTime="21:00:00"
      slotDuration="00:15:00"
      selectable={true}
      editable={true}
      eventOverlap={false}
      selectOverlap={false}
      businessHours={schedule?.businessHours}
      select={onSlotClick}        // Click empty slot -> booking dialog
      eventClick={onEventClick}   // Click event -> detail dialog
      eventDrop={onEventDrop}     // Drag-drop -> reschedule
      events={events}
      locale="vi"
      firstDay={1}                // Monday first
    />
  )
}
```

### Anti-Patterns to Avoid

- **Cross-module EF navigation properties:** Patient and Scheduling are separate modules. Scheduling stores DoctorId and PatientId as raw Guids, never as navigation properties to Auth.User or Patient.Patient entities. Use Contracts DTOs for display data.
- **Sharing PatientDbContext with SchedulingDbContext:** Each module has its own DbContext and schema. If Scheduling needs patient name for display, it stores a denormalized copy (from PatientRegistered event) or queries via a lightweight contract service.
- **Storing sequence counter in application memory:** Always use database sequence for GK-YYYY-NNNN to handle concurrent registrations correctly.
- **Client-side-only double-booking prevention:** The calendar UI should show occupied slots, but the server MUST also validate. Never trust the client.
- **Putting allergy data in a JSON column:** Use a proper owned entity collection for allergies so they can be indexed, queried, and validated individually.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Calendar UI | Custom grid/table calendar | FullCalendar @fullcalendar/react | Handles timezone, drag-drop, responsive, event overlap, business hours -- months of work to replicate |
| Autocomplete/search dropdown | Custom input + dropdown | cmdk (shadcn Command) + Popover | Keyboard navigation, accessibility, fuzzy matching, composable -- complex to build correctly |
| Date picker | Custom date input | react-day-picker (shadcn Calendar) | Localization, accessibility, range selection, keyboard nav |
| Debounced search | setTimeout/clearTimeout manually | useDeferredValue or custom useDebounce hook | React 19 has built-in useDeferredValue; for query string debouncing use a small custom hook (~5 lines) |
| Rate limiting (anti-spam) | Custom IP/phone tracking | ASP.NET Core rate limiting middleware (built-in .NET 8+) | Microsoft.AspNetCore.RateLimiting with fixed/sliding window policies. Built-in, tested, configurable. |
| Vietnamese text normalization | Custom Unicode decomposition | SQL Server collation (Vietnamese_CI_AI) | Database does accent-insensitive comparison natively. No application-layer normalization needed for search. |
| Patient photo storage | Local file system | Azure Blob Storage (already in project) | SAS tokens, CDN, versioning, soft delete -- already integrated via IAzureBlobService |

**Key insight:** The calendar and search components represent weeks of development each if built from scratch. FullCalendar's free tier covers the requirements completely. The shadcn Command pattern handles autocomplete search elegantly. SQL Server collation handles Vietnamese diacritics without any application code.

## Common Pitfalls

### Pitfall 1: GK-YYYY-NNNN Sequence Reset at Year Boundary
**What goes wrong:** The sequence counter must reset to 0001 each January 1st, but SQL Server sequences don't auto-reset by year.
**Why it happens:** Developers assume a single auto-increment sequence handles the year-scoped counter.
**How to avoid:** Two approaches: (A) Store year + sequence number separately, query MAX(SequenceNumber) WHERE Year = currentYear (simple, fine for low concurrency). (B) Create a new SQL sequence per year via migration or stored procedure. Approach A is recommended for a boutique clinic.
**Warning signs:** Patient codes jumping from GK-2026-0150 to GK-2027-0151 instead of GK-2027-0001.

### Pitfall 2: Calendar Timezone Handling
**What goes wrong:** Appointments display at wrong times because the server stores UTC but the calendar renders in local time (or vice versa).
**Why it happens:** Mixing UTC and local time without explicit timezone handling.
**How to avoid:** Store all times as UTC in the database. The frontend converts to Asia/Ho_Chi_Minh (UTC+7) for display. FullCalendar has a `timeZone` prop -- set it to `'Asia/Ho_Chi_Minh'`. The API accepts and returns ISO 8601 with timezone offset.
**Warning signs:** Appointments appearing 7 hours earlier or later than expected.

### Pitfall 3: Race Condition in Double-Booking Check
**What goes wrong:** Two staff members book the same slot simultaneously. The application-level overlap check passes for both because neither has committed yet.
**Why it happens:** The overlap check (SELECT) and the insert happen in separate transactions or with READ COMMITTED isolation.
**How to avoid:** Use a unique filtered index as the ultimate safety net. Catch the `DbUpdateException` for unique constraint violation and return a user-friendly "slot already taken" error. The application-level check prevents most conflicts; the database constraint catches the rare race condition.
**Warning signs:** Duplicate appointments appearing for the same doctor at the same time.

### Pitfall 4: Allergy Data Scattered Across Modules
**What goes wrong:** Allergy display needed in prescribing (Phase 5), but Patient module data isn't accessible from Pharmacy module.
**Why it happens:** Modular monolith boundaries prevent direct database access across modules.
**How to avoid:** Design the allergy alert as a reusable frontend component that fetches from the Patient API. The prescribing page calls the Patient API to get allergies by patientId and displays the alert banner. No cross-module backend coupling needed -- the frontend orchestrates the data.
**Warning signs:** Developers adding Patient.Infrastructure as a dependency to Pharmacy.Infrastructure.

### Pitfall 5: Self-Booking Spam
**What goes wrong:** Bots or malicious users flood the self-booking endpoint with thousands of fake booking requests.
**Why it happens:** Public endpoint with no authentication and no rate limiting.
**How to avoid:** Apply ASP.NET Core rate limiting middleware on the public booking group (e.g., max 3 requests per phone number per hour). Also enforce max 2 pending bookings per phone number at the application level. Consider a simple honeypot field or basic CAPTCHA if spam becomes an issue post-launch.
**Warning signs:** Database filling with fake SelfBookingRequest records, staff overwhelmed with pending approvals.

### Pitfall 6: FullCalendar Bundle Size
**What goes wrong:** Importing all FullCalendar plugins bloats the bundle.
**Why it happens:** Developers import from '@fullcalendar/core' instead of specific plugins.
**How to avoid:** Only import the specific plugins needed: @fullcalendar/timegrid, @fullcalendar/daygrid, @fullcalendar/interaction. Do NOT install premium plugins (resource-timegrid, timeline) -- they add bundle weight and license requirements for unused features.
**Warning signs:** Frontend bundle size increase of more than 150KB gzipped from FullCalendar.

## Code Examples

### Patient Registration Handler (Backend)

```csharp
// Source: Follows established CreateUserHandler pattern from Auth module
// Patient.Application/Features/RegisterPatient.cs

public record RegisterPatientCommand(
    string FullName,
    string Phone,
    DateTime? DateOfBirth,
    Gender? Gender,
    PatientType Type,           // Medical or WalkIn
    string? Address,
    string? Cccd,
    List<AllergyInput>? Allergies);

public record AllergyInput(string Name, AllergySeverity Severity);

public class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20)
            .Matches(@"^0\d{9,10}$").WithMessage("Invalid Vietnamese phone number.");

        // Medical patients require DOB and Gender
        When(x => x.Type == PatientType.Medical, () =>
        {
            RuleFor(x => x.DateOfBirth).NotNull();
            RuleFor(x => x.Gender).NotNull();
        });

        RuleForEach(x => x.Allergies).ChildRules(allergy =>
        {
            allergy.RuleFor(a => a.Name).NotEmpty().MaximumLength(200);
            allergy.RuleFor(a => a.Severity).IsInEnum();
        });
    }
}

public static class RegisterPatientHandler
{
    public static async Task<Result<Guid>> Handle(
        RegisterPatientCommand command,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IValidator<RegisterPatientCommand> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return Result<Guid>.Failure(Error.Validation(
                string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))));

        // Check duplicate phone
        var phoneExists = await patientRepository.PhoneExistsAsync(command.Phone, cancellationToken);
        if (phoneExists)
            return Result<Guid>.Failure(Error.Conflict("A patient with this phone number already exists."));

        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var patient = Patient.Create(
            command.FullName, command.Phone, command.Type, branchId,
            command.DateOfBirth, command.Gender, command.Address, command.Cccd);

        if (command.Allergies is { Count: > 0 })
        {
            foreach (var allergy in command.Allergies)
                patient.AddAllergy(allergy.Name, allergy.Severity);
        }

        patientRepository.Add(patient);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate patient code after save (sequence number assigned by DB)
        patient.SetPatientCode(DateTime.UtcNow.Year);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.Id;
    }
}
```

### Global Search (Frontend)

```typescript
// Source: shadcn Command + Popover pattern
// shared/components/GlobalSearch.tsx

import { useState, useDeferredValue } from 'react'
import { useNavigate } from '@tanstack/react-router'
import { Command, CommandInput, CommandList, CommandEmpty, CommandGroup, CommandItem } from '@/shared/components/Command'
import { Popover, PopoverContent, PopoverTrigger } from '@/shared/components/Popover'
import { IconSearch, IconUser } from '@tabler/icons-react'
import { usePatientSearch } from '@/features/patient/hooks/usePatientSearch'
import { useRecentPatientsStore } from '@/shared/stores/recentPatientsStore'

export function GlobalSearch() {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState('')
  const deferredQuery = useDeferredValue(query)
  const { data: results, isLoading } = usePatientSearch(deferredQuery, { enabled: deferredQuery.length >= 2 })
  const recentPatients = useRecentPatientsStore(s => s.recent)
  const navigate = useNavigate()

  const showRecent = open && query.length < 2

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <button className="...">
          <IconSearch className="h-4 w-4" />
          <span>{t('common.search')}</span>
        </button>
      </PopoverTrigger>
      <PopoverContent className="w-80 p-0" align="start">
        <Command shouldFilter={false}>
          <CommandInput value={query} onValueChange={setQuery} placeholder="..." />
          <CommandList>
            {showRecent && recentPatients.length > 0 && (
              <CommandGroup heading={t('patient.recent')}>
                {recentPatients.map(p => (
                  <CommandItem key={p.id} onSelect={() => navigate({ to: `/patients/${p.id}` })}>
                    <IconUser className="h-4 w-4 mr-2" />
                    <span>{p.fullName}</span>
                    <span className="ml-auto text-xs text-muted-foreground">{p.patientCode}</span>
                  </CommandItem>
                ))}
              </CommandGroup>
            )}
            {!showRecent && (
              <>
                <CommandEmpty>{isLoading ? t('common.loading') : t('common.noResults')}</CommandEmpty>
                <CommandGroup heading={t('sidebar.patients')}>
                  {results?.map(p => (
                    <CommandItem key={p.id} onSelect={() => { navigate({ to: `/patients/${p.id}` }); setOpen(false) }}>
                      <IconUser className="h-4 w-4 mr-2" />
                      <div className="flex flex-col">
                        <span>{p.fullName}</span>
                        <span className="text-xs text-muted-foreground">{p.patientCode} &middot; {p.phone}</span>
                      </div>
                    </CommandItem>
                  ))}
                </CommandGroup>
              </>
            )}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
```

### Appointment Booking with Overlap Check (Backend)

```csharp
// Scheduling.Application/Features/BookAppointment.cs
public record BookAppointmentCommand(
    Guid PatientId,
    Guid DoctorId,
    DateTime StartTime,
    Guid AppointmentTypeId,
    string? Notes);

public static class BookAppointmentHandler
{
    public static async Task<Result<Guid>> Handle(
        BookAppointmentCommand command,
        IAppointmentRepository appointmentRepository,
        IClinicScheduleRepository scheduleRepository,
        IUnitOfWork unitOfWork,
        IValidator<BookAppointmentCommand> validator,
        CancellationToken cancellationToken)
    {
        // Validate input
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return Result<Guid>.Failure(Error.Validation(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))));

        // Get appointment type for duration
        var appointmentType = await appointmentRepository.GetAppointmentTypeAsync(
            command.AppointmentTypeId, cancellationToken);
        if (appointmentType is null)
            return Result<Guid>.Failure(Error.NotFound("AppointmentType", command.AppointmentTypeId));

        var endTime = command.StartTime.AddMinutes(appointmentType.DefaultDurationMinutes);

        // Check clinic operating hours
        var schedule = await scheduleRepository.GetForDayAsync(
            command.StartTime.DayOfWeek, cancellationToken);
        if (schedule is null || !schedule.IsOpen)
            return Result<Guid>.Failure(Error.Validation("Clinic is closed on this day."));
        if (!schedule.IsWithinHours(command.StartTime.TimeOfDay, endTime.TimeOfDay))
            return Result<Guid>.Failure(Error.Validation("Appointment is outside clinic operating hours."));

        // Check for double-booking
        var hasOverlap = await appointmentRepository.HasOverlappingAsync(
            command.DoctorId, command.StartTime, endTime, cancellationToken);
        if (hasOverlap)
            return Result<Guid>.Failure(Error.Conflict("This time slot is already booked for the selected doctor."));

        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var appointment = Appointment.Create(
            command.PatientId, command.DoctorId, command.StartTime, endTime,
            command.AppointmentTypeId, branchId, command.Notes);

        appointmentRepository.Add(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| FullCalendar v5 (bundled plugins) | FullCalendar v6 (peer-dep @fullcalendar/core) | 2023 (v6 release) | Must install @fullcalendar/core separately. Import utilities from core, not from connector. |
| react-big-calendar for all calendars | FullCalendar for feature-rich, Schedule-X for modern design | 2024-2025 | react-big-calendar is less actively maintained. FullCalendar and Schedule-X are the current leaders. |
| AutoMapper for DTO mapping | Riok.Mapperly (source gen) | 2024 (AutoMapper went commercial) | Zero runtime overhead. Already noted in project stack research. |
| Custom patient ID with MAX() query | EF Core HasSequence + UseSequence (EF Core 7+) | 2022 (EF Core 7) | Database-level sequence generation is now first-class in EF Core. |
| Application-level collation workarounds | EF.Functions.Collate() (EF Core 5+) | 2021 (EF Core 5) | Query-level collation for accent-insensitive search without schema changes. |

**Deprecated/outdated:**
- Swashbuckle.AspNetCore: Still in project but being replaced by native OpenAPI in .NET. Not relevant for this phase.
- FullCalendar v4/v5 patterns: v6 changed plugin loading. Context7 docs showed v4/v5 patterns -- use v6 API.

## Open Questions

1. **Year-boundary sequence reset strategy**
   - What we know: SQL Server sequences don't auto-reset per year. We need GK-YYYY-NNNN with NNNN resetting to 0001 each January.
   - What's unclear: Whether to use a SQL Server sequence (and manually reset/create new ones per year) or application-level MAX() query.
   - Recommendation: Use application-level approach: store Year and SequenceNumber columns, query MAX(SequenceNumber) WHERE Year = currentYear, add 1. Unique constraint on (Year, SequenceNumber) prevents duplicates. Simpler than managing yearly sequences for a clinic with <5000 patients/year.

2. **Doctor list source for scheduling**
   - What we know: Doctors are Auth.User entities with the "Doctor" role. Scheduling module can't reference Auth module directly.
   - What's unclear: How Scheduling gets the list of doctors for calendar filtering and booking.
   - Recommendation: Create a lightweight synchronous query contract: `IGetDoctorsQuery` in Scheduling.Contracts, implemented via Auth module. Or: seed a denormalized DoctorLookup table in Scheduling schema, updated via UserCreated/UserUpdated integration events from Auth. The synchronous query approach is simpler for Phase 02 (few doctors, low frequency).

3. **FullCalendar React 19 compatibility**
   - What we know: @fullcalendar/react v6.1.20 was last published ~2 months ago. Project uses React 19.
   - What's unclear: Explicit React 19 peer dependency support not confirmed in docs.
   - Recommendation: Install and test. FullCalendar React wrapper is thin -- it renders a div and calls FullCalendar API. React 19 compatibility issues are unlikely but should be validated during implementation. If issues arise, the wrapper is simple enough to fork/fix.

4. **Photo upload for patient profile**
   - What we know: Azure Blob Storage is already integrated (IAzureBlobService). Patient photos are optional with initials avatar fallback.
   - What's unclear: Whether to implement photo upload in this phase or defer to avoid scope creep.
   - Recommendation: Include a basic photo upload in this phase (it's mentioned in the user decisions). Reuse the existing IAzureBlobService. Store photo URL on Patient entity. Simple file input with preview -- no cropping or resizing needed for v1.

## Sources

### Primary (HIGH confidence)
- Context7 /dotnet/entityframework.docs - HasSequence, HasIndex IsUnique, UseCollation, global query filters, computed columns
- Context7 /fullcalendar/fullcalendar-docs - React integration, resource views, eventOverlap, eventConstraint, drag-drop configuration
- Context7 /schedule-x/schedule-x - Resource scheduler config, React integration, drag-and-drop plugin
- Existing codebase (Phase 01/01.1/01.2) - Established patterns for domain entities, handlers, repositories, IoC, Minimal API endpoints, frontend features

### Secondary (MEDIUM confidence)
- [FullCalendar Pricing](https://fullcalendar.io/pricing) - Standard (free, MIT) vs Premium ($480+/dev/yr). timeGridWeek is free. resource-timegrid is premium.
- [FullCalendar Plugin Index](https://fullcalendar.io/docs/plugin-index) - Free plugins: daygrid, timegrid, interaction, list. Premium: resource-timegrid, resource-timeline.
- [FullCalendar React docs](https://fullcalendar.io/docs/react) - v6 requires @fullcalendar/core as peer dependency
- [Schedule-X Premium](https://schedule-x.dev/premium) - EUR 299/yr per developer. Resource scheduler is premium only.
- [EF Core Collations](https://learn.microsoft.com/en-us/ef/core/miscellaneous/collations-and-case-sensitivity) - EF.Functions.Collate() for query-level collation, UseCollation() for column-level
- [SQL Server Vietnamese Collation](https://learn.microsoft.com/en-us/answers/questions/1385767/sql-server-2017-vietnamese-collation) - Vietnamese_CI_AI collation for accent-insensitive matching
- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit) - Built-in rate limiting middleware for .NET 7+

### Tertiary (LOW confidence)
- FullCalendar React 19 compatibility - Not explicitly confirmed in docs. v6.1.20 likely works but needs testing.
- cmdk v1.0.4 React 19 compatibility - cmdk is widely used with latest React. Likely compatible but verify.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All backend libraries already in project. FullCalendar free tier verified as sufficient for requirements.
- Architecture: HIGH - Follows exact patterns from Phase 01 (handler-per-message, repository-per-aggregate, Minimal API, schema-per-module).
- Pitfalls: HIGH - Double-booking prevention, timezone handling, sequence reset, and cross-module boundaries are well-documented patterns.
- Calendar library choice: MEDIUM - FullCalendar free tier covers requirements, but React 19 compatibility and specific v6 behavior need validation during implementation.
- Vietnamese search: HIGH - SQL Server Vietnamese_CI_AI collation is purpose-built for this exact use case.

**Research date:** 2026-03-01
**Valid until:** 2026-03-31 (stable domain, established patterns)
