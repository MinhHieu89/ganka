using Microsoft.EntityFrameworkCore;

namespace Patient.Infrastructure;

/// <summary>
/// EF Core DbContext for the Patient module.
/// Uses schema-per-module isolation with the "patient" schema.
/// Entity configurations and DbSets will be added as the module is implemented.
/// </summary>
public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("patient");

        // Entity configurations will be added as this module is implemented
        // in its respective phase plan.

        base.OnModelCreating(modelBuilder);
    }
}
