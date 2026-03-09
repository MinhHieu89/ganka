using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace Auth.Infrastructure;

/// <summary>
/// EF Core DbContext for the Auth module.
/// Uses schema-per-module isolation with the "auth" schema.
/// Includes named query filters for BranchId tenant isolation and soft delete.
/// </summary>
public class AuthDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

        modelBuilder.ApplySharedConventions();

        // Global query filter: soft delete on User
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

        // Global query filter: soft delete on Role
        modelBuilder.Entity<Role>().HasQueryFilter(r => !r.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}
