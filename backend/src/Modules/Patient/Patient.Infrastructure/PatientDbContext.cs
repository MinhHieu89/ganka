using Microsoft.EntityFrameworkCore;
using Patient.Domain.Entities;

namespace Patient.Infrastructure;

/// <summary>
/// EF Core DbContext for the Patient module.
/// Uses schema-per-module isolation with the "patient" schema.
/// Includes entity configurations and global query filters.
/// </summary>
public class PatientDbContext : DbContext
{
    public DbSet<Domain.Entities.Patient> Patients => Set<Domain.Entities.Patient>();
    public DbSet<Allergy> Allergies => Set<Allergy>();
    public DbSet<AllergyCatalogItem> AllergyCatalogItems => Set<AllergyCatalogItem>();

    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("patient");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PatientDbContext).Assembly);

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

        // Global query filter: soft delete on Patient
        modelBuilder.Entity<Domain.Entities.Patient>().HasQueryFilter(p => !p.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}
