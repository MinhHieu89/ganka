using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain;
using Wolverine;

namespace Shared.Infrastructure.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that dispatches domain events from aggregate roots
/// via Wolverine's IMessageBus after successful save. This enables the cascading handler
/// pattern where domain events are converted to integration events for cross-module communication.
/// </summary>
public sealed class DomainEventDispatcherInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcherInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context);
        }

        return result;
    }

    private async Task DispatchDomainEventsAsync(
        Microsoft.EntityFrameworkCore.DbContext context)
    {
        var aggregateRoots = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        if (aggregateRoots.Count == 0) return;

        var domainEvents = aggregateRoots
            .SelectMany(ar => ar.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregateRoots)
        {
            aggregate.ClearDomainEvents();
        }

        using var scope = _serviceProvider.CreateScope();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        foreach (var domainEvent in domainEvents)
        {
            await messageBus.PublishAsync(domainEvent);
        }
    }
}
