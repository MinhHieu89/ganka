# Phase 5: Prescriptions & Document Printing - Research

**Researched:** 2026-03-05
**Domain:** Drug/Optical Prescriptions, PDF Generation, Pharmacy Catalog, Allergy Checking
**Confidence:** HIGH

## Summary

Phase 5 adds drug and optical prescription writing to the clinical visit workflow, implements drug-allergy cross-checking, builds the pharmacy drug catalog with seeding, and introduces backend PDF generation for all clinical document types. The phase spans three modules: Clinical (prescriptions as visit child entities), Pharmacy (drug catalog and seeding), and a new shared document generation service using QuestPDF.

The codebase has well-established patterns for all major implementation concerns: Visit aggregate child entities with backing fields and `EnsureEditable()` guards, VisitSection collapsible cards with `headerExtra` slots, Icd10Combobox search-and-select patterns, AllergyAlert banner components, and AllergyCatalogSeeder as the IHostedService seeding template. The Pharmacy module scaffolding already exists (empty Domain/Application/Contracts/Infrastructure layers with PharmacyDbContext using "pharmacy" schema). The existing Refraction entity provides the exact data model to auto-populate optical prescriptions.

**Primary recommendation:** Follow existing aggregate child entity patterns exactly -- DrugPrescription and OpticalPrescription as Visit children with backing field collections. Use QuestPDF 2026.2.2 with embedded Vietnamese-compatible font (Noto Sans) for all PDF generation. Build drug catalog in Pharmacy module, reference it from Clinical via Pharmacy.Contracts. Cross-reference patient allergies via Patient.Contracts for allergy warnings during prescribing.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Hybrid dosage entry: structured fields (dose, frequency, duration, route) auto-generate instruction text, PLUS free-text override field for doctor customization
- Add-then-edit pattern per drug line: doctor clicks "Add Drug" -> fills form in dialog/inline row -> clicks "Add" to commit. Edit/remove available per line. Same interaction pattern as Diagnosis section
- Separate "Drug Prescription" VisitSection on visit detail page (not combined with optical Rx)
- Drug Rx and Optical Rx are fully independent -- doctor can write either, both, or neither per visit
- Prescriptions become read-only after visit sign-off (follows existing immutability pattern)
- Inline red warning banner appears immediately below drug field when allergy match detected during drug selection
- Blocking AlertDialog confirmation when doctor tries to save/finalize a prescription with allergy conflict -- doctor must explicitly confirm or cancel
- Both mechanisms combined: awareness during selection + hard stop before commit
- Separate "Optical Prescription" VisitSection on visit detail page
- Auto-populate SPH/CYL/AXIS/ADD/PD from current visit's manifest refraction, doctor can edit values before finalizing
- Distance Rx / Near Rx sections for bifocal/progressive prescriptions
- Far PD / Near PD fields (interpupillary distance for distance and near)
- Lens type recommendation field (single vision / bifocal / progressive / reading)
- Backend PDF generation via QuestPDF (.NET library, free/MIT) for all document types
- Frontend gets PDF URL, opens in new browser tab for print/download
- A5 paper size (148 x 210mm) for drug prescriptions -- standard Vietnamese clinical convention
- A4 for other documents (referral letters, consent forms, optical Rx) -- Claude's discretion on per-document sizing
- Configurable clinic header template: admin can set logo image, clinic name, address, phone, fax, license number, tagline -- stored as clinic settings, all documents pull from this config
- Invoice/receipt printing (PRT-03) deferred to Phase 7 when billing system exists
- Drug catalog entity lives in Pharmacy module (already scaffolded, empty) -- Clinical module references via Contracts/cross-module query
- Seeded with ~50-100 common ophthalmic drugs (eye drops, antibiotics, anti-inflammatories, etc.)
- All Vietnamese seed data must use proper diacritics (consistent with Phase 01.2 decision)
- Essential fields per drug: name (EN/VI), generic name, form (drops/tablet/ointment/injection), strength (e.g., 0.5%), route (topical/oral/IM), unit (bottle/tube/box), default dosage template
- Phase 6 adds supplier, price, batch tracking, stock levels to existing catalog entries
- Admin UI for drug catalog management (add/edit drugs, Excel import for bulk additions)
- Off-catalog drugs: doctor types drug name + dosage manually as free-text entry, flagged as "off-catalog" (no stock deduction link in Phase 6)

### Claude's Discretion
- Copy-from-previous-visit shortcut for optical Rx (whether to include and how)
- Exact A4 vs A5 sizing per non-prescription document type
- Staining score / grading display on printed documents
- Pharmacy label layout and label paper size
- Referral letter and consent form content templates
- Drug catalog seed data selection (specific drugs to include)
- Loading states and error handling
- Exact MOH required field placement on prescription PDF

### Deferred Ideas (OUT OF SCOPE)
- Invoice/receipt printing (PRT-03) -- deferred to Phase 7 when billing system is built
- Full MOH drug database import -- Vietnamese Duoc dien is too large for boutique clinic needs; seed ~50-100 common drugs instead
- Drug interaction checking (beyond allergy warnings) -- explicitly out of scope for v1 per PROJECT.md
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| RX-01 | Doctor can write drug prescription by selecting from pharmacy catalog or adding off-catalog drugs | Drug catalog in Pharmacy module, DrugPrescription + PrescriptionItem entities as Visit children, DrugCombobox search pattern mirroring Icd10Combobox |
| RX-02 | Catalog-linked prescriptions auto-deduct stock when dispensed; off-catalog drugs flagged as manual | PrescriptionItem.DrugCatalogItemId nullable FK -- null = off-catalog. Stock deduction is Phase 6 scope, but flag must exist now |
| RX-03 | Doctor can write glasses prescription (optical Rx) with full refraction parameters | OpticalPrescription entity with OD/OS SPH/CYL/AXIS/ADD/PD fields, auto-populated from manifest Refraction |
| RX-04 | Prescriptions comply with MOH format requirements (required fields, dosage format per Bo Y te) | QuestPDF A5 layout with MOH-required fields: clinic header, patient info, diagnosis, drug list with dosage, doctor signature, prescription date, loi dan |
| RX-05 | System warns when prescribing drugs the patient is allergic to | Cross-reference patient allergies (Patient.Contracts) against drug selection, inline AllergyAlert + blocking AlertDialog |
| PRT-01 | System prints drug prescriptions with clinic header, doctor name, patient info, drug list with dosage | QuestPDF DrugPrescriptionDocument with A5 page, configurable ClinicHeader, drug table rows |
| PRT-02 | System prints glasses prescriptions (optical Rx) with refraction parameters | QuestPDF OpticalPrescriptionDocument with A4 page, OD/OS refraction grid |
| PRT-03 | System prints invoices/receipts with itemized charges and payment method | DEFERRED to Phase 7 -- explicitly excluded per CONTEXT.md |
| PRT-04 | System prints referral letters (giay chuyen vien) with patient info, diagnosis, reason | QuestPDF ReferralLetterDocument with A4 page, patient info, diagnosis, referral reason |
| PRT-05 | System prints treatment consent forms with patient name, procedure type, date | QuestPDF ConsentFormDocument with A4 page, procedure description, patient signature line |
| PRT-06 | System prints pharmacy labels with patient name, drug name, dose, frequency, expiry | QuestPDF PharmacyLabelDocument with small label size (~70x35mm), compact drug info |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| QuestPDF | 2026.2.2 | PDF document generation | Locked decision. Free for <$1M revenue (Community MIT). Fluent C# API, supports A4/A5/custom sizes, font embedding, font subsetting since 2024.3.0 |
| Noto Sans | Latest | Vietnamese-compatible PDF font | Google font with full Vietnamese diacritic support (Noto Sans Vietnamese). Free, widely used, clean rendering for clinical documents |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| FluentValidation | 12.* | Command validation | Already in stack. Use for prescription command validation (required fields, dosage format) |
| NSubstitute | 5.* | Test mocking | Already in stack. Mock repositories for prescription handler tests |
| FluentAssertions | 8.* | Test assertions | Already in stack. Assert prescription creation, allergy detection |
| xunit | 2.* | Test framework | Already in stack. Unit tests for all handlers |

### Frontend (existing, no new packages needed)
| Library | Already Installed | Purpose |
|---------|------------------|---------|
| React Hook Form + Zod | Yes | Prescription forms with validation |
| TanStack Query | Yes | API queries and mutations for prescriptions |
| shadcn/ui components | Yes | Card, Dialog, AlertDialog, Select, Command, Button, Badge, Input |
| @tabler/icons-react | Yes | Icons for prescription UI |
| react-i18next | Yes | i18n for prescription labels |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| QuestPDF | iTextSharp/iText7 | iText7 is AGPL (requires commercial license). QuestPDF is locked decision |
| QuestPDF | Puppeteer/Playwright HTML-to-PDF | Heavy dependency, server-side browser. QuestPDF is native .NET, no browser needed |
| Noto Sans | System fonts | System fonts vary by deployment environment. Embedded font guarantees consistent rendering |

**Installation (backend):**
```bash
# Add to Directory.Packages.props
<PackageVersion Include="QuestPDF" Version="2026.2.*" />

# Add to Clinical.Infrastructure or a new Shared.Documents project
<PackageReference Include="QuestPDF" />
```

**No new frontend packages needed** -- all UI components use existing shadcn/ui + React Hook Form + TanStack Query stack.

## Architecture Patterns

### Recommended Project Structure

```
backend/src/
  Modules/
    Clinical/
      Clinical.Domain/
        Entities/
          DrugPrescription.cs          # Visit child aggregate
          PrescriptionItem.cs          # Per-drug line in prescription
          OpticalPrescription.cs       # Visit child aggregate
        Enums/
          DrugForm.cs                  # Drops/Tablet/Ointment/Injection
          DrugRoute.cs                 # Topical/Oral/IM/IV
          LensType.cs                  # SingleVision/Bifocal/Progressive/Reading
          PrescriptionFrequency.cs     # Common frequencies for structured dosage
      Clinical.Application/
        Features/
          AddDrugPrescription.cs       # Command + Handler
          UpdateDrugPrescription.cs
          RemoveDrugPrescription.cs
          AddOpticalPrescription.cs
          UpdateOpticalPrescription.cs
          CheckDrugAllergy.cs          # Cross-module query
          GenerateDocument.cs          # PDF generation endpoint
        Interfaces/
          IDocumentService.cs          # PDF generation abstraction
          IDrugCatalogQuery.cs         # Cross-module drug catalog query
      Clinical.Contracts/
        Dtos/
          DrugPrescriptionDto.cs
          PrescriptionItemDto.cs
          OpticalPrescriptionDto.cs
          DocumentGenerateRequest.cs
      Clinical.Infrastructure/
        Configurations/
          DrugPrescriptionConfiguration.cs
          PrescriptionItemConfiguration.cs
          OpticalPrescriptionConfiguration.cs
        Documents/                      # QuestPDF document implementations
          DrugPrescriptionDocument.cs
          OpticalPrescriptionDocument.cs
          ReferralLetterDocument.cs
          ConsentFormDocument.cs
          PharmacyLabelDocument.cs
          Shared/
            ClinicHeaderComponent.cs    # Reusable header for all documents
            DocumentFontManager.cs      # Font registration utility
        Services/
          DocumentService.cs            # IDocumentService implementation
    Pharmacy/
      Pharmacy.Domain/
        Entities/
          DrugCatalogItem.cs           # Drug catalog entry
        Enums/
          DrugForm.cs                  # Shared with Clinical? Or reference via Contracts
          DrugRoute.cs
      Pharmacy.Application/
        Features/
          SearchDrugCatalog.cs
          CreateDrugCatalogItem.cs
          UpdateDrugCatalogItem.cs
          ImportDrugCatalog.cs         # Excel bulk import
        Interfaces/
          IDrugCatalogItemRepository.cs
      Pharmacy.Contracts/
        Dtos/
          DrugCatalogItemDto.cs
          SearchDrugCatalogQuery.cs
      Pharmacy.Infrastructure/
        Configurations/
          DrugCatalogItemConfiguration.cs
        Repositories/
          DrugCatalogItemRepository.cs
        Seeding/
          DrugCatalogSeeder.cs         # IHostedService seeding
    Shared/
      Shared.Application/
        Services/
          IClinicSettingsService.cs    # Clinic header config service interface
  Shared/
    Shared.Infrastructure/
      Entities/
        ClinicSettings.cs             # Or in a Settings module

frontend/src/
  features/
    clinical/
      components/
        DrugPrescriptionSection.tsx    # VisitSection wrapper
        DrugPrescriptionForm.tsx       # Add/edit drug line dialog
        DrugCombobox.tsx               # Drug catalog search (like Icd10Combobox)
        DrugAllergyWarning.tsx         # Inline red warning banner
        OpticalPrescriptionSection.tsx # VisitSection wrapper
        OpticalPrescriptionForm.tsx    # SPH/CYL/AXIS/ADD/PD form
        PrintButton.tsx               # Generic print PDF button
      api/
        prescription-api.ts           # Drug & optical Rx API hooks
        document-api.ts               # PDF generation API hooks
    pharmacy/                          # New feature folder
      components/
        DrugCatalogPage.tsx            # Admin drug catalog management
        DrugCatalogTable.tsx
        DrugFormDialog.tsx
        DrugImportDialog.tsx           # Excel import
      api/
        pharmacy-api.ts
    admin/
      components/
        ClinicSettingsPage.tsx         # Clinic header configuration
```

### Pattern 1: Visit Child Entity (DrugPrescription)
**What:** DrugPrescription as a Visit aggregate child, following the exact pattern of DryEyeAssessment/VisitDiagnosis
**When to use:** Always for prescription entities
**Example:**
```csharp
// Domain: Visit.cs -- add backing field collection
private readonly List<DrugPrescription> _drugPrescriptions = [];
public IReadOnlyCollection<DrugPrescription> DrugPrescriptions => _drugPrescriptions.AsReadOnly();

public void AddDrugPrescription(DrugPrescription prescription)
{
    EnsureEditable();
    _drugPrescriptions.Add(prescription);
    SetUpdatedAt();
}

// Domain: DrugPrescription.cs
public class DrugPrescription : Entity
{
    public Guid VisitId { get; private set; }
    public string? Notes { get; private set; }          // "Loi dan" (doctor's advice)
    private readonly List<PrescriptionItem> _items = [];
    public IReadOnlyCollection<PrescriptionItem> Items => _items.AsReadOnly();
    // Factory, Update, AddItem methods...
}

// Domain: PrescriptionItem.cs
public class PrescriptionItem : Entity
{
    public Guid DrugPrescriptionId { get; private set; }
    public Guid? DrugCatalogItemId { get; private set; }  // null = off-catalog
    public string DrugName { get; private set; }           // Denormalized
    public string? GenericName { get; private set; }
    public string? Strength { get; private set; }
    public DrugForm Form { get; private set; }
    public DrugRoute Route { get; private set; }
    public string Dosage { get; private set; }             // Structured auto-generated text
    public string? DosageOverride { get; private set; }    // Free-text override
    public int Quantity { get; private set; }
    public string Unit { get; private set; }
    public string? Frequency { get; private set; }         // e.g., "3 lan/ngay"
    public int? DurationDays { get; private set; }
    public bool IsOffCatalog { get; private set; }
    public bool HasAllergyWarning { get; private set; }    // Flagged but doctor confirmed
    public int SortOrder { get; private set; }
}
```

### Pattern 2: Cross-Module Drug Catalog Query
**What:** Clinical module queries Pharmacy drug catalog via Contracts query record + Wolverine bus
**When to use:** When doctor searches for drugs during prescribing
**Example:**
```csharp
// Pharmacy.Contracts/Dtos/SearchDrugCatalogQuery.cs
public sealed record SearchDrugCatalogQuery(string SearchTerm);

// Pharmacy.Application/Features/SearchDrugCatalog.cs
public static class SearchDrugCatalogHandler
{
    public static async Task<List<DrugCatalogItemDto>> Handle(
        SearchDrugCatalogQuery query,
        IDrugCatalogItemRepository repository,
        CancellationToken ct)
    {
        return await repository.SearchAsync(query.SearchTerm, ct);
    }
}

// Clinical handler invokes via IMessageBus:
var drugs = await bus.InvokeAsync<List<DrugCatalogItemDto>>(
    new SearchDrugCatalogQuery(searchTerm), ct);
```

### Pattern 3: Allergy Cross-Check
**What:** Cross-reference patient allergies against selected drug during prescribing
**When to use:** When doctor selects a drug from catalog
**Example:**
```csharp
// Application: CheckDrugAllergy handler
// 1. Fetch patient allergies from Patient module via Contracts query
// 2. Compare drug name / generic name against allergy names
// 3. Return list of matching allergies

// Frontend: on drug selection in DrugCombobox
// 1. Call allergy check endpoint or do client-side match
// 2. If match found, show inline red warning (DrugAllergyWarning component)
// 3. On form submit, if allergy warnings exist, show blocking AlertDialog
```

### Pattern 4: QuestPDF Document Generation
**What:** Backend generates PDF, stores as blob or returns stream, frontend opens in new tab
**When to use:** All document printing
**Example:**
```csharp
// IDocumentService interface
public interface IDocumentService
{
    Task<byte[]> GenerateDrugPrescriptionAsync(Guid visitId, CancellationToken ct);
    Task<byte[]> GenerateOpticalPrescriptionAsync(Guid visitId, CancellationToken ct);
    Task<byte[]> GenerateReferralLetterAsync(Guid visitId, CancellationToken ct);
    Task<byte[]> GenerateConsentFormAsync(Guid visitId, string procedureType, CancellationToken ct);
    Task<byte[]> GeneratePharmacyLabelAsync(Guid prescriptionItemId, CancellationToken ct);
}

// QuestPDF Document Implementation
public class DrugPrescriptionDocument : IDocument
{
    private readonly DrugPrescriptionData _data;
    private readonly ClinicHeaderData _clinicHeader;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A5);
            page.Margin(10, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontFamily("Noto Sans").FontSize(9));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        // Clinic logo, name, address, phone, license number
        // Patient info: name, DOB, gender, address, CCCD
        // Diagnosis
    }
}

// Endpoint: returns PDF as file download
group.MapGet("/{visitId:guid}/print/drug-rx", async (Guid visitId, IDocumentService docService, CancellationToken ct) =>
{
    var pdfBytes = await docService.GenerateDrugPrescriptionAsync(visitId, ct);
    return Results.File(pdfBytes, "application/pdf", "drug-prescription.pdf");
});
```

### Pattern 5: Configurable Clinic Header
**What:** Admin-configurable clinic settings stored in database, used in all printed documents
**When to use:** Clinic header on every PDF document
**Example:**
```csharp
public class ClinicSettings : Entity
{
    public string ClinicName { get; private set; }
    public string? ClinicNameVi { get; private set; }
    public string Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Fax { get; private set; }
    public string? LicenseNumber { get; private set; }  // "So GPHN" / license number
    public string? Tagline { get; private set; }
    public string? LogoBlobUrl { get; private set; }
    public Guid BranchId { get; private set; }          // Multi-branch support
}
```

### Anti-Patterns to Avoid
- **Do NOT create prescription entities outside the Visit aggregate:** Prescriptions are Visit children -- always go through Visit.AddDrugPrescription() for EnsureEditable guard
- **Do NOT use client-side PDF generation (jsPDF, html2canvas):** Backend QuestPDF is the locked decision. Provides consistent output, proper font embedding, and server-side control
- **Do NOT hardcode clinic header info:** All clinic branding comes from configurable ClinicSettings entity
- **Do NOT skip the allergy check on backend:** Even though frontend shows warnings, backend must validate allergy flags before persisting prescription
- **Do NOT combine Drug Rx and Optical Rx into one entity:** They are explicitly independent per locked decisions
- **Do NOT use WolverineFx.Http in Pharmacy.Application:** Remove it from csproj (Application layer must be HTTP-free per established pattern)

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| PDF generation | HTML string concatenation or Razor-to-PDF | QuestPDF fluent API | Proper pagination, font subsetting, A4/A5 sizing, image embedding |
| Vietnamese font rendering | System font fallback | Embedded Noto Sans via FontManager.RegisterFont | Consistent rendering across all deployment environments |
| Drug catalog search | Custom text search | SQL Server LIKE with Vietnamese_CI_AI collation | Already used for patient search -- accent-insensitive matching |
| Excel import parsing | Manual CSV/XLSX parsing | ClosedXML or EPPlus (free tier) | Handles encoding, cell types, large files. EPPlus is free for <$1M revenue |
| Form validation | Custom field checks | FluentValidation (backend) + Zod (frontend) | Already in stack, consistent with all other forms |

**Key insight:** The project already has every UI pattern needed for prescription writing (VisitSection, Combobox search, add-then-edit lists, AlertDialog confirmation). The only truly new capability is QuestPDF for PDF generation.

## Common Pitfalls

### Pitfall 1: Vietnamese Diacritics in PDF
**What goes wrong:** PDF renders "Thuoc nho mat" instead of "Thuoc nho mat" with proper diacritics, or renders placeholder glyphs
**Why it happens:** Default QuestPDF Lato font doesn't support all Vietnamese diacritics; system fonts vary by environment
**How to avoid:** Embed Noto Sans font at application startup via `FontManager.RegisterFont()`. Set `QuestPDF.Settings.UseEnvironmentFonts = false` in production. Use `QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = true` in development.
**Warning signs:** Placeholder squares or question marks in PDF output during testing

### Pitfall 2: QuestPDF License Configuration
**What goes wrong:** Exception at runtime: "QuestPDF requires a valid license"
**Why it happens:** QuestPDF requires explicit license type declaration since 2024.x
**How to avoid:** Add `QuestPDF.Settings.License = LicenseType.Community;` during application startup (in Program.cs or IoC registration). Ganka28 qualifies for Community license (<$1M revenue)
**Warning signs:** Runtime exception on first PDF generation attempt

### Pitfall 3: Prescription Immutability After Sign-Off
**What goes wrong:** Doctor can modify prescription after visit is signed off
**Why it happens:** Missing EnsureEditable() guard in domain method, or direct DbContext manipulation bypassing aggregate
**How to avoid:** Always use Visit.AddDrugPrescription() domain method which calls EnsureEditable(). Never add PrescriptionItems directly to DbContext without going through the aggregate
**Warning signs:** Prescription data changes on signed visits

### Pitfall 4: Allergy Matching False Negatives
**What goes wrong:** System doesn't detect an allergy match when it should
**Why it happens:** Case-sensitive or exact-match comparison between allergy name and drug name/generic name
**How to avoid:** Use case-insensitive, accent-insensitive comparison. Match against both drug name AND generic name. Consider partial matching (e.g., patient allergic to "Penicillin" should match "Amoxicillin" note: this is a drug interaction, which is out of scope -- only direct name matches for v1)
**Warning signs:** Known allergies not triggering warnings during prescribing

### Pitfall 5: Pharmacy Module WolverineFx.Http in Application Layer
**What goes wrong:** Architecture test fails because Pharmacy.Application references HTTP concerns
**Why it happens:** The scaffolded Pharmacy.Application.csproj currently includes WolverineFx.Http
**How to avoid:** Remove WolverineFx.Http from Pharmacy.Application.csproj and replace with FluentValidation.DependencyInjectionExtensions (same pattern as Clinical.Application, Auth.Application)
**Warning signs:** Architecture test violations on build

### Pitfall 6: Drug Catalog Cross-Module Dependency Direction
**What goes wrong:** Clinical module takes a direct project reference to Pharmacy.Infrastructure
**Why it happens:** Need to query drug catalog from prescription handler
**How to avoid:** Clinical references only Pharmacy.Contracts. Use Wolverine IMessageBus to invoke queries defined in Pharmacy.Contracts, handled by Pharmacy.Application
**Warning signs:** Circular project references, architecture test violations

### Pitfall 7: A5 Paper Size Margin Calculations
**What goes wrong:** Content overflows or looks cramped on A5 (148x210mm)
**Why it happens:** A5 is significantly smaller than A4, but content designed for A4 margins
**How to avoid:** Use smaller margins (8-10mm), smaller font sizes (8-9pt body, 10-11pt headers), compact table layouts. Test with real data (5-8 drugs per prescription is typical)
**Warning signs:** Content overflow warnings from QuestPDF, truncated drug names

## Code Examples

### QuestPDF Document Setup (application startup)
```csharp
// Source: QuestPDF official docs (questpdf.com/quick-start.html)
// In Program.cs or a startup service:
QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = true; // dev only

// Register Vietnamese-compatible font
using var fontStream = typeof(DocumentFontManager).Assembly
    .GetManifestResourceStream("Clinical.Infrastructure.Documents.Fonts.NotoSans-Regular.ttf");
FontManager.RegisterFont(fontStream!);

// Also register bold variant
using var boldStream = typeof(DocumentFontManager).Assembly
    .GetManifestResourceStream("Clinical.Infrastructure.Documents.Fonts.NotoSans-Bold.ttf");
FontManager.RegisterFont(boldStream!);
```

### QuestPDF A5 Drug Prescription Layout
```csharp
// Source: QuestPDF API reference (questpdf.com/api-reference/page/settings.html)
container.Page(page =>
{
    page.Size(PageSizes.A5);  // 148 x 210 mm
    page.Margin(8, Unit.Millimetre);
    page.DefaultTextStyle(x => x.FontFamily("Noto Sans").FontSize(9));

    page.Header().Column(col =>
    {
        // Row 1: Clinic logo + name + address
        col.Item().Row(row =>
        {
            row.ConstantItem(30).Image(logoBytes);
            row.RelativeItem().Column(c =>
            {
                c.Item().Text(clinicName).Bold().FontSize(11);
                c.Item().Text(clinicAddress).FontSize(7);
                c.Item().Text($"DT: {phone} | Fax: {fax}").FontSize(7);
            });
        });
        // Row 2: "DON THUOC" title centered
        col.Item().PaddingTop(4).AlignCenter()
            .Text("DON THUOC").Bold().FontSize(12);
    });

    page.Content().Column(col =>
    {
        // Patient info block
        col.Item().Row(row =>
        {
            row.RelativeItem().Text($"Ho ten: {patientName}");
            row.ConstantItem(80).Text($"Gioi tinh: {gender}");
        });
        col.Item().Text($"Ngay sinh: {dob}");
        col.Item().Text($"Dia chi: {address}");
        col.Item().Text($"Chan doan: {diagnosis}");

        // Drug table
        col.Item().PaddingTop(4).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(20);   // STT
                cols.RelativeColumn(3);     // Drug name + dosage
                cols.ConstantColumn(40);    // Quantity
                cols.ConstantColumn(35);    // Unit
            });
            // Header row + drug items...
        });

        // Doctor's advice (Loi dan)
        col.Item().PaddingTop(4).Text($"Loi dan: {notes}");
    });

    page.Footer().Row(row =>
    {
        row.RelativeItem().Column(c =>
        {
            c.Item().Text($"Ngay tai kham: {followUpDate}");
        });
        row.ConstantItem(100).AlignCenter().Column(c =>
        {
            c.Item().Text($"Ngay {date}").FontSize(8);
            c.Item().Text("Bac si kham benh").Bold();
            c.Item().PaddingTop(30).Text(doctorName);
        });
    });
});
```

### Drug Catalog Seeder Pattern (mirroring AllergyCatalogSeeder)
```csharp
// Source: Existing AllergyCatalogSeeder in Patient.Infrastructure
public sealed class DrugCatalogSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DrugCatalogSeeder> _logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();

        var existingCount = await dbContext.DrugCatalogItems.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            _logger.LogInformation("DrugCatalogSeeder: Already seeded. Skipping.");
            return;
        }

        var items = GetCatalogItems();
        dbContext.DrugCatalogItems.AddRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<DrugCatalogItem> GetCatalogItems()
    {
        return
        [
            // Eye drops -- Thuoc nho mat
            DrugCatalogItem.Create("Tobramycin 0.3%", "Tobramycin 0.3%",
                "Tobramycin", DrugForm.EyeDrops, "0.3%", DrugRoute.Topical,
                "Chai", "1-2 giot x 4 lan/ngay"),
            DrugCatalogItem.Create("Ofloxacin 0.3%", "Ofloxacin 0.3%",
                "Ofloxacin", DrugForm.EyeDrops, "0.3%", DrugRoute.Topical,
                "Chai", "1-2 giot x 4 lan/ngay"),
            // ... ~50-100 common ophthalmic drugs
        ];
    }
}
```

### Frontend DrugCombobox (mirroring Icd10Combobox pattern)
```typescript
// Source: Existing Icd10Combobox pattern in clinical/components/
// Uses Popover + Command search pattern with Button trigger (not Input)
// shouldFilter={false} for external API-driven search
// Debounced search term state

function DrugCombobox({ onSelect, patientAllergies, disabled }: DrugComboboxProps) {
  const [open, setOpen] = useState(false)
  const [search, setSearch] = useState("")
  const debouncedSearch = useDebounce(search, 300)
  const { data: drugs } = useDrugCatalogSearch(debouncedSearch)

  const checkAllergy = (drugName: string, genericName?: string) => {
    return patientAllergies?.some(a =>
      a.name.toLowerCase() === drugName.toLowerCase() ||
      (genericName && a.name.toLowerCase() === genericName.toLowerCase())
    )
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <div>
          <Button variant="outline" disabled={disabled}>
            {t("prescription.addDrug")}
          </Button>
        </div>
      </PopoverTrigger>
      <PopoverContent>
        <Command shouldFilter={false}>
          <CommandInput value={search} onValueChange={setSearch} />
          <CommandList>
            {drugs?.map(drug => (
              <CommandItem key={drug.id} onSelect={() => {
                onSelect(drug, checkAllergy(drug.name, drug.genericName))
                setOpen(false)
              }}>
                {drug.name} ({drug.strength})
                {checkAllergy(drug.name, drug.genericName) && (
                  <Badge variant="destructive">Allergy</Badge>
                )}
              </CommandItem>
            ))}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
```

### PDF Print Button Pattern (frontend)
```typescript
// Frontend: open PDF in new browser tab
function PrintButton({ visitId, documentType, label }: PrintButtonProps) {
  const handlePrint = () => {
    const token = useAuthStore.getState().accessToken
    const url = `${API_URL}/api/clinical/${visitId}/print/${documentType}`
    // Open in new tab -- browser handles PDF display/print
    window.open(`${url}?access_token=${token}`, '_blank')
  }
  // Or use fetch + blob URL for more control:
  const handlePrintBlob = async () => {
    const token = useAuthStore.getState().accessToken
    const res = await fetch(`${API_URL}/api/clinical/${visitId}/print/${documentType}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    const blob = await res.blob()
    const url = URL.createObjectURL(blob)
    window.open(url, '_blank')
  }

  return <Button variant="outline" size="sm" onClick={handlePrintBlob}>{label}</Button>
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| iTextSharp (GPL) | QuestPDF (Community MIT) | 2024+ | Free for small businesses, fluent C# API, active development |
| System fonts for PDF | Font subsetting + embedding | QuestPDF 2024.3.0 | Smaller PDF file sizes, consistent rendering |
| Client-side PDF (jsPDF) | Server-side QuestPDF | Project decision | Reliable, consistent output, proper Vietnamese font support |
| Thong tu 52/2017 prescription format | Thong tu 26/2025 prescription format | July 1, 2025 | Updated required fields, electronic prescription mandate |

**Important regulatory update:**
- Circular 26/2025/TT-BYT (effective July 1, 2025) replaced the old Circular 52/2017/TT-BYT for prescription formatting
- Required fields: patient name, DOB, gender, CCCD/citizen ID, address, diagnosis, drug list (generic name preferred), dosage, quantity, route, doctor signature, prescription date
- For children under 72 months: age in months and weight are mandatory
- Prescription code (ma don thuoc) -- 14-character auto-generated ID
- Electronic prescriptions must be implemented by January 1, 2026 for non-hospital facilities
- "Loi dan" (doctor's advice/instructions) section is required

## Open Questions

1. **Clinic Settings Storage Location**
   - What we know: ClinicSettings needs a database table with logo URL, clinic name, address, phone, etc.
   - What's unclear: Whether to put this in a new "Settings" module, in Shared.Infrastructure, or in the existing Auth module (which already has SystemSettings)
   - Recommendation: Create a minimal ClinicSettings entity in Shared.Infrastructure (or a new Settings module) since all document-generating modules need it. The Auth module already has a PermissionModule.Settings enum value available.

2. **PDF Delivery Mechanism**
   - What we know: Frontend opens PDF in new browser tab
   - What's unclear: Whether to use direct file streaming (Results.File) or store PDFs in Azure Blob Storage with SAS URLs (like medical images)
   - Recommendation: Use direct streaming via `Results.File()` for on-demand PDF generation. No need to persist PDFs -- they can be regenerated from data at any time. This avoids blob storage costs and stale document concerns.

3. **Excel Import Library for Drug Catalog**
   - What we know: Admin needs Excel import for bulk drug additions
   - What's unclear: Whether to use ClosedXML (MIT) or EPPlus (Polyform for >$1M revenue, free under that)
   - Recommendation: Use ClosedXML (fully MIT license, no revenue restrictions). Add `<PackageVersion Include="ClosedXML" Version="0.104.*" />` to Directory.Packages.props.

4. **Copy-from-Previous-Visit for Optical Rx**
   - What we know: This is Claude's discretion
   - What's unclear: Whether it adds enough value for Phase 5 scope
   - Recommendation: Include as a secondary feature. Query the patient's most recent visit's optical prescription and offer a "Copy from previous visit" button. Simple query + form pre-fill, low complexity.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xunit 2.* + FluentAssertions 8.* + NSubstitute 5.* |
| Config file | Clinical.Unit.Tests.csproj (exists), Pharmacy.Unit.Tests.csproj (Wave 0) |
| Quick run command | `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~Prescription" -x` |
| Full suite command | `dotnet test backend/tests/Clinical.Unit.Tests && dotnet test backend/tests/Pharmacy.Unit.Tests` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| RX-01 | Add drug prescription from catalog + off-catalog | unit | `dotnet test --filter "AddDrugPrescription" -x` | Wave 0 |
| RX-02 | Catalog-linked items flagged, off-catalog flagged | unit | `dotnet test --filter "PrescriptionItem*CatalogFlag" -x` | Wave 0 |
| RX-03 | Write optical Rx with refraction params | unit | `dotnet test --filter "OpticalPrescription" -x` | Wave 0 |
| RX-04 | MOH-compliant prescription format | unit | `dotnet test --filter "DrugPrescriptionDocument" -x` | Wave 0 |
| RX-05 | Drug allergy warning system | unit | `dotnet test --filter "DrugAllergy" -x` | Wave 0 |
| PRT-01 | Print drug prescription PDF | unit | `dotnet test --filter "DrugPrescriptionDocument" -x` | Wave 0 |
| PRT-02 | Print optical Rx PDF | unit | `dotnet test --filter "OpticalPrescriptionDocument" -x` | Wave 0 |
| PRT-04 | Print referral letter | unit | `dotnet test --filter "ReferralLetterDocument" -x` | Wave 0 |
| PRT-05 | Print consent form | unit | `dotnet test --filter "ConsentFormDocument" -x` | Wave 0 |
| PRT-06 | Print pharmacy label | unit | `dotnet test --filter "PharmacyLabelDocument" -x` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test backend/tests/Clinical.Unit.Tests --filter "FullyQualifiedName~Prescription" -x`
- **Per wave merge:** `dotnet test backend/tests/Clinical.Unit.Tests && dotnet test backend/tests/Pharmacy.Unit.Tests`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `backend/tests/Pharmacy.Unit.Tests/` -- new test project for pharmacy handlers
- [ ] `backend/tests/Pharmacy.Unit.Tests/Pharmacy.Unit.Tests.csproj` -- test project setup
- [ ] `backend/tests/Clinical.Unit.Tests/Features/AddDrugPrescriptionHandlerTests.cs`
- [ ] `backend/tests/Clinical.Unit.Tests/Features/AddOpticalPrescriptionHandlerTests.cs`
- [ ] `backend/tests/Clinical.Unit.Tests/Features/CheckDrugAllergyHandlerTests.cs`
- [ ] `backend/tests/Clinical.Unit.Tests/Documents/DrugPrescriptionDocumentTests.cs`
- [ ] QuestPDF NuGet package added to solution

## Sources

### Primary (HIGH confidence)
- Existing codebase: Visit.cs entity, VisitSection.tsx, DiagnosisSection.tsx, AllergyAlert.tsx, AllergyCatalogSeeder.cs -- direct code inspection
- Existing codebase: ClinicalApiEndpoints.cs, clinical-api.ts -- established API patterns
- Existing codebase: PharmacyDbContext.cs, Pharmacy module scaffolding -- confirmed empty, ready for implementation
- Existing codebase: Refraction.cs entity -- OD/OS SPH/CYL/AXIS/ADD/PD fields available for optical Rx auto-population
- [QuestPDF Quick Start](https://www.questpdf.com/quick-start.html) -- license setup, basic API
- [QuestPDF Page Settings](https://www.questpdf.com/api-reference/page/settings.html) -- A4/A5/custom page sizes
- [QuestPDF Font Management](https://www.questpdf.com/api-reference/text/font-management.html) -- font registration, subsetting, fallback
- [QuestPDF NuGet 2026.2.2](https://www.nuget.org/packages/QuestPDF) -- latest stable version

### Secondary (MEDIUM confidence)
- [Vietnamese MOH Circular 26/2025/TT-BYT](https://luatvietnam.vn/y-te/thong-tu-26-2025-tt-byt-quy-dinh-ke-don-thuoc-hoa-duoc-sinh-pham-tai-co-so-kham-chua-benh-404246-d1.html) -- prescription format requirements effective July 1, 2025
- [Vietnamese prescription template fields](https://luatvietnam.vn/bieu-mau/mau-don-thuoc-571-91178-article.html) -- required fields: patient info, drug list, dosage, doctor signature, loi dan
- [Electronic prescription mandate](https://www.vietnam.vn/en/tu-ngay-1-10-tat-ca-cac-benh-vien-bat-buoc-phai-thuc-hien-viec-ke-don-thuoc-bang-hinh-thuc-dien-tu) -- deadline for non-hospital facilities: January 1, 2026

### Tertiary (LOW confidence)
- Exact 14-character prescription code format (ma don thuoc) -- specific format rules not fully extracted from Circular 26/2025. May need manual review of the PDF circular document for exact character pattern.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- QuestPDF locked decision, all other libraries already in codebase
- Architecture: HIGH -- all patterns directly mirror existing codebase patterns (Visit child entities, VisitSection, Combobox, AllergyAlert)
- Pitfalls: HIGH -- verified against existing code patterns and QuestPDF official docs
- MOH compliance: MEDIUM -- prescription required fields confirmed from Vietnamese legal sources, but exact 14-character code format needs validation
- Drug catalog seed data: MEDIUM -- specific drugs will be selected during implementation, confirmed ~50-100 range from CONTEXT.md

**Research date:** 2026-03-05
**Valid until:** 2026-04-05 (30 days -- stable domain, no rapidly changing dependencies)
