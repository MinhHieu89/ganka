using Microsoft.Extensions.DependencyInjection;

namespace Optical.Infrastructure;

/// <summary>
/// DI registration for the Optical Infrastructure layer.
/// Registers repositories, unit of work, and catalog seeders.
/// Note: OpticalDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddOpticalInfrastructure(this IServiceCollection services)
    {
        // Repositories and UoW will be registered here as they are implemented
        // in subsequent plans (08-25 through 08-31).
        return services;
    }
}
