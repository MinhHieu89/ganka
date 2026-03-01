using Microsoft.Extensions.DependencyInjection;
using Patient.Application.Interfaces;
using Patient.Infrastructure.Repositories;
using Patient.Infrastructure.Seeding;

namespace Patient.Infrastructure;

/// <summary>
/// DI registration for the Patient Infrastructure layer.
/// Registers repositories, Unit of Work, and data seeder.
/// Note: PatientDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddPatientInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAllergyCatalogRepository, AllergyCatalogRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Data seeding
        services.AddHostedService<AllergyCatalogSeeder>();

        return services;
    }
}
