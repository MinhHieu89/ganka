using Microsoft.Extensions.DependencyInjection;

namespace Patient.Presentation;

/// <summary>
/// DI registration for the Patient Presentation layer.
/// Placeholder -- no presentation-specific services currently.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddPatientPresentation(this IServiceCollection services)
    {
        return services;
    }
}
