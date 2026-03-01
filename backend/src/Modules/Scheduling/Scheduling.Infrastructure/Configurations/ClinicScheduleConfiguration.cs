using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Configurations;

public class ClinicScheduleConfiguration : IEntityTypeConfiguration<ClinicSchedule>
{
    public void Configure(EntityTypeBuilder<ClinicSchedule> builder)
    {
        builder.ToTable("ClinicSchedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.IsOpen).IsRequired();

        builder.Property(s => s.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Unique index: one schedule per day per branch
        builder.HasIndex(s => new { s.DayOfWeek, s.BranchId })
            .IsUnique();
    }
}
