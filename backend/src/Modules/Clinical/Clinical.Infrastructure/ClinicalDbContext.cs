using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinical.Infrastructure;

/// <summary>
/// EF Core DbContext for the Clinical module.
/// Uses schema-per-module isolation with the "clinical" schema.
/// </summary>
public class ClinicalDbContext : DbContext
{
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<VisitAmendment> VisitAmendments => Set<VisitAmendment>();
    public DbSet<Refraction> Refractions => Set<Refraction>();
    public DbSet<VisitDiagnosis> VisitDiagnoses => Set<VisitDiagnosis>();
    public DbSet<DoctorIcd10Favorite> DoctorIcd10Favorites => Set<DoctorIcd10Favorite>();
    public DbSet<DryEyeAssessment> DryEyeAssessments => Set<DryEyeAssessment>();
    public DbSet<MedicalImage> MedicalImages => Set<MedicalImage>();
    public DbSet<OsdiSubmission> OsdiSubmissions => Set<OsdiSubmission>();
    public DbSet<DrugPrescription> DrugPrescriptions => Set<DrugPrescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<OpticalPrescription> OpticalPrescriptions => Set<OpticalPrescription>();

    public ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("clinical");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicalDbContext).Assembly);

        // All domain entities generate their own Guid IDs in the constructor (client-side).
        // Override EF Core's default ValueGeneratedOnAdd to prevent it from treating
        // new entities with set IDs as existing (Modified) instead of new (Added).
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty is not null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
