---
phase: 04-dry-eye-template-medical-imaging
plan: 01b
type: execute
wave: 2
depends_on: [04-01a]
files_modified:
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DryEyeAssessmentConfiguration.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/MedicalImageConfiguration.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/OsdiSubmissionConfiguration.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/MedicalImageRepository.cs
  - backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/OsdiSubmissionRepository.cs
  - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs
  - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IMedicalImageRepository.cs
  - backend/src/Modules/Clinical/Clinical.Application/Interfaces/IOsdiSubmissionRepository.cs
autonomous: true
requirements: [DRY-01, DRY-02, IMG-01]

must_haves:
  truths:
    - "Dry eye assessment data can be saved and retrieved per visit"
    - "Medical images can be stored independently of visit sign-off"
    - "OSDI questionnaire tokens can be generated and validated"
    - "OSDI submissions can be created, queried by token, and queried by visit"
  artifacts:
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DryEyeAssessmentConfiguration.cs"
      provides: "EF Core config for DryEyeAssessment table with proper schema and indexes"
      min_lines: 20
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/MedicalImageConfiguration.cs"
      provides: "EF Core config for MedicalImages table with composite index on (VisitId, Type)"
      min_lines: 20
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/OsdiSubmissionConfiguration.cs"
      provides: "EF Core config for OsdiSubmissions with unique filtered index on PublicToken"
      min_lines: 15
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs"
      provides: "Updated DbContext with DryEyeAssessments, MedicalImages, OsdiSubmissions DbSets"
      contains: "DbSet<DryEyeAssessment>"
    - path: "backend/src/Modules/Clinical/Clinical.Application/Interfaces/IMedicalImageRepository.cs"
      provides: "Repository interface for medical image CRUD"
      min_lines: 10
    - path: "backend/src/Modules/Clinical/Clinical.Application/Interfaces/IOsdiSubmissionRepository.cs"
      provides: "Repository interface for OSDI submission CRUD by token and visit"
      min_lines: 10
    - path: "backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/OsdiSubmissionRepository.cs"
      provides: "Repository implementation for OSDI submission queries"
      min_lines: 20
  key_links:
    - from: "VisitRepository"
      to: "DryEyeAssessment"
      via: "Include in GetByIdWithDetailsAsync"
      pattern: "Include.*DryEyeAssessments"
    - from: "MedicalImageRepository"
      to: "ClinicalDbContext"
      via: "DbSet<MedicalImage> queries"
      pattern: "DbSet<MedicalImage>"
    - from: "OsdiSubmissionRepository"
      to: "ClinicalDbContext"
      via: "DbSet<OsdiSubmission> queries"
      pattern: "DbSet<OsdiSubmission>"
---

<objective>
Create EF Core configurations, repository interfaces and implementations, and database migration for all Phase 4 entities.

Purpose: Wire the domain entities (created in Plan 01a) to the database with proper schema, indexes, constraints, and data access patterns.
Output: 3 EF Core configurations, 3 repository interfaces, 3 repository implementations, updated DbContext, and a database migration.
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/04-dry-eye-template-medical-imaging/04-CONTEXT.md
@.planning/phases/04-dry-eye-template-medical-imaging/04-RESEARCH.md
@.planning/phases/04-dry-eye-template-medical-imaging/04-01a-SUMMARY.md

<interfaces>
<!-- From Plan 01a output -->

From Clinical.Domain/Entities/DryEyeAssessment.cs (created in Plan 01a):
```csharp
public class DryEyeAssessment : Entity
{
    public Guid VisitId { get; private set; }
    public decimal? OdTbut, OsTbut, OdSchirmer, OsSchirmer;
    public int? OdMeibomianGrading, OsMeibomianGrading;
    public decimal? OdTearMeniscus, OsTearMeniscus;
    public int? OdStaining, OsStaining;
    public decimal? OsdiScore;
    public OsdiSeverity? OsdiSeverity;
}
```

From Clinical.Domain/Entities/MedicalImage.cs (created in Plan 01a):
```csharp
public class MedicalImage : Entity
{
    public Guid VisitId { get; private set; }
    public Guid UploadedById { get; private set; }
    public ImageType Type { get; private set; }
    public EyeTag? EyeTag { get; private set; }
    public string OriginalFileName { get; private set; }
    public string BlobName { get; private set; }
    public string ContentType { get; private set; }
    public long FileSize { get; private set; }
    public string? Description { get; private set; }
}
```

From Clinical.Domain/Entities/OsdiSubmission.cs (created in Plan 01a):
```csharp
public class OsdiSubmission : Entity
{
    public Guid VisitId { get; private set; }
    public string SubmittedBy { get; private set; }
    public string AnswersJson { get; private set; }
    public int QuestionsAnswered { get; private set; }
    public decimal Score { get; private set; }
    public OsdiSeverity Severity { get; private set; }
    public string? PublicToken { get; private set; }
    public DateTime? TokenExpiresAt { get; private set; }
}
```

From Clinical.Infrastructure/ClinicalDbContext.cs:
```csharp
// Has DbSets for Visit, Refraction, VisitDiagnosis, VisitAmendment, DoctorIcd10Favorite
// Uses "clinical" schema
// Assembly-based configuration scanning via ApplyConfigurationsFromAssembly
```

From Clinical.Application/Interfaces/IVisitRepository.cs:
```csharp
public interface IVisitRepository
{
    Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Visit?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<Visit>> GetActiveVisitsAsync(CancellationToken ct = default);
    Task AddAsync(Visit visit, CancellationToken ct = default);
    Task<bool> HasActiveVisitForPatientAsync(Guid patientId, CancellationToken ct = default);
    void AddRefraction(Refraction refraction);
    void AddDiagnosis(VisitDiagnosis diagnosis);
    void AddAmendment(VisitAmendment amendment);
}
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: EF Core configurations, repositories, and migration</name>
  <files>
    backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/DryEyeAssessmentConfiguration.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/MedicalImageConfiguration.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Configurations/OsdiSubmissionConfiguration.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/ClinicalDbContext.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/VisitRepository.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/MedicalImageRepository.cs,
    backend/src/Modules/Clinical/Clinical.Infrastructure/Repositories/OsdiSubmissionRepository.cs,
    backend/src/Modules/Clinical/Clinical.Application/Interfaces/IVisitRepository.cs,
    backend/src/Modules/Clinical/Clinical.Application/Interfaces/IMedicalImageRepository.cs,
    backend/src/Modules/Clinical/Clinical.Application/Interfaces/IOsdiSubmissionRepository.cs
  </files>
  <action>
    **Create DryEyeAssessmentConfiguration:**
    - Table "DryEyeAssessments" in "clinical" schema
    - Required VisitId FK with index
    - All decimal fields use precision(5,2) (consistent with Refraction)
    - MeibomianGrading and Staining are int? (no precision needed)
    - OsdiScore uses precision(7,2) (range 0.00-100.00)
    - OsdiSeverity stored as int
    - PropertyAccessMode.Field on Visit._dryEyeAssessments backing field

    **Create MedicalImageConfiguration:**
    - Table "MedicalImages" in "clinical" schema
    - Required VisitId FK with index
    - Required UploadedById FK (no navigation, just FK)
    - ImageType stored as int, required
    - EyeTag stored as int, nullable
    - OriginalFileName max 500, required
    - BlobName max 1000, required
    - ContentType max 100, required
    - Description max 500, nullable
    - NO navigation property from Visit to MedicalImage (kept separate from aggregate)
    - Index on (VisitId, Type) for same-type image queries

    **Create OsdiSubmissionConfiguration:**
    - Table "OsdiSubmissions" in "clinical" schema
    - Required VisitId FK with index
    - SubmittedBy max 100, required
    - AnswersJson max 500
    - PublicToken max 100, nullable, unique index with filter (IS NOT NULL)
    - TokenExpiresAt nullable

    **Update ClinicalDbContext:**
    - Add DbSet<DryEyeAssessment>
    - Add DbSet<MedicalImage>
    - Add DbSet<OsdiSubmission>

    **Update VisitConfiguration:**
    - Add HasMany<DryEyeAssessment> with PropertyAccessMode.Field on "_dryEyeAssessments" backing field (same pattern as _refractions)

    **Update IVisitRepository:**
    - Add `void AddDryEyeAssessment(DryEyeAssessment assessment)` (same pattern as AddRefraction)
    - Add `Task<List<DryEyeAssessment>> GetDryEyeAssessmentsByPatientAsync(Guid patientId, CancellationToken ct)` for trend chart
    - Add `Task<DryEyeAssessment?> GetDryEyeAssessmentByVisitAsync(Guid visitId, CancellationToken ct)` for single visit lookup

    **Update VisitRepository:**
    - Implement the new IVisitRepository methods
    - Extend GetByIdWithDetailsAsync to .Include(v => v.DryEyeAssessments)
    - GetDryEyeAssessmentsByPatientAsync: query DryEyeAssessments where Visit.PatientId matches, Include Visit for VisitDate, order by VisitDate

    **Create IMedicalImageRepository interface:**
    - Task<MedicalImage?> GetByIdAsync(Guid id, CancellationToken ct)
    - Task<List<MedicalImage>> GetByVisitIdAsync(Guid visitId, CancellationToken ct)
    - Task<List<MedicalImage>> GetByVisitIdAndTypeAsync(Guid visitId, ImageType type, CancellationToken ct)
    - Task AddAsync(MedicalImage image, CancellationToken ct)
    - void Delete(MedicalImage image)

    **Create MedicalImageRepository:**
    - Implement all IMedicalImageRepository methods
    - GetByVisitIdAsync ordered by CreatedAt descending

    **Create IOsdiSubmissionRepository interface:**
    - Task AddAsync(OsdiSubmission submission, CancellationToken ct)
    - Task<OsdiSubmission?> GetByTokenAsync(string token, CancellationToken ct)
    - Task<OsdiSubmission?> GetByVisitIdAsync(Guid visitId, CancellationToken ct)
    - Task<List<OsdiSubmission>> GetByVisitIdsAsync(IEnumerable<Guid> visitIds, CancellationToken ct)

    **Create OsdiSubmissionRepository:**
    - Implement all IOsdiSubmissionRepository methods using ClinicalDbContext
    - GetByTokenAsync: query where PublicToken == token, SingleOrDefaultAsync
    - GetByVisitIdAsync: query where VisitId == visitId, order by CreatedAt descending, FirstOrDefaultAsync (latest submission)
    - GetByVisitIdsAsync: query where VisitId in visitIds, for batch loading

    **Update Clinical.Infrastructure IoC.cs:**
    - Register IMedicalImageRepository -> MedicalImageRepository as scoped
    - Register IOsdiSubmissionRepository -> OsdiSubmissionRepository as scoped

    **Create and run migration:**
    - `dotnet ef migrations add AddDryEyeAndImaging --project backend/src/Modules/Clinical/Clinical.Infrastructure --startup-project backend/src/Bootstrapper`
    - `dotnet ef database update --project backend/src/Modules/Clinical/Clinical.Infrastructure --startup-project backend/src/Bootstrapper`
  </action>
  <verify>
    <automated>dotnet build backend/ --no-restore && dotnet ef migrations list --project backend/src/Modules/Clinical/Clinical.Infrastructure --startup-project backend/src/Bootstrapper 2>&1 | tail -5</automated>
  </verify>
  <done>All 3 EF Core configurations created with proper schema, indexes, and constraints. ClinicalDbContext has 3 new DbSets. VisitRepository extended with DryEyeAssessment queries and Include. MedicalImageRepository created and registered in IoC. IOsdiSubmissionRepository interface and OsdiSubmissionRepository implementation created with AddAsync, GetByTokenAsync, GetByVisitIdAsync, GetByVisitIdsAsync methods and registered in IoC. Migration created and applied successfully. Backend builds with 0 errors.</done>
</task>

</tasks>

<verification>
- `dotnet build backend/` succeeds with 0 errors
- Migration exists and lists in `dotnet ef migrations list`
- ClinicalDbContext has DbSet<DryEyeAssessment>, DbSet<MedicalImage>, DbSet<OsdiSubmission>
- VisitRepository includes DryEyeAssessments in GetByIdWithDetailsAsync
- MedicalImageRepository registered in IoC
- IOsdiSubmissionRepository registered in IoC with GetByTokenAsync, GetByVisitIdAsync, AddAsync
</verification>

<success_criteria>
- All EF configs in "clinical" schema with proper indexes
- Migration applies cleanly
- Repository interfaces and implementations follow established patterns
- MedicalImage has no navigation property from Visit (kept separate from aggregate)
- IOsdiSubmissionRepository provides token-based lookup for Plan 02 handlers (GenerateOsdiLink, GetOsdiByToken, SubmitOsdiQuestionnaire)
</success_criteria>

<output>
After completion, create `.planning/phases/04-dry-eye-template-medical-imaging/04-01b-SUMMARY.md`
</output>
