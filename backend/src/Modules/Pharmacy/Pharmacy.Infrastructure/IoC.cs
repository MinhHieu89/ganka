using Microsoft.Extensions.DependencyInjection;
using Pharmacy.Application.Interfaces;
using Pharmacy.Infrastructure.Repositories;
using Pharmacy.Infrastructure.Seeding;

namespace Pharmacy.Infrastructure;

/// <summary>
/// DI registration for the Pharmacy Infrastructure layer.
/// Registers repositories, Unit of Work, and the drug catalog seeder.
/// Note: PharmacyDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddPharmacyInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IDrugCatalogItemRepository, DrugCatalogItemRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IDrugBatchRepository, DrugBatchRepository>();
        services.AddScoped<IStockImportRepository, StockImportRepository>();
        services.AddScoped<IDispensingRepository, DispensingRepository>();
        services.AddScoped<IOtcSaleRepository, OtcSaleRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Seeder
        services.AddHostedService<DrugCatalogSeeder>();

        return services;
    }
}
