using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(ss => ss.Id);

        builder.Property(ss => ss.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(ss => ss.Key)
            .IsUnique();

        builder.Property(ss => ss.Value)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(ss => ss.Description)
            .HasMaxLength(500);
    }
}
