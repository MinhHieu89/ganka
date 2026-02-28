using Microsoft.Extensions.DependencyInjection;

namespace Audit.Application;

/// <summary>
/// DI registration for the Audit Application layer.
/// Placeholder -- audit handlers are discovered by Wolverine via Marker assembly.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddAuditApplication(this IServiceCollection services)
    {
        return services;
    }
}
