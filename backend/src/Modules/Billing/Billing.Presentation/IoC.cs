using Microsoft.Extensions.DependencyInjection;

namespace Billing.Presentation;

/// <summary>
/// DI registration for the Billing Presentation layer.
/// Placeholder -- no presentation-specific services currently.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddBillingPresentation(this IServiceCollection services)
    {
        return services;
    }
}
