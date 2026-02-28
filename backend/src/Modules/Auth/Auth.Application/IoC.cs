using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Application;

/// <summary>
/// DI registration for the Auth Application layer.
/// Registers FluentValidation validators from this assembly.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddAuthApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Marker).Assembly, ServiceLifetime.Scoped);
        return services;
    }
}
