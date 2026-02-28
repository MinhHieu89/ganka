using Microsoft.Extensions.DependencyInjection;
using Shared.Application;
using Shared.Application.Services;
using Shared.Infrastructure.Services;

namespace Shared.Infrastructure;

/// <summary>
/// DI registration for the Shared Infrastructure layer.
/// Registers HttpContextAccessor, CurrentUser, BranchContext, and AzureBlobService.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IBranchContext, BranchContext>();
        services.AddScoped<IAzureBlobService, AzureBlobService>();

        return services;
    }
}
