using Microsoft.Extensions.DependencyInjection;

namespace Scheduling.Presentation;

/// <summary>
/// DI registration for the Scheduling Presentation layer.
/// Placeholder -- no presentation-specific services currently.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddSchedulingPresentation(this IServiceCollection services)
    {
        return services;
    }
}
