using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Pharmacy.Application;

/// <summary>
/// DI registration for the Pharmacy Application layer.
/// Registers FluentValidation validators from this assembly.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddPharmacyApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Marker).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
