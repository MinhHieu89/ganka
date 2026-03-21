using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Application;
using Shared.Application.Interfaces;
using Shared.Application.Services;
using Shared.Infrastructure.Interceptors;
using Shared.Infrastructure.Repositories;
using Shared.Infrastructure.Services;

namespace Shared.Infrastructure;

/// <summary>
/// DI registration for the Shared Infrastructure layer.
/// Registers HttpContextAccessor, CurrentUser, BranchContext, blob storage, and ClinicSettingsService.
/// In Development, uses LocalFileStorageService (local disk) instead of AzureBlobService (Azure/Azurite).
/// </summary>
public static class IoC
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IBranchContext, BranchContext>();

        // Blob storage: use local file system in Development, Azure Blob in other environments
        services.AddScoped<IAzureBlobService>(sp =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            if (env.IsDevelopment())
            {
                var logger = sp.GetRequiredService<ILogger<LocalFileStorageService>>();
                var basePath = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
                var baseUrl = "http://localhost:5255";
                return new LocalFileStorageService(logger, basePath, baseUrl);
            }

            var configuration = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var azureLogger = sp.GetRequiredService<ILogger<AzureBlobService>>();
            return new AzureBlobService(configuration, azureLogger);
        });

        services.AddScoped<IClinicSettingsService, ClinicSettingsService>();
        services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
        services.AddScoped<DomainEventDispatcherInterceptor>();

        return services;
    }
}
