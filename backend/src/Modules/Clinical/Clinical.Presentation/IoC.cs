using Microsoft.Extensions.DependencyInjection;

namespace Clinical.Presentation;

/// <summary>
/// DI registration for the Clinical Presentation layer.
/// Placeholder -- no presentation-specific services currently.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddClinicalPresentation(this IServiceCollection services)
    {
        return services;
    }
}
