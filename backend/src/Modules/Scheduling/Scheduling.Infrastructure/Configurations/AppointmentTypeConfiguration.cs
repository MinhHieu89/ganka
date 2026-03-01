using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Configurations;

public class AppointmentTypeConfiguration : IEntityTypeConfiguration<AppointmentType>
{
    public void Configure(EntityTypeBuilder<AppointmentType> builder)
    {
        builder.ToTable("AppointmentTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.NameVi)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.DefaultDurationMinutes).IsRequired();

        builder.Property(t => t.Color)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);
    }
}
