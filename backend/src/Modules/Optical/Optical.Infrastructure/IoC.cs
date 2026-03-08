using Microsoft.Extensions.DependencyInjection;
using Optical.Application.Interfaces;
using Optical.Infrastructure.Repositories;
using Optical.Infrastructure.Seeding;

namespace Optical.Infrastructure;

/// <summary>
/// DI registration for the Optical Infrastructure layer.
/// Registers repositories, Unit of Work, and the optical supplier seeder.
/// Note: OpticalDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddOpticalInfrastructure(this IServiceCollection services)
    {
        // Frame and Lens catalog repositories (OPT-01, OPT-02)
        services.AddScoped<IFrameRepository, FrameRepository>();
        services.AddScoped<ILensCatalogRepository, LensCatalogRepository>();

        // Glasses order repository (OPT-03, OPT-04)
        services.AddScoped<IGlassesOrderRepository, GlassesOrderRepository>();

        // Combo package repository (OPT-06)
        services.AddScoped<IComboPackageRepository, ComboPackageRepository>();

        // Warranty claim repository (OPT-07)
        services.AddScoped<IWarrantyClaimRepository, WarrantyClaimRepository>();

        // Stocktaking repository (OPT-09)
        services.AddScoped<IStocktakingRepository, StocktakingRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Optical supplier seeder (idempotent IHostedService)
        // Seeds Essilor Vietnam, Hoya Lens Vietnam, and Kinh mat Viet Phap
        // with SupplierType.Optical flag using PharmacyDbContext cross-module access.
        services.AddHostedService<OpticalSupplierSeeder>();

        return services;
    }
}
