using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Billing.Application;

/// <summary>
/// DI registration for the Billing Application layer.
/// Registers FluentValidation validators from this assembly.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddBillingApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Marker).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
