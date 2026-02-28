using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Unique per BranchId (composite unique index)
        builder.HasIndex(r => new { r.Name, r.BranchId })
            .IsUnique();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsSystem)
            .HasDefaultValue(false);

        builder.Property(r => r.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // RolePermission relationship
        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserRole relationship
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
