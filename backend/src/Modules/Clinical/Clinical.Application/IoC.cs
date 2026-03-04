using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Clinical.Application;

/// <summary>
/// DI registration for the Clinical Application layer.
/// Registers FluentValidation validators from this assembly.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddClinicalApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Marker).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
