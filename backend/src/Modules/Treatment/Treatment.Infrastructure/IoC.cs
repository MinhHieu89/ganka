using Microsoft.Extensions.DependencyInjection;
using Treatment.Application;
using Treatment.Application.Interfaces;
using Treatment.Infrastructure.Repositories;
using Treatment.Infrastructure.Services;

namespace Treatment.Infrastructure;

/// <summary>
/// DI registration for the Treatment Infrastructure layer.
/// Registers repositories and Unit of Work.
/// Note: TreatmentDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddTreatmentInfrastructure(this IServiceCollection services)
    {
        // Protocol template repository (TRT-01)
        services.AddScoped<ITreatmentProtocolRepository, TreatmentProtocolRepository>();

        // Treatment package repository (TRT-02)
        services.AddScoped<ITreatmentPackageRepository, TreatmentPackageRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // OSDI token store (in-memory singleton with TTL expiration)
        services.AddSingleton<IOsdiTokenStore, InMemoryOsdiTokenStore>();

        return services;
    }
}
