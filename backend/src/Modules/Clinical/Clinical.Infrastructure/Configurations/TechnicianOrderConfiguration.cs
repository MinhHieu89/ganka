using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class TechnicianOrderConfiguration : IEntityTypeConfiguration<TechnicianOrder>
{
    public void Configure(EntityTypeBuilder<TechnicianOrder> builder)
    {
        builder.ToTable("TechnicianOrders", "clinical");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.VisitId).IsRequired();

        builder.Property(o => o.OrderType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.TechnicianName)
            .HasMaxLength(200);

        builder.Property(o => o.RedFlagReason)
            .HasMaxLength(500);

        builder.Property(o => o.OrderedByDoctorName)
            .HasMaxLength(200);

        builder.Property(o => o.Instructions)
            .HasMaxLength(1000);

        builder.Property(o => o.OrderedAt).IsRequired();

        // Unique filtered index: only one PreExam order per visit
        builder.HasIndex(o => new { o.VisitId, o.OrderType })
            .HasFilter("[OrderType] = 'PreExam'")
            .IsUnique();

        // Performance index for technician dashboard queries
        builder.HasIndex(o => o.TechnicianId);
    }
}
