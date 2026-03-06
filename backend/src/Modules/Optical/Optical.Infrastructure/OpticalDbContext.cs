using Microsoft.EntityFrameworkCore;

namespace Optical.Infrastructure;

/// <summary>
/// EF Core DbContext for the Optical module.
/// Uses schema-per-module isolation with the "optical" schema.
/// Entity configurations and DbSets will be added as the module is implemented.
/// </summary>
public class OpticalDbContext : DbContext
{
    public OpticalDbContext(DbContextOptions<OpticalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("optical");

        // Entity configurations will be added as this module is implemented
        // in its respective phase plan.

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
