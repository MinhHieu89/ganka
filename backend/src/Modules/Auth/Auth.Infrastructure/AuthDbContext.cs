using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure;

/// <summary>
/// EF Core DbContext for the Auth module.
/// Uses schema-per-module isolation with the "auth" schema.
/// Entity configurations will be added in plan 01-03 (Auth domain entities).
/// </summary>
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        // Auth entity configurations (Users, Roles, Permissions, RefreshTokens)
        // will be added in plan 01-03.

        base.OnModelCreating(modelBuilder);
    }
}
