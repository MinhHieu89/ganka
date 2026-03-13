using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain;

namespace Billing.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds default shift templates (Morning and Afternoon).
/// Idempotent: skips seeding if templates already exist.
/// Matches the AllergyCatalogSeeder pattern from Phase 2.
/// </summary>
public sealed class ShiftTemplateSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShiftTemplateSeeder> _logger;

    public ShiftTemplateSeeder(
        IServiceProvider serviceProvider,
        ILogger<ShiftTemplateSeeder> logger)
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
            _logger.LogInformation("ShiftTemplateSeeder: Starting shift template seeding...");

            if (await dbContext.ShiftTemplates.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("ShiftTemplateSeeder: Shift templates already seeded. Skipping.");
                return;
            }

            var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

            var templates = new List<ShiftTemplate>
            {
                ShiftTemplate.Create(
                    "Morning",
                    "Ca sáng",
                    new TimeOnly(8, 0),
                    new TimeOnly(12, 0),
                    branchId),

                ShiftTemplate.Create(
                    "Afternoon",
                    "Ca chiều",
                    new TimeOnly(13, 0),
                    new TimeOnly(20, 0),
                    branchId)
            };

            dbContext.ShiftTemplates.AddRange(templates);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ShiftTemplateSeeder: Seeded {Count} shift templates.", templates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShiftTemplateSeeder: Error seeding shift templates.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
