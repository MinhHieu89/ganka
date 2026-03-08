using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Optical.Application;

/// <summary>
/// DI registration for the Optical Application layer.
/// Registers FluentValidation validators from this assembly.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddOpticalApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Marker).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
