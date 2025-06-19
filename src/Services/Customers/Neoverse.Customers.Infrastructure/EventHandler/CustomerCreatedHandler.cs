using Neoverse.Customers.Domain.Events;
using Neoverse.SharedKernel.Events;

namespace Neoverse.Customers.Infrastructure.EventHandler;

public class CustomerCreatedHandler(KafkaMessageBus bus) : IDomainEventHandler<CustomerCreatedEvent>
{
    public Task HandleAsync(CustomerCreatedEvent domainEvent, CancellationToken cancellationToken = default)
        => bus.ProduceAsync("customers", $"created:{domainEvent.Customer.Id}", cancellationToken);
}
