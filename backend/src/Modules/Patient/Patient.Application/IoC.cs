using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Patient.Application;

/// <summary>
/// DI registration for the Patient Application layer.
/// Registers FluentValidation validators from this assembly.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddPatientApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Marker).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
