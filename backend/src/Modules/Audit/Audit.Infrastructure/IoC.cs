using Audit.Application.Interfaces;
using Audit.Infrastructure.Interceptors;
using Audit.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Infrastructure;

/// <summary>
/// DI registration for the Audit Infrastructure layer.
/// Registers AuditInterceptor, AuditDbContext, IAuditReadRepository, and Icd10Seeder.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddAuditInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        // AuditInterceptor (shared singleton used by other module DbContexts)
        services.AddSingleton<AuditInterceptor>();

        // AuditDbContext does NOT get the AuditInterceptor (prevents infinite recursion)
        services.AddDbContext<AuditDbContext>(options =>
            options.UseSqlServer(connectionString),
            optionsLifetime: ServiceLifetime.Singleton);

        // IAuditReadRepository for Application layer query access
        services.AddScoped<IAuditReadRepository>(sp => sp.GetRequiredService<AuditDbContext>());

        // ICD-10 ophthalmology code seeder (runs on startup, idempotent)
        services.AddHostedService<Icd10Seeder>();

        return services;
    }
}
