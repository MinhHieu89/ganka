using Microsoft.EntityFrameworkCore;
using Scheduling.Domain.Entities;
using Shared.Infrastructure;

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

        modelBuilder.ApplySharedConventions();

        base.OnModelCreating(modelBuilder);
    }
}
