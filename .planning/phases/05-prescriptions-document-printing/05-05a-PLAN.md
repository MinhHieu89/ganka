---
phase: 05-prescriptions-document-printing
plan: 05a
type: execute
wave: 2
depends_on: ["05-04"]
files_modified:
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DrugPrescriptionConfiguration.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/PrescriptionItemConfiguration.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/OpticalPrescriptionConfiguration.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs
autonomous: true
requirements:
  - RX-01
  - RX-02
  - RX-03
must_haves:
  truths:
    - "DrugPrescription, PrescriptionItem, and OpticalPrescription have proper EF Core configs"
    - "Backing field access configured on VisitConfiguration for new prescription collections"
  artifacts:
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DrugPrescriptionConfiguration.cs"
      provides: "EF config for DrugPrescription with Items collection"
      contains: "IEntityTypeConfiguration<DrugPrescription>"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/OpticalPrescriptionConfiguration.cs"
      provides: "EF config for OpticalPrescription with decimal precision"
      contains: "IEntityTypeConfiguration<OpticalPrescription>"
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs"
      provides: "Updated with backing field access for _drugPrescriptions, _opticalPrescriptions"
      contains: "PropertyAccessMode.Field"
  key_links:
    - from: "VisitConfiguration.cs"
      to: "DrugPrescription/OpticalPrescription"
      via: "PropertyAccessMode.Field on backing fields"
      pattern: "PropertyAccessMode.Field"
---

<objective>
Create EF Core configurations for prescription entities and update VisitConfiguration with backing field access.

Purpose: Sets up the database mapping for prescriptions, including backing field access on Visit (critical for aggregate pattern), decimal precision for refraction values, and nullable FK for catalog-link.

Output: 3 new EF configurations, updated VisitConfiguration
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
@backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs
@backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/RefractionConfiguration.cs

<interfaces>
From 05-04:
```csharp
// DrugPrescription: Entity with VisitId, Notes, PrescriptionCode, PrescribedAt, Items collection
// PrescriptionItem: Entity with DrugPrescriptionId, DrugCatalogItemId?, DrugName, GenericName, Strength, Form(int), Route(int), Dosage, DosageOverride, Quantity, Unit, Frequency, DurationDays, IsOffCatalog, HasAllergyWarning, SortOrder
// OpticalPrescription: Entity with VisitId, OD/OS Sph/Cyl/Axis/Add, Far/Near PD, Near OD/OS override, LensType, Notes, PrescribedAt
// Visit: _drugPrescriptions, _opticalPrescriptions backing fields added
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Create EF Core configurations for prescription entities and update VisitConfiguration</name>
  <files>
    backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DrugPrescriptionConfiguration.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/PrescriptionItemConfiguration.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/OpticalPrescriptionConfiguration.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/VisitConfiguration.cs
  </files>
  <action>
**DrugPrescriptionConfiguration.cs**:
- Table "DrugPrescriptions" in clinical schema
- VisitId: required FK to Visits table
- Notes: max length 1000
- PrescriptionCode: max length 20
- PrescribedAt: required
- Items collection: HasMany with cascade delete
- PropertyAccessMode.Field on Items navigation for backing field `_items`
- Follow VisitDiagnosisConfiguration pattern for Visit child entity

**PrescriptionItemConfiguration.cs**:
- Table "PrescriptionItems" in clinical schema
- DrugPrescriptionId: required FK
- DrugCatalogItemId: optional (nullable FK -- null = off-catalog)
- DrugName: required, max length 200
- GenericName: max length 200
- Strength: max length 50
- Unit: required, max length 50
- Dosage: max length 500
- DosageOverride: max length 500
- Frequency: max length 100
- Form, Route: int columns
- SortOrder: default 0

**OpticalPrescriptionConfiguration.cs**:
- Table "OpticalPrescriptions" in clinical schema
- VisitId: required FK to Visits table
- All decimal fields: precision(5,2) following RefractionConfiguration pattern
- LensType: int conversion
- Notes: max length 500
- PrescribedAt: required

**VisitConfiguration.cs** (EXISTING -- modify):
- Add PropertyAccessMode.Field configuration for the two new backing field collections (_drugPrescriptions, _opticalPrescriptions)
- Follow the same pattern used for _refractions, _diagnoses, _dryEyeAssessments, _amendments
- This is CRITICAL for the aggregate pattern -- without it, EF Core will fail to load prescription collections
  </action>
  <verify>
    <automated>dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj</automated>
  </verify>
  <done>All three EF configurations compile. VisitConfiguration updated with backing field access for prescriptions. Decimal precision set to (5,2).</done>
</task>

</tasks>

<verification>
- `dotnet build backend/src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj` passes
- VisitConfiguration has PropertyAccessMode.Field for _drugPrescriptions and _opticalPrescriptions
</verification>

<success_criteria>
EF Core persistence configured for all prescription entities. Visit aggregate properly wired with backing field access for new collections.
</success_criteria>

<output>
After completion, create `.planning/phases/05-prescriptions-document-printing/05-05a-SUMMARY.md`
</output>
