using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for the ShiftTemplate entity.
/// Stores pre-configured shift templates with default time ranges.
/// </summary>
public class ShiftTemplateConfiguration : IEntityTypeConfiguration<ShiftTemplate>
{
    public void Configure(EntityTypeBuilder<ShiftTemplate> builder)
    {
        builder.ToTable("ShiftTemplates");

        builder.HasKey(st => st.Id);

        builder.Property(st => st.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(st => st.NameVi)
            .HasMaxLength(100);

        // TimeOnly properties for DefaultStartTime and DefaultEndTime
        // EF Core 8+ has built-in TimeOnly support for SQL Server

        // BranchId value object conversion
        builder.Property(st => st.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Performance index on BranchId
        builder.HasIndex(st => st.BranchId);
    }
}
