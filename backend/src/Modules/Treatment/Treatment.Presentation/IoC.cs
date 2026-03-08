using Microsoft.Extensions.DependencyInjection;

namespace Treatment.Presentation;

/// <summary>
/// DI registration for the Treatment Presentation layer.
/// Placeholder -- no presentation-specific services currently.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddTreatmentPresentation(this IServiceCollection services)
    {
        return services;
    }
}
