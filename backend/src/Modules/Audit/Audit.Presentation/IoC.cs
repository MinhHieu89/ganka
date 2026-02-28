using Microsoft.Extensions.DependencyInjection;

namespace Audit.Presentation;

/// <summary>
/// DI registration for the Audit Presentation layer.
/// Placeholder -- no presentation-specific services currently.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddAuditPresentation(this IServiceCollection services)
    {
        return services;
    }
}
