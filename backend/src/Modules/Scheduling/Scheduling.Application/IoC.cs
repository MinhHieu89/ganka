using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Scheduling.Application;

/// <summary>
/// DI registration for the Scheduling Application layer.
/// Registers FluentValidation validators from this assembly.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddSchedulingApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Marker).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
