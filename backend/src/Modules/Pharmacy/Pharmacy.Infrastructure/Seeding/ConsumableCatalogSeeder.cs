using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds the consumables warehouse with core IPL/LLLT treatment supplies.
/// Idempotent: only seeds if the ConsumableItems table is empty.
/// All Vietnamese names use proper diacritics per project convention.
/// Seeded items cover the most common ophthalmic treatment supplies for an eye clinic.
/// </summary>
public sealed class ConsumableCatalogSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConsumableCatalogSeeder> _logger;

    /// <summary>
    /// Default BranchId used for seed data. Matches the seeded branch in the system.
    /// </summary>
    private static readonly BranchId SeedBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    /// <summary>
    /// Default minimum stock level for all seeded consumable items.
    /// Items are seeded with CurrentStock = 0 — clinic must import actual stock.
    /// </summary>
    private const int DefaultMinStockLevel = 10;

    public ConsumableCatalogSeeder(
        IServiceProvider serviceProvider,
        ILogger<ConsumableCatalogSeeder> logger)
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
            _logger.LogInformation("ConsumableCatalogSeeder: Starting consumables warehouse seeding...");

            var existingCount = await dbContext.ConsumableItems.CountAsync(cancellationToken);
            if (existingCount > 0)
            {
                _logger.LogInformation(
                    "ConsumableCatalogSeeder: Consumables already seeded ({Count} items). Skipping.",
                    existingCount);
                return;
            }

            var items = GetCatalogItems();
            dbContext.ConsumableItems.AddRange(items);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ConsumableCatalogSeeder: Seeded {Count} consumable items.", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConsumableCatalogSeeder: Error seeding consumables catalog.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<ConsumableItem> GetCatalogItems()
    {
        return
        [
            // ── IPL Treatment Supplies ──────────────────────────────────────────
            ConsumableItem.Create(
                "IPL Gel",
                "Gel IPL",
                "Tuýp",
                ConsumableTrackingMode.SimpleStock,
                DefaultMinStockLevel,
                SeedBranchId),

            ConsumableItem.Create(
                "Eye Shields",
                "Kính chắn mắt",
                "Cái",
                ConsumableTrackingMode.SimpleStock,
                DefaultMinStockLevel,
                SeedBranchId),

            ConsumableItem.Create(
                "LLLT Disposable Tips",
                "Đầu LLLT dùng một lần",
                "Cái",
                ConsumableTrackingMode.SimpleStock,
                DefaultMinStockLevel,
                SeedBranchId),

            ConsumableItem.Create(
                "Lid Care Pads",
                "Miếng vệ sinh mi mắt",
                "Cái",
                ConsumableTrackingMode.SimpleStock,
                DefaultMinStockLevel,
                SeedBranchId),

            ConsumableItem.Create(
                "Sterile Wipes",
                "Khăn lau vô trùng",
                "Cái",
                ConsumableTrackingMode.SimpleStock,
                DefaultMinStockLevel,
                SeedBranchId),

            // ── Expiry-Tracked Supplies ─────────────────────────────────────────
            ConsumableItem.Create(
                "Anesthetic Eye Drops",
                "Thuốc nhỏ tê",
                "Chai",
                ConsumableTrackingMode.ExpiryTracked,
                DefaultMinStockLevel,
                SeedBranchId),

            ConsumableItem.Create(
                "Fluorescein Strips",
                "Giấy thử fluorescein",
                "Hộp",
                ConsumableTrackingMode.ExpiryTracked,
                DefaultMinStockLevel,
                SeedBranchId),

            ConsumableItem.Create(
                "Saline Solution",
                "Nước muối sinh lý",
                "Chai",
                ConsumableTrackingMode.ExpiryTracked,
                DefaultMinStockLevel,
                SeedBranchId),

            // ── General Clinic Supplies ─────────────────────────────────────────
            ConsumableItem.Create(
                "Cotton Applicators",
                "Que bông tăm",
                "Túi",
                ConsumableTrackingMode.SimpleStock,
                DefaultMinStockLevel,
                SeedBranchId),

            ConsumableItem.Create(
                "Disposable Gloves",
                "Găng tay dùng một lần",
                "Hộp",
                ConsumableTrackingMode.SimpleStock,
                DefaultMinStockLevel,
                SeedBranchId),

            // ── MGD / Meibomian Gland Supplies ─────────────────────────────────
            ConsumableItem.Create(
                "MGD Expression Forceps Tips",
                "Đầu kẹp nặn tuyến Meibomian",
                "Cái",
                ConsumableTrackingMode.SimpleStock,
                DefaultMinStockLevel,
                SeedBranchId),

            ConsumableItem.Create(
                "Thermal Pulsation Eyecups",
                "Cốc mắt nhiệt xung",
                "Cái",
                ConsumableTrackingMode.SimpleStock,
                DefaultMinStockLevel,
                SeedBranchId),
        ];
    }
}
