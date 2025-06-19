using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using Neoverse.SharedKernel.Entities;
using Neoverse.SharedKernel.Events;

namespace Neoverse.SharedKernel.Interceptors;

public class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _dispatcher;

    public DomainEventDispatchInterceptor(IDomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var domainEntities = eventData.Context.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            foreach (var entity in domainEntities)
            {
                var events = entity.DomainEvents.ToArray();
                entity.ClearDomainEvents();
                await _dispatcher.DispatchAsync(events, cancellationToken);
            }
        }
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
