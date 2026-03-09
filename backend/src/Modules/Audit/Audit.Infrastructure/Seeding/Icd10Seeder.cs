using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain;
using Shared.Infrastructure;

namespace Audit.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds ICD-10 ophthalmology codes on startup.
/// Loads codes from the embedded icd10-ophthalmology.json resource file.
/// Idempotent: inserts new codes and updates existing ones with changed descriptions.
/// </summary>
public sealed class Icd10Seeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Icd10Seeder> _logger;

    public Icd10Seeder(IServiceProvider serviceProvider, ILogger<Icd10Seeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ReferenceDbContext>();

            // Ensure the reference schema and table exist
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            var existingEntities = await dbContext.Icd10Codes
                .ToDictionaryAsync(c => c.Code, cancellationToken);

            var seedData = await LoadSeedDataAsync(cancellationToken);

            var newCount = 0;
            var updatedCount = 0;

            foreach (var seed in seedData)
            {
                if (existingEntities.TryGetValue(seed.Code, out var existing))
                {
                    // Update existing entity if descriptions have changed
                    if (existing.DescriptionEn != seed.DescriptionEn ||
                        existing.DescriptionVi != seed.DescriptionVi)
                    {
                        dbContext.Entry(existing).Property(e => e.DescriptionEn).CurrentValue = seed.DescriptionEn;
                        dbContext.Entry(existing).Property(e => e.DescriptionVi).CurrentValue = seed.DescriptionVi;
                        updatedCount++;
                    }
                }
                else
                {
                    // Insert new code
                    var newCode = Icd10Code.Create(
                        seed.Code,
                        seed.DescriptionEn,
                        seed.DescriptionVi,
                        seed.Category,
                        seed.RequiresLaterality);
                    dbContext.Icd10Codes.Add(newCode);
                    newCount++;
                }
            }

            if (newCount > 0 || updatedCount > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);

                if (newCount > 0 && updatedCount > 0)
                {
                    _logger.LogInformation(
                        "Seeded {NewCount} new and updated {UpdatedCount} existing ICD-10 codes ({Total} total)",
                        newCount,
                        updatedCount,
                        existingEntities.Count + newCount);
                }
                else if (newCount > 0)
                {
                    _logger.LogInformation(
                        "Seeded {NewCount} ICD-10 ophthalmology codes ({Total} total in database)",
                        newCount,
                        existingEntities.Count + newCount);
                }
                else
                {
                    _logger.LogInformation(
                        "Updated {UpdatedCount} ICD-10 codes ({Total} total in database)",
                        updatedCount,
                        existingEntities.Count);
                }
            }
            else
            {
                _logger.LogInformation(
                    "ICD-10 codes already seeded ({Count} codes in database)",
                    existingEntities.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed ICD-10 ophthalmology codes");
            // Don't throw -- seeding failure should not prevent application startup
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task<List<Icd10SeedEntry>> LoadSeedDataAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("icd10-ophthalmology.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is not null)
        {
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is not null)
            {
                var data = await JsonSerializer.DeserializeAsync<List<Icd10SeedEntry>>(stream, JsonOptions, cancellationToken);
                return data ?? [];
            }
        }

        // Fallback: try loading from file path relative to the assembly
        var assemblyDir = Path.GetDirectoryName(assembly.Location) ?? string.Empty;
        var filePath = Path.Combine(assemblyDir, "Seeding", "icd10-ophthalmology.json");

        if (!File.Exists(filePath))
        {
            // Try looking relative to the project source
            var projectDir = Path.GetDirectoryName(assembly.Location);
            while (projectDir is not null)
            {
                var candidate = Path.Combine(projectDir, "Seeding", "icd10-ophthalmology.json");
                if (File.Exists(candidate))
                {
                    filePath = candidate;
                    break;
                }
                projectDir = Path.GetDirectoryName(projectDir);
            }
        }

        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<List<Icd10SeedEntry>>(json, JsonOptions) ?? [];
        }

        return [];
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// DTO for deserializing seed data from JSON.
    /// </summary>
    private sealed record Icd10SeedEntry(
        string Code,
        string DescriptionEn,
        string DescriptionVi,
        string Category,
        bool RequiresLaterality);
}
