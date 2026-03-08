using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Treatment.Application;

/// <summary>
/// DI registration for the Treatment Application layer.
/// Registers FluentValidation validators from this assembly.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddTreatmentApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Marker).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
