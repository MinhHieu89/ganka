using Microsoft.EntityFrameworkCore;
using Treatment.Domain.Entities;

namespace Treatment.Infrastructure;

/// <summary>
/// EF Core DbContext for the Treatment module.
/// Uses schema-per-module isolation with the "treatment" schema.
/// Applies all entity configurations via ApplyConfigurationsFromAssembly.
/// </summary>
public class TreatmentDbContext : DbContext
{
    // Protocol templates (TRT-01)
    public DbSet<TreatmentProtocol> TreatmentProtocols => Set<TreatmentProtocol>();

    // Patient treatment packages (TRT-02)
    public DbSet<TreatmentPackage> TreatmentPackages => Set<TreatmentPackage>();

    // Individual treatment sessions (TRT-03)
    public DbSet<TreatmentSession> TreatmentSessions => Set<TreatmentSession>();

    // Consumables used during sessions (TRT-11)
    public DbSet<SessionConsumable> SessionConsumables => Set<SessionConsumable>();

    // Protocol version snapshots for mid-course modifications (TRT-07)
    public DbSet<ProtocolVersion> ProtocolVersions => Set<ProtocolVersion>();

    // Cancellation requests for treatment packages (TRT-09)
    public DbSet<CancellationRequest> CancellationRequests => Set<CancellationRequest>();

    public TreatmentDbContext(DbContextOptions<TreatmentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("treatment");

        // Auto-discover all IEntityTypeConfiguration implementations in this assembly.
        // Configurations are added as entities are implemented in their respective plan files.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TreatmentDbContext).Assembly);

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
