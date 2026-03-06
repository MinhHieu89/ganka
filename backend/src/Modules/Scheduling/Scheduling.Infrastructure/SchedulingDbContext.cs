using Microsoft.EntityFrameworkCore;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure;

/// <summary>
/// EF Core DbContext for the Scheduling module.
/// Uses schema-per-module isolation with the "scheduling" schema.
/// </summary>
public class SchedulingDbContext : DbContext
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<SelfBookingRequest> SelfBookingRequests => Set<SelfBookingRequest>();
    public DbSet<ClinicSchedule> ClinicSchedules => Set<ClinicSchedule>();
    public DbSet<AppointmentType> AppointmentTypes => Set<AppointmentType>();

    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("scheduling");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedulingDbContext).Assembly);

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
