using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for ShiftTemplate entity.
/// </summary>
public sealed class ShiftTemplateConfiguration : IEntityTypeConfiguration<ShiftTemplate>
{
    public void Configure(EntityTypeBuilder<ShiftTemplate> builder)
    {
        builder.ToTable("ShiftTemplates");

        builder.Property(st => st.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(st => st.NameVi).HasMaxLength(100);

        builder.Property(st => st.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        builder.HasIndex(st => st.BranchId);
    }
}
