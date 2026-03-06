using Microsoft.Extensions.DependencyInjection;
using Pharmacy.Application.Interfaces;
using Pharmacy.Infrastructure.Repositories;
using Pharmacy.Infrastructure.Seeding;

namespace Pharmacy.Infrastructure;

/// <summary>
/// DI registration for the Pharmacy Infrastructure layer.
/// Registers repositories, Unit of Work, catalog seeders, and consumables repository.
/// Note: PharmacyDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddPharmacyInfrastructure(this IServiceCollection services)
    {
        // Pharmacy drug repositories
        services.AddScoped<IDrugCatalogItemRepository, DrugCatalogItemRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IDrugBatchRepository, DrugBatchRepository>();
        services.AddScoped<IStockImportRepository, StockImportRepository>();
        services.AddScoped<IDispensingRepository, DispensingRepository>();
        services.AddScoped<IOtcSaleRepository, OtcSaleRepository>();

        // Consumables warehouse repository (CON-01, CON-02)
        services.AddScoped<IConsumableRepository, ConsumableRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Catalog seeders (idempotent IHostedService implementations)
        services.AddHostedService<DrugCatalogSeeder>();
        services.AddHostedService<ConsumableCatalogSeeder>();

        return services;
    }
}
