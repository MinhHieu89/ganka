using Billing.Application.Interfaces;
using Billing.Infrastructure.Repositories;
using Billing.Infrastructure.Seeding;
using Billing.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Billing.Infrastructure;

/// <summary>
/// DI registration for the Billing Infrastructure layer.
/// Registers repositories, Unit of Work, document services, and catalog seeders.
/// Note: BillingDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class InfrastructureIoC
{
    public static IServiceCollection AddBillingInfrastructure(this IServiceCollection services)
    {
        // Billing repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICashierShiftRepository, CashierShiftRepository>();
        services.AddScoped<IServiceCatalogRepository, ServiceCatalogRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Document services (PDF generation and e-invoice export)
        services.AddScoped<IBillingDocumentService, BillingDocumentService>();

        // Real-time notifications (SignalR push to cashier dashboard)
        services.AddScoped<IBillingNotificationService, BillingNotificationService>();

        // Shift template seeder (idempotent IHostedService)
        services.AddHostedService<ShiftTemplateSeeder>();

        // Service catalog seeder (idempotent IHostedService)
        services.AddHostedService<ServiceCatalogSeeder>();

        return services;
    }
}
