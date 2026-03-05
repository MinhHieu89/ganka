---
phase: 04-dry-eye-template-medical-imaging
plan: 01a
type: execute
wave: 1
depends_on: []
files_modified:
  - backend/src/Modules/Clinical/Clinical.Domain/Entities/DryEyeAssessment.cs
  - backend/src/Modules/Clinical/Clinical.Domain/Entities/MedicalImage.cs
  - backend/src/Modules/Clinical/Clinical.Domain/Entities/OsdiSubmission.cs
  - backend/src/Modules/Clinical/Clinical.Domain/Enums/ImageType.cs
  - backend/src/Modules/Clinical/Clinical.Domain/Enums/EyeTag.cs
  - backend/src/Modules/Clinical/Clinical.Domain/Enums/OsdiSeverity.cs
  - backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs
  - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeAssessmentDto.cs
  - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiHistoryDto.cs
  - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/MedicalImageDto.cs
  - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeComparisonDto.cs
  - backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiQuestionnaireDto.cs
autonomous: true
requirements: [DRY-01, DRY-02, IMG-01, IMG-02]

must_haves:
  truths:
    - "Dry eye assessment data can be modeled per visit with per-eye metrics (TBUT, Schirmer, Meibomian grading, Tear meniscus, Staining) and patient-level OSDI score"
    - "Medical images can be stored independently of visit sign-off (append-only pattern)"
    - "OSDI questionnaire tokens can be generated for patient self-fill without authentication"
  artifacts:
    - path: "backend/src/Modules/Clinical/Clinical.Domain/Entities/DryEyeAssessment.cs"
      provides: "Dry eye assessment entity with per-eye metrics and OSDI score"
      min_lines: 40
    - path: "backend/src/Modules/Clinical/Clinical.Domain/Entities/MedicalImage.cs"
      provides: "Medical image metadata entity (NOT a Visit child via aggregate)"
      min_lines: 30
    - path: "backend/src/Modules/Clinical/Clinical.Domain/Entities/OsdiSubmission.cs"
      provides: "OSDI questionnaire submission with public token"
      min_lines: 30
    - path: "backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeAssessmentDto.cs"
      provides: "All contract DTOs for dry eye, OSDI, and imaging features"
      min_lines: 20
  key_links:
    - from: "DryEyeAssessment"
      to: "Visit"
      via: "VisitId foreign key"
      pattern: "VisitId.*Guid"
    - from: "MedicalImage"
      to: "Visit"
      via: "VisitId foreign key (but NOT through Visit aggregate)"
      pattern: "VisitId.*Guid"
---

<objective>
Create domain entities, enums, and contracts DTOs for Dry Eye assessment, Medical Imaging, and OSDI submission features. Modify Visit entity to add DryEyeAssessment as a child.

Purpose: Define the domain model and data contracts that all subsequent plans (EF Core configs, handlers, frontend) will build upon.
Output: 3 new domain entities (DryEyeAssessment, MedicalImage, OsdiSubmission), 3 new enums (ImageType, EyeTag, OsdiSeverity), 13 contract DTOs, and Visit entity modification.
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/phases/04-dry-eye-template-medical-imaging/04-CONTEXT.md
@.planning/phases/04-dry-eye-template-medical-imaging/04-RESEARCH.md

<interfaces>
<!-- Key types and contracts the executor needs from existing codebase -->

From backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs:
```csharp
public class Visit : AggregateRoot, IAuditable
{
    public Guid PatientId { get; private set; }
    public VisitStatus Status { get; private set; }
    public DateTime VisitDate { get; private set; }
    // ... backing fields for Refractions, Diagnoses, Amendments
    private readonly List<Refraction> _refractions = [];
    // EnsureEditable() guard throws if Status == Signed
    // NOTE: DryEyeAssessment IS a Visit child (subject to EnsureEditable)
    // NOTE: MedicalImage is NOT a Visit child (append-only even after sign-off)
}
```

From backend/src/Modules/Clinical/Clinical.Domain/Entities/Refraction.cs (pattern to follow):
```csharp
public class Refraction : Entity
{
    public Guid VisitId { get; private set; }
    public RefractionType Type { get; private set; }
    // Per-eye flat columns: OdSph, OsSph, OdCyl, OsCyl, etc.
    public decimal? OdSph { get; private set; }
    // ...
    private Refraction() { }
    public static Refraction Create(Guid visitId, RefractionType type) => new() { ... };
    public void Update(/* all fields */) { /* set all + SetUpdatedAt() */ }
}
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Domain entities, enums, and contracts DTOs</name>
  <files>
    backend/src/Modules/Clinical/Clinical.Domain/Entities/DryEyeAssessment.cs,
    backend/src/Modules/Clinical/Clinical.Domain/Entities/MedicalImage.cs,
    backend/src/Modules/Clinical/Clinical.Domain/Entities/OsdiSubmission.cs,
    backend/src/Modules/Clinical/Clinical.Domain/Enums/ImageType.cs,
    backend/src/Modules/Clinical/Clinical.Domain/Enums/EyeTag.cs,
    backend/src/Modules/Clinical/Clinical.Domain/Enums/OsdiSeverity.cs,
    backend/src/Modules/Clinical/Clinical.Domain/Entities/Visit.cs,
    backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeAssessmentDto.cs,
    backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiHistoryDto.cs,
    backend/src/Modules/Clinical/Clinical.Contracts/Dtos/MedicalImageDto.cs,
    backend/src/Modules/Clinical/Clinical.Contracts/Dtos/DryEyeComparisonDto.cs,
    backend/src/Modules/Clinical/Clinical.Contracts/Dtos/OsdiQuestionnaireDto.cs
  </files>
  <action>
    **Create 3 new enums:**
    1. `ImageType` enum: Fluorescein=0, Meibography=1, OCT=2, SpecularMicroscopy=3, Topography=4, Video=5
    2. `EyeTag` enum: OD=0, OS=1, OU=2
    3. `OsdiSeverity` enum: Normal=0, Mild=1, Moderate=2, Severe=3

    **Create DryEyeAssessment entity** (follows Refraction pattern exactly):
    - Extends Entity, has VisitId (Guid)
    - Per-eye flat columns: OdTbut/OsTbut (decimal?), OdSchirmer/OsSchirmer (decimal?), OdMeibomianGrading/OsMeibomianGrading (int?, 0-3 Arita), OdTearMeniscus/OsTearMeniscus (decimal?), OdStaining/OsStaining (int?, Oxford 0-5)
    - Single OsdiScore (decimal?, 0-100) -- NOT per-eye (OSDI is patient-reported, not per-eye)
    - OsdiSeverity (OsdiSeverity?) -- calculated from OsdiScore
    - Private constructor, static Create(Guid visitId) factory
    - Update() method that sets all fields + calls SetUpdatedAt()
    - SetOsdiScore(decimal score, OsdiSeverity severity) method

    **Create MedicalImage entity** (new pattern -- NOT a Visit child via aggregate):
    - Extends Entity, has VisitId (Guid), UploadedById (Guid)
    - Type (ImageType), EyeTag (EyeTag?, optional)
    - OriginalFileName (string), BlobName (string), ContentType (string), FileSize (long)
    - Description (string?, optional caption)
    - Private constructor, static Create() factory with all required fields
    - IMPORTANT: This entity is NOT added through Visit aggregate. No Visit.AddImage() method. MedicalImage bypasses EnsureEditable because images are append-only even after sign-off.

    **Create OsdiSubmission entity:**
    - Extends Entity, has VisitId (Guid)
    - SubmittedBy (string) -- "patient" or userId
    - AnswersJson (string, default "[]") -- JSON array of 12 answers (each 0-4 or null)
    - QuestionsAnswered (int), Score (decimal, calculated)
    - Severity (OsdiSeverity, calculated)
    - PublicToken (string?, for patient self-fill)
    - TokenExpiresAt (DateTime?, 24-hour expiry)
    - Private constructor, static Create() factory, static CreateWithToken() factory

    **Modify Visit entity:**
    - Add `_dryEyeAssessments` backing field: `private readonly List<DryEyeAssessment> _dryEyeAssessments = [];`
    - Add read-only collection: `public IReadOnlyCollection<DryEyeAssessment> DryEyeAssessments => _dryEyeAssessments.AsReadOnly();`
    - Add `AddDryEyeAssessment(DryEyeAssessment assessment)` method that calls EnsureEditable(), adds to list, calls SetUpdatedAt()
    - Do NOT add MedicalImage or OsdiSubmission as Visit children (they bypass the aggregate)

    **Create contract DTOs (all as sealed records):**
    1. `DryEyeAssessmentDto` -- mirrors all entity fields for read
    2. `UpdateDryEyeAssessmentCommand` -- all per-eye fields for write
    3. `OsdiHistoryDto` -- visitId, visitDate, osdiScore, severity (for trend chart)
    4. `OsdiHistoryResponse` -- list of OsdiHistoryDto items
    5. `MedicalImageDto` -- id, visitId, type, eyeTag, fileName, url (SAS URL), contentType, fileSize, description, createdAt
    6. `DryEyeComparisonDto` -- two sets of dry eye data (visit1 and visit2) with metadata
    7. `OsdiQuestionnaireDto` -- 12 questions with Vietnamese + English text, current answers if any, visit date
    8. `SubmitOsdiCommand` -- token, answers array (12 items)
    9. `GenerateOsdiLinkCommand` -- visitId
    10. `OsdiLinkResponse` -- token, url, expiresAt
    11. `UploadMedicalImageCommand` -- visitId, stream, fileName, contentType, fileSize, imageType, eyeTag (all as record)
    12. `GetImageComparisonQuery` -- patientId, visitId1, visitId2, imageType
    13. `ImageComparisonResponse` -- visit1Images, visit2Images (both MedicalImageDto[])
  </action>
  <verify>
    <automated>dotnet build backend/src/Modules/Clinical/Clinical.Domain/ --no-restore && dotnet build backend/src/Modules/Clinical/Clinical.Contracts/ --no-restore</automated>
  </verify>
  <done>All 3 entities, 3 enums, and all contract DTOs compile successfully. DryEyeAssessment follows Refraction pattern with per-eye flat columns. MedicalImage is independent from Visit aggregate. Visit entity has _dryEyeAssessments backing field with AddDryEyeAssessment() method.</done>
</task>

</tasks>

<verification>
- `dotnet build backend/src/Modules/Clinical/Clinical.Domain/` succeeds with 0 errors
- `dotnet build backend/src/Modules/Clinical/Clinical.Contracts/` succeeds with 0 errors
- Visit entity has _dryEyeAssessments backing field and AddDryEyeAssessment method
- MedicalImage is NOT a Visit child (no navigation from Visit to MedicalImage, no AddImage method on Visit)
</verification>

<success_criteria>
- All domain entities follow established patterns (Entity base, private constructor, factory methods)
- DryEyeAssessment uses per-eye flat columns (OdTbut/OsTbut) consistent with Refraction
- MedicalImage is independent from Visit aggregate (append-only after sign-off)
- OSDI score is patient-level, NOT per-eye
- All contract DTOs defined as sealed records
</success_criteria>

<output>
After completion, create `.planning/phases/04-dry-eye-template-medical-imaging/04-01a-SUMMARY.md`
</output>
