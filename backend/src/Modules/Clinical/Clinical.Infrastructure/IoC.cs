using Clinical.Application.Interfaces;
using Clinical.Infrastructure.Repositories;
using Clinical.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Clinical.Infrastructure;

/// <summary>
/// DI registration for the Clinical Infrastructure layer.
/// Registers repositories and Unit of Work.
/// Note: ClinicalDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddClinicalInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<IDoctorIcd10FavoriteRepository, DoctorIcd10FavoriteRepository>();
        services.AddScoped<IMedicalImageRepository, MedicalImageRepository>();
        services.AddScoped<IOsdiSubmissionRepository, OsdiSubmissionRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddSingleton<IOsdiNotificationService, OsdiNotificationService>();

        return services;
    }
}
