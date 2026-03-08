using Microsoft.Extensions.DependencyInjection;

namespace Optical.Presentation;

/// <summary>
/// DI registration for the Optical Presentation layer.
/// Placeholder -- no presentation-specific services currently.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddOpticalPresentation(this IServiceCollection services)
    {
        return services;
    }
}
