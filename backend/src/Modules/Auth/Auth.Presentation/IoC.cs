using Microsoft.Extensions.DependencyInjection;

namespace Auth.Presentation;

/// <summary>
/// DI registration for the Auth Presentation layer.
/// Placeholder -- no presentation-specific services currently.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddAuthPresentation(this IServiceCollection services)
    {
        return services;
    }
}
