using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Patient.Domain.Entities;

namespace Patient.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds the allergy catalog with common ophthalmology-relevant allergies.
/// Idempotent: only creates items that don't already exist.
/// </summary>
public sealed class AllergyCatalogSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AllergyCatalogSeeder> _logger;

    public AllergyCatalogSeeder(
        IServiceProvider serviceProvider,
        ILogger<AllergyCatalogSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PatientDbContext>();

        try
        {
            _logger.LogInformation("AllergyCatalogSeeder: Starting allergy catalog seeding...");

            var existingCount = await dbContext.AllergyCatalogItems.CountAsync(cancellationToken);
            if (existingCount > 0)
            {
                _logger.LogInformation("AllergyCatalogSeeder: Allergy catalog already seeded ({Count} items). Skipping.", existingCount);
                return;
            }

            var items = GetCatalogItems();
            dbContext.AllergyCatalogItems.AddRange(items);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("AllergyCatalogSeeder: Seeded {Count} allergy catalog items.", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AllergyCatalogSeeder: Error seeding allergy catalog.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<AllergyCatalogItem> GetCatalogItems()
    {
        return
        [
            // Ophthalmic drug allergies
            AllergyCatalogItem.Create("Atropine", "Atropine", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Fluorescein", "Fluorescein", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Tropicamide", "Tropicamide", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Proparacaine", "Proparacaine", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Tetracaine", "Tetracaine", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Timolol", "Timolol", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Latanoprost", "Latanoprost", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Brimonidine", "Brimonidine", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Dorzolamide", "Dorzolamide", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Cyclopentolate", "Cyclopentolate", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Pilocarpine", "Pilocarpine", "Ophthalmic Drug"),
            AllergyCatalogItem.Create("Prednisolone Acetate", "Prednisolone Acetate", "Ophthalmic Drug"),

            // General drug allergies
            AllergyCatalogItem.Create("Penicillin", "Penicillin", "General Drug"),
            AllergyCatalogItem.Create("Sulfonamides", "Sulfonamid", "General Drug"),
            AllergyCatalogItem.Create("Aspirin", "Aspirin", "General Drug"),
            AllergyCatalogItem.Create("Ibuprofen", "Ibuprofen", "General Drug"),
            AllergyCatalogItem.Create("Codeine", "Codein", "General Drug"),
            AllergyCatalogItem.Create("Amoxicillin", "Amoxicillin", "General Drug"),
            AllergyCatalogItem.Create("Cephalosporins", "Cephalosporin", "General Drug"),

            // Material allergies
            AllergyCatalogItem.Create("Latex", "Cao su (Latex)", "Material"),
            AllergyCatalogItem.Create("Contact Lens Solution", "Dung dich kinh ap trong", "Material"),
            AllergyCatalogItem.Create("Adhesive Tape", "Bang dinh y te", "Material"),

            // Environmental allergies
            AllergyCatalogItem.Create("Dust", "Bui", "Environmental"),
            AllergyCatalogItem.Create("Pollen", "Phan hoa", "Environmental"),
            AllergyCatalogItem.Create("Animal Dander", "Long dong vat", "Environmental"),
            AllergyCatalogItem.Create("Mold", "Nam moc", "Environmental"),
        ];
    }
}
