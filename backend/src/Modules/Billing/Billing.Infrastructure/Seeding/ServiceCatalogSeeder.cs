using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain;

namespace Billing.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds default service catalog items (Consultation and Follow-up).
/// Idempotent: skips seeding if items already exist.
/// </summary>
public sealed class ServiceCatalogSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceCatalogSeeder> _logger;

    public ServiceCatalogSeeder(
        IServiceProvider serviceProvider,
        ILogger<ServiceCatalogSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        try
        {
            _logger.LogInformation("ServiceCatalogSeeder: Starting service catalog seeding...");

            if (await dbContext.ServiceCatalogItems.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("ServiceCatalogSeeder: Service catalog items already seeded. Skipping.");
                return;
            }

            var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

            var items = new List<ServiceCatalogItem>
            {
                ServiceCatalogItem.Create(
                    "CONSULTATION",
                    "Consultation",
                    "Kham benh",
                    150000,
                    branchId),

                ServiceCatalogItem.Create(
                    "FOLLOWUP",
                    "Follow-up Visit",
                    "Tai kham",
                    100000,
                    branchId)
            };

            dbContext.ServiceCatalogItems.AddRange(items);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ServiceCatalogSeeder: Seeded {Count} service catalog items.", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ServiceCatalogSeeder: Error seeding service catalog items.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
