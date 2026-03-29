using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

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
    public DbSet<ImagingRequest> ImagingRequests => Set<ImagingRequest>();
    public DbSet<ImagingService> ImagingServices => Set<ImagingService>();
    public DbSet<StageSkip> StageSkips => Set<StageSkip>();
    public DbSet<VisitPayment> VisitPayments => Set<VisitPayment>();
    public DbSet<PharmacyDispensing> PharmacyDispensings => Set<PharmacyDispensing>();
    public DbSet<DispensingLineItem> DispensingLineItems => Set<DispensingLineItem>();
    public DbSet<OpticalOrder> OpticalOrders => Set<OpticalOrder>();
    public DbSet<HandoffChecklist> HandoffChecklists => Set<HandoffChecklist>();
    public DbSet<TechnicianOrder> TechnicianOrders => Set<TechnicianOrder>();

    public ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("clinical");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicalDbContext).Assembly);

        modelBuilder.ApplySharedConventions();

        base.OnModelCreating(modelBuilder);
    }
}
