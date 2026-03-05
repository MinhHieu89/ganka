---
phase: 05-prescriptions-document-printing
plan: 05b
type: execute
wave: 2
depends_on: ["05-04"]
files_modified:
  - backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs
  - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DrugPrescriptionDto.cs
autonomous: true
requirements:
  - RX-01
  - RX-02
  - RX-03
must_haves:
  truths:
    - "ClinicalDbContext has DbSets for DrugPrescription, PrescriptionItem, OpticalPrescription"
    - "Contract DTOs exist for prescription data transfer"
    - "VisitDetailDto includes prescription collections"
  artifacts:
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs"
      provides: "Updated with DrugPrescription, PrescriptionItem, OpticalPrescription DbSets"
      contains: "DbSet<DrugPrescription>"
    - path: "backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DrugPrescriptionDto.cs"
      provides: "Prescription contract DTOs"
      contains: "DrugPrescriptionDto"
  key_links:
    - from: "ClinicalDbContext.cs"
      to: "DrugPrescriptionConfiguration.cs"
      via: "ApplyConfigurationsFromAssembly"
      pattern: "DbSet<DrugPrescription>"
---

<objective>
Update ClinicalDbContext with prescription DbSets and create contract DTOs.

Purpose: Adds the DbSet registrations and creates the DTO records needed by the application layer handlers.

Output: Updated ClinicalDbContext, prescription DTOs
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/05-prescriptions-document-printing/05-CONTEXT.md
@.planning/phases/05-prescriptions-document-printing/05-04-SUMMARY.md

@backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs

<interfaces>
From 05-04:
```csharp
// DrugPrescription: Entity with VisitId, Notes, PrescriptionCode, PrescribedAt, Items collection
// PrescriptionItem: Entity with DrugPrescriptionId, DrugCatalogItemId?, DrugName, GenericName, Strength, Form(int), Route(int), Dosage, DosageOverride, Quantity, Unit, Frequency, DurationDays, IsOffCatalog, HasAllergyWarning, SortOrder
// OpticalPrescription: Entity with VisitId, OD/OS Sph/Cyl/Axis/Add, Far/Near PD, Near OD/OS override, LensType, Notes, PrescribedAt
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Update ClinicalDbContext and create contract DTOs</name>
  <files>
    backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs,
    backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DrugPrescriptionDto.cs
  </files>
  <action>
**ClinicalDbContext.cs** -- add DbSets:
```csharp
public DbSet<DrugPrescription> DrugPrescriptions => Set<DrugPrescription>();
public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
public DbSet<OpticalPrescription> OpticalPrescriptions => Set<OpticalPrescription>();
```
Keep all existing DbSets and OnModelCreating code intact.

**DrugPrescriptionDto.cs** (Clinical.Contracts/Dtos/) -- contains all prescription DTOs in one file:
```csharp
public sealed record DrugPrescriptionDto(
    Guid Id, Guid VisitId, string? Notes, string? PrescriptionCode,
    DateTime PrescribedAt, List<PrescriptionItemDto> Items);

public sealed record PrescriptionItemDto(
    Guid Id, Guid? DrugCatalogItemId, string DrugName, string? GenericName,
    string? Strength, int Form, int Route, string? Dosage, string? DosageOverride,
    int Quantity, string Unit, string? Frequency, int? DurationDays,
    bool IsOffCatalog, bool HasAllergyWarning, int SortOrder);

public sealed record OpticalPrescriptionDto(
    Guid Id, Guid VisitId,
    decimal? OdSph, decimal? OdCyl, int? OdAxis, decimal? OdAdd,
    decimal? OsSph, decimal? OsCyl, int? OsAxis, decimal? OsAdd,
    decimal? FarPd, decimal? NearPd,
    decimal? NearOdSph, decimal? NearOdCyl, int? NearOdAxis,
    decimal? NearOsSph, decimal? NearOsCyl, int? NearOsAxis,
    int LensType, string? Notes, DateTime PrescribedAt);
```

Also update **VisitDetailDto** to include prescription collections:
- Add `List<DrugPrescriptionDto> DrugPrescriptions` parameter
- Add `List<OpticalPrescriptionDto> OpticalPrescriptions` parameter
(This may require modifying VisitDetailDto.cs -- if so, count it as part of this file since it's in the same Contracts/Dtos directory)
  </action>
  <verify>
    <automated>dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj && dotnet build backend/src/Modules/Clinical/Clinical.Contracts/Clinical.Contracts.csproj</automated>
  </verify>
  <done>ClinicalDbContext has 3 new DbSets. DrugPrescriptionDto, PrescriptionItemDto, OpticalPrescriptionDto exist. VisitDetailDto includes prescription collections.</done>
</task>

</tasks>

<verification>
- `dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj` passes
- `dotnet build backend/src/Modules/Clinical/Clinical.Contracts/Clinical.Contracts.csproj` passes
- ClinicalDbContext has DrugPrescriptions, PrescriptionItems, OpticalPrescriptions DbSets
- VisitDetailDto includes prescription data
</verification>

<success_criteria>
ClinicalDbContext updated with prescription DbSets. DTOs ready for application layer handlers.
</success_criteria>

<output>
After completion, create `.planning/phases/05-prescriptions-document-printing/05-05b-SUMMARY.md`
</output>
