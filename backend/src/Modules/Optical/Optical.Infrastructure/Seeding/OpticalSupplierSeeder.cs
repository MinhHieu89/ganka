using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Pharmacy.Infrastructure;
using Shared.Domain;

namespace Optical.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds known optical suppliers into the Pharmacy supplier registry.
/// Idempotent: checks supplier name before inserting, and adds the Optical flag if missing.
///
/// Seeds three optical suppliers at startup:
///   - Essilor Vietnam
///   - Hoya Lens Vietnam
///   - Kinh mat Viet Phap
///
/// Architecture: Uses PharmacyDbContext directly as an interim cross-module access pattern.
/// TODO: Migrate to Pharmacy.Contracts CreateSupplier command via IMessageBus when that
/// contract is exposed (keeping clean module boundary per OPT-02 architectural decision).
/// </summary>
public sealed class OpticalSupplierSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OpticalSupplierSeeder> _logger;

    /// <summary>
    /// Default BranchId used for seeded suppliers. Matches the seeded branch in the system.
    /// </summary>
    private static readonly BranchId SeedBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    /// <summary>
    /// The three known optical suppliers to seed.
    /// All suppliers are tagged with SupplierType.Optical so they appear in optical supplier dropdowns.
    /// </summary>
    private static readonly IReadOnlyList<(string Name, string ContactInfo)> OpticalSuppliers =
    [
        ("Essilor Vietnam", "Ho Chi Minh City"),
        ("Hoya Lens Vietnam", "Ho Chi Minh City"),
        ("Kinh mat Viet Phap", "Ho Chi Minh City"),
    ];

    public OpticalSupplierSeeder(
        IServiceProvider serviceProvider,
        ILogger<OpticalSupplierSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();

        try
        {
            _logger.LogInformation("OpticalSupplierSeeder: Starting optical supplier seeding...");

            var existingSuppliers = await dbContext.Suppliers.ToListAsync(cancellationToken);

            int created = 0;
            int updated = 0;

            foreach (var (name, contactInfo) in OpticalSuppliers)
            {
                var existing = existingSuppliers.FirstOrDefault(
                    s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

                if (existing is null)
                {
                    // Supplier does not exist — create with Optical flag
                    var supplier = Supplier.Create(
                        name,
                        contactInfo,
                        phone: null,
                        email: null,
                        SeedBranchId,
                        SupplierType.Optical);

                    dbContext.Suppliers.Add(supplier);
                    created++;

                    _logger.LogInformation(
                        "OpticalSupplierSeeder: Creating optical supplier '{Name}'.", name);
                }
                else if ((existing.SupplierTypes & SupplierType.Optical) == 0)
                {
                    // Supplier exists but does not have Optical flag — add it via bitwise OR
                    existing.SetSupplierTypes(existing.SupplierTypes | SupplierType.Optical);
                    updated++;

                    _logger.LogInformation(
                        "OpticalSupplierSeeder: Adding Optical flag to existing supplier '{Name}'.", name);
                }
                else
                {
                    _logger.LogDebug(
                        "OpticalSupplierSeeder: Supplier '{Name}' already has Optical flag. Skipping.", name);
                }
            }

            if (created > 0 || updated > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "OpticalSupplierSeeder: Seeding complete. Created={Created}, Updated={Updated}.",
                    created, updated);
            }
            else
            {
                _logger.LogInformation(
                    "OpticalSupplierSeeder: All optical suppliers already seeded. Skipping.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpticalSupplierSeeder: Error seeding optical suppliers.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
