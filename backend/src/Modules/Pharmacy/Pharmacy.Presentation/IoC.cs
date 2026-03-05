using Microsoft.Extensions.DependencyInjection;

namespace Pharmacy.Presentation;

/// <summary>
/// DI registration for the Pharmacy Presentation layer.
/// Placeholder -- no presentation-specific services currently.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddPharmacyPresentation(this IServiceCollection services)
    {
        return services;
    }
}
