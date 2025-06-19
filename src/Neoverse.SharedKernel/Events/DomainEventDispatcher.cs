using Microsoft.Extensions.DependencyInjection;

namespace Neoverse.SharedKernel.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _provider;
    public DomainEventDispatcher(IServiceProvider provider) => _provider = provider;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _provider.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                var handleMethod = handlerType.GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    var task = (Task?)handleMethod.Invoke(handler, new object[] { domainEvent, ct });
                    if (task is not null)
                        await task;
                }
            }
        }
    }
}

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
