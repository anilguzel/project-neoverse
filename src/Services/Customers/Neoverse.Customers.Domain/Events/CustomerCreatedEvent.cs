using Neoverse.SharedKernel.Events;
using Neoverse.Customers.Domain.Entities;

namespace Neoverse.Customers.Domain.Events;

public record CustomerCreatedEvent(Customer Customer) : DomainEventBase(DateTime.UtcNow);
