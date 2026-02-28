using Auth.Application.Interfaces;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Seeding;
using Auth.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

/// <summary>
/// DI registration for the Auth Infrastructure layer.
/// Registers repositories, Unit of Work, infrastructure services, and data seeder.
/// Note: AuthDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Infrastructure services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        // Data seeding
        services.AddHostedService<AuthDataSeeder>();

        return services;
    }
}
